using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.USServiceReference;
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
using System.Web.Script.Serialization;
using JSONResponseDeserialization;
using System.Globalization;

namespace EnquiryInsertToCRM.DataService
{
    public static class USCRMReference
    {
        public static string GetToken()
        {
            string token = string.Empty;
            //initialize a string variable to store the return value
            //construct the JSON request using Newtonsoft.Json.Linq.JObject
            JObject authRequest = new JObject(
                new JProperty("Database", ConfigurationManager.AppSettings["USMaximizerDatabase"]),
                new JProperty("UID", ConfigurationManager.AppSettings["USMaximizerUID"]),
                new JProperty("Password", ConfigurationManager.AppSettings["USMaximizerPassword"])
              );

            //instantiate the WCF service client ("MaximizerWebDataService" is the service namespace)
            using (USServiceReference.DataClient client = new USServiceReference.DataClient())
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
        public static UnionMemberModel ReadCompanyClientIdExisting(string clientId)
        {
            UnionMemberModel model = new UnionMemberModel();
            string createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                                new JProperty("Key", 1),
                                new JProperty("FirstName", 1),
                                new JProperty("LastName", 1),
                                new JProperty("Phone",
                                new JObject()
                                    {
                                        { "Default", 1 },
                                        { "Description", 1},
                                        { "Number", 1},
                                        { "Extension", 1}
                                    }
                                ),
                                new JProperty("Phone2",
                                new JObject()
                                    {
                                        { "Default", 1 },
                                        { "Description", 1},
                                        { "Number", 1},
                                        { "Extension", 1}
                                    }
                                ),
                                new JProperty("Phone3",
                                new JObject()
                                    {
                                        { "Default", 1 },
                                        { "Description", 1},
                                        { "Number", 1},
                                        { "Extension", 1}
                                    }
                                ),
                                new JProperty("Phone4",
                                new JObject()
                                    {
                                        { "Default", 1 },
                                        { "Description", 1},
                                        { "Number", 1},
                                        { "Extension", 1}
                                    }
                                ),
                                new JProperty("Email", 1),
                                new JProperty("Udf/$TYPEID(2)", 1), //Member ID
                                new JProperty("Udf/$TYPEID(3)", 1),//Member Type
                                new JProperty("Udf/$TYPEID(4)", 1),   //Union ID
                                new JProperty("Udf/$TYPEID(401)", 1),  //Active,
                                new JProperty("Udf/$TYPEID(656)", 1), //Direct Offers From Union Shopper
                                new JProperty("Udf/$TYPEID(14)", 1),//Subscribe to our Latest Offers Email
                                new JProperty("Udf/$TYPEID(25)", 1),//COMMENT
                                new JProperty("Udf/$TYPEID(667)", 1),//UsWebPwd
                                new JProperty("Address",
                                new JObject()
                                    {
                                        { "Key", 1 },
                                        { "Description", 1},
                                        { "AddressLine1", 1},
                                        { "AddressLine2", 1},
                                        { "City", 1},
                                        { "ZipCode", 1},
                                        { "Country", 1},
                                        { "StateProvince", 1},
                                        { "Default", true }
                                    }
                                )
                         )
                      )
                    )
                  ),
                  new JProperty("Criteria",
                    new JObject(
                        new JProperty("SearchQuery",
                            new JObject(
                                new JProperty("$AND", new JArray(
                                         new JObject(new JProperty("Key",
                                                            new JObject(
                                                                new JProperty("$EQ", new JObject(
                                                                new JProperty("ID", (clientId ?? "")),
                                                                new JProperty("Number", 0)
                                                                ))
                                                       )
                                                   ))
                                                   ,
                                        new JObject(
                                                new JProperty("$EQ", new JObject(
                                                            new JProperty("Type", "Individual")
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
            using (USServiceReference.DataClient client = new USServiceReference.DataClient())
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
                            model.key = Convert.ToString(arrayAppointment[0]["Key"] ?? null);
                            model.Addresskey = Convert.ToString(arrayAppointment[0]["Address"]["Key"] ?? null);
                            model.address = Convert.ToString(arrayAppointment[0]["Address"]["AddressLine1"] ?? "");
                            model.suburb = Convert.ToString(arrayAppointment[0]["Address"]["City"] ?? "");
                            model.ZipCode = Convert.ToString(arrayAppointment[0]["Address"]["ZipCode"] ?? "");
                            model.state = Convert.ToString(arrayAppointment[0]["Address"]["StateProvince"] ?? "");
                            model.country = Convert.ToString(arrayAppointment[0]["Address"]["Country"] ?? "");
                            model.firstname = Convert.ToString(arrayAppointment[0]["FirstName"] ?? "");
                            model.lastname = Convert.ToString(arrayAppointment[0]["LastName"] ?? "");
                            model.mobile = Convert.ToString(arrayAppointment[0]["Phone"]["Number"] ?? "");
                            if (!string.IsNullOrEmpty(model.mobile)) {
                                model.mobile = System.Text.RegularExpressions.Regex.Replace(model.mobile, @"\s+", "");
                            }
                            model.home = Convert.ToString(arrayAppointment[0]["Phone2"]["Number"] ?? "");
                            if (!string.IsNullOrEmpty(model.home))
                            {
                                model.home = System.Text.RegularExpressions.Regex.Replace(model.home, @"\s+", "");
                            }
                            model.work = Convert.ToString(arrayAppointment[0]["Phone3"]["Number"] ?? "");
                            if (!string.IsNullOrEmpty(model.work))
                            {
                                model.work = System.Text.RegularExpressions.Regex.Replace(model.work, @"\s+", "");
                            }
                            model.email = Convert.ToString(arrayAppointment[0]["Email"] ?? "");
                            model.UnionMemberID = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(2)"] ?? "");
                            model.comments = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(25)"] ?? "");
                            if (!string.IsNullOrEmpty(Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(4)"] ?? "")))
                            {
                                model.lstUnionID = fnGetTableItemIdWithIsSelectFlag("Udf/$TYPEID(4)", Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(4)"] ?? ""));
                                model.UnionID = (from c in model.lstUnionID
                                                 where c.Selected == true
                                                 select c.Key).FirstOrDefault();
                            }
                            else
                            {
                                model.lstUnionID = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(4)");
                            }
                            model.strDirectOffers = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(656)"] ?? "");
                            model.UsWebPwd = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(667)"] ?? "");
                            if (!string.IsNullOrEmpty(model.strDirectOffers))
                            {
                                model.lstDirectOffers = fnGetTableItemIdWithIsSelectFlag("Udf/$TYPEID(656)", model.strDirectOffers);
                            }
                            else
                            {
                                model.lstDirectOffers = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(656)");
                            }
                            List<string> lstitemNameReceiveGeneralOffers = fnGetTableSelectedItemName("Udf/$TYPEID(14)", Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(14)"] ?? ""));
                            if (lstitemNameReceiveGeneralOffers != null && lstitemNameReceiveGeneralOffers.Count > 0)
                            {
                                model.IsReceiveGeneralOffersEmail = (lstitemNameReceiveGeneralOffers[0] == "Yes" ? true : false);
                            }
                        }
                        else
                        {
                            model.IsInvalidClientId = true;
                        }
                    }
                }
            }
            return model;
        }

        //Function check for Company already exist based on ClientID.
        public static UnionMemberModel ReadCompanyEmailExisting(string Email)
        {
            UnionMemberModel model = new UnionMemberModel();
            string createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                                new JProperty("Key", 1),
                                new JProperty("FirstName", 1),
                                new JProperty("LastName", 1),
                                new JProperty("Phone",
                                new JObject()
                                    {
                                        { "Default", 1 },
                                        { "Description", 1},
                                        { "Number", 1},
                                        { "Extension", 1}
                                    }
                                ),
                                new JProperty("Phone2",
                                new JObject()
                                    {
                                        { "Default", 1 },
                                        { "Description", 1},
                                        { "Number", 1},
                                        { "Extension", 1}
                                    }
                                ),
                                new JProperty("Phone3",
                                new JObject()
                                    {
                                        { "Default", 1 },
                                        { "Description", 1},
                                        { "Number", 1},
                                        { "Extension", 1}
                                    }
                                ),
                                new JProperty("Phone4",
                                new JObject()
                                    {
                                        { "Default", 1 },
                                        { "Description", 1},
                                        { "Number", 1},
                                        { "Extension", 1}
                                    }
                                ),
                                new JProperty("Email", 1),
                                new JProperty("Udf/$TYPEID(2)", 1), //Member ID
                                new JProperty("Udf/$TYPEID(3)", 1),//Member Type
                                new JProperty("Udf/$TYPEID(4)", 1),   //Union ID
                                new JProperty("Udf/$TYPEID(401)", 1),  //Active,
                                new JProperty("Udf/$TYPEID(656)", 1), //Direct Offers From Union Shopper
                                new JProperty("Udf/$TYPEID(14)", 1),//Subscribe to our Latest Offers Email
                                new JProperty("Udf/$TYPEID(25)", 1),//COMMENT
                                new JProperty("Udf/$TYPEID(667)", 1),//UsWebPwd
                                new JProperty("Address",
                                new JObject()
                                    {
                                        { "Key", 1 },
                                        { "Description", 1},
                                        { "AddressLine1", 1},
                                        { "AddressLine2", 1},
                                        { "City", 1},
                                        { "ZipCode", 1},
                                        { "Country", 1},
                                        { "StateProvince", 1},
                                        { "Default", true }
                                    }
                                )
                         )
                      )
                    )
                  ),
                  new JProperty("Criteria",
                    new JObject(
                        new JProperty("SearchQuery",
                            new JObject(
                                new JProperty("$AND", new JArray(
                                       new JObject(
                                                new JProperty("$EQ", new JObject(
                                                            new JProperty("Email", Email)
                                                    ))
                                            )
                                                   ,
                                        new JObject(
                                                new JProperty("$EQ", new JObject(
                                                            new JProperty("Type", "Individual")
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
            using (USServiceReference.DataClient client = new USServiceReference.DataClient())
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
                            model.key = Convert.ToString(arrayAppointment[0]["Key"] ?? null);
                            model.Addresskey = Convert.ToString(arrayAppointment[0]["Address"]["Key"] ?? null);
                            model.address = Convert.ToString(arrayAppointment[0]["Address"]["AddressLine1"] ?? "");
                            model.suburb = Convert.ToString(arrayAppointment[0]["Address"]["City"] ?? "");
                            model.ZipCode = Convert.ToString(arrayAppointment[0]["Address"]["ZipCode"] ?? "");
                            model.state = Convert.ToString(arrayAppointment[0]["Address"]["StateProvince"] ?? "");
                            model.country = Convert.ToString(arrayAppointment[0]["Address"]["Country"] ?? "");
                            model.firstname = Convert.ToString(arrayAppointment[0]["FirstName"] ?? "");
                            model.lastname = Convert.ToString(arrayAppointment[0]["LastName"] ?? "");
                            model.mobile = Convert.ToString(arrayAppointment[0]["Phone"]["Number"] ?? "");
                            if (!string.IsNullOrEmpty(model.mobile))
                            {
                                model.mobile = System.Text.RegularExpressions.Regex.Replace(model.mobile, @"\s+", "");
                            }
                            model.home = Convert.ToString(arrayAppointment[0]["Phone2"]["Number"] ?? "");
                            if (!string.IsNullOrEmpty(model.home))
                            {
                                model.home = System.Text.RegularExpressions.Regex.Replace(model.home, @"\s+", "");
                            }
                            model.work = Convert.ToString(arrayAppointment[0]["Phone3"]["Number"] ?? "");
                            if (!string.IsNullOrEmpty(model.work))
                            {
                                model.work = System.Text.RegularExpressions.Regex.Replace(model.work, @"\s+", "");
                            }
                            model.email = Convert.ToString(arrayAppointment[0]["Email"] ?? "");
                            model.UnionMemberID = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(2)"] ?? "");
                            model.comments = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(25)"] ?? "");
                            if (!string.IsNullOrEmpty(Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(4)"] ?? "")))
                            {
                                model.lstUnionID = fnGetTableItemIdWithIsSelectFlag("Udf/$TYPEID(4)", Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(4)"] ?? ""));
                                model.UnionID = (from c in model.lstUnionID
                                                 where c.Selected == true
                                                 select c.Key).FirstOrDefault();
                            }
                            else
                            {
                                model.lstUnionID = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(4)");
                            }
                            model.strDirectOffers = Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(656)"] ?? "");
                            if (!string.IsNullOrEmpty(model.strDirectOffers))
                            {
                                model.lstDirectOffers = fnGetTableItemIdWithIsSelectFlag("Udf/$TYPEID(656)", model.strDirectOffers);
                            }
                            else
                            {
                                model.lstDirectOffers = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(656)");
                            }
                            List<string> lstitemNameReceiveGeneralOffers = fnGetTableSelectedItemName("Udf/$TYPEID(14)", Convert.ToString(arrayAppointment[0]["Udf/$TYPEID(14)"] ?? ""));
                            if (lstitemNameReceiveGeneralOffers != null && lstitemNameReceiveGeneralOffers.Count > 0)
                            {
                                model.IsReceiveGeneralOffersEmail = (lstitemNameReceiveGeneralOffers[0] == "Yes" ? true : false);
                            }
                        }
                        else
                        {
                            model.IsInvalidClientId = true;
                        }
                    }
                }
            }
            return model;
        }


        //Function create note for each address book successfull inserted.
        public static string CreateOrUpdateNote(NotesModel model)
        {
            StringBuilder sb = new StringBuilder();
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

            USServiceReference.DataClient client = new USServiceReference.DataClient();
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
                
                sb.Clear();
                sb.AppendLine(responseJSON.ToString());
                CommonMethod.LogFile(sb, false);
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
            using (USServiceReference.DataClient client = new USServiceReference.DataClient())
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

        public static List<AbEntryFieldInfo> GetFieldList()
        {
            List<AbEntryFieldInfo> lstAbEntryFieldInfo = new List<AbEntryFieldInfo>();
            string json = GetAbEntryProperties();
            var json_serializer = new JavaScriptSerializer();
            var field_list = (IDictionary<string, object>)json_serializer.DeserializeObject(json);
            List<string> lstUDFFieldsName = new List<string>();//UDFFieldsName.Split(',').ToList();
            if (field_list != null && field_list.Count > 0)
            {
                foreach (var item in field_list.Where(s => s.Key.Contains("Udf/$TYPEID")).ToList())
                {
                    AbEntryFieldInfo tempmodel = new AbEntryFieldInfo();
                    try
                    {
                        var temp = JsonConvert.DeserializeObject<IDictionary<string, AbEntryAlias>>("{" + JsonConvert.SerializeObject(item.Key) + ":" + JsonConvert.SerializeObject(item.Value) + "}", Converter1.Settings1);
                        foreach (var t in temp)
                        {
                            if (!string.IsNullOrEmpty(t.Key) && t.Value != null)
                            {
                                if (!string.IsNullOrEmpty(t.Value.Name))
                                {
                                    tempmodel.UniqueKey = t.Key;
                                    if (t.Value != null)
                                    {
                                        tempmodel.Name = t.Value.Name;
                                        if (t.Value.Type != null && t.Value.Type.Count > 0)
                                        {
                                            tempmodel.Type = t.Value.Type[1];
                                        }
                                    }
                                    lstAbEntryFieldInfo.Add(tempmodel);
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                        throw;
                    }
                }
                if (lstAbEntryFieldInfo != null && lstAbEntryFieldInfo.Count > 0)
                {
                    lstAbEntryFieldInfo = lstAbEntryFieldInfo.Where(s => lstUDFFieldsName.Contains(s.Name)).ToList();
                }
            }
            return lstAbEntryFieldInfo;
        }
        #region Get Prticular UDF item list return from CRM 
        //Pass UDF Type Id
        public static List<DropdownModel> GetAbEntryGetFieldOptions(string UdfTypeid, string filter = "")
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

            USServiceReference.DataClient client = new USServiceReference.DataClient();
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
        public static List<string> fnGetTableSelectedItemId(string UniqueKey, string Value)
        {
            List<string> lstItemId = new List<string>();
            List<DropdownModel> tempModelList = new List<DropdownModel>();
            tempModelList = GetAbEntryGetFieldOptions(UniqueKey);
            if (tempModelList != null && tempModelList.Count > 0)
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    List<string> arryItem = Value.Split(',').ToList();
                    if (arryItem != null && arryItem.Count > 0)
                    {
                        lstItemId = (from c in tempModelList
                                     where (arryItem.Contains(c.Value))
                                     select c.Key).ToList();
                    }
                }
            }
            return lstItemId;
        }
        public static List<DropdownModel> fnGetTableItemIdWithIsSelectFlag(string UniqueKey, string strValue)
        {
            List<string> lstItemId = new List<string>();
            List<DropdownModel> tempModelList = new List<DropdownModel>();
            tempModelList = GetAbEntryGetFieldOptions(UniqueKey);
            if (tempModelList != null && tempModelList.Count > 0)
            {
                if (!string.IsNullOrEmpty(strValue))
                {
                    List<string> arryItem = JArray.Parse(strValue).ToObject<List<string>>().ToList();
                    if (arryItem != null && arryItem.Count > 0)
                    {
                        lstItemId = (from c in tempModelList
                                     where (arryItem.Contains(c.Key))
                                     select c.Key).ToList();
                        if (lstItemId != null && lstItemId.Count > 0)
                        {
                            foreach (var item in tempModelList.Where(s => lstItemId.Contains(s.Key)).ToList())
                            {
                                item.Selected = true;
                            }
                        }
                    }

                }
            }
            return tempModelList;
        }
        public static List<string> fnGetTableSelectedItemName(string UniqueKey, string strValue)
        {
            List<string> lstItemName = new List<string>();
            List<DropdownModel> tempModelList = new List<DropdownModel>();
            tempModelList = GetAbEntryGetFieldOptions(UniqueKey);
            if (tempModelList != null && tempModelList.Count > 0)
            {
                if (!string.IsNullOrEmpty(strValue))
                {
                    List<string> arryItem = JArray.Parse(strValue).ToObject<List<string>>().ToList();
                    if (arryItem != null && arryItem.Count > 0)
                    {
                        lstItemName = (from c in tempModelList
                                       where (arryItem.Contains(c.Key))
                                       select c.Value).ToList();
                    }
                }
            }
            return lstItemName;
        }
        public static string fnGetListToStringReturn(List<string> lstItemList)
        {
            string res = "";
            if (lstItemList != null && lstItemList.Count > 0)
            {
                res = string.Join(",", lstItemList);
            }
            return res;
        }
        #endregion

        #region Create Company
        public static string CreateOrUpdateIndividual(UnionMemberModel objUnionMemberModel)
        {
            StringBuilder sb = new StringBuilder();
            string dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddT00:00:00Z");
            string rtnRes = "";
            StringBuilder divNoteText = new StringBuilder();
            StringBuilder divNoteForCommentsText = new StringBuilder();
            
            divNoteText.Clear();
            List<JProperty> lstJProperty = new List<JProperty>();
            try
            {
                if (!string.IsNullOrEmpty(objUnionMemberModel.key))
                {
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Record updated successfully:</b> " + objUnionMemberModel.firstname + " " + objUnionMemberModel.lastname + "</div>");
                }
                else
                {
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Record created successfully:</b> " + objUnionMemberModel.firstname + " " + objUnionMemberModel.lastname + "</div>");
                }

                if (!string.IsNullOrEmpty(objUnionMemberModel.firstname))
                {
                    lstJProperty.Add(new JProperty("FirstName", objUnionMemberModel.firstname));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>FirstName:</b> " + objUnionMemberModel.firstname + "</div>");
                }
                if (!string.IsNullOrEmpty(objUnionMemberModel.lastname))
                {
                    lstJProperty.Add(new JProperty("LastName", objUnionMemberModel.lastname));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>LastName:</b> " + objUnionMemberModel.lastname + "</div>");
                }
                if (!string.IsNullOrEmpty(objUnionMemberModel.email))
                {
                    lstJProperty.Add(new JProperty("Email", objUnionMemberModel.email));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>E-mail:</b> " + objUnionMemberModel.email + "</div>");
                }
                lstJProperty.Add(
                new JProperty("Address",
                       new JObject()
                           {
                                { "Key", objUnionMemberModel.Addresskey},
                                { "Description", "Main Address" },
                                { "AddressLine1", objUnionMemberModel.address},
                                { "City", objUnionMemberModel.suburb},
                                { "StateProvince", objUnionMemberModel.state},
                                { "ZipCode", objUnionMemberModel.ZipCode},
                                //{ "Country", objUnionMemberModel.country},                                
                                { "Default", true }
                           }
                       ));
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>AddressLine1:</b> " + objUnionMemberModel.address + "</div>");
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>City:</b> " + objUnionMemberModel.suburb + "</div>");
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>StateProvince:</b> " + objUnionMemberModel.state + "</div>");
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>ZipCode:</b> " + objUnionMemberModel.ZipCode + "</div>");
                //divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Country:</b> " + objUnionMemberModel.country + "</div>");            

                if (!string.IsNullOrEmpty(objUnionMemberModel.mobile))
                {
                    objUnionMemberModel.mobile = System.Text.RegularExpressions.Regex.Replace(objUnionMemberModel.mobile, @"\s+", "");
                    //lstJProperty.Add(new JProperty("Phone", objUnionMemberModel.mobile));
                    lstJProperty.Add(
                        new JProperty("Phone",
                                    new JObject()
                                        {
                                        { "Default", true },
                                        { "Description", "Mobile"},
                                        { "Number", objUnionMemberModel.mobile},
                                        { "Extension", ""}
                                        }
                                    )
                        );
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Phone:</b> " + objUnionMemberModel.mobile + "</div>");
                }
                if (!string.IsNullOrEmpty(objUnionMemberModel.home))
                {
                    objUnionMemberModel.home = System.Text.RegularExpressions.Regex.Replace(objUnionMemberModel.home, @"\s+", "");
                    //lstJProperty.Add(new JProperty("Phone2", objUnionMemberModel.home));
                    lstJProperty.Add(
                        new JProperty("Phone2",
                                    new JObject()
                                        {
                                        { "Default", false },
                                        { "Description", "Home"},
                                        { "Number", objUnionMemberModel.home},
                                        { "Extension", ""}
                                        }
                                    )
                        );
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Home:</b> " + objUnionMemberModel.home + "</div>");
                }
                if (!string.IsNullOrEmpty(objUnionMemberModel.work))
                {
                    objUnionMemberModel.work = System.Text.RegularExpressions.Regex.Replace(objUnionMemberModel.work, @"\s+", "");
                    //lstJProperty.Add(new JProperty("Phone3", objUnionMemberModel.work));
                    lstJProperty.Add(
                        new JProperty("Phone3",
                                    new JObject()
                                        {
                                        { "Default", false },
                                        { "Description", "Work"},
                                        { "Number", objUnionMemberModel.work},
                                        { "Extension", ""}
                                        }
                                    )
                        );
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Work:</b> " + objUnionMemberModel.work + "</div>");
                }
                if (!string.IsNullOrEmpty(objUnionMemberModel.UnionID))
                {
                    lstJProperty.Add(new JProperty("Udf/$TYPEID(4)", new JArray(objUnionMemberModel.UnionID)));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Union Name:</b> " + objUnionMemberModel.UnionName + "</div>");
                }
                if (!string.IsNullOrEmpty(objUnionMemberModel.UnionMemberID))
                {
                    lstJProperty.Add(new JProperty("Udf/$TYPEID(2)", objUnionMemberModel.UnionMemberID));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Union Member ID:</b> " + objUnionMemberModel.UnionMemberID + "</div>");
                }
                List<string> lstitemNameReceiveGeneralOffers = fnGetTableSelectedItemId("Udf/$TYPEID(14)", (objUnionMemberModel.IsReceiveGeneralOffersEmail ?? false) == true ? "Yes" : "No");
                lstJProperty.Add(new JProperty("Udf/$TYPEID(14)", new JArray(lstitemNameReceiveGeneralOffers[0])));
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Would you like to receive our General Offers Email?</b> " + ((objUnionMemberModel.IsReceiveGeneralOffersEmail ?? false) == true ? "Yes" : "No") + "</div>");

                if (!string.IsNullOrEmpty(objUnionMemberModel.DirectOffers))
                {
                    string[] strId = objUnionMemberModel.DirectOffers.Split(',');
                    lstJProperty.Add(new JProperty("Udf/$TYPEID(656)", new JArray(strId)));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>What Direct Offers would you like to see from Union Shopper?:</b> " + objUnionMemberModel.strDirectOffersName + "</div>");
                }
                if (!string.IsNullOrEmpty(objUnionMemberModel.comments))
                {
                    //lstJProperty.Add(new JProperty("Udf/$TYPEID(25)", objUnionMemberModel.comments));
                    divNoteForCommentsText.AppendLine("<div style='margin-bottom:5px;'><b>Note:</b> " + objUnionMemberModel.comments + "</div>");
                }

                lstJProperty.Add(new JProperty("Udf/$TYPEID(659)", dtUtcNow));
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Web Form Completed Date:</b> " + dtUtcNow + "</div>");

                string requestString = new JObject(
                    new JProperty("Token", GetToken()),
                    new JProperty("AbEntry", new JObject(
                        new JProperty("Data", new JObject(
                            new JProperty("Key", objUnionMemberModel.key),
                            new JProperty("Type", "Individual"),
                            lstJProperty
                        ))
                    ))
                ).ToString();
                System.IO.Stream response = null;
                USServiceReference.DataClient client = new USServiceReference.DataClient();
                if (!string.IsNullOrEmpty(objUnionMemberModel.key))
                {
                    response = client.AbEntryUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                }
                else
                {
                    response = client.AbEntryCreate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                }
                JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

                if (responseJSON.Value<int>("Code") != 0)
                {
                    sb.Clear();
                    sb.AppendLine(responseJSON.ToString());
                    CommonMethod.LogFile(sb, false);
                    rtnRes = "abentryError";
                    return rtnRes;
                }
                else
                {
                    rtnRes = "success";
                    NotesModel objNoteModel = new NotesModel();
                    string ParentKey = "";
                    JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                    ParentKey = Data.Value<string>("Key");
                    if (!string.IsNullOrEmpty(ParentKey))
                    {
                        //Note read from CRM
                        objNoteModel.Parent = ParentKey;
                        if (!string.IsNullOrEmpty(ParentKey))
                        {
                            if (divNoteText != null)
                            {
                                objNoteModel.Text = "";
                                objNoteModel.Text = Convert.ToString(divNoteText);
                            }
                            dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ssZ");
                            objNoteModel.DateTime = dtUtcNow;
                            rtnRes = CreateOrUpdateNote(objNoteModel);
                            if (divNoteForCommentsText != null)
                            {
                                objNoteModel.Text = "";
                                objNoteModel.Text = Convert.ToString(divNoteForCommentsText);
                                CreateOrUpdateNote(objNoteModel);
                            }
                        }
                        //Note

                        if (!string.IsNullOrWhiteSpace(rtnRes))
                        {
                            if (rtnRes == "badrequest")
                            {
                                rtnRes = "noteError";
                                sb.Clear();
                                sb.AppendLine("During company note create throw error!");
                                sb.AppendLine(responseJSON.ToString());
                                CommonMethod.LogFile(sb, false);
                            }
                            else if (rtnRes == "success")
                            {
                                if (!string.IsNullOrEmpty(objUnionMemberModel.key))
                                {
                                    rtnRes = "successupdated";
                                }
                            }
                        }
                    }                    
                }
            }
            catch (Exception ex)
            {
                sb.Clear();
                sb.AppendLine("CreateOrUpdateIndividual Exception: " + ex.Message);
                CommonMethod.LogFile(sb, false);
            }
            return rtnRes;
        }
        #endregion
    }
}




