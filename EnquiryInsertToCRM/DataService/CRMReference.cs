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
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace EnquiryInsertToCRM.DataService
{
    public static class CommonServices
    {
        public static string GetToken()
        {
            string token = string.Empty;
            //initialize a string variable to store the return value
            //construct the JSON request using Newtonsoft.Json.Linq.JObject
            JObject authRequest = new JObject(
                new JProperty("Database", ConfigurationManager.AppSettings["MaximizerDatabase"]),
                new JProperty("UID", ConfigurationManager.AppSettings["MaximizerUID"]),
                new JProperty("Password", ConfigurationManager.AppSettings["MaximizerPassword"])
              );

            //instantiate the WCF service client ("MaximizerWebDataService" is the service namespace)
            using (Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient())
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
        public static string CreateOrUpdateIndividual(EnquiryModel model)
        {
            string mode = "add";
            string res = string.Empty;
            string rtnRes = string.Empty;
            string ParentKey = string.Empty;
            model.Type = "Individual";
            List<string> jArraysaleperson = new List<string>();
            DateTime dt = CommonMethod.GetUserTimeZoneDateTime(DateTime.UtcNow.ToString());
            dt = dt.AddHours(-10);
            string dtUtcNow = dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            NotesModel objNoteModel = new NotesModel();
            EnquiryModel existingModel = new EnquiryModel();
            StringBuilder strb = new StringBuilder();


            objNoteModel.Text = "<div style='margin-bottom:5px;'><b>Record Inserted from web form</b></div>";
            
            objNoteModel.DateTime = dtUtcNow;
            existingModel = ReadExistingEmail(model);
            if (!string.IsNullOrEmpty(existingModel.key))
            {
                mode = "edit";
                model.key = existingModel.key;
                model.Addresskey = existingModel.Addresskey;
                objNoteModel.Text = "<div style='margin-bottom:5px;'><b>Record updated from web form</b></div>";
            }
            strb.AppendLine(objNoteModel.Text);
            strb.AppendLine("");
            if (!string.IsNullOrEmpty(model.SalesPerson))
            {
                var salepersonList = GetSalesPerson();
                if (salepersonList != null && salepersonList.Count > 0)
                {

                    jArraysaleperson = (from c in salepersonList
                                        where c.Value.ToLower().Trim() == model.SalesPerson.ToLower().Trim()
                                        select c.Key).ToList();

                }
            }
            string token = GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Data", new JObject(
                        new JProperty("Key", model.key),
                        new JProperty("Type", model.Type),
                        new JProperty("FirstName", model.FirstName),
                        new JProperty("MiddleName", (model.MiddleName ?? "")),
                        new JProperty("LastName", model.LastName),
                        new JProperty("Phone", model.Mobile),
                        new JProperty("Address",
                        new JObject()
                            {
                                { "Key", model.Addresskey },
                                { "Description", "Main Address" },
                                { "AddressLine1", model.AddressLine1 },
                                { "AddressLine2", model.AddressLine2 },
                                { "City", model.City },
                                { "Country", model.Country },
                                { "StateProvince", model.StateProvince },
                                { "ZipCode", model.ZipCode},
                                { "Default", true }
                            }
                        ),                        
                        new JProperty("Email", model.Email),
                        new JProperty("Udf/$TYPEID(217)", model.ReferenceBy),
                        new JProperty("Udf/$TYPEID(222)", model.PreferredInvestmentLevel),
                        new JProperty("Udf/$TYPEID(4)", model.BWSource),
                        new JProperty("Udf/$TYPEID(13)", model.InvestmentLevel),
                        new JProperty("Udf/$TYPEID(7)", new JArray(jArraysaleperson.ToArray()))
                    )
                  )
                )
              )
            ).ToString();

            Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
            System.IO.Stream response = null;
            if (mode == "add")
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
                return res = "badrequest";
            }
            else
            {
                JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                ParentKey = Data.Value<string>("Key");
                if (!string.IsNullOrEmpty(ParentKey))
                {
                    res = "noteinsertsuccess";
                    if (mode == "edit")
                    {
                        res = "noteupdatesuccess";
                    }
                    if (!string.IsNullOrEmpty(objNoteModel.Text))
                    {
                        //Note read from CRM
                        objNoteModel.Parent = ParentKey;
                        if (!string.IsNullOrEmpty(ParentKey))
                        {
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>First Names:</b> " + model.FirstName + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Last Names:</b> " + model.LastName + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Email:</b> " + model.Email + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Mobile No:</b> " + model.Mobile + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Street Address:</b> " + model.AddressLine1 + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Suburb:</b> " + model.City + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>State:</b> " + model.StateProvince + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Postcode:</b> " + model.ZipCode + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>How did you hear about Brookwater:</b> " + model.strReferenceBy + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Preferred Investment Level for H & L:</b> " + model.strPreferredInvestmentLevel + "</div>");                            
                            strb.AppendLine("<div style='margin-bottom:5px;'><b>Sales Person:</b> " + model.SalesPerson + "</div>");
                            if (strb != null)
                            {
                                objNoteModel.Text = "";
                                objNoteModel.Text = Convert.ToString(strb);
                            }
                            objNoteModel.DateTime = dtUtcNow;
                            
                        }
                        //Note

                        rtnRes = CreateOrUpdateNote(objNoteModel);
                    }
                    if (!string.IsNullOrWhiteSpace(rtnRes))
                    {
                        if (rtnRes == "badrequest")
                        {
                            res = "badrequestfornote";
                        }
                    }
                }
            }
            return res;
        }
        //Function return refrence dropdown fill
        public static List<DropdownModel> GetReferenceByList()
        {
            string token = GetToken();
            List<DropdownModel> referencemodelList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Options", new JObject(
                        new JProperty("Udf/$TYPEID(217)",
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

            Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
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
                var model2 = Data["Udf/$TYPEID(217)"];
                var Referencearray = Data["Udf/$TYPEID(217)"].Value<JArray>();
                if (Referencearray != null && Referencearray.Count > 0)
                {
                    referencemodelList = Referencearray.ToObject<List<DropdownModel>>();
                }
            }
            return referencemodelList;
        }
        public static List<DropdownModel> GetPreferredInvestmentLevelList()
        {
            string token = GetToken();
            List<DropdownModel> preferredinvestmentlevelList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Options", new JObject(
                        new JProperty("Udf/$TYPEID(222)",
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

            Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
            System.IO.Stream response = client.AbEntryGetFieldOptions(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                return preferredinvestmentlevelList;
            }
            else
            {
                JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                var model2 = Data["Udf/$TYPEID(222)"];
                var preferredinvestmentlevelarray = Data["Udf/$TYPEID(222)"].Value<JArray>();
                if (preferredinvestmentlevelarray != null && preferredinvestmentlevelarray.Count > 0)
                {
                    preferredinvestmentlevelList = preferredinvestmentlevelarray.ToObject<List<DropdownModel>>();
                }
            }
            return preferredinvestmentlevelList;
        }
        public static List<DropdownModel> GetBWSourceList()
        {
            string token = GetToken();
            List<DropdownModel> bwsourcemodelList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Options", new JObject(
                        new JProperty("Udf/$TYPEID(4)",
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

            Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
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
                var model2 = Data["Udf/$TYPEID(4)"];
                var Referencearray = Data["Udf/$TYPEID(4)"].Value<JArray>();
                if (Referencearray != null && Referencearray.Count > 0)
                {
                    bwsourcemodelList = Referencearray.ToObject<List<DropdownModel>>();
                }
            }
            return bwsourcemodelList;
        }
        public static List<DropdownModel> GetInvestmentLevelList()
        {
            string token = GetToken();
            List<DropdownModel> investmentlevelList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Options", new JObject(
                        new JProperty("Udf/$TYPEID(13)",
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

            Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
            System.IO.Stream response = client.AbEntryGetFieldOptions(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                return investmentlevelList;
            }
            else
            {
                JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                var model2 = Data["Udf/$TYPEID(13)"];
                var preferredinvestmentlevelarray = Data["Udf/$TYPEID(13)"].Value<JArray>();
                if (preferredinvestmentlevelarray != null && preferredinvestmentlevelarray.Count > 0)
                {
                    investmentlevelList = preferredinvestmentlevelarray.ToObject<List<DropdownModel>>();
                }
            }
            return investmentlevelList;
        }
        public static List<DropdownModel> GetSalesPerson()
        {
            string token = GetToken();
            List<DropdownModel> salespersonList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Options", new JObject(
                        new JProperty("Udf/$TYPEID(7)",
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

            Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
            System.IO.Stream response = client.AbEntryGetFieldOptions(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                return salespersonList;
            }
            else
            {
                JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                var model2 = Data["Udf/$TYPEID(7)"];
                var salespersonListarray = Data["Udf/$TYPEID(7)"].Value<JArray>();
                if (salespersonListarray != null && salespersonListarray.Count > 0)
                {
                    salespersonList = salespersonListarray.ToObject<List<DropdownModel>>();
                }
            }
            return salespersonList;
        }
        //Function check for individually already emailid exist.
        public static EnquiryModel ReadExistingEmail(EnquiryModel objAbEntryReadModel)
        {

            JObject createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                                new JProperty("Key", 1),
                                new JProperty("Type", objAbEntryReadModel.Type),
                                new JProperty("FirstName", 1),
                                new JProperty("MiddleName", 1),
                                new JProperty("LastName", 1),
                                new JProperty("Phone", 1),
                                new JProperty("Address",
                                new JObject()
                                    {
                                        { "Key", 1 },
                                        { "Description", 1 },
                                        { "AddressLine1", 1},
                                        { "AddressLine2", 1},
                                        { "City", 1 },
                                        { "Country", 1 },
                                        { "StateProvince", 1 },
                                        { "ZipCode", 1},
                                        { "Default", "true" }
                                    }
                                ),
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
                                            new JProperty("Email", objAbEntryReadModel.Email)
                                        )
                                     ))
                                    //,
                                    //   new JObject(
                                    //new JProperty("$EQ",
                                    //   new JObject(
                                    //       new JProperty("Type", objAbEntryReadModel.Type)
                                    //   )
                                    //))

                                    )
                               )
                            //    new JObject(
                            //new JProperty("$EQ",
                            //    new JObject(
                            //            new JProperty("Email", objAbEntryReadModel.Email)
                            //    )
                            //)
                            //)


                            )
                        )
                  )
                )
              )
            );
            //instantiate the WCF service client
            using (Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient())
            {

                Stream response = client.AbEntryRead(new MemoryStream(Encoding.UTF8.GetBytes(createRequest.ToString())));
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
                            objAbEntryReadModel.key = Convert.ToString(arrayAppointment[0]["Key"] ?? "");
                            //objAbEntryReadModel.FirstName = Convert.ToString(arrayAppointment[0]["FirstName"] ?? "");
                            //objAbEntryReadModel.MiddleName = Convert.ToString(arrayAppointment[0]["MiddleName"] ?? "");
                            //objAbEntryReadModel.LastName = Convert.ToString(arrayAppointment[0]["LastName"] ?? "");
                            //objAbEntryReadModel.Mobile = Convert.ToString(arrayAppointment[0]["Phone"] ?? "");
                            objAbEntryReadModel.Addresskey = Convert.ToString(arrayAppointment[0]["Address"]["Key"] ?? "");
                            //objAbEntryReadModel.AddressLine1 = Convert.ToString(arrayAppointment[0]["Address"]["AddressLine1"] ?? "");
                            //objAbEntryReadModel.AddressLine2 = Convert.ToString(arrayAppointment[0]["Address"]["AddressLine2"] ?? "");
                            //objAbEntryReadModel.City = Convert.ToString(arrayAppointment[0]["Address"]["City"] ?? "");
                            //objAbEntryReadModel.Country = Convert.ToString(arrayAppointment[0]["Address"]["Country"] ?? "");
                            //objAbEntryReadModel.StateProvince = Convert.ToString(arrayAppointment[0]["Address"]["StateProvince"] ?? "");
                            //objAbEntryReadModel.ZipCode = Convert.ToString(arrayAppointment[0]["Address"]["ZipCode"] ?? "");
                        }
                        if (!string.IsNullOrEmpty(objAbEntryReadModel.key))
                        {
                            return objAbEntryReadModel;
                        }
                    }
                }
            }
            return objAbEntryReadModel;
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

            Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
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
            using (Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient())
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
            using (Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient())
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

        //Create UDF Add Opotions
        //AbEntryFieldInfo
        //public static string CreateTableFieldOptions(AbEntryFieldInfo model)
        //{
        //    string res = "";
        //    string requestString = new JObject(
        //      new JProperty("Token", GetToken()),
        //      new JProperty("AbEntry", new JObject(
        //           new JProperty("Data", new JObject(
        //               new JProperty("Key", null)
        //               )),
        //              new JProperty("Options", new JObject(
        //                      new JProperty("Udf/$TYPEID(217)",
        //                      new JArray(new JObject()
        //                          {
        //                                { "Key", null},
        //                                { "Value", "Test" },
        //                                { "Inactive", false },
        //                                { "SortValue",""}
        //                          }
        //                      )
        //                )
        //          ))
        //      ))
        //    ).ToString();
        //    Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
        //    System.IO.Stream response = null;

        //    response = client.AbEntryCreate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));

        //    JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

        //    if (responseJSON.Value<int>("Code") != 0)
        //    {
        //        //there was a problem with the request
        //        return res = "badrequest";
        //    }
        //    else
        //    {
        //        JObject Data = (JObject)responseJSON["Note"]["Data"];
        //        res = Data.Value<String>("Key");
        //        //if (!string.IsNullOrEmpty(rtnRes))
        //        //{
        //        //    rtnRes = "success";
        //        //}
        //        return res;
        //    }
        //    return res;
        //}

    }
}


