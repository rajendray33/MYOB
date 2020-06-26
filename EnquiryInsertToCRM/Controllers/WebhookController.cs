using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.Models;
using JotForm;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace EnquiryInsertToCRM.Controllers
{
    public class WebhookController : Controller
    {
        public object CreateOrUpdateIndividual { get; private set; }

        // Created For common code used but API not aacess for GPTQ
        //[HttpPost]
        //public void WebhookPost()
        //{
        //    StringBuilder strb = new StringBuilder();
        //    string APIKey = ConfigurationManager.AppSettings["GTPQWebAPIKey"];
        //    string strsubmissionID = "";
        //    string webrequestPath= "https://gptq.jotform.com/API/submission/[SUBMISSIONID]?apikey=[APIKEY]";
        //    string formListPath = "https://gptq.jotform.com/API/user/forms?apikey=[APIKEY]&orderby=id";
        //    try
        //    {
        //        List<Models.WebHookResponse.WebHookResponseModel> modelList = new List<Models.WebHookResponse.WebHookResponseModel>();
        //        Request.InputStream.Position = 0;

        //        using (Stream receiveStream = Request.InputStream)
        //        {
        //            var form = HttpContext.Request.Form;
        //            foreach (string key in form.AllKeys)
        //            {
        //                if (key == "submissionID")
        //                {
        //                    strb.AppendLine("submissionID: " + form[key]);
        //                    CommonMethod.LogFile(strb, true);
        //                    strsubmissionID = form[key];
        //                }
        //            }
        //            if (!string.IsNullOrEmpty(strsubmissionID))
        //            {
        //                webrequestPath = webrequestPath.Replace("[SUBMISSIONID]", strsubmissionID);
        //                webrequestPath = webrequestPath.Replace("[APIKEY]", APIKey);
        //                formListPath = formListPath.Replace("[APIKEY]", APIKey);
        //                List<AbEntryFieldInfoOrderBy> abEntryModelList = new List<AbEntryFieldInfoOrderBy>();
        //                CoreResponseModel responseModel = new CoreResponseModel();
        //                abEntryModelList = JotFormReference_LatestBackup.GetJotFormToAbEntryFieldList(webrequestPath, (APIKey ?? ""), "gptq", formListPath);
        //                responseModel = JotFormReference_LatestBackup.GPTQCreateOrUpdateIndividual(abEntryModelList);

        //                strb.Clear();
        //                strb.AppendLine("SubmissionID: " + strsubmissionID + "");
        //                strb.AppendLine("countofGetSubmission: " + responseModel.countofGetSubmission + "");
        //                strb.AppendLine("countofNewEntry: " + responseModel.countofNewEntry + "");
        //                strb.AppendLine("countofUpdateEntry: " + responseModel.countofUpdateEntry + "");
        //                strb.AppendLine("countofInvalidForm: " + responseModel.countofInvalidForm + "");
        //                strb.AppendLine("countofUpdateEntry: " + responseModel.countofUpdateEntry + "");
        //                strb.AppendLine("countofInvalidSchema: " + responseModel.countofInvalidSchema + "");
        //                strb.AppendLine("countofbadRequest: " + responseModel.countofbadRequest + "");
        //                strb.AppendLine("countofSkipSubmissionIdForm: " + responseModel.countofSkipSubmissionIdForm + "");
        //                strb.AppendLine("countofbadrequestfornote: " + responseModel.countofbadrequestfornote + "");

        //                CommonMethod.LogFile(strb, false);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        strb.Clear();
        //        strb.AppendLine("Error: " + ex.Message + "");
        //        CommonMethod.LogFile(strb, false);
        //    }
        //}

        public JsonResult JotFormPrePopulateBasedOnEMailORPHone(string strEmail, string strPhone)
        {
            WebHookResponse.WebHookValidationForExistingModel enqmodel = new WebHookResponse.WebHookValidationForExistingModel();
            JArray searchJArray = new JArray();
            if (!string.IsNullOrEmpty(strEmail))
            {
                searchJArray.Add(new JObject(new JProperty("$PHRASE", strEmail)));
                enqmodel = GPTQCRMReference.ReadCheckEmailOrPhoneExisting(enqmodel, searchJArray);
            }
            if (!string.IsNullOrEmpty(strPhone))
            {
                searchJArray.Add(new JObject(new JProperty("$PHRASE", strPhone)));
                enqmodel = GPTQCRMReference.ReadCheckEmailOrPhoneExisting(enqmodel, searchJArray);
            }
            var jsonData = new
            {
                enqmodel
            };
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public void CessonckWebhookPost()
        {
            StringBuilder strb = new StringBuilder();
            string APIKey = ConfigurationManager.AppSettings["CessnockWebAPIKey"];
            string strsubmissionID = "";
            string webrequestPath = "https://api.jotform.com/submission/[SUBMISSIONID]?apikey=[APIKEY]";
            string formListPath = "https://api.jotform.com/user/forms?apikey=[APIKEY]&orderby=id";
            try
            {
                List<Models.WebHookResponse.WebHookResponseModel> modelList = new List<Models.WebHookResponse.WebHookResponseModel>();
                Request.InputStream.Position = 0;                
                using (Stream receiveStream = Request.InputStream)
                {
                    var form = HttpContext.Request.Form;
                    foreach (string key in form.AllKeys)
                    {
                        if (key == "submissionID")
                        {
                            strb.AppendLine("submissionID: " + form[key]);
                            CommonMethod.LogFile(strb,true);
                            strsubmissionID = form[key];
                        }
                    }
                }
                if (!string.IsNullOrEmpty(strsubmissionID))
                {
                    webrequestPath = webrequestPath.Replace("[SUBMISSIONID]", strsubmissionID);
                    webrequestPath = webrequestPath.Replace("[APIKEY]", APIKey);
                    formListPath = formListPath.Replace("[APIKEY]", APIKey);
                    List<AbEntryFieldInfoOrderBy> abEntryModelList = new List<AbEntryFieldInfoOrderBy>();
                    CoreResponseModel responseModel = new CoreResponseModel();
                    abEntryModelList = JotFormReference_LatestBackup.GetJotFormToAbEntryFieldList(webrequestPath, (APIKey??""), "cessnock", formListPath);
                    responseModel = JotFormReference_LatestBackup.ProcessForCessnockCRM(abEntryModelList);

                    strb.Clear();
                    strb.AppendLine("SubmissionID: " + strsubmissionID + "");
                    strb.AppendLine("countofGetSubmission: " + responseModel.countofGetSubmission + "");
                    strb.AppendLine("countofNewEntry: " + responseModel.countofNewEntry + "");
                    strb.AppendLine("countofUpdateEntry: " + responseModel.countofUpdateEntry + "");
                    strb.AppendLine("countofInvalidForm: " + responseModel.countofInvalidForm + "");
                    strb.AppendLine("countofUpdateEntry: " + responseModel.countofUpdateEntry + "");
                    strb.AppendLine("countofInvalidSchema: " + responseModel.countofInvalidSchema + "");
                    strb.AppendLine("countofbadRequest: " + responseModel.countofbadRequest + "");
                    strb.AppendLine("countofSkipSubmissionIdForm: " + responseModel.countofSkipSubmissionIdForm + "");
                    strb.AppendLine("countofbadrequestfornote: " + responseModel.countofbadrequestfornote + "");

                    CommonMethod.LogFile(strb, false);
                }
            }
            catch (Exception ex)
            {
                strb.Clear();
                strb.AppendLine("Error: " + ex.Message + "");
                CommonMethod.LogFile(strb, false);
            }
        }
        
    }
}