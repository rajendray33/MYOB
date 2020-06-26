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
    public static class eNewsCRMReference
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
        public static string CreateOrUpdateIndividual(eNewsModel model)
        {
            string mode = "add";
            string res = string.Empty;
            string rtnRes = string.Empty;
            string ParentKey = string.Empty;
            model.Type = "Individual";
            List<string> jArray = new List<string>();
            List<JProperty> lstJProperty = new List<JProperty>();
            List<string> jArraysaleperson = new List<string>();
            DateTime dt = CommonMethod.GetUserTimeZoneDateTime(DateTime.UtcNow.ToString());
            dt = dt.AddHours(-10);
            string dtUtcNow = dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
            NotesModel objNoteModel = new NotesModel();
            eNewsModel existingModel = new eNewsModel();
            StringBuilder strb = new StringBuilder();
            if (model.formname == "enews")
            {
                if (!string.IsNullOrEmpty(model.gsgeneralenews))
                {
                    model.gsgeneralenews = ((model.gsgeneralenews.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.gsgeneralenews = "No";
                }
                if (!string.IsNullOrEmpty(model.gsnewsandviews))
                {
                    model.gsnewsandviews = ((model.gsnewsandviews.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.gsnewsandviews = "No";
                }
            }
            else if (model.formname == "a-learning-city")
            {
                if (!string.IsNullOrWhiteSpace(model.gsbrochurerequest))
                {
                    model.gsbrochurerequest = ((model.gsbrochurerequest.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.gsbrochurerequest = "No";
                }
                if (!string.IsNullOrWhiteSpace(model.gsupdaterequest))
                {
                    model.gsupdaterequest = ((model.gsupdaterequest.ToLower() ?? "") == "yes" ? "Yes" : "No");
                    //model.gsgeneralenews = model.gsupdaterequest;
                }
                else
                {
                    model.gsupdaterequest = "No";
                    //model.gsgeneralenews = model.gsupdaterequest;
                }
                if (!string.IsNullOrWhiteSpace(model.learningcoalitioncommittee))
                {
                    model.learningcoalitioncommittee = ((model.learningcoalitioncommittee.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.learningcoalitioncommittee = "No";
                }
                if (!string.IsNullOrWhiteSpace(model.gsopportunitiesrequest))
                {
                    model.gsopportunitiesrequest = ((model.gsopportunitiesrequest.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.gsopportunitiesrequest = "No";
                }
            }
            else if (model.formname == "retail-and-hospitality-leasing")
            {
                if (!string.IsNullOrWhiteSpace(model.gsbrochurerequest))
                {
                    model.gsbrochurerequest = ((model.gsbrochurerequest.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.gsbrochurerequest = "No";
                }
                if (!string.IsNullOrWhiteSpace(model.gsupdaterequest))
                {
                    model.gsupdaterequest = ((model.gsupdaterequest.ToLower() ?? "") == "yes" ? "Yes" : "No");
                    //model.gsgeneralenews = model.gsupdaterequest;
                }
                else
                {
                    model.gsupdaterequest = "No";
                    // model.gsgeneralenews = model.gsupdaterequest;
                }
            }
            else if (model.formname == "medical-specialist-office-suites-lease")
            {
                if (!string.IsNullOrWhiteSpace(model.gsreceivelatestnews))
                {
                    model.gsreceivelatestnews = ((model.gsreceivelatestnews.ToLower() ?? "") == "yes" ? "Yes" : "No");
                    //model.gsgeneralenews = model.gsreceivelatestnews;
                }
                else
                {
                    model.gsreceivelatestnews = "No";
                    //model.gsgeneralenews = model.gsreceivelatestnews;
                }
                if (!string.IsNullOrWhiteSpace(model.inspectionrequest))
                {
                    model.inspectionrequest = ((model.inspectionrequest.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.inspectionrequest = "No";
                }
                if (!string.IsNullOrWhiteSpace(model.informationpackrequest))
                {
                    model.informationpackrequest = ((model.informationpackrequest.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.informationpackrequest = "No";
                }
            }
            else if (model.formname == "springfield-central-office-sales-leasing")
            {
                if (!string.IsNullOrWhiteSpace(model.gsreceivelatestnews))
                {
                    model.gsreceivelatestnews = ((model.gsreceivelatestnews.ToLower() ?? "") == "yes" ? "Yes" : "No");
                    //model.gsgeneralenews = model.gsreceivelatestnews;
                }
                else
                {
                    model.gsreceivelatestnews = "No";
                    //model.gsgeneralenews = model.gsreceivelatestnews;
                }
            }
            else if (model.formname == "golf-and-country-club-enquiry")
            {
                if (!string.IsNullOrEmpty(model.wouldliketobecontactedbyphone))
                {
                    model.wouldliketobecontactedbyphone = ((model.wouldliketobecontactedbyphone.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.wouldliketobecontactedbyphone = "No";
                }
                if (!string.IsNullOrEmpty(model.golflessonenquiry))
                {
                    model.golflessonenquiry = ((model.golflessonenquiry.ToLower() ?? "") == "yes" ? "Yes" : "No");
                }
                else
                {
                    model.golflessonenquiry = "No";
                }
            }

            objNoteModel.Text = "<div style='margin-bottom:5px;'><b>Record Inserted from web form</b></div>";

            objNoteModel.DateTime = dtUtcNow;
            existingModel = ReadExistingEmail(model);
            if (!string.IsNullOrEmpty(existingModel.key))
            {
                mode = "edit";
                model.key = existingModel.key;
                model.Addresskey = existingModel.Addresskey;
                model.formId = existingModel.formId;
                objNoteModel.Text = "<div style='margin-bottom:5px;'><b>Record updated from web form</b></div>";
            }
            strb.AppendLine(objNoteModel.Text);
            strb.AppendLine("");
            if (!string.IsNullOrEmpty(model.formname))
            {
                var formnameList = GetCommonTableOptionList("Udf/$TYPEID(226)");
                if (formnameList != null && formnameList.Count > 0)
                {

                    var arrayList = (from c in formnameList
                                     where c.Value.ToLower().Trim() == model.formname.ToLower().Trim()
                                     select c.Key).ToList();
                    if (model.formId != null)
                    {
                        foreach (var item in model.formId)
                        {
                            arrayList.Add(Convert.ToString(item));
                        }
                    }
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(226)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.gsgeneralenews))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(53)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.gsgeneralenews.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(53)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.gsnewsandviews))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(131)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.gsnewsandviews.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(131)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.interestedin))
            {
                model.interestedin = model.interestedin.ToLower().Trim();

                List<string> Arryinterestedin = model.interestedin.Split(',').ToList();
                List<string> arryKeyId = new List<string>();

                if (Arryinterestedin != null && Arryinterestedin.Count > 0)
                {
                    var interestedinList = GetCommonTableOptionList("Udf/$TYPEID(225)");
                    if (interestedinList != null && interestedinList.Count > 0)
                    {
                        arryKeyId = (from c in interestedinList
                                     where Arryinterestedin.Contains(c.Value.ToLower().Trim())
                                     select c.Key).ToList();

                        if (arryKeyId != null && arryKeyId.Count > 0)
                        {
                            lstJProperty.Add(new JProperty("Udf/$TYPEID(225)", new JArray(arryKeyId.ToArray())));
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.interestedintype))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(229)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.interestedintype.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(229)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.interestinleaseorsale))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(230)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.interestinleaseorsale.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(230)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.howdidyouhearaboutus))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(217)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.howdidyouhearaboutus.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(217)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.inspectionrequest))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(231)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.inspectionrequest.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(231)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.informationpackrequest))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(232)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.informationpackrequest.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(232)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.gsbrochurerequest))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(233)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.gsbrochurerequest.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(233)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.title))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(237)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.title.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(237)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.gsupdaterequest))
            {
                //var responseList = GetCommonTableOptionList("Udf/$TYPEID(235)");
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(53)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.gsupdaterequest.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(53)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.gsopportunitiesrequest))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(236)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.gsopportunitiesrequest.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(236)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.learningcoalitioncommittee))
            {
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(234)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.learningcoalitioncommittee.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(234)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.gsreceivelatestnews))
            {
                //var responseList = GetCommonTableOptionList("Udf/$TYPEID(238)");
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(53)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.gsreceivelatestnews.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(53)", new JArray(arrayList.ToArray())));
                    }
                }
            }

            #region Campaign Monitor Golf Enquiry
            if (!string.IsNullOrEmpty(model.wouldliketobecontactedbyphone))
            {
                //var responseList = GetCommonTableOptionList("Udf/$TYPEID(238)");
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(238)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.wouldliketobecontactedbyphone.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(238)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.golflessonenquiry))
            {
                //var responseList = GetCommonTableOptionList("Udf/$TYPEID(238)");
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(239)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where c.Value.ToLower().Trim() == model.golflessonenquiry.ToLower().Trim()
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(239)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            if (!string.IsNullOrEmpty(model.telephone))
            {
                lstJProperty.Add(new JProperty("Phone", model.telephone));
            }
            if (!string.IsNullOrEmpty(model.phone))
            {
                lstJProperty.Add(new JProperty("Phone", model.phone));
            }
            if (!string.IsNullOrEmpty(model.fullname))
            {
                lstJProperty.Add(new JProperty("FullName", model.fullname));                                
                if (model.fullname.IndexOf(' ') > 0)
                {
                    var firstSpaceIndex = model.fullname.IndexOf(" ");
                    model.firstname = model.fullname.Substring(0, firstSpaceIndex); // INAGX4
                    model.lastname = model.fullname.Remove(0, model.fullname.IndexOf(' ') + 1);
                }
                else {
                    model.firstname = model.fullname;
                    model.lastname = "not provided";
                }
            }
            if (!string.IsNullOrEmpty(model.enquiry))
            {
                string strValue = model.enquiry;
                if (model.enquiry.Length > 255)
                {
                    strValue = model.enquiry.Substring(0, 255);
                }
                lstJProperty.Add(new JProperty("Udf/$TYPEID(241)", strValue));
            }
            if (!string.IsNullOrEmpty(model.enquirytopic))
            {
                
                var responseList = GetCommonTableOptionList("Udf/$TYPEID(240)");
                if (responseList != null && responseList.Count > 0)
                {
                    var arrayList = (from c in responseList
                                     where (c.Value.ToLower().Trim() == model.enquirytopic.ToLower().Trim())
                                     select c.Key).ToList();
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        lstJProperty.Add(new JProperty("Udf/$TYPEID(240)", new JArray(arrayList.ToArray())));
                    }
                }
            }
            #endregion
            if (!string.IsNullOrEmpty(model.comments))
            {
                lstJProperty.Add(new JProperty("Udf/$TYPEID(48)", model.comments));
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
                        new JProperty("FirstName", (model.firstname ?? null)),
                        new JProperty("LastName", (model.lastname ?? null)),
                        new JProperty("Email", (model.email ?? null)),
                        new JProperty("Address",
                        new JObject()
                            {
                                { "Key", model.Addresskey },
                                { "Description", "Main Address" },
                                { "AddressLine1", (model.address??null) },
                                { "City", (model.suburb??null) },
                                { "Country", (model.country??null) },
                                { "StateProvince", (model.state??null) },
                                { "ZipCode", (model.postcode??null)},
                                { "Default", true }
                            }
                        ),
                        //new JProperty("Udf/$TYPEID(48)", (model.comments ?? null)),
                        lstJProperty
                    )
                  )
                )
              )
            ).ToString();

            Maximizerwebdata.DataClient client = new Maximizerwebdata.DataClient();
            Stream response = null;
            if (mode == "add")
            {
                response = client.AbEntryCreate(new MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            }
            else
            {
                response = client.AbEntryUpdate(new MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            }

            JToken responseJSON = JToken.Parse(new StreamReader(response).ReadToEnd());

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
                            if (!string.IsNullOrEmpty(model.formname))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Form Name:</b> " + model.formname.Substring(0, 1).ToUpper() + model.formname.Substring(1, (model.formname.Length - 1)) + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.title))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Title:</b> " + model.title + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.gsgeneralenews))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>GS General eNews:</b> " + model.gsgeneralenews + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.gsnewsandviews))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>GS News and Views:</b> " + model.gsnewsandviews + "</div>");
                            }
                            if (model.formname != "golf-and-country-club-enquiry")
                            {
                                if (!string.IsNullOrEmpty(model.firstname))
                                {
                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>First Name:</b> " + model.firstname + "</div>");
                                }
                                if (!string.IsNullOrEmpty(model.lastname))
                                {
                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>Last Name:</b> " + model.lastname + "</div>");
                                }
                            }
                            if (!string.IsNullOrEmpty(model.fullname))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Full Name:</b> " + model.fullname + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.email))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Email:</b> " + model.email + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.phone))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Phone:</b> " + model.phone + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.telephone))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Telephone:</b> " + model.telephone + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.enquirytopic))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Enquiry Topic:</b> " + model.enquirytopic + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.address))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Address:</b> " + model.address + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.suburb))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Suburb:</b> " + model.suburb + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.state))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>State:</b> " + model.state + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.country))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Country:</b> " + model.country + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.postcode))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Postcode:</b> " + model.postcode + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.interestedintype))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>I am interested in:</b> " + model.interestedintype + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.interestinleaseorsale))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>I am interested in(Lease Or Sale):</b> " + model.interestinleaseorsale + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.howdidyouhearaboutus))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>How did you hear about us?:</b> " + model.howdidyouhearaboutus + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.comments))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Comments:</b> " + model.comments + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.enquiry))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Enquiry:</b> " + model.enquiry + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.interestedin))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Interested In:</b> " + model.interestedin + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.gsreceivelatestnews))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Yes, I would like to receive the latest news on Greater Springfield:</b> " + model.gsreceivelatestnews + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.inspectionrequest))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Please contact me regarding an inspection:</b> " + model.inspectionrequest + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.informationpackrequest))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Please send me Information Pack:</b> " + model.informationpackrequest + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.gsupdaterequest))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Yes, I'd like to receive the latest Greater Springfield updates:</b> " + model.gsupdaterequest + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.gsbrochurerequest))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Yes, I'd like to receive the Greater Springfield Learning City Brochure:</b> " + model.gsbrochurerequest + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.learningcoalitioncommittee))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Yes, I would like to make contact with the Learning Coalition Committee:</b> " + model.learningcoalitioncommittee + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.gsopportunitiesrequest))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Yes, I would like to discuss opportunities for my business/education facility:</b> " + model.gsopportunitiesrequest + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.wouldliketobecontactedbyphone))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>I would like to be contacted by phone:</b> " + model.wouldliketobecontactedbyphone + "</div>");
                            }
                            if (!string.IsNullOrEmpty(model.golflessonenquiry))
                            {
                                strb.AppendLine("<div style='margin-bottom:5px;'><b>Golf lesson Enquiry:</b> " + model.golflessonenquiry + "</div>");
                            }

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
        public static eNewsModel ReadExistingEmail(eNewsModel objAbEntryReadModel)
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
                                new JProperty("Type", 1),
                                new JProperty("Address",
                                new JObject()
                                    {
                                        { "Key", 1 }
                                    }
                                ),
                                new JProperty("Email", 1),
                                new JProperty("Udf/$TYPEID(226)", 1)
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
                                            new JProperty("Email", objAbEntryReadModel.email.ToLower().Trim())
                                        )
                                    )
                                 )
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
                    JObject responseData = (JObject)createResponse["AbEntry"];
                    if (responseData != null)
                    {
                        var arrayData = (JArray)responseData["Data"];
                        if (arrayData != null && arrayData.Count > 0)
                        {
                            objAbEntryReadModel.key = Convert.ToString(arrayData[0]["Key"] ?? "");
                            objAbEntryReadModel.Type = Convert.ToString(arrayData[0]["Type"] ?? "");
                            objAbEntryReadModel.Addresskey = Convert.ToString(arrayData[0]["Address"]["Key"] ?? "");
                            if (arrayData[0]["Udf/$TYPEID(226)"] != null)
                            {
                                objAbEntryReadModel.formId = arrayData[0]["Udf/$TYPEID(226)"].ToArray();
                            }
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
        public static List<DropdownModel> GetCommonTableOptionList(string Udfkey)
        {
            string token = GetToken();
            List<DropdownModel> formnameList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Options", new JObject(
                        new JProperty(Udfkey,
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
            Stream response = client.AbEntryGetFieldOptions(new MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            JToken responseJSON = JToken.Parse(new StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                return formnameList;
            }
            else
            {
                JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                var formnameListarray = Data[Udfkey].Value<JArray>();
                if (formnameListarray != null && formnameListarray.Count > 0)
                {
                    formnameList = formnameListarray.ToObject<List<DropdownModel>>();
                }
            }
            return formnameList;
        }
        public static bool IsValidFormRequest(string formname)
        {
            bool res = false;
            if (!string.IsNullOrEmpty(formname))
            {
                var formnameList = GetCommonTableOptionList("Udf/$TYPEID(226)");
                if (formnameList != null && formnameList.Count > 0)
                {
                    var arrayList = (from c in formnameList
                                     where c.Value.ToLower().Trim() == formname.ToLower().Trim()
                                     select c.Key).FirstOrDefault();
                    if (arrayList != null)
                    {
                        res = true;
                    }
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

    }
}


