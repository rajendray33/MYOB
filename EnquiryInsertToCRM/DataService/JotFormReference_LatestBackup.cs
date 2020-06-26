using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using EnquiryInsertToCRM.Models;
using JotForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JSONResponseDeserialization;
using System.IO;
//using Common;
using System.Net;

namespace EnquiryInsertToCRM.DataService
{
    public static class JotFormReference_LatestBackup
    {
        public static List<DropdownModel> GetFormList(string path)
        {            
            List<DropdownModel> formTitles = new List<DropdownModel>();
            var response = fnExecuteGetRequest(path);
            var forms = response["content"];

            formTitles = (from form in forms
                          where form.Value<string>("status") == "ENABLED"
                          select new DropdownModel()
                          {
                              Key = form.Value<string>("id"),
                              Value = form.Value<string>("title")
                          }).ToList();
            return formTitles;
        }
        public static List<AbEntryFieldInfoOrderBy> GetJotFormToAbEntryFieldList(string submissionpath, string APIKey, string CRMName, string formListPath)
        {
            List<AbEntryFieldInfo> mainAbEntryFieldInfo = new List<AbEntryFieldInfo>();
            List<AbEntryFieldInfoOrderBy> lstAbEntryFieldInfoOrderBy = new List<AbEntryFieldInfoOrderBy>();

            if (string.IsNullOrEmpty(submissionpath))
            {
                return lstAbEntryFieldInfoOrderBy;
            }
            string clientid = "";
            string Ip = "";
            string form_id = "";
            string submission_id = "";

            bool OutOfDateRange = false;

            var formsubmissons = fnExecuteGetRequest(submissionpath)["content"];
            //var formsArray = (from f in formsubmissons
            //                  select f).ToList();
            var jotFormList = GetFormList(formListPath);
            if (formsubmissons != null)
            {
                StringBuilder strb = new StringBuilder();
                strb.AppendLine("CRM Process Start");
                if (CRMName == "cessnock")
                {
                    mainAbEntryFieldInfo = GetCessnockAbEntryFieldInfo();
                }
                else if (CRMName == "gptq")
                {
                    mainAbEntryFieldInfo = GetGPTQAbEntryFieldInfo();
                }
                
                clientid = "";
                Ip = "";
                form_id = "";

                List<AbEntryFieldInfoTemp> lstAbEntryFieldInfoTemp = Mapper.Map<List<AbEntryFieldInfo>, List<AbEntryFieldInfoTemp>>(mainAbEntryFieldInfo);
                List<JotFormFieldInfo> modellist = new List<JotFormFieldInfo>();
                AbEntryFieldInfoOrderBy objAbEntryFieldInfoOrderBy = new AbEntryFieldInfoOrderBy();
                OutOfDateRange = false;


                var purpleQuickType1 = JsonConvert.DeserializeObject<JSONDeserialization>(formsubmissons.ToString(), Converter.Settings);
                if (purpleQuickType1 != null && purpleQuickType1.Answers.Count > 0)
                {
                    if (!string.IsNullOrEmpty(purpleQuickType1.Ip))
                    {
                        Ip = purpleQuickType1.Ip;
                    }

                    if (!string.IsNullOrEmpty(purpleQuickType1.FormId))
                    {
                        form_id = purpleQuickType1.FormId;
                    }
                    if (!string.IsNullOrEmpty(purpleQuickType1.Id))
                    {
                        submission_id = purpleQuickType1.Id;
                    }
                    List<AbEntryContactModel> AbEntryContactModelList = new List<AbEntryContactModel>();
                    AbEntryAddressFieldInfo modelAbEntryAddressFieldInfo = new AbEntryAddressFieldInfo();
                    foreach (var ans in purpleQuickType1.Answers)
                    {
                        JotFormFieldInfo model = new JotFormFieldInfo();
                        if (!string.IsNullOrEmpty(ans.Value.Type))
                        {
                            model.Type = ans.Value.Type;
                        }

                        if (!string.IsNullOrEmpty(ans.Value.Text))
                        {
                            model.Text = ans.Value.Text;
                        }
                        if (!string.IsNullOrEmpty(ans.Value.Name))
                        {
                            model.UniqueKey = ans.Value.Name;
                        }
                        if (ans.Value.Answer != null)
                        {
                            if ((ans.Value.Answer.Value.String ?? "") != "")
                            {
                                model.Value = ans.Value.Answer.Value.String;
                                modellist.Add(model);
                            }
                            else
                            {
                                if ((ans.Value.Type ?? "") == "control_address")
                                {
                                    if (!string.IsNullOrEmpty(ans.Value.Name))
                                    {
                                        if (ans.Value.Name == "CompanyAddress1")
                                        {

                                            model.Value = (ans.Value.PrettyFormat ?? "");
                                            if (ans.Value.Answer.Value.AnswerAnswerClass != null)
                                            {
                                                if (ans.Value.Answer.Value.AnswerAnswerClass.AddrLine1 != null)
                                                {
                                                    modelAbEntryAddressFieldInfo.AddrLine1 = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.AddrLine1);
                                                    modelAbEntryAddressFieldInfo.IsAddressAvailable = true;
                                                }
                                                if (ans.Value.Answer.Value.AnswerAnswerClass.AddrLine2 != null)
                                                {
                                                    modelAbEntryAddressFieldInfo.AddrLine2 = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.AddrLine2);
                                                    modelAbEntryAddressFieldInfo.IsAddressAvailable = true;
                                                }
                                                if (ans.Value.Answer.Value.AnswerAnswerClass.State != null)
                                                {
                                                    modelAbEntryAddressFieldInfo.State = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.State);
                                                    modelAbEntryAddressFieldInfo.IsAddressAvailable = true;
                                                }
                                                if (ans.Value.Answer.Value.AnswerAnswerClass.Country != null)
                                                {
                                                    modelAbEntryAddressFieldInfo.Country = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.Country);
                                                    modelAbEntryAddressFieldInfo.IsAddressAvailable = true;
                                                }
                                                if (ans.Value.Answer.Value.AnswerAnswerClass.Postal != null)
                                                {
                                                    modelAbEntryAddressFieldInfo.Postal = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.Postal);
                                                    modelAbEntryAddressFieldInfo.IsAddressAvailable = true;
                                                }
                                                if (ans.Value.Answer.Value.AnswerAnswerClass.City != null)
                                                {
                                                    modelAbEntryAddressFieldInfo.City = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.City);
                                                    modelAbEntryAddressFieldInfo.IsAddressAvailable = true;
                                                }
                                            }
                                            modellist.Add(model);
                                        }
                                    }
                                }
                                else
                                if ((ans.Value.Type ?? "") == "control_fullname")
                                {
                                    //if (ans.Value.Name == "ContactPerson1")
                                    //{
                                    model.Value = (ans.Value.PrettyFormat ?? "");
                                    if (ans.Value.Answer.Value.AnswerAnswerClass != null)
                                    {
                                        AbEntryContactModel fmodel = new AbEntryContactModel();
                                        if (ans.Value.Answer.Value.AnswerAnswerClass.First != null)
                                        {
                                            fmodel.FirstName = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.First);
                                        }
                                        if (ans.Value.Answer.Value.AnswerAnswerClass.Last != null)
                                        {
                                            fmodel.LastName = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.Last);
                                        }
                                        if (ans.Value.Answer.Value.AnswerAnswerClass.Middle != null)
                                        {
                                            fmodel.MiddleName = Convert.ToString(ans.Value.Answer.Value.AnswerAnswerClass.Middle);
                                        }
                                        if (ans.Value.Name == "ContactPerson1")
                                        {
                                            //var objCompanyAddress1 = purpleQuickType1.Answers.Where(s => (s.Value.Name == "CompanyAddress1" && (s.Value.Type ?? "") == "control_address")).Select(s => s.Value.Answer).FirstOrDefault();
                                            //if (objCompanyAddress1 != null)
                                            //{
                                            //    if (objCompanyAddress1.Value.AnswerAnswerClass != null)
                                            //    {
                                            //        if (objCompanyAddress1.Value.AnswerAnswerClass.AddrLine1 != null)
                                            //        {
                                            //            fmodel.AddrLine1 = Convert.ToString(objCompanyAddress1.Value.AnswerAnswerClass.AddrLine1);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress1.Value.AnswerAnswerClass.AddrLine2 != null)
                                            //        {
                                            //            fmodel.AddrLine2 = Convert.ToString(objCompanyAddress1.Value.AnswerAnswerClass.AddrLine2);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress1.Value.AnswerAnswerClass.State != null)
                                            //        {
                                            //            fmodel.State = Convert.ToString(objCompanyAddress1.Value.AnswerAnswerClass.State);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress1.Value.AnswerAnswerClass.Country != null)
                                            //        {
                                            //            fmodel.Country = Convert.ToString(objCompanyAddress1.Value.AnswerAnswerClass.Country);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress1.Value.AnswerAnswerClass.Postal != null)
                                            //        {
                                            //            fmodel.Postal = Convert.ToString(objCompanyAddress1.Value.AnswerAnswerClass.Postal);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress1.Value.AnswerAnswerClass.City != null)
                                            //        {
                                            //            fmodel.City = Convert.ToString(objCompanyAddress1.Value.AnswerAnswerClass.City);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //    }                                                    
                                            //}
                                            var objContactPosition1 = purpleQuickType1.Answers.Where(s => s.Value.Name == "ContactPosition1").Select(s => s.Value.Answer).FirstOrDefault();
                                            if (objContactPosition1 != null)
                                            {
                                                if (objContactPosition1.Value.String != null)
                                                {
                                                    fmodel.Position = objContactPosition1.Value.String;
                                                }
                                            }
                                            var objEmailContact1 = purpleQuickType1.Answers.Where(s => s.Value.Name == "EmailContact1").Select(s => s.Value.Answer).FirstOrDefault();
                                            if (objEmailContact1 != null)
                                            {
                                                if (objEmailContact1.Value.String != null)
                                                {
                                                    fmodel.Email = objEmailContact1.Value.String;
                                                }
                                            }
                                            var objCompanyPhone1 = purpleQuickType1.Answers.Where(s => s.Value.Name == "CompanyPhone1").Select(s => s.Value.Answer).FirstOrDefault();
                                            if (objCompanyPhone1 != null)
                                            {
                                                if (objCompanyPhone1.Value.String != null)
                                                {
                                                    fmodel.Phone = objCompanyPhone1.Value.String;
                                                }
                                            }
                                        }
                                        else if (ans.Value.Name == "ContactPerson2")
                                        {
                                            //var objCompanyAddress2 = purpleQuickType1.Answers.Where(s => (s.Value.Name == "CompanyAddress2" && (s.Value.Type ?? "") == "control_address")).Select(s => s.Value.Answer).FirstOrDefault();
                                            //if (objCompanyAddress2 != null)
                                            //{
                                            //    if (objCompanyAddress2.Value.AnswerAnswerClass != null)
                                            //    {
                                            //        if (objCompanyAddress2.Value.AnswerAnswerClass.AddrLine1 != null)
                                            //        {
                                            //            fmodel.AddrLine1 = Convert.ToString(objCompanyAddress2.Value.AnswerAnswerClass.AddrLine1);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress2.Value.AnswerAnswerClass.AddrLine2 != null)
                                            //        {
                                            //            fmodel.AddrLine2 = Convert.ToString(objCompanyAddress2.Value.AnswerAnswerClass.AddrLine2);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress2.Value.AnswerAnswerClass.State != null)
                                            //        {
                                            //            fmodel.State = Convert.ToString(objCompanyAddress2.Value.AnswerAnswerClass.State);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress2.Value.AnswerAnswerClass.Country != null)
                                            //        {
                                            //            fmodel.Country = Convert.ToString(objCompanyAddress2.Value.AnswerAnswerClass.Country);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress2.Value.AnswerAnswerClass.Postal != null)
                                            //        {
                                            //            fmodel.Postal = Convert.ToString(objCompanyAddress2.Value.AnswerAnswerClass.Postal);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //        if (objCompanyAddress2.Value.AnswerAnswerClass.City != null)
                                            //        {
                                            //            fmodel.City = Convert.ToString(objCompanyAddress2.Value.AnswerAnswerClass.City);
                                            //            fmodel.IsAddressAvailable = true;
                                            //        }
                                            //    }
                                            //}

                                            var objContactPosition2 = purpleQuickType1.Answers.Where(s => s.Value.Name == "ContactPosition2").Select(s => s.Value.Answer).FirstOrDefault();
                                            if (objContactPosition2 != null)
                                            {
                                                if (objContactPosition2.Value.String != null)
                                                {
                                                    fmodel.Position = objContactPosition2.Value.String;
                                                }
                                            }
                                            var objEmailContact2 = purpleQuickType1.Answers.Where(s => s.Value.Name == "EmailContact2").Select(s => s.Value.Answer).FirstOrDefault();
                                            if (objEmailContact2 != null)
                                            {
                                                if (objEmailContact2.Value.String != null)
                                                {
                                                    fmodel.Email = objEmailContact2.Value.String;
                                                }
                                            }
                                            var objCompanyPhone2 = purpleQuickType1.Answers.Where(s => s.Value.Name == "CompanyPhone2").Select(s => s.Value.Answer).FirstOrDefault();
                                            if (objCompanyPhone2 != null)
                                            {
                                                if (objCompanyPhone2.Value.String != null)
                                                {
                                                    fmodel.Phone = objCompanyPhone2.Value.String;
                                                }
                                            }
                                        }
                                        objAbEntryFieldInfoOrderBy.lstAbEntryContactModelList.Add(fmodel);
                                    }
                                    //}
                                }
                                else
                                {
                                    model.Value = (ans.Value.PrettyFormat ?? "");
                                    modellist.Add(model);
                                }
                            }

                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(ans.Value.PrettyFormat))
                            {
                                model.Value = ans.Value.PrettyFormat;
                            }
                            modellist.Add(model);
                        }
                    }
                    if (modellist != null && modellist.Count > 0)
                    {
                        if (CRMName == "cessnock")
                        {
                            //Submission Id Add in Model List
                            JotFormFieldInfo objmodel = new JotFormFieldInfo();
                            objmodel.Name = "Jotform_Submission_IDs";
                            objmodel.Text = "Jotform_Submission_IDs";
                            objmodel.Type = "control_textbox";
                            objmodel.UniqueKey = "Jotform_Submission_IDs";
                            objmodel.Value = submission_id;
                            modellist.Add(objmodel);
                        }
                        var modelJotForm = (from jf in modellist
                                            where ((jf.Value ?? "") != "" && (jf.UniqueKey ?? "") != "")
                                            select jf).ToList();

                        List<AbEntryFieldInfo> AbEntryFieldInfo1 = new List<AbEntryFieldInfo>();
                        foreach (var item in modelJotForm)
                        {
                            if (CRMName == "gptq")
                            {
                                if (item.UniqueKey.ToLower().Trim() == "touchpoint")
                                {
                                    item.UniqueKey = item.Value.Replace("_", "").Replace(" ", "").ToLower();
                                    item.Value = "Yes";
                                }
                            }
                            var tempModel = (from ab in lstAbEntryFieldInfoTemp
                                             where ("\\" + ab.Name_Unique.ToLower() + "\\").Contains("\\" + (item.UniqueKey.ToLower() ?? "") + "\\")
                                             select ab).FirstOrDefault();
                            if (tempModel != null)
                            {

                                if (!string.IsNullOrEmpty(item.Value ?? ""))
                                {
                                    tempModel.Value = (item.Value ?? "");
                                }
                                if (!string.IsNullOrEmpty(item.Text))
                                {
                                    if (item.Text.ToLower().Trim() == "business website".Trim())
                                    {
                                        if (CRMName == "cessnock")
                                        {
                                            tempModel.Text = item.UniqueKey;
                                            tempModel.UniqueKey = item.UniqueKey;
                                            tempModel.Name_Unique = item.UniqueKey;
                                            tempModel.Name = item.UniqueKey;
                                            tempModel.Type = "string";
                                        }
                                    }
                                    else
                                    {
                                        tempModel.Text = (item.Text ?? "");
                                    }
                                }
                                if (!string.IsNullOrEmpty(tempModel.UniqueKey))
                                {
                                    AbEntryFieldInfo1.Add(Mapper.Map<AbEntryFieldInfoTemp, AbEntryFieldInfo>(tempModel));
                                }
                            }
                        }

                        if (AbEntryFieldInfo1 != null && AbEntryFieldInfo1.Count > 0)
                        {
                            var clientIdModel = (from jf in modellist
                                                 where (jf.UniqueKey ?? "") == "ClientID"
                                                 select jf).FirstOrDefault();
                            if (clientIdModel != null)
                            {
                                if (!string.IsNullOrEmpty(clientIdModel.Value ?? ""))
                                {
                                    objAbEntryFieldInfoOrderBy.clientid = (clientIdModel.Value ?? "");
                                }
                            }
                           
                            if (!string.IsNullOrEmpty(form_id))
                            {
                                if (jotFormList != null && jotFormList.Count > 0)
                                {
                                    objAbEntryFieldInfoOrderBy.form_title = jotFormList.Where(s => s.Key == form_id).Select(s => s.Value).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(objAbEntryFieldInfoOrderBy.form_title))
                                    {
                                        objAbEntryFieldInfoOrderBy.form_title = objAbEntryFieldInfoOrderBy.form_title + " <b>[FIRSTNAME] [LASTNAME]</b>";
                                    }
                                }
                            }
                            objAbEntryFieldInfoOrderBy.created_at = Convert.ToDateTime(purpleQuickType1.created_at);
                            objAbEntryFieldInfoOrderBy.displayCreated_at = Convert.ToString(purpleQuickType1.created_at);
                            objAbEntryFieldInfoOrderBy.OutOfDateRange = OutOfDateRange;
                            objAbEntryFieldInfoOrderBy.Ip = Ip;
                            objAbEntryFieldInfoOrderBy.submission_id = submission_id;
                            objAbEntryFieldInfoOrderBy.lstAbEntryFieldInfo = AbEntryFieldInfo1;
                            objAbEntryFieldInfoOrderBy.lstAbEntryAddressFieldInfo = modelAbEntryAddressFieldInfo;

                            lstAbEntryFieldInfoOrderBy.Add(objAbEntryFieldInfoOrderBy);
                        }
                    }
                }
            }
            return lstAbEntryFieldInfoOrderBy;
        }
        public static CoreResponseModel ProcessForCessnockCRM(List<AbEntryFieldInfoOrderBy> lstAbEntryFieldInfoOrderBy)
        {
            CoreResponseModel responseModel = new CoreResponseModel();
            int count = 0;

            string strRes = "";
            int countofGetSubmission = 0;
            responseModel.countofGetSubmission = 0;
            int countofNewEntry = 0;
            List<string> ListofInvalidSchema = new List<string>();
            int countofInvalidSchema = 0;
            int countofUpdateEntry = 0;
            int countofInvalidForm = 0;
            int countofSkipSubmissionIdForm = 0;
            int countofbadRequest = 0;
            int countofbadrequestfornote = 0;

            if (lstAbEntryFieldInfoOrderBy != null && lstAbEntryFieldInfoOrderBy.Count > 0)
            {
                foreach (var item in lstAbEntryFieldInfoOrderBy)
                {
                    strRes = CommonServices_CessnockAdmin.CreateOrUpdateAbEntry(item.lstAbEntryFieldInfo, item.clientid, item.submission_id, item.Ip, item.form_title, item.lstAbEntryAddressFieldInfo, item.lstAbEntryContactModelList);
                    count += 1;
                    countofGetSubmission += 1;

                    if (strRes == "companynamerquired")
                    {
                        countofInvalidForm += 1;
                    }
                    else if (strRes == "skipSubmissionId")
                    {
                        countofSkipSubmissionIdForm += 1;
                    }
                    else if (!string.IsNullOrEmpty(strRes) && strRes.Contains("invalidschema-"))
                    {
                        countofInvalidSchema += 1;
                        ListofInvalidSchema.Add(strRes.Replace("invalidschema-", "Submission Date: " + item.displayCreated_at + ", Error: "));
                    }
                    else if (strRes == "badrequest")
                    {
                        countofbadRequest += 1;
                    }
                    else if (strRes == "noteinsertsuccess")
                    {
                        countofNewEntry += 1;
                    }
                    else if (strRes == "noteupdatesuccess")
                    {
                        countofUpdateEntry += 1;
                    }
                    else if (strRes == "badrequestfornote")
                    {
                        countofbadrequestfornote += 1;
                    }
                    else if (strRes == "contactinsertsuccess")
                    {
                        countofNewEntry += 1;
                    }
                    else if (strRes == "contactupdatesuccess")
                    {
                        countofUpdateEntry += 1;
                    }
                    else if (strRes == "badrequestforcontact")
                    {
                        countofbadrequestfornote += 1;
                    }
                    if (countofInvalidForm > 0)
                    {
                        responseModel.countofInvalidForm = countofInvalidForm;
                    }
                    if (countofbadRequest > 0)
                    {
                        responseModel.countofbadRequest = countofbadRequest;
                    }
                    if (countofNewEntry > 0)
                    {
                        responseModel.countofNewEntry = countofNewEntry;
                    }
                    if (countofUpdateEntry > 0)
                    {
                        responseModel.countofUpdateEntry = countofUpdateEntry;
                    }
                    if (countofSkipSubmissionIdForm > 0)
                    {
                        responseModel.countofSkipSubmissionIdForm = countofSkipSubmissionIdForm;
                    }
                    if (countofbadrequestfornote > 0)
                    {
                        responseModel.countofbadrequestfornote = countofbadrequestfornote;
                    }
                    if (countofInvalidSchema > 0)
                    {
                        responseModel.countofInvalidSchema = countofInvalidSchema;
                        if (ListofInvalidSchema != null && ListofInvalidSchema.Count > 0)
                        {
                            responseModel.ListofInvalidSchema = string.Join(";", ListofInvalidSchema);
                        }
                    }
                }
            }
            if (countofGetSubmission > 0)
            {
                responseModel.countofGetSubmission = countofGetSubmission;
            }


            return responseModel;
        }
        public static List<AbEntryFieldInfo> GetCessnockAbEntryFieldInfo()
        {
            List<AbEntryFieldInfo> lstAbEntryFieldInfo = new List<AbEntryFieldInfo>();
            string json = CommonServices_CessnockAdmin.GetAbEntryProperties();
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
        public static List<AbEntryFieldInfo> GetGPTQAbEntryFieldInfo()
        {
            List<AbEntryFieldInfo> lstAbEntryFieldInfo = new List<AbEntryFieldInfo>();
            string json = GPTQCRMReference.GetAbEntryProperties();
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
        public static CoreResponseModel GPTQCreateOrUpdateIndividual(List<AbEntryFieldInfoOrderBy> lstAbEntryFieldInfoOrderBy)
        {
            CoreResponseModel responseModel = new CoreResponseModel();
            int count = 0;

            string strRes = "";
            int countofGetSubmission = 0;
            responseModel.countofGetSubmission = 0;
            int countofNewEntry = 0;
            List<string> ListofInvalidSchema = new List<string>();
            int countofInvalidSchema = 0;
            int countofUpdateEntry = 0;
            int countofInvalidForm = 0;
            int countofSkipSubmissionIdForm = 0;
            int countofbadRequest = 0;
            int countofbadrequestfornote = 0;

            if (lstAbEntryFieldInfoOrderBy != null && lstAbEntryFieldInfoOrderBy.Count > 0)
            {
                foreach (var item in lstAbEntryFieldInfoOrderBy)
                {
                    strRes = GPTQCRMReference.CreateOrUpdateIndividual_Latest(item.lstAbEntryFieldInfo);
                    count += 1;
                    countofGetSubmission += 1;

                    if (strRes == "companynamerquired")
                    {
                        countofInvalidForm += 1;
                    }
                    else if (strRes == "skipSubmissionId")
                    {
                        countofSkipSubmissionIdForm += 1;
                    }
                    else if (!string.IsNullOrEmpty(strRes) && strRes.Contains("invalidschema-"))
                    {
                        countofInvalidSchema += 1;
                        ListofInvalidSchema.Add(strRes.Replace("invalidschema-", "Submission Date: " + item.displayCreated_at + ", Error: "));
                    }
                    else if (strRes == "badrequest")
                    {
                        countofbadRequest += 1;
                    }
                    else if (strRes == "noteinsertsuccess")
                    {
                        countofNewEntry += 1;
                    }
                    else if (strRes == "noteupdatesuccess")
                    {
                        countofUpdateEntry += 1;
                    }
                    else if (strRes == "badrequestfornote")
                    {
                        countofbadrequestfornote += 1;
                    }
                    else if (strRes == "contactinsertsuccess")
                    {
                        countofNewEntry += 1;
                    }
                    else if (strRes == "contactupdatesuccess")
                    {
                        countofUpdateEntry += 1;
                    }
                    else if (strRes == "badrequestforcontact")
                    {
                        countofbadrequestfornote += 1;
                    }
                    if (countofInvalidForm > 0)
                    {
                        responseModel.countofInvalidForm = countofInvalidForm;
                    }
                    if (countofbadRequest > 0)
                    {
                        responseModel.countofbadRequest = countofbadRequest;
                    }
                    if (countofNewEntry > 0)
                    {
                        responseModel.countofNewEntry = countofNewEntry;
                    }
                    if (countofUpdateEntry > 0)
                    {
                        responseModel.countofUpdateEntry = countofUpdateEntry;
                    }
                    if (countofSkipSubmissionIdForm > 0)
                    {
                        responseModel.countofSkipSubmissionIdForm = countofSkipSubmissionIdForm;
                    }
                    if (countofbadrequestfornote > 0)
                    {
                        responseModel.countofbadrequestfornote = countofbadrequestfornote;
                    }
                    if (countofInvalidSchema > 0)
                    {
                        responseModel.countofInvalidSchema = countofInvalidSchema;
                        if (ListofInvalidSchema != null && ListofInvalidSchema.Count > 0)
                        {
                            responseModel.ListofInvalidSchema = string.Join(";", ListofInvalidSchema);
                        }
                    }
                }
            }
            if (countofGetSubmission > 0)
            {
                responseModel.countofGetSubmission = countofGetSubmission;
            }


            return responseModel;
        }
        #region API RETURN  Jobject
        /// <summary>
        /// parameter in pass url
        /// </summary>
        public static JObject fnExecuteGetRequest(string path)
        {
            WebRequest req = WebRequest.Create(path);
            req.Method = "GET";
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream answerStream = resp.GetResponseStream();
            String answerString = new StreamReader(answerStream).ReadToEnd();
            var responseCode = JObject.Parse(answerString)["responseCode"];

            if (responseCode.ToString() != "200")
            {
                if (responseCode.ToString() == "401")
                {
                    throw new JotformException("Unauthozired API call");
                }
                else if (responseCode.ToString() == "404")
                {
                    throw new JotformException(JObject.Parse(answerString)["message"].ToString());
                }
                else if (responseCode.ToString() == "503")
                {
                    throw new JotformException("Service is unavaible, rate limits etc exceeded!");
                }
            }
            return JObject.Parse(answerString);
        }
        #endregion
    }
}
