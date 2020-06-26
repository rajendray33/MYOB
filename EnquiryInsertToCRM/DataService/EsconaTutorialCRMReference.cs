using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.EsconaTutorialServiceReference;
using EnquiryInsertToCRM.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EnquiryInsertToCRM.DataService
{
    public static class EsconaTutorialCRMReference
    {
        public static string GetToken()
        {
            string token = string.Empty;
            //initialize a string variable to store the return value
            //construct the JSON request using Newtonsoft.Json.Linq.JObject
            JObject authRequest = new JObject(
                new JProperty("Database", ConfigurationManager.AppSettings["EsconaTutorialMaximizerDatabase"]),
                new JProperty("UID", ConfigurationManager.AppSettings["EsconaTutorialMaximizerUID"]),
                new JProperty("Password", ConfigurationManager.AppSettings["EsconaTutorialMaximizerPassword"])
              );

            //instantiate the WCF service client ("MaximizerWebDataService" is the service namespace)
            using (EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient())
            {
                //call the Authenticate method of the WCF service client and parse the result
                Stream response = client.Authenticate(new MemoryStream(Encoding.UTF8.GetBytes(authRequest.ToString())));
                JToken authResponse = JToken.Parse(new StreamReader(response).ReadToEnd());

                //check the return value of the response                
                if (authResponse.Value<int>("Code") == 0)
                {
                    //if the response is OK, return the token
                    JObject Data = (JObject)authResponse["Data"];
                    token = Data.Value<string>("Token");
                }
            }
            return token;
        }

        //Function check for myobID already exist based on Company name.
        public static AbEntryKeyModel ReadExistingCompanyNameOrMYOBID(string companyname, string myobID)
        {
            List<JProperty> lstFilterJProperty = new List<JProperty>();
            List<JProperty> lstRequestJProperty = new List<JProperty>();
            
            if (!string.IsNullOrEmpty(companyname))
            {
                lstFilterJProperty.Add(new JProperty("CompanyName", (companyname.Trim() ?? "")));
                lstRequestJProperty.Add(new JProperty("Udf/$TYPEID(115)", 1));
                
            }
            else if (!string.IsNullOrEmpty(myobID))
            {
                lstFilterJProperty.Add(new JProperty("Udf/$TYPEID(115)", (myobID.Trim() ?? "")));
                lstRequestJProperty.Add(new JProperty("CompanyName", 1));
                
            }
            AbEntryKeyModel model = new AbEntryKeyModel();
            string createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                                new JProperty("Key", 1),
                               lstRequestJProperty
                         )
                      )
                    )
                  ),
                  new JProperty("Criteria",
                    new JObject(
                        new JProperty("SearchQuery", new JObject(
                            new JProperty("$AND", new JArray(

                                    new JObject(
                                            new JProperty("$EQ", new JObject(
                                                   lstFilterJProperty
                                           ))
                                        ),
                                    new JObject(
                                            new JProperty("$EQ", new JObject(
                                                        new JProperty("Type", "Company")
                                                ))
                                        )
                                ))
                            )
                          )
                        )
                  )
                )
              )
            ).ToString();
            //instantiate the WCF service client
            using (EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient())
            {
                Stream response = client.AbEntryRead(new MemoryStream(Encoding.UTF8.GetBytes(createRequest)));
                JToken createResponse = JToken.Parse(new StreamReader(response).ReadToEnd());
                //check the return value of the response

              
                if (createResponse.Value<int>("Code") == 0)
                {
                 
                    JObject AppointmentData = (JObject)createResponse["AbEntry"];
                    if (AppointmentData != null)
                    {
                        var arrayAppointment = (JArray)AppointmentData["Data"];
                        if (arrayAppointment != null && arrayAppointment.Count > 0)
                        {
                            model.Key = Convert.ToString(arrayAppointment[0]["Key"] ?? "");
                            if (!string.IsNullOrEmpty(companyname))
                            {
                                model.MyObID = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(115)"] ?? "");
                            }
                            else if (!string.IsNullOrEmpty(myobID))
                            {
                                model.CompanyName = Convert.ToString(arrayAppointment[0]["CompanyName"] ?? "");
                            }
                        }
                    }
                }
            }
            return model;
        }
        public static string UpdateCompanyNameOrMyObIdInCRM(string companyname, string myobID)
        {
            string res = "";
            StringBuilder sb = new StringBuilder();
            try
            {
                string ParentKey = "";
                string rtnRes = "";
                bool isCompanyName = false;
                bool ismyobupdate = false;
                AbEntryKeyModel model = new AbEntryKeyModel();
                NotesModel objNoteModel = new NotesModel();
                string dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ssZ");
                List<string> strMyObID = new List<string>();
                List<JProperty> lstRequestJProperty = new List<JProperty>();
               
                if (!string.IsNullOrEmpty(myobID))
                {
                    model = ReadExistingCompanyNameOrMYOBID("", myobID);
                    if (string.IsNullOrEmpty(model.Key ?? ""))
                    {
                        model = ReadExistingCompanyNameOrMYOBID(companyname, "");
                        if (string.IsNullOrEmpty(model.Key))
                        {
                            return "invalidcompanyname";
                        }
                        else
                        {
                            lstRequestJProperty.Add(new JProperty("Udf/$TYPEID(115)", myobID));
                            ismyobupdate = true;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(model.Key))
                        {
                            return "alreadyexistmyobID";
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(model.CompanyName))
                            {
                                if (model.CompanyName.ToLower().Trim() == companyname.ToLower().Trim())
                                {
                                    return "alreadyexistmyobID";
                                }
                                else
                                {
                                    lstRequestJProperty.Add(new JProperty("CompanyName", companyname));
                                    isCompanyName = true;
                                    //return "companynameupdate";
                                }
                            }
                        }
                    }
                }
                if (!string.IsNullOrEmpty(model.Key))
                {
                    string requestString = new JObject(
                    new JProperty("Token", GetToken()),
                    new JProperty("AbEntry", new JObject(
                        new JProperty("Data", new JObject(
                            new JProperty("Key", model.Key),
                            new JProperty("Type", "Company"),
                            lstRequestJProperty
                        ))
                    ))
                ).ToString();

                    EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient();
                    System.IO.Stream response = null;
                    if (!string.IsNullOrEmpty(model.Key))
                    {
                        response = client.AbEntryUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                    }
                    JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

                    if (responseJSON.Value<int>("Code") != 0)
                    {
                        //there was a problem with the request                        
                    }
                    else
                    {
                        JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                        ParentKey = Data.Value<string>("Key");
                        if (!string.IsNullOrEmpty(ParentKey))
                        {
                            //Note read from CRM
                            objNoteModel.Parent = ParentKey;
                            if (!string.IsNullOrEmpty(ParentKey))
                            {
                                sb.Clear();
                                if (isCompanyName)
                                {
                                    res = "companynameupdate";
                                    sb.AppendLine("<div style='margin-bottom:5px;'><b>" + model.CompanyName + "</b> company name replaced with <b>" + companyname + ".</b></div>");
                                    sb.AppendLine("<div style='margin-bottom:5px;'><b>Record updated successfully.</b></div>");
                                }
                                if (ismyobupdate)
                                {
                                    res = "myobupdate";
                                    sb.AppendLine("<div style='margin-bottom:5px;'><b>MYOBID: </b>" + myobID + "</div>");
                                    sb.AppendLine("<div style='margin-bottom:5px;'><b>Record updated successfully.</b></div>");
                                }
                                
                                if (sb != null)
                                {
                                    objNoteModel.Text = "";
                                    objNoteModel.Text = Convert.ToString(sb);
                                }
                                objNoteModel.DateTime = dtUtcNow;
                            }
                            //Note
                            rtnRes = CreateOrUpdateNote(objNoteModel);

                            if (!string.IsNullOrWhiteSpace(rtnRes))
                            {
                                if (rtnRes == "badrequest")
                                {
                                    res = "badrequestfornote";
                                }
                            }
                        }
                        //res = "success";
                    }
                }

            }
            catch (Exception ex)
            {
                
                sb.Clear();
                sb.AppendLine("");
                sb.AppendLine("#CRM Step error:" + ex.Message);
                sb.AppendLine("");
                CommonMethod.LogFile(sb, false);
            }

            return res;
        }


        //Function check for Company already exist based on Company name.
        public static AbEntryKeyModel ReadCompanyNameExisting(string companyname)
        {
            AbEntryKeyModel model = new AbEntryKeyModel();
            string createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                                new JProperty("Key", 1),
                                new JProperty("CompanyName", 1),
                                new JProperty("Udf/$TYPEID(115)", 1)
                         )
                      )
                    )
                  ),
                  new JProperty("Criteria",
                    new JObject(
                        new JProperty("SearchQuery", new JObject(
                            new JProperty("$AND", new JArray(
                                    new JObject(
                                            new JProperty("$EQ", new JObject(
                                                        new JProperty("CompanyName", (companyname.Trim() ?? ""))
                                                   ))
                                        ),
                                    new JObject(
                                            new JProperty("$EQ", new JObject(
                                                        new JProperty("Type", "Company")
                                                ))
                                        )
                                ))
                            )
                          )
                        )
                  )
                )
              )
            ).ToString();
            //instantiate the WCF service client
            using (EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient())
            {
                Stream response = client.AbEntryRead(new MemoryStream(Encoding.UTF8.GetBytes(createRequest)));
                JToken createResponse = JToken.Parse(new StreamReader(response).ReadToEnd());
                //check the return value of the response
                if (createResponse.Value<int>("Code") == 0)
                {
                    JObject AppointmentData = (JObject)createResponse["AbEntry"];
                    if (AppointmentData != null)
                    {
                        var arrayAppointment = (JArray)AppointmentData["Data"];
                        if (arrayAppointment != null && arrayAppointment.Count > 0)
                        {
                            model.Key = Convert.ToString(arrayAppointment[0]["Key"] ?? "");
                            model.MyObID = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(115)"] ?? "");
                        }
                    }
                }
            }
            return model;
        }

        public static string UpdateMyObIdInCompany(string companyname, string myobID)
        {
            string res = "";
            StringBuilder sb = new StringBuilder();
            try
            {
                string ParentKey = "";
                string rtnRes = "";
                AbEntryKeyModel model = new AbEntryKeyModel();
                NotesModel objNoteModel = new NotesModel();
                StringBuilder strb = new StringBuilder();
                string dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ssZ");
                List<string> strMyObID = new List<string>();
                if (!string.IsNullOrEmpty(companyname) && !string.IsNullOrEmpty(myobID))
                {
                    model = ReadCompanyNameExisting(companyname);
                    if ((model.MyObID ?? "") == (myobID ?? ""))
                    {
                        return res = "alreadyexistmyobID";
                    }
                    if (string.IsNullOrEmpty(model.Key))
                    {
                        return res = "invalidcompanyname";
                    }
                    if (model.MyObID.Contains(','))
                    {
                        strMyObID = model.MyObID.Split(',').ToList();
                    }
                    strMyObID.Add(myobID);

                    if (!string.IsNullOrEmpty(model.Key))
                    {
                        string requestString = new JObject(
                        new JProperty("Token", GetToken()),
                        new JProperty("AbEntry", new JObject(
                            new JProperty("Data", new JObject(
                                new JProperty("Key", model.Key),
                                new JProperty("Type", "Company"),
                                new JProperty("Udf/$TYPEID(115)", string.Join(",", strMyObID))
                            ))
                        ))
                    ).ToString();

                        EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient();
                        System.IO.Stream response = null;
                        if (!string.IsNullOrEmpty(model.Key))
                        {
                            response = client.AbEntryUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                        }
                        JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

                        if (responseJSON.Value<int>("Code") != 0)
                        {
                            //there was a problem with the request                        
                        }
                        else
                        {
                            JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                            ParentKey = Data.Value<string>("Key");
                            if (!string.IsNullOrEmpty(ParentKey))
                            {
                                //Note read from CRM
                                objNoteModel.Parent = ParentKey;
                                if (!string.IsNullOrEmpty(ParentKey))
                                {

                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>MYOBID: </b>" + myobID + "</div>");
                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>Record updated successfully.</b></div>");
                                    if (strb != null)
                                    {
                                        objNoteModel.Text = "";
                                        objNoteModel.Text = Convert.ToString(strb);
                                    }
                                    objNoteModel.DateTime = dtUtcNow;
                                }
                                //Note
                                rtnRes = CreateOrUpdateNote(objNoteModel);

                                if (!string.IsNullOrWhiteSpace(rtnRes))
                                {
                                    if (rtnRes == "badrequest")
                                    {
                                        res = "badrequestfornote";
                                    }
                                }
                            }
                            res = "success";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.Clear();
                sb.AppendLine("");
                sb.AppendLine("#CRM Step error:" + ex.Message);
                sb.AppendLine("");
                CommonMethod.LogFile(sb, false);
            }

            return res;
        }
        //Function create note for each address book successfull inserted.
        public static string CreateOrUpdateNote(NotesModel model)
        {
            string rtnRes = string.Empty;
            string mode = string.Empty;
            string requestString = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("Note", new JObject(
                    new JProperty("Data", new JObject(
                        new JProperty("Key", model.Key),
                        new JProperty("Type", 0),
                        new JProperty("ParentKey", model.Parent),
                        new JProperty("DateTime", model.DateTime),
                        new JProperty("RichText", model.Text)
                    ))
                ))
            ).ToString();

            EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient();
            Stream response = null;
            if (string.IsNullOrEmpty(model.Key))
            {
                response = client.NoteCreate(new MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            }
            else
            {
                response = client.NoteUpdate(new MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            }
            JToken responseJSON = JToken.Parse(new StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                return rtnRes = "badrequest";
            }
            else
            {
                JObject Data = (JObject)responseJSON["Note"]["Data"];
                rtnRes = Data.Value<String>("Key");
                if (!string.IsNullOrEmpty(rtnRes))
                {
                    rtnRes = "success";
                }
                return rtnRes;
            }
        }
        //Function Get note for perticular address book entry
        public static List<NotesModel> ReadNote(string parentkey)
        {
            List<NotesModel> notesmodelList = new List<NotesModel>();
            JObject createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("Note",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                            new JProperty("Key", 1),
                            new JProperty("Type", 1),
                            new JProperty("ParentKey", 1),
                            new JProperty("DateTime", 1),
                            new JProperty("RichText", 1)
                         )
                      )
                    )
                  ),
                  new JProperty("Criteria",
                    new JObject(
                        new JProperty("SearchQuery",
                                        new JObject(
                                            new JProperty("$EQ",
                                                new JObject(
                                                        new JProperty("ParentKey", parentkey)
                                                       )
                                                     )
                                                  )
                                     )
                         )
                  )
                )
              )
            );
            using (EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient())
            {
                Stream response = client.NoteRead(new MemoryStream(Encoding.UTF8.GetBytes(createRequest.ToString())));
                JToken readResponse = JToken.Parse(new StreamReader(response).ReadToEnd());

                if (readResponse.Value<int>("Code") == 0)
                {
                    var arrayNotes = readResponse["Note"]["Data"].Value<JArray>();
                    if (arrayNotes != null)
                    {
                        if (arrayNotes != null && arrayNotes.Count > 0)
                        {
                            notesmodelList = arrayNotes.ToObject<List<NotesModel>>();
                        }
                        return notesmodelList;
                    }
                }
            }
            return notesmodelList;
        }
        public static string UpdateCompanyUDFBasedOnMYOBCustomerID(CustomerInfo objCustomerInfo)
        {
            string res = "";
            StringBuilder sb = new StringBuilder();
            AbEntryKeyModel model = new AbEntryKeyModel();
            string createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                                new JProperty("Key", 1),
                                new JProperty("CompanyName", 1),
                                new JProperty("Udf/$TYPEID(115)", 1),
                                new JProperty("Udf/$TYPEID(117)", 1),
                                new JProperty("Udf/$TYPEID(118)", 1)
                         )
                      )
                    )
                  ),
                  new JProperty("Criteria",
                    new JObject(
                        new JProperty("SearchQuery", new JObject(
                            new JProperty("$AND", new JArray(
                                    new JObject(
                                            new JProperty("$EQ", new JObject(
                                                        new JProperty("Udf/$TYPEID(115)", (objCustomerInfo.DisplayId.Trim() ?? ""))
                                                   ))
                                        ),
                                    new JObject(
                                            new JProperty("$EQ", new JObject(
                                                        new JProperty("Type", "Company")
                                                ))
                                        )
                                ))
                            )
                          )
                        )
                  )
                )
              )
            ).ToString();
            //instantiate the WCF service client
            using (EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient())
            {
                Stream response = client.AbEntryRead(new MemoryStream(Encoding.UTF8.GetBytes(createRequest)));
                JToken createResponse = JToken.Parse(new StreamReader(response).ReadToEnd());
                //check the return value of the response
                if (createResponse.Value<int>("Code") == 0)
                {
                    JObject AppointmentData = (JObject)createResponse["AbEntry"];
                    if (AppointmentData != null)
                    {
                        var arrayAppointment = (JArray)AppointmentData["Data"];
                        if (arrayAppointment != null && arrayAppointment.Count > 0)
                        {
                            model.Key = Convert.ToString(arrayAppointment[0]["Key"] ?? "");
                            decimal Total_Balance = 0;
                            decimal Overdue_Balance = 0;
                            if (!string.IsNullOrEmpty(Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(117)"])))
                            {
                                Total_Balance = Convert.ToDecimal(arrayAppointment[0]["Udf/$TYPEID(117)"]);
                            }
                            if (!string.IsNullOrEmpty(Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(118)"])))
                            {
                                Overdue_Balance = Convert.ToDecimal(arrayAppointment[0]["Udf/$TYPEID(118)"]);
                            }

                            string strTotal_Balance = string.Format("{0:0.00}", Total_Balance);
                            string strOverdue_Balance = string.Format("{0:0.00}", Overdue_Balance);
                            string strCurrentBalance = string.Format("{0:0.00}", objCustomerInfo.CurrentBalance);
                            string strPastDue = string.Format("{0:0.00}", objCustomerInfo.SellingDetails.Credit.PastDue);

                            if (!string.IsNullOrEmpty(model.Key))
                            {
                                sb.Clear();
                                sb.AppendLine("");
                                sb.AppendLine("CompanyName:" + objCustomerInfo.CompanyName);
                                sb.AppendLine("Total_Balance:" + strTotal_Balance);
                                sb.AppendLine("Overdue_Balance:" + strOverdue_Balance);
                                sb.AppendLine("objCustomerInfo.Total_Balance:" + strCurrentBalance);
                                sb.AppendLine("objCustomerInfo.Overdue_Balance:" + strPastDue);
                                if ((strTotal_Balance != strCurrentBalance) || (strOverdue_Balance != strPastDue))
                                {
                                    res = fnCompanyUDFBasedOnMYOBCustomerID(model, objCustomerInfo);                                    
                                }
                                else
                                {                                    
                                    sb.AppendLine("Already Exist Data");
                                }
                                
                                res = "success";
                            }
                            
                        }
                        else
                        {
                            res = "invalidcompanyname";
                        }
                    }
                    
                }
            }
            return res;
        }
        public static string fnCompanyUDFBasedOnMYOBCustomerID(AbEntryKeyModel model, CustomerInfo objCustomerInfo)
        {
            string res = "";
            string ParentKey = "";
            NotesModel objNoteModel = new NotesModel();
            StringBuilder strb = new StringBuilder();
            string dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ssZ");
            string requestString = new JObject(
                     new JProperty("Token", GetToken()),
                     new JProperty("AbEntry", new JObject(
                         new JProperty("Data", new JObject(
                             new JProperty("Key", model.Key),
                             new JProperty("Type", "Company"),
                             new JProperty("Udf/$TYPEID(117)", objCustomerInfo.CurrentBalance),
                             new JProperty("Udf/$TYPEID(118)", objCustomerInfo.SellingDetails.Credit.PastDue)
                         ))
                     ))
                 ).ToString();
            //instantiate the WCF service client
            using (EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient())
            {
                System.IO.Stream response = null;
                if (!string.IsNullOrEmpty(model.Key))
                {
                    response = client.AbEntryUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                }
                JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

                if (responseJSON.Value<int>("Code") != 0)
                {
                    //there was a problem with the request                        
                }
                else
                {
                    JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                    ParentKey = Data.Value<string>("Key");
                    if (!string.IsNullOrEmpty(ParentKey))
                    {
                        //Note read from CRM
                        objNoteModel.Parent = ParentKey;
                        if (!string.IsNullOrEmpty(ParentKey))
                        {

                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Total Balance: </b>" + objCustomerInfo.CurrentBalance + "</div>");
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Overdue Balance: </b>" + objCustomerInfo.SellingDetails.Credit.PastDue + "</div>");
                            if (strb != null)
                            {
                                objNoteModel.Text = "";
                                objNoteModel.Text = Convert.ToString(strb);
                            }
                            objNoteModel.DateTime = dtUtcNow;
                        }
                        //Note
                        res = CreateOrUpdateNote(objNoteModel);

                        if (!string.IsNullOrWhiteSpace(res))
                        {
                            if (res == "badrequest")
                            {
                                res = "badrequestfornote";
                            }
                        }
                    }
                    res = "success";
                }
            }
            return res;
        }
        public static string GetAbEntryProperties()
        {
            //build the request string by constructing a JSON object
            JObject createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                 new JObject(
                  new JProperty("Options",
                    new JObject(
                      new JProperty("Complex", false)
                    )
                  )
                )
              )
            );

            //instantiate the WCF service client
            using (EsconaTutorialServiceReference.DataClient client = new EsconaTutorialServiceReference.DataClient())
            {
                //call Get AbEntry Properties method of the WCF client and parse the result
                Stream response = client.AbEntryGetFieldInfo(new MemoryStream(Encoding.UTF8.GetBytes(createRequest.ToString())));
                JToken createResponse = JToken.Parse(new StreamReader(response).ReadToEnd());

                //check the return value of the response
                if (createResponse.Value<int>("Code") == 0)
                {
                    return Convert.ToString(createResponse["AbEntry"]["Data"]["properties"]);
                }
                return "";
            }
        }

        
    }
}




