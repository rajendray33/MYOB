using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.Maximizerwebdata;
using EnquiryInsertToCRM.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EnquiryInsertToCRM.DataService
{
    public static class CommonServices_CessnockAdmin
    {
        public static string GetToken()
        {
            string token = string.Empty;
            //initialize a string variable to store the return value
            //construct the JSON request using Newtonsoft.Json.Linq.JObject
            JObject authRequest = new JObject(
                new JProperty("Database", ConfigurationManager.AppSettings["CessnockAdminMaximizerDatabase"]),
                new JProperty("UID", ConfigurationManager.AppSettings["CessnockAdminMaximizerUID"]),
                new JProperty("Password", ConfigurationManager.AppSettings["CessnockAdminMaximizerPassword"])
              );

            //instantiate the WCF service client ("MaximizerWebDataService" is the service namespace)
            using (CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient())
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

        //Function check for Company already exist based on ClientID.
        public static AbEntryKeyModel ReadCompanyClientIdExisting(string clientId)
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
                                new JProperty("Type", "Company"),
                                new JProperty("Udf/$TYPEID(276)", 1),
                                new JProperty("CompanyName", 1),
                                new JProperty("Address",
                                new JObject()
                                    {
                                        { "Key", 1 },
                                        { "Description", "Main Address" },
                                        { "AddressLine1",  1}
                                    }
                                )
                         )
                      )
                    )
                  ),
                  new JProperty("Criteria",
                    new JObject(
                        new JProperty("SearchQuery", new JObject(
                            new JProperty("$AND", new JArray(
                                    new JObject(new JProperty("$EQ",
                                                            new JObject(
                                                                new JProperty("Key", new JObject(
                                                                new JProperty("ID", (clientId ?? ""))
                                                                ))
                                                       )
                                                   )
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
            using (CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient())
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
                            model.CompanyName = Convert.ToString(arrayAppointment[0]["CompanyName"] ?? "");
                            model.AddressKey = Convert.ToString(arrayAppointment[0]["Address"]["Key"] ?? "");
                            model.Jotform_Submission_IDs = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(276)"] ?? "");
                        }
                    }
                }
            }
            return model;
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
                                new JProperty("Type", 1),
                                new JProperty("CompanyName", 1),
                                new JProperty("Udf/$TYPEID(276)", 1),
                                new JProperty("Address",
                                new JObject()
                                    {
                                        { "Key", 1 },
                                        { "Description", "Main Address" },
                                        { "AddressLine1",  1}
                                    }
                                )
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
            using (CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient())
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
                            model.AddressKey = Convert.ToString(arrayAppointment[0]["Address"]["Key"] ?? "");
                            model.Jotform_Submission_IDs = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(276)"] ?? "");
                        }
                    }
                }
            }
            return model;
        }

        //Function create note for each address book successfull inserted.
        public static string CreateOrUpdateNote(NotesModel model)
        {
            string requestString = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("Note", new JObject(
                    new JProperty("Data", new JObject(
                        new JProperty("Key", model.Key),
                        new JProperty("Type", "0"),
                        new JProperty("ParentKey", model.Parent),
                        new JProperty("DateTime", model.DateTime),
                        new JProperty("RichText", model.Text)
                    ))
                ))
            ).ToString();

            CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient();
            System.IO.Stream response = null;
            if (string.IsNullOrEmpty(model.Key))
            {
                response = client.NoteCreate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            }
            else
            {
                response = client.NoteUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            }
            JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                return "badrequest";
            }
            else
            {
                JObject Data = (JObject)responseJSON["Note"]["Data"];
                return Data.Value<string>("Key");
            }
        }

        #region Get AbEntry Field Info
        public static string GetAbEntryProperties()
        {
            //build the request string by constructing a JSON object
            JObject createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                 new JObject(
                  new JProperty("Options",
                    new JObject(
                        new JProperty("Type", "Contact"),
                      new JProperty("Complex", false)
                    )
                  )
                )
              )
            );

            //instantiate the WCF service client
            using (CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient())
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
        #endregion

        //Function create Company AbEntry.
        public static string CreateOrUpdateAbEntry(List<AbEntryFieldInfo> modelList, string clientid, string submission_id, string Ip, string form_title, AbEntryAddressFieldInfo modelAbEntryAddressFieldInfo, List<AbEntryContactModel> AbEntryContactModelList)
        {
            StringBuilder strb = new StringBuilder();
            string res = "";
            string resCont = "";
            string mode = "add";
            bool mandetoryFieldRequired = false;
            string rtnRes = string.Empty;
            string ParentKey = string.Empty;
            DateTime dt = CommonMethod.GetUserTimeZoneDateTime(DateTime.UtcNow.ToString());
            dt = dt.AddHours(-10);
            string dtUtcNow = dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            try
            {
                if (modelList != null && modelList.Count > 0)
                {
                    string strCompanyName = modelList.Where(s => s.UniqueKey == "CompanyName").Select(s => s.Value).FirstOrDefault();
                    NotesModel objNoteModel = new NotesModel();
                    AbEntryKeyModel objKeyModel = new AbEntryKeyModel();
                    List<JProperty> lstJProperty = new List<JProperty>();
                    JArray jAccommodationFeatures = new JArray();

                    #region Find To ClientId Already Exist or not in CRM
                    objNoteModel.Text = "Record Inserted from web form";
                    objNoteModel.DateTime = dtUtcNow;
                    if (!string.IsNullOrEmpty(clientid))
                    {
                        objKeyModel = ReadCompanyClientIdExisting(clientid);

                        //This validation put when ClientID entered wrong then CRM return empty.
                        if (string.IsNullOrEmpty(objKeyModel.Key))
                        {
                            if (!string.IsNullOrEmpty(strCompanyName))
                            {
                                objKeyModel = ReadCompanyNameExisting(strCompanyName);
                            }
                            else {
                                mandetoryFieldRequired = true;
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(strCompanyName))
                        {
                            objKeyModel = ReadCompanyNameExisting(strCompanyName);
                            //if (!string.IsNullOrEmpty(objKeyModel.Key))
                            //{
                            //    return res = "skipSubmissionId";
                            //}
                        }
                    }
                    if (!string.IsNullOrEmpty(objKeyModel.Key))
                    {
                        //ReadAbEntryContact(objKeyModel.Key);
                        if (!string.IsNullOrEmpty(objKeyModel.Jotform_Submission_IDs))
                        {
                            List<string> strSubmissionId = new List<string>();
                            if (objKeyModel.Jotform_Submission_IDs.Contains(','))
                            {
                                strSubmissionId = objKeyModel.Jotform_Submission_IDs.Split(',').ToList();
                                if (strSubmissionId != null && strSubmissionId.Count > 0)
                                {
                                    var submissionId = (from c in strSubmissionId
                                                        where c == submission_id
                                                        select c).FirstOrDefault();
                                    if (submissionId != null)
                                    {
                                        return res = "skipSubmissionId";
                                    }
                                    else
                                    {
                                        mode = "edit";
                                        strSubmissionId.Add(submission_id);
                                    }
                                }
                            }
                            else
                            {
                                if (objKeyModel.Jotform_Submission_IDs == submission_id)
                                {
                                    return res = "skipSubmissionId";
                                }
                                else
                                {
                                    mode = "edit";
                                    strSubmissionId.Add(objKeyModel.Jotform_Submission_IDs);
                                    strSubmissionId.Add(submission_id);
                                }
                            }
                            if (strSubmissionId != null && strSubmissionId.Count > 0)
                            {
                                var obJotForm_Submission_Id = (from c in modelList
                                                               where c.Name.ToLower().Trim() == "Jotform_Submission_IDs".ToLower().Trim()
                                                               select c).FirstOrDefault();
                                if (obJotForm_Submission_Id != null)
                                {
                                    obJotForm_Submission_Id.Value = string.Join(",", strSubmissionId);
                                }
                            }
                        }
                        else
                        {
                            mode = "edit";
                            objNoteModel.Text = "Record updated from web form";
                        }
                    }
                    #endregion
                    strb.AppendLine("<div style='margin-bottom:5px;'><b>Form Title:</b> " + form_title + "</div>");
                    strb.AppendLine("");
                    foreach (var model in modelList)
                    {
                        if (model.UniqueKey == "CompanyName") {
                            if (string.IsNullOrEmpty(model.Value)) {
                                model.Value = objKeyModel.CompanyName;
                            }
                        }
                        if (model.Type == "array")
                        {
                            if (model.UniqueKey.Contains("Udf"))
                            {
                                if (!string.IsNullOrEmpty(model.Text) && !string.IsNullOrEmpty(model.Value))
                                {
                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>" + model.Text + "</b>: " + model.Value + "</div>");
                                    strb.AppendLine("");
                                }
                                List<DropdownModel> tempModelList = new List<DropdownModel>();
                                tempModelList = GetAbEntryGetFieldOptions(model.UniqueKey);
                                if (tempModelList != null && tempModelList.Count > 0)
                                {
                                    if (!string.IsNullOrEmpty(model.Value))
                                    {
                                        string[] arryItem = model.Value.Split(';');
                                        List<string> ayyId = new List<string>();
                                        if (arryItem != null && arryItem.Length > 0)
                                        {
                                            foreach (var ari in arryItem)
                                            {
                                                string strMatch = ari;
                                                if (!string.IsNullOrEmpty(strMatch))
                                                {
                                                    if (strMatch.Contains("("))
                                                    {
                                                        strMatch = strMatch.Split('(')[0].ToString();
                                                    }
                                                }
                                                string modelItemId = (from c in tempModelList
                                                                      where (c.Value.ToLower().Trim()).Contains(strMatch.ToLower().Trim())
                                                                      select c.Key).FirstOrDefault();
                                                if (!string.IsNullOrEmpty(modelItemId))
                                                {
                                                    ayyId.Add(modelItemId);
                                                }
                                            }
                                        }
                                        if (ayyId != null && ayyId.Count > 0)
                                        {
                                            lstJProperty.Add(new JProperty(model.UniqueKey, new JArray(ayyId.ToArray())));
                                        }
                                    }
                                }
                            }
                            else if (model.Name_Unique == "CompanyAddress1")
                            {

                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(model.Value))
                                {
                                    if (!string.IsNullOrEmpty(model.Text) && !string.IsNullOrEmpty(model.Value))
                                    {
                                        strb.AppendLine("<div style='margin-bottom:5px;'><b>" + model.Text + "</b>: " + model.Value + "</div>");
                                        strb.AppendLine("");
                                    }
                                    lstJProperty.Add(new JProperty(model.UniqueKey, new JArray(model.Value.Split(';'))));
                                }
                            }
                        }
                        else if (model.Type == "double")
                        {
                            Regex rgx = new Regex("^(-?)(0|([1-9][0-9]*))(\\.[0-9]+)?$");
                            if (!string.IsNullOrEmpty(model.Text) && !string.IsNullOrEmpty(model.Value))
                            {
                                bool result = rgx.IsMatch(model.Value);
                                if (result)
                                {
                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>" + model.Text + "</b>: " + model.Value + "</div>");
                                    strb.AppendLine("");
                                    lstJProperty.Add(new JProperty(model.UniqueKey, model.Value));
                                }
                            }

                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(model.Text) && !string.IsNullOrEmpty(model.Value))
                            {
                                if (model.Value.Length > 255)
                                {
                                    string strValue = model.Value.Substring(0, 255);
                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>" + model.Text + "</b>: " + strValue + "</div>");
                                    lstJProperty.Add(new JProperty(model.UniqueKey, strValue));
                                }
                                else
                                {
                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>" + model.Text + "</b>: " + model.Value + "</div>");
                                    lstJProperty.Add(new JProperty(model.UniqueKey, model.Value));
                                }
                                strb.AppendLine("");
                            }

                        }
                    }
                    if (modelAbEntryAddressFieldInfo.IsAddressAvailable)
                    {
                        if (!string.IsNullOrEmpty(modelAbEntryAddressFieldInfo.AddrLine1))
                        {
                            //Business Address
                            strb.AppendLine("<div style='margin-bottom:5px;margin-top:15px;'><b>Business Address </b></div>");
                            strb.AppendLine("<div style='margin-bottom:5px;'>Street Address: " + modelAbEntryAddressFieldInfo.AddrLine1 + "</div>");
                        }
                        if (!string.IsNullOrEmpty(modelAbEntryAddressFieldInfo.AddrLine2))
                        {
                            strb.AppendLine("<div style='margin-bottom:5px;'>Street Address Line 2: " + modelAbEntryAddressFieldInfo.AddrLine2 + "</div>");
                        }
                        if (!string.IsNullOrEmpty(modelAbEntryAddressFieldInfo.City))
                        {
                            strb.AppendLine("<div style='margin-bottom:5px;'>City: " + modelAbEntryAddressFieldInfo.City + "</div>");
                        }
                        if (!string.IsNullOrEmpty(modelAbEntryAddressFieldInfo.Country))
                        {
                            strb.AppendLine("<div style='margin-bottom:5px;'>Country: " + modelAbEntryAddressFieldInfo.Country + "</div>");
                        }
                        if (!string.IsNullOrEmpty(modelAbEntryAddressFieldInfo.State))
                        {
                            strb.AppendLine("<div style='margin-bottom:5px;'>StateProvince: " + modelAbEntryAddressFieldInfo.State + "</div>");
                        }
                        if (!string.IsNullOrEmpty(modelAbEntryAddressFieldInfo.Postal))
                        {
                            strb.AppendLine("<div style='margin-bottom:5px;'>Postal: " + modelAbEntryAddressFieldInfo.Postal + "</div>");
                        }
                        strb.AppendLine("");

                        lstJProperty.Add(new JProperty("Address",
                            new JObject()
                                    {
                                    { "Key", objKeyModel.AddressKey },
                                    { "Description", "Main Address" },
                                    { "AddressLine1",  modelAbEntryAddressFieldInfo.AddrLine1},
                                    { "AddressLine2", modelAbEntryAddressFieldInfo.AddrLine2},
                                    { "City", modelAbEntryAddressFieldInfo.City},
                                    { "Country", modelAbEntryAddressFieldInfo.Country },
                                    { "StateProvince", modelAbEntryAddressFieldInfo.State },
                                    { "ZipCode", modelAbEntryAddressFieldInfo.Postal},
                                    { "Default", "true" }
                                    }
                            ));
                    }
                    if (AbEntryContactModelList != null && AbEntryContactModelList.Count > 0)
                    {
                        int countcmd = 0;
                        foreach (var cmd in AbEntryContactModelList)
                        {
                            countcmd++;
                            strb.AppendLine("<div style='margin-bottom:5px;margin-top:15px;'><b>Contact Person " + countcmd + "</b></div>");
                            strb.AppendLine("");
                            strb.AppendLine("<div style='margin-bottom:5px;'>FirstName: " + cmd.FirstName + "</div>");
                            strb.AppendLine("");
                            strb.AppendLine("<div style='margin-bottom:5px;'>LastName: " + cmd.LastName + "</div>");
                            strb.AppendLine("");
                            strb.AppendLine("<div style='margin-bottom:5px;'>Phone: " + cmd.Phone + "</div>");
                            strb.AppendLine("");
                            strb.AppendLine("<div style='margin-bottom:5px;'>Email: " + cmd.Email + "</div>");
                            strb.AppendLine("");
                            strb.AppendLine("<div style='margin-bottom:5px;'>Position: " + cmd.Position + "</div>");
                            strb.AppendLine("");
                        }
                    }
                    string token = GetToken();
                    if (string.IsNullOrEmpty(token))
                    {
                        return res = "badrequest";
                    }
                    string requestString = new JObject(
                        new JProperty("Token", token),
                        new JProperty("AbEntry", new JObject(
                            new JProperty("Data", new JObject(
                                new JProperty("Key", objKeyModel.Key),
                                new JProperty("Type", "Company"),
                                lstJProperty
                            )
                          )
                        )
                      )
                    ).ToString();

                    CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient();
                    System.IO.Stream response = null;

                    if (mode == "add")
                    {
                        response = client.AbEntryCreate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                    }
                    else
                    {
                        response = client.AbEntryUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                    }
                    //JToken responseJSON = "";
                    JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());
                    if (responseJSON.ToString() != "")
                    {
                        if (responseJSON.Value<int>("Code") != 0)
                        {
                            JArray Data = (JArray)responseJSON["Msg"];
                            if (Data != null && Data.Count > 0)
                            {
                                if (mandetoryFieldRequired)
                                {
                                    res = "invalidschema-(Client ID or Company name required once need to pass) Or Client ID invalid.";
                                }
                                else
                                {
                                    res = "invalidschema-" + Data[0];
                                }
                            }
                            else
                            {
                                res = "badrequest";
                            }

                        }
                        else
                        {
                            JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                            ParentKey = Data.Value<string>("Key");
                            if (!string.IsNullOrEmpty(ParentKey))
                            {
                                //we need the key of the parent company in order to create a contact entry
                                resCont = CreateOrUpdateContact(AbEntryContactModelList, ParentKey);

                                if (resCont != "badrequestforcontact")
                                {
                                    objNoteModel.Parent = ParentKey;
                                    /// <summary>                                    
                                    /// <Currently ReadNote function note used bcz of client required to each time new note entry insert.>
                                    /// </summary>                              
                                    objNoteModel.DateTime = dtUtcNow;
                                    if (strb != null)
                                    {
                                        strb.AppendLine("<div style='margin-bottom:5px;'><b>Ip:</b> " + Ip + "</div>");
                                        objNoteModel.Text = strb.ToString();
                                    }
                                    if (!string.IsNullOrEmpty(objNoteModel.Text))
                                    {
                                        if (AbEntryContactModelList != null && AbEntryContactModelList.Count > 0)
                                        {
                                            objNoteModel.Text = objNoteModel.Text.Replace("[FIRSTNAME]", AbEntryContactModelList.Select(s => s.FirstName).FirstOrDefault());
                                            objNoteModel.Text = objNoteModel.Text.Replace("[LASTNAME]", AbEntryContactModelList.Select(s => s.LastName).FirstOrDefault());
                                        }
                                        //finally, we'll add a note to our company
                                        rtnRes = CreateOrUpdateNote(objNoteModel);

                                        if (rtnRes != "badrequest")
                                        {
                                            res = "noteinsertsuccess";
                                            if (mode == "edit")
                                            {
                                                res = "noteupdatesuccess";
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    res = "badrequestforcontact";
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                strb.Clear();
                strb.AppendLine("Error: " + ex.Message + "");
                CommonMethod.LogFile(strb, false);
            }
            return res;
        }

        #region Get Prticular UDF item list return from CRM 
        //Pass UDF Type Id
        public static List<DropdownModel> GetAbEntryGetFieldOptions(string UdfTypeid)
        {
            string token = GetToken();
            List<DropdownModel> itemmodelList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Options", new JObject(
                        new JProperty(UdfTypeid,
                        new JArray(new JObject()
                            {
                                { "Key", 1 },
                                { "Value", 1 }
                            }
                        )
                    )
                  ))
                ))
            ).ToString();

            CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient();
            System.IO.Stream response = client.AbEntryGetFieldOptions(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                return null;
            }
            else
            {
                JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                var model2 = Data[UdfTypeid];
                var getoptionsarray = Data[UdfTypeid].Value<JArray>();
                if (getoptionsarray != null && getoptionsarray.Count > 0)
                {
                    itemmodelList = getoptionsarray.ToObject<List<DropdownModel>>();
                }
            }
            return itemmodelList;
        }
        #endregion        
        public static string CreateOrUpdateContact(List<AbEntryContactModel> modelList, string key)
        {
            string rtnRes = string.Empty;
            AbEntryContactModel modelContactModel = new AbEntryContactModel();
            if (modelList != null && modelList.Count > 0)
            {
                foreach (var item in modelList)
                {
                    modelContactModel.key = null;
                    modelContactModel.FirstName = item.FirstName;
                    modelContactModel.LastName = item.LastName;
                    modelContactModel.Phone = item.Phone;
                    modelContactModel.Email = item.Email;
                    modelContactModel.Position = item.Position;
                    modelContactModel.Website = item.Website;

                    if (!string.IsNullOrEmpty(modelContactModel.FirstName) && !string.IsNullOrEmpty(modelContactModel.LastName))
                    {
                        if (!string.IsNullOrEmpty(key))
                        {
                            modelContactModel.Parentkey = key;
                        }
                        modelContactModel = ReadAbEntryContact(modelContactModel);
                    }
                    string mode = string.Empty;
                    string requestString = new JObject(
                        new JProperty("Token", GetToken()),
                        new JProperty("AbEntry", new JObject(
                            new JProperty("Data", new JObject(
                                new JProperty("Key", modelContactModel.key),
                                new JProperty("Type", "Contact"),
                                new JProperty("ParentKey", modelContactModel.Parentkey),
                                new JProperty("FirstName", modelContactModel.FirstName),
                                new JProperty("LastName", modelContactModel.LastName),
                                new JProperty("Phone", modelContactModel.Phone),
                                new JProperty("Email", modelContactModel.Email),
                                new JProperty("Position", modelContactModel.Position)
                            ))
                        ))
                    ).ToString();

                    CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient();
                    System.IO.Stream response = null;
                    if (string.IsNullOrEmpty(modelContactModel.key))
                    {
                        response = client.AbEntryCreate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                    }
                    else
                    {
                        response = client.AbEntryUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                    }
                    JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

                    if (responseJSON.Value<int>("Code") != 0)
                    {
                        //there was a problem with the request
                        rtnRes = "badrequestforcontact";
                    }
                    else
                    {
                        JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                        rtnRes = Data.Value<string>("Key");
                        if (!string.IsNullOrEmpty(rtnRes))
                        {
                            rtnRes = "contactinsertsuccess";
                            if (!string.IsNullOrEmpty(modelContactModel.key))
                            {
                                rtnRes = "contactupdatesuccess";
                            }
                        }

                        //return rtnRes;
                    }
                }
            }
            return rtnRes;
        }
        public static AbEntryContactModel ReadAbEntryContact(AbEntryContactModel model)
        {
            if (!string.IsNullOrEmpty(model.FirstName) && !string.IsNullOrEmpty(model.LastName))
            {
                string createRequest = new JObject(
                    new JProperty("Token", GetToken()),
                    new JProperty("AbEntry",
                    new JObject(
                      new JProperty("Scope",
                        new JObject(
                            new JProperty("Fields",
                                new JObject(
                                    new JProperty("Key", 1),
                                    new JProperty("ParentKey", 1),
                                    new JProperty("FirstName", 1),
                                    new JProperty("LastName", 1),
                                    new JProperty("Phone", 1),
                                    new JProperty("Email", 1)
                             )
                          )
                        )
                      ),
                      new JProperty("Criteria",
                        new JObject(
                            new JProperty("SearchQuery",
                            new JObject(
                            new JProperty("$AND",
                                            new JObject(
                                                new JProperty("$EQ",
                                                    new JObject(
                                                            new JProperty("FirstName", model.FirstName.Trim().ToLower())
                                                           )
                                                         )
                                                      ),
                                            new JObject(
                                                new JProperty("$EQ",
                                                    new JObject(
                                                            new JProperty("LastName", model.LastName.Trim().ToLower())
                                                           )
                                                         )
                                                      ),
                                            new JObject(
                                                new JProperty("$EQ",
                                                    new JObject(
                                                            new JProperty("Type", "Contact")
                                                           )
                                                         )
                                                      )
                                                )
                                            )
                                     )
                             )
                      )
                    )
                  )
                ).ToString();

                CessnockAdminWebData.DataClient client = new CessnockAdminWebData.DataClient();
                System.IO.Stream response = null;

                response = client.AbEntryRead(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(createRequest)));

                JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());
                if (responseJSON == null)
                {
                    return model;
                }
                if (responseJSON.Value<int>("Code") != 0)
                {
                    return model;
                }
                else
                {
                    JObject responseData = (JObject)responseJSON["AbEntry"];
                    if (responseData != null)
                    {
                        var responseDataArray = (JArray)responseData["Data"];
                        if (responseDataArray != null && responseDataArray.Count > 0)
                        {
                            model.key = Convert.ToString(responseDataArray[0]["Key"] ?? "");
                            model.Parentkey = Convert.ToString(responseDataArray[0]["ParentKey"] ?? "");
                        }
                    }
                    return model;
                }
            }
            else
            {
                return model;
            }
        }

    }
}


