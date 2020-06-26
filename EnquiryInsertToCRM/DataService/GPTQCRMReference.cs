using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.Maximizerwebdata;
using EnquiryInsertToCRM.Models;
using JSONResponseDeserialization;
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
using System.Web.Script.Serialization;

namespace EnquiryInsertToCRM.DataService
{
    public static class GPTQCRMReference
    {
        public static string GetToken()
        {
            string token = string.Empty;
            //initialize a string variable to store the return value
            //construct the JSON request using Newtonsoft.Json.Linq.JObject
            JObject authRequest = new JObject(
                new JProperty("Database", ConfigurationManager.AppSettings["GPTQMaximizerDatabase"]),
                new JProperty("UID", ConfigurationManager.AppSettings["GPTQMaximizerUID"]),
                new JProperty("Password", ConfigurationManager.AppSettings["GPTQMaximizerPassword"])
              );

            //instantiate the WCF service client ("MaximizerWebDataService" is the service namespace)
            using (GptqWebData.DataClient client = new GptqWebData.DataClient())
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


        //Function create note for each address book successfull inserted.
        public static string CreateOrUpdateNote(NotesModel model)
        {
            model.Key = null;
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

            GptqWebData.DataClient client = new GptqWebData.DataClient();
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
                      new JProperty("Complex", false)
                    )
                  )
                )
              )
            );

            //instantiate the WCF service client
            using (GptqWebData.DataClient client = new GptqWebData.DataClient())
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

        //Function create Individual AbEntry This Method Used When API access enabled.
        public static string CreateOrUpdateIndividual_Latest(List<AbEntryFieldInfo> lstWebHookResponseModel)
        {
            WebHookResponse.WebHookValidationForExistingModel enqmodel = new WebHookResponse.WebHookValidationForExistingModel();
            AbEntryFieldInfo objWebHookResponseModel = new AbEntryFieldInfo();
            JArray searchJArray = new JArray();
            NotesModel objNoteModel = new NotesModel();
            List<AbEntryFieldInfo> mainAbEntryFieldInfo = new List<AbEntryFieldInfo>();
            List<AbEntryFieldInfo> webhookresponsemodel = new List<AbEntryFieldInfo>();
            List<JProperty> lstJProperty = new List<JProperty>();
            JArray jAccommodationFeatures = new JArray();
            StringBuilder strb = new StringBuilder();

            string res = string.Empty;
            string mode = "add";
            string rtnRes = string.Empty;
            string ParentKey = string.Empty;
            string strSearchEmail = string.Empty;
            string strSearchPhone = string.Empty;

            string strEmailKey = string.Empty;
            string strEmailKey1 = string.Empty;
            string strEmailKey2 = string.Empty;
            string strEmailKey3 = string.Empty;
            string strEmailFieldName = string.Empty;
            string strEmailFieldName1 = string.Empty;
            string strEmailFieldName2 = string.Empty;
            string strEmailFieldName3 = string.Empty;
            string strEmailValue = string.Empty;
            string strEmailValue1 = string.Empty;
            string strEmailValue2 = string.Empty;
            string strEmailValue3 = string.Empty;

            string FirstInteraction = "Yes";
            enqmodel.Type = "Individual";
            string dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ssZ");
            var filename = "/images/WebHook_Response4.txt";
            //mainAbEntryFieldInfo = GetAbEntryFieldInfo();
            FirstInteraction = (lstWebHookResponseModel.Where(s => s.Name_Unique.ToLower().Contains("\\firstinteraction")).Select(s => s.Value).FirstOrDefault() == "False"?"No":"Yes") ;
            strSearchEmail = lstWebHookResponseModel.Where(s => s.Name_Unique.ToLower() == "emailaddress").Select(s => s.Value).FirstOrDefault();
            strSearchPhone = lstWebHookResponseModel.Where(s => s.Name_Unique.ToLower() == "phone").Select(s => s.Value).FirstOrDefault();
            if (!string.IsNullOrEmpty(strSearchEmail))
            {
                if (FirstInteraction == "No")
                {
                    //searchJArray.Add(new JObject(new JProperty("$EQ", new JObject(new JProperty("Type", enqmodel.Type)))));
                    searchJArray.Add(new JObject(new JProperty("$PHRASE", strSearchEmail)));
                    enqmodel = ReadCheckEmailOrPhoneExisting(enqmodel, searchJArray);
                    if (string.IsNullOrEmpty(enqmodel.Email))
                    {
                        if (!string.IsNullOrEmpty(strSearchPhone))
                        {
                            searchJArray.Clear();
                            //searchJArray.Add(new JObject(new JProperty("$EQ", new JObject(new JProperty("Type", enqmodel.Type)))));
                            searchJArray.Add(new JObject(new JProperty("$PHRASE", strSearchPhone)));
                            enqmodel = ReadCheckEmailOrPhoneExisting(enqmodel, searchJArray);
                            if (!string.IsNullOrEmpty(enqmodel.Key))
                            {
                                //First Interaction Set False When Phone number Exist
                                //FirstInteraction = "No";                            
                                if (!string.IsNullOrEmpty(enqmodel.Email) && string.IsNullOrEmpty(enqmodel.Email2))
                                {
                                    strEmailKey = "Email";
                                    strEmailFieldName = "Email Address";
                                    strEmailValue = enqmodel.Email;

                                    if (enqmodel.Email.ToLower().Trim() != strSearchEmail.ToLower().Trim())
                                    {
                                        objWebHookResponseModel.UniqueKey = "Email2";
                                        objWebHookResponseModel.Name = "E-mail Address 2";
                                        objWebHookResponseModel.Name_Unique = ("E-mail Address 2").Replace("_", "").Replace(" ", "").ToLower().Trim();
                                        objWebHookResponseModel.Value = enqmodel.Email;
                                        lstWebHookResponseModel.Add(objWebHookResponseModel);

                                        lstWebHookResponseModel.Remove(lstWebHookResponseModel.Where(s => s.Name_Unique == "emailaddress").FirstOrDefault());

                                        AbEntryFieldInfo objWebHookResponseModel1 = new AbEntryFieldInfo();
                                        objWebHookResponseModel1.UniqueKey = strEmailKey;
                                        objWebHookResponseModel1.Name = strEmailFieldName;
                                        objWebHookResponseModel1.Name_Unique = strEmailFieldName.Replace("_", "").Replace(" ", "").ToLower().Trim();
                                        objWebHookResponseModel1.Value = strSearchEmail;
                                        lstWebHookResponseModel.Add(objWebHookResponseModel1);
                                    }
                                }
                                else if (!string.IsNullOrEmpty(enqmodel.Email) && string.IsNullOrEmpty(enqmodel.Email3))
                                {
                                    strEmailKey = "Email";
                                    strEmailFieldName = "Email Address";
                                    strEmailValue = enqmodel.Email;
                                    if (enqmodel.Email3.ToLower().Trim() != strSearchEmail.ToLower().Trim())
                                    {
                                        objWebHookResponseModel.UniqueKey = "Email3";
                                        objWebHookResponseModel.Name = "E-mail Address 3";
                                        objWebHookResponseModel.Name_Unique = ("E-mail Address 3").Replace("_", "").Replace(" ", "").ToLower().Trim();
                                        objWebHookResponseModel.Value = enqmodel.Email;
                                        lstWebHookResponseModel.Add(objWebHookResponseModel);

                                        lstWebHookResponseModel.Remove(lstWebHookResponseModel.Where(s => s.Name_Unique == "emailaddress").FirstOrDefault());

                                        AbEntryFieldInfo objWebHookResponseModel1 = new AbEntryFieldInfo();
                                        objWebHookResponseModel1.UniqueKey = strEmailKey;
                                        objWebHookResponseModel1.Name = strEmailFieldName;
                                        objWebHookResponseModel1.Name_Unique = strEmailFieldName.Replace("_", "").Replace(" ", "").ToLower().Trim();
                                        objWebHookResponseModel1.Value = strSearchEmail;

                                        lstWebHookResponseModel.Add(objWebHookResponseModel1);
                                    }
                                }
                            }
                        }
                    }
                    else {
                        if (!string.IsNullOrEmpty(enqmodel.Email) && string.IsNullOrEmpty(enqmodel.Email2))
                        {
                            strEmailKey = "Email";
                            strEmailFieldName = "Email Address";
                            strEmailValue = enqmodel.Email;
                            if (enqmodel.Email.ToLower().Trim() != strSearchEmail.ToLower().Trim())
                            {
                                objWebHookResponseModel.UniqueKey = "Email2";
                                objWebHookResponseModel.Name = "E-mail Address 2";
                                objWebHookResponseModel.Name_Unique = ("E-mail Address 2").Replace("_", "").Replace(" ", "").ToLower().Trim();
                                objWebHookResponseModel.Value = enqmodel.Email;
                                lstWebHookResponseModel.Add(objWebHookResponseModel);
                                lstWebHookResponseModel.Remove(lstWebHookResponseModel.Where(s => s.Name_Unique == "emailaddress").FirstOrDefault());

                                AbEntryFieldInfo objWebHookResponseModel1 = new AbEntryFieldInfo();
                                objWebHookResponseModel1.UniqueKey = strEmailKey;
                                objWebHookResponseModel1.Name = strEmailFieldName;
                                objWebHookResponseModel1.Name_Unique = strEmailFieldName.Replace("_", "").Replace(" ", "").ToLower().Trim();
                                objWebHookResponseModel1.Value = strSearchEmail;
                                lstWebHookResponseModel.Add(objWebHookResponseModel1);
                            }
                        }
                        else if (!string.IsNullOrEmpty(enqmodel.Email) && string.IsNullOrEmpty(enqmodel.Email3))
                        {
                            strEmailKey = "Email";
                            strEmailFieldName = "Email Address";
                            strEmailValue = enqmodel.Email;
                            if (enqmodel.Email3.ToLower().Trim() != strSearchEmail.ToLower().Trim())
                            {
                                objWebHookResponseModel.UniqueKey = "Email3";
                                objWebHookResponseModel.Name = "E-mail Address 3";
                                objWebHookResponseModel.Name_Unique = ("E-mail Address 3").Replace("_", "").Replace(" ", "").ToLower().Trim();
                                objWebHookResponseModel.Value = enqmodel.Email;
                                lstWebHookResponseModel.Add(objWebHookResponseModel);

                                lstWebHookResponseModel.Remove(lstWebHookResponseModel.Where(s => s.Name_Unique == "emailaddress").FirstOrDefault());

                                AbEntryFieldInfo objWebHookResponseModel1 = new AbEntryFieldInfo();
                                objWebHookResponseModel1.UniqueKey = strEmailKey;
                                objWebHookResponseModel1.Name = strEmailFieldName;
                                objWebHookResponseModel1.Name_Unique = strEmailFieldName.Replace("_", "").Replace(" ", "").ToLower().Trim();
                                objWebHookResponseModel1.Value = strSearchEmail;

                                lstWebHookResponseModel.Add(objWebHookResponseModel1);
                            }
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(enqmodel.Key))
            {
                mode = "edit";
                objNoteModel.Text = "<div style='margin-bottom:5px;'><b>Record updated from web form</b></div>";
            }
            else
            {
                objNoteModel.Text = "<div style='margin-bottom:5px;'><b>Record Inserted from web form</b></div>";
                //FirstInteraction = "Yes";
            }
            using (StreamWriter sw = System.IO.File.AppendText(System.Web.HttpContext.Current.Server.MapPath(filename)))
            {
                sw.WriteLine("------------------------------AbEntry Process In : " + DateTime.Now + "------------------------------");
                sw.WriteLine("\n");
                sw.WriteLine("Before Count: " + lstWebHookResponseModel.Count + " \n");
                strb.AppendLine("<div style='margin-bottom:5px;'><b>Form Title: </b>Marketing CRM feedback form</div>");
                sw.WriteLine("\n");
                foreach (var model in lstWebHookResponseModel)
                {
                    if (model.Name_Unique.ToLower().Trim() == "firstinteraction")
                    {
                        model.Value = FirstInteraction;
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
                
                sw.WriteLine("\n");
                sw.Dispose();
            }
            using (StreamWriter sw = System.IO.File.AppendText(System.Web.HttpContext.Current.Server.MapPath(filename)))
            {
                foreach (var item1 in webhookresponsemodel.Where(s => (s.Value ?? "") != "").ToList())
                {
                    sw.WriteLine("{0} : {1} \n", item1.UniqueKey + "-" + item1.Name + "-" + item1.Type, item1.Value);
                }
                sw.WriteLine("\n");
                sw.Dispose();
            }
            strb.AppendLine(objNoteModel.Text);
            strb.AppendLine("");
            string token = GetToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                return null;
            }
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Data", new JObject(
                        new JProperty("Key", enqmodel.Key),
                        new JProperty("Type", "Individual"),
                        lstJProperty
                    )
                  )
                )
              )
            ).ToString();
            GptqWebData.DataClient client = new GptqWebData.DataClient();
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
                
        //Function check for individually already emailid exist.
        public static EnquiryModel ReadExistingEmail(EnquiryModel objAbEntryReadModel)
        {
            objAbEntryReadModel.Type = "Individual";
            JObject createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                                new JProperty("Key", 1),
                                new JProperty("Type", 1),
                                new JProperty("FirstName", 1),
                                new JProperty("MiddleName", 1),
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
                                            new JProperty("Email", objAbEntryReadModel.Email)
                                        )
                                     ))
                                    ,
                                       new JObject(
                                    new JProperty("$EQ",
                                       new JObject(
                                           new JProperty("Type", objAbEntryReadModel.Type)
                                       )
                                    ))
                                    )
                               )
                          )
                        )
                  )
                )
              )
            );
            //instantiate the WCF service client
            using (GptqWebData.DataClient client = new GptqWebData.DataClient())
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

            GptqWebData.DataClient client = new GptqWebData.DataClient();
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

        public static List<AbEntryFieldInfo> GetAbEntryFieldInfo()
        {
            List<AbEntryFieldInfo> lstAbEntryFieldInfo = new List<AbEntryFieldInfo>();
            string json = GetAbEntryProperties();
            var json_serializer = new JavaScriptSerializer();
            var field_list = (IDictionary<string, object>)json_serializer.DeserializeObject(json);
            if (field_list != null && field_list.Count > 0)
            {
                foreach (var item in field_list.Skip(2))
                {
                    AbEntryFieldInfo tempmodel = new AbEntryFieldInfo();
                    try
                    {
                        string strd3 = "{" + JsonConvert.SerializeObject(item.Key) + ":" + JsonConvert.SerializeObject(item.Value) + "}";
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
                                        tempmodel.Name_Unique = (t.Value.Name).Replace(" ", "");

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
            }
            return lstAbEntryFieldInfo;
        }

        public static WebHookResponse.WebHookValidationForExistingModel ReadCheckEmailOrPhoneExisting(WebHookResponse.WebHookValidationForExistingModel model, JArray searchJArray)
        {
            string createRequest = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry",
                new JObject(
                  new JProperty("Scope",
                    new JObject(
                        new JProperty("Fields",
                            new JObject(
                                new JProperty("Key", new JObject(
                                        new JProperty("Value", 1),
                                        new JProperty("ID", 1)
                                )),
                                new JProperty("Type", 1),
                                new JProperty("CompanyName", 1),
                                new JProperty("FirstName", 1),
                                new JProperty("LastName", 1),
                                new JProperty("Email", 1),
                                new JProperty("Email1", 1),
                                new JProperty("Email2", 1),
                                new JProperty("Email3", 1),
                                new JProperty("Phone", 1),
                                new JProperty("Phone1", 1),
                                new JProperty("Phone2", 1),
                                new JProperty("Phone3", 1),
                                new JProperty("Phone4", 1)
                         )
                      )
                    )
                  ),
                  new JProperty("Criteria",
                    new JObject(
                         new JProperty("SearchQuery", new JObject(
                            new JProperty("$AND",
                            searchJArray
                                   //new JArray(
                                   //        new JObject(
                                   //                    new JProperty("$PHRASE", "01444444444")                                                
                                   //                   ),
                                   //        new JObject(
                                   //                    new JProperty("$PHRASE", "test2307_1@gmail.com")
                                   //                   ),
                                   //        new JObject(
                                   //                new JProperty("$EQ", new JObject(
                                   //                            new JProperty("Type", "Individual")
                                   //                    ))
                                   //            )
                                   //         )
                                   )
                                )
                            )
                        )
                  )
                )
              )
            ).ToString();
            //instantiate the WCF service client
            using (GptqWebData.DataClient client = new GptqWebData.DataClient())
            {
                Stream response = client.AbEntryRead(new MemoryStream(Encoding.UTF8.GetBytes(createRequest)));
                JToken createResponse = JToken.Parse(new StreamReader(response).ReadToEnd());
                //check the return value of the response
                if (createResponse.Value<int>("Code") == 0)
                {
                    JObject readData = (JObject)createResponse["AbEntry"];
                    if (readData != null)
                    {
                        var arrayData = (JArray)readData["Data"];
                        if (arrayData != null && arrayData.Count > 0)
                        {
                            model.Type = Convert.ToString(arrayData[0]["Type"] ?? "");
                            model.FirstName = Convert.ToString(arrayData[0]["FirstName"] ?? "");
                            model.LastName = Convert.ToString(arrayData[0]["LastName"] ?? "");
                            model.Key = Convert.ToString(arrayData[0]["Key"]["Value"] ?? "");
                            model.ID = Convert.ToString(arrayData[0]["Key"]["ID"] ?? "");
                            model.Email = Convert.ToString(arrayData[0]["Email"] ?? "");
                            model.Email1 = Convert.ToString(arrayData[0]["Email1"] ?? "");
                            model.Email2 = Convert.ToString(arrayData[0]["Email2"] ?? "");
                            model.Email3 = Convert.ToString(arrayData[0]["Email3"] ?? "");
                            model.Phone = Convert.ToString(arrayData[0]["Phone"] ?? "");
                            model.Phone1 = Convert.ToString(arrayData[0]["Phone1"] ?? "");
                            model.Phone2 = Convert.ToString(arrayData[0]["Phone2"] ?? "");
                            model.Phone3 = Convert.ToString(arrayData[0]["Phone3"] ?? "");
                            model.Phone4 = Convert.ToString(arrayData[0]["Phone4"] ?? "");
                        }
                    }
                }
            }
            return model;
        }

    }
}


