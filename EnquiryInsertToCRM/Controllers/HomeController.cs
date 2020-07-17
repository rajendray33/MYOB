using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.Models;
using Newtonsoft.Json.Linq;
using System.Configuration;
using MYOB.AccountRight.SDK;
using MYOB.AccountRight.SDK.Services;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using AutoMapper;
using MYOB.AccountRight.SDK.Contracts;
using GeneralLedgerJournalTransaction;
using System.Net.Mail;
using System.Net;
using System.Globalization;

namespace EnquiryInsertToCRM.Controllers
{
    public class HomeController : Controller
    {
        public static bool IsUpdatedtoCRM = false;
        public static bool IsPreviousSynInProcess = false;
        public ActionResult Index(string SalesPerson)
        {

            //List<AbEntryFieldInfo> lstFyOrdersList = RigonCRMReference.getFY_OrdersUdfList();
            //RigonCRMReference.GetFieldList();
            //if (SalesPerson.Contains("\\n"))
            //{
            //    SalesPerson = SalesPerson.Replace(@"""", @"\""").Replace(@"\""", @"").Replace("\\n", "\n");
            //    var str2 = SalesPerson.Split('\n').ToList();
            //}
            //else
            //{
            //    string str = "";
            //}
            //var str = PKCRMReference.GetAbEntryProperties();
            //RigonCRMReference.ReadExistingCompanyNameOrMYOBID("", "aeb086f8-7647-4217-84f2-7da7783ea987");
            //RigonCRMReference.GetAbEntryProperties();
            //RigonCRMReference.GetReferenceByList();

            //var str = CommonMethod.GetFinancialYearList();

            //AbEntryKeyModel model1 = new AbEntryKeyModel();
            //model1 = RigonCRMReference.ReadExistingCompanyNameOrMYOBID(CustomerInfo.CompanyName, "");
            //if (!string.IsNullOrEmpty(model1.Key))
            //{
            //    string key = RigonCRMReference.CreateCompany(CustomerInfo);
            //    if (!string.IsNullOrEmpty(key))
            //    {

            //    }
            //}

            ViewBag.pageheader = "BROOKWATER";
            EnquiryModel model = new EnquiryModel();
            ViewBag.referencemodelList = "";
            ViewBag.preferredinvestmentlevelList = "";
            model.SalesPerson = SalesPerson;

            var referencemodelList = CommonServices.GetReferenceByList();
            if (referencemodelList != null && referencemodelList.Count > 0)
            {
                ViewBag.referencemodelList = referencemodelList;
            }
            var preferredinvestmentlevelList = CommonServices.GetPreferredInvestmentLevelList();
            if (preferredinvestmentlevelList != null && preferredinvestmentlevelList.Count > 0)
            {
                ViewBag.preferredinvestmentlevelList = preferredinvestmentlevelList;
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public string EnquirySubmit(EnquiryModel model)
        {
            string res = "";
            string mailres = "";

            var referencemodelList = CommonServices.GetReferenceByList();
            var preferredinvestmentlevelList = CommonServices.GetPreferredInvestmentLevelList();

            if (ModelState.IsValid)
            {
                if (referencemodelList != null && referencemodelList.Count > 0)
                {
                    model.strReferenceBy = (from c in referencemodelList
                                            where c.Key == model.ReferenceBy
                                            select c.Value).FirstOrDefault();

                }
                if (preferredinvestmentlevelList != null && preferredinvestmentlevelList.Count > 0)
                {
                    model.strPreferredInvestmentLevel = (from c in preferredinvestmentlevelList
                                                         where c.Key == model.PreferredInvestmentLevel
                                                         select c.Value).FirstOrDefault();
                }
                var BwSourceList = CommonServices.GetBWSourceList();
                var InvestmentLevelList = CommonServices.GetInvestmentLevelList();

                if (BwSourceList != null && BwSourceList.Count > 0)
                {
                    model.BWSource = (from c in BwSourceList
                                      where c.Value == model.strReferenceBy
                                      select c.Key).FirstOrDefault();

                }
                if (InvestmentLevelList != null && InvestmentLevelList.Count > 0)
                {
                    model.InvestmentLevel = (from c in InvestmentLevelList
                                             where c.Value == model.strPreferredInvestmentLevel
                                             select c.Key).FirstOrDefault();
                }


                res = CommonServices.CreateOrUpdateIndividual(model);
                if (res != "badrequestfornote" || res != "badrequest")
                {
                    string strTemplateFilePath = "";
                    string strBody = "";
                    string strMailSubject = "Brookwater Web Form [FIRSTNAME] - [LASTNAME]";
                    strTemplateFilePath = Server.MapPath("~/images/EmailTemplate/InquiryToAdmin.html");
                    System.IO.StreamReader strFile = new System.IO.StreamReader(strTemplateFilePath);
                    string strFileContent = strFile.ReadToEnd();
                    strMailSubject = strMailSubject.Replace("[FIRSTNAME]", model.FirstName);
                    strMailSubject = strMailSubject.Replace("[LASTNAME]", model.LastName);
                    strFileContent = strFileContent.Replace("[FIRSTNAME]", model.FirstName);
                    strFileContent = strFileContent.Replace("[LASTNAME]", model.LastName);
                    strFileContent = strFileContent.Replace("[EMAIL]", model.Email);
                    strFileContent = strFileContent.Replace("[STREETADDRESS]", model.AddressLine1);
                    strFileContent = strFileContent.Replace("[MOBILE]", model.Mobile);
                    strFileContent = strFileContent.Replace("[SUBURB]", model.City);
                    strFileContent = strFileContent.Replace("[STATE]", model.StateProvince);
                    strFileContent = strFileContent.Replace("[POSTCODE]", model.ZipCode);
                    strFileContent = strFileContent.Replace("[REFERENCEBY]", model.strReferenceBy);
                    strFileContent = strFileContent.Replace("[PREFERREDINVESTMENTLEVEL]", model.strPreferredInvestmentLevel);
                    strFileContent = strFileContent.Replace("[SALESPERSON]", model.SalesPerson);
                    strFile.Close();
                    strBody = strFileContent;
                    mailres = CommonMethod.MailSend(strMailTo: "", strMailSubject: strMailSubject, strMailBody: strBody, blnAllowMultipleBCC: false, sendBCC: true, blnSendToAdmin: true);
                    if (mailres == "failed")
                    {
                        res = "sendingmailfailed";
                    }
                }
            }
            return res;
        }

        #region demobusinessform
        public ActionResult demobusinessform()
        {
            ViewBag.pageheader = "JOTFORM TO CRM";
            FormSubmissionsModel model1 = new FormSubmissionsModel();
            List<DropdownModel> formTitles = new List<DropdownModel>();
            ViewBag.ddFormList = "";
            formTitles = JotFormReference.GetFormList();
            if (formTitles != null && formTitles.Count > 0)
            {
                ViewBag.ddFormList = formTitles.ToList();
            }
            return View(model1);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult GetDemoBusinessFormSubmissionList(FormSubmissionsModel model)
        {

            StringBuilder strb = new StringBuilder();
            CoreResponseModel ResponseModel = new CoreResponseModel();
            if (ModelState.IsValid)
            {
                try
                {
                    ResponseModel = JotFormReference.GetDemoBusinessFormSubmissionList(model);
                }
                catch (Exception ex)
                {
                    strb.AppendLine(ex.Message);
                    CommonMethod.LogFile(strb, false);
                }

            }
            var jsonData = new
            {
                data = new
                {
                    ResponseModel
                }
            };
            var filename = "/images/Response.txt";

            strb.AppendLine("countofGetSubmission: " + ResponseModel.countofGetSubmission + "");
            strb.AppendLine("countofNewEntry: " + ResponseModel.countofNewEntry + "");
            strb.AppendLine("countofUpdateEntry: " + ResponseModel.countofUpdateEntry + "");
            strb.AppendLine("countofInvalidForm: " + ResponseModel.countofInvalidForm + "");
            strb.AppendLine("countofUpdateEntry: " + ResponseModel.countofUpdateEntry + "");
            strb.AppendLine("countofInvalidSchema: " + ResponseModel.countofInvalidSchema + "");
            strb.AppendLine("countofbadRequest: " + ResponseModel.countofbadRequest + "");
            strb.AppendLine("countofSkipSubmissionIdForm: " + ResponseModel.countofSkipSubmissionIdForm + "");
            strb.AppendLine("countofbadrequestfornote: " + ResponseModel.countofbadrequestfornote + "");
            using (StreamWriter sw = System.IO.File.CreateText(Server.MapPath(filename)))
            {
                sw.WriteLine(DateTime.Now.ToShortDateString() + " - " + strb.ToString());
            }
            return Json(jsonData, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region enews
        public ActionResult enews()
        {
            eNewsModel model = new eNewsModel();
            ViewBag.pageheader = "GREATER SPRINGFIELD";
            ViewBag.SuccessStage = "";
            if (TempData["SuccessStage"] != null)
            {
                ViewBag.SuccessStage = TempData["SuccessStage"];
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        //[HttpPost]        
        public ActionResult eNewsPost(eNewsModel model)
        {
            ViewBag.inValidformrequest = "0";
            if (!eNewsCRMReference.IsValidFormRequest(model.formname))
            {
                ViewBag.inValidformrequest = "1";
                return View();
            }

            #region custom validdation            
            if (model.formname == "medical-specialist-office-suites-lease" || model.formname == "a-learning-city" || model.formname == "springfield-central-office-sales-leasing" || model.formname == "retail-and-hospitality-leasing")
            {
                if (string.IsNullOrWhiteSpace(model.phone))
                {
                    ModelState.AddModelError("phone", "This field is required.");
                }

                if (string.IsNullOrWhiteSpace(model.howdidyouhearaboutus))
                {
                    ModelState.AddModelError("howdidyouhearaboutus", "This field is required.");
                }
            }
            if (model.formname == "medical-specialist-office-suites-lease")
            {
                if (string.IsNullOrWhiteSpace(model.interestinleaseorsale))
                {
                    ModelState.AddModelError("interestinleaseorsale", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.postcode))
                {
                    ModelState.AddModelError("postcode", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.title))
                {
                    ModelState.AddModelError("title", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.interestedintype))
                {
                    ModelState.AddModelError("interestedintype", "This field is required.");
                }
            }
            if (model.formname == "springfield-central-office-sales-leasing")
            {
                if (string.IsNullOrWhiteSpace(model.interestinleaseorsale))
                {
                    ModelState.AddModelError("interestinleaseorsale", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.postcode))
                {
                    ModelState.AddModelError("postcode", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.title))
                {
                    ModelState.AddModelError("title", "This field is required.");
                }
            }
            if (model.formname == "enews")
            {
                if (string.IsNullOrWhiteSpace(model.country))
                {
                    ModelState.AddModelError("country", "This field is required.");
                }
            }
            if (model.formname == "golf-and-country-club-enquiry")
            {
                if (string.IsNullOrWhiteSpace(model.fullname))
                {
                    ModelState.AddModelError("fullname", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.telephone))
                {
                    ModelState.AddModelError("telephone", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.firstname))
                {
                    ModelState["firstname"].Errors.Clear();
                }
                if (string.IsNullOrWhiteSpace(model.lastname))
                {
                    ModelState["lastname"].Errors.Clear();
                }
                if (string.IsNullOrWhiteSpace(model.enquiry))
                {
                    ModelState.AddModelError("enquiry", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.enquirytopic))
                {
                    ModelState.AddModelError("enquiry", "This field is required.");
                }
                if (string.IsNullOrWhiteSpace(model.wouldliketobecontactedbyphone))
                {
                    //ModelState.AddModelError("wouldliketobecontactedbyphone", "This field is optional(Note*: Pass only Yes/No).");
                }
                if (string.IsNullOrWhiteSpace(model.golflessonenquiry))
                {
                    // ModelState.AddModelError("golflessonenquiry", "This field is optional(Note*: Pass only Yes/No).");
                }

            }
            #endregion

            string res = "";
            string mailres = "";
            ViewBag.response = "";
            ViewBag.ErrorList = "";
            if (ModelState.IsValid)
            {
                res = eNewsCRMReference.CreateOrUpdateIndividual(model);
                if (res != "badrequestfornote" || res != "badrequest")
                {
                    string strTemplateFilePath = "";
                    string strBody = "";
                    #region Dynamic Mail Body Content
                    string strMailSubject = "[FORMNAME] Web Form [FIRSTNAME] - [LASTNAME]";
                    strTemplateFilePath = Server.MapPath("~/images/EmailTemplate/eNewsFormToAdmin.html");
                    if (model.formname == "golf-and-country-club-enquiry")
                    {
                        strTemplateFilePath = "";
                        strTemplateFilePath = Server.MapPath("~/images/EmailTemplate/BrookwaterGolfCourseAndCountryClubToAdmin.html");
                    }
                    System.IO.StreamReader strFile = new System.IO.StreamReader(strTemplateFilePath);
                    string strFileContent = strFile.ReadToEnd();
                    if (!string.IsNullOrEmpty(model.formname))
                    {
                        strMailSubject = strMailSubject.Replace("[FORMNAME]", model.formname.Substring(0, 1).ToUpper() + model.formname.Substring(1, (model.formname.Length - 1)));
                        strFileContent = strFileContent.Replace("[FORMNAME]", model.formname.Substring(0, 1).ToUpper() + model.formname.Substring(1, (model.formname.Length - 1)));
                    }
                    else
                    {
                        strMailSubject = strMailSubject.Replace("[FORMNAME]", "");
                        strFileContent = strFileContent.Replace("[FORMNAME]", "");
                    }
                    if (model.formname == "golf-and-country-club-enquiry")
                    {
                        strMailSubject = "[FORMNAME] Web Form [FULLNAME]";
                        strMailSubject = strMailSubject.Replace("[FORMNAME]", model.formname.Substring(0, 1).ToUpper() + model.formname.Substring(1, (model.formname.Length - 1)));
                        strMailSubject = strMailSubject.Replace("[FULLNAME]", model.fullname);
                        strFileContent = strFileContent.Replace("[FULLNAME]", model.fullname);

                        strFileContent = strFileContent.Replace("[EMAIL]", model.email);
                        if (!string.IsNullOrEmpty(model.wouldliketobecontactedbyphone))
                        {
                            strFileContent = strFileContent.Replace("[WOULDLIKETOBECONTACTEDBYPHONE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">I would like to be contacted by phone:&nbsp; <span style=\"font-weight: normal;\">" + model.wouldliketobecontactedbyphone + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[WOULDLIKETOBECONTACTEDBYPHONE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.golflessonenquiry))
                        {
                            strFileContent = strFileContent.Replace("[GOLFLESSONENQUIRY]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Golf lesson Enquiry:&nbsp; <span style=\"font-weight: normal;\">" + model.golflessonenquiry + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[GOLFLESSONENQUIRY]", "");
                        }
                        if (!string.IsNullOrEmpty(model.telephone))
                        {
                            strFileContent = strFileContent.Replace("[TELEPHONE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Telephone:&nbsp; <span style=\"font-weight: normal;\">" + model.telephone + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[TELEPHONE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.postcode))
                        {
                            strFileContent = strFileContent.Replace("[POSTCODE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Postcode:&nbsp; <span style=\"font-weight: normal;\">" + model.postcode + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[POSTCODE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.enquiry))
                        {
                            strFileContent = strFileContent.Replace("[ENQUIRY]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Enquiry:&nbsp; <span style=\"font-weight: normal;\">" + model.enquiry + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[ENQUIRY]", "");
                        }
                        if (!string.IsNullOrEmpty(model.enquirytopic))
                        {
                            strFileContent = strFileContent.Replace("[ENQUIRYTOPIC]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Enquiry Topic:&nbsp; <span style=\"font-weight: normal;\">" + model.enquirytopic + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[ENQUIRYTOPIC]", "");
                        }

                    }
                    else
                    {
                        strMailSubject = strMailSubject.Replace("[FIRSTNAME]", model.firstname);
                        strMailSubject = strMailSubject.Replace("[LASTNAME]", model.lastname);


                        strFileContent = strFileContent.Replace("[FIRSTNAME]", model.firstname);
                        strFileContent = strFileContent.Replace("[LASTNAME]", model.lastname);
                        strFileContent = strFileContent.Replace("[EMAIL]", model.email);
                        if (!string.IsNullOrEmpty(model.gsgeneralenews))
                        {
                            strFileContent = strFileContent.Replace("[GSGENERALENEWS]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">GS General eNews:&nbsp; <span style=\"font-weight: normal;\">" + model.gsgeneralenews + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[GSGENERALENEWS]", "");
                        }
                        if (!string.IsNullOrEmpty(model.gsnewsandviews))
                        {
                            strFileContent = strFileContent.Replace("[GSNEWSANDVIEWS]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">GS News and Views:&nbsp; <span style=\"font-weight: normal;\">" + model.gsnewsandviews + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[GSNEWSANDVIEWS]", "");
                        }
                        if (!string.IsNullOrEmpty(model.phone))
                        {
                            strFileContent = strFileContent.Replace("[PHONE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Phone:&nbsp; <span style=\"font-weight: normal;\">" + model.phone + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[PHONE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.address))
                        {
                            strFileContent = strFileContent.Replace("[ADDRESS]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Address:&nbsp; <span style=\"font-weight: normal;\">" + model.address + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[ADDRESS]", "");
                        }
                        if (!string.IsNullOrEmpty(model.suburb))
                        {
                            strFileContent = strFileContent.Replace("[SUBURB]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Suburb:&nbsp; <span style=\"font-weight: normal;\">" + model.suburb + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[SUBURB]", "");
                        }
                        if (!string.IsNullOrEmpty(model.state))
                        {
                            strFileContent = strFileContent.Replace("[STATE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">State:&nbsp; <span style=\"font-weight: normal;\">" + model.state + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[STATE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.country))
                        {
                            strFileContent = strFileContent.Replace("[COUNTRY]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Country:&nbsp; <span style=\"font-weight: normal;\">" + model.country + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[COUNTRY]", "");
                        }
                        if (!string.IsNullOrEmpty(model.postcode))
                        {
                            strFileContent = strFileContent.Replace("[POSTCODE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Postcode:&nbsp; <span style=\"font-weight: normal;\">" + model.postcode + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[POSTCODE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.comments))
                        {
                            strFileContent = strFileContent.Replace("[COMMENTS]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Comments:&nbsp; <span style=\"font-weight: normal;\">" + model.comments + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[COMMENTS]", "");
                        }
                        if (!string.IsNullOrEmpty(model.interestedin))
                        {
                            strFileContent = strFileContent.Replace("[INTERESTEDIN]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Interested In:&nbsp; <span style=\"font-weight: normal;\">" + model.interestedin + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[INTERESTEDIN]", "");
                        }
                        if (!string.IsNullOrEmpty(model.title))
                        {
                            strFileContent = strFileContent.Replace("[TITLE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Title:&nbsp; <span style=\"font-weight: normal;\">" + model.title + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[TITLE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.interestedintype))
                        {
                            strFileContent = strFileContent.Replace("[INTERESTEDINTYPE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">I am interested in:&nbsp; <span style=\"font-weight: normal;\">" + model.interestedintype + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[INTERESTEDINTYPE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.interestinleaseorsale))
                        {
                            strFileContent = strFileContent.Replace("[INTERESTINLEASEORSALE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">I am interested in(Lease Or Sale):&nbsp; <span style=\"font-weight: normal;\">" + model.interestinleaseorsale + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[INTERESTINLEASEORSALE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.howdidyouhearaboutus))
                        {
                            strFileContent = strFileContent.Replace("[HOWDIDYOUHEARABOUTUS]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">How did you hear about us?:&nbsp; <span style=\"font-weight: normal;\">" + model.howdidyouhearaboutus + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[HOWDIDYOUHEARABOUTUS]", "");
                        }
                        if (!string.IsNullOrEmpty(model.inspectionrequest))
                        {
                            strFileContent = strFileContent.Replace("[INSPECTIONREQUEST]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Please contact me regarding an inspection:&nbsp; <span style=\"font-weight: normal;\">" + model.inspectionrequest + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[INSPECTIONREQUEST]", "");
                        }
                        if (!string.IsNullOrEmpty(model.informationpackrequest))
                        {
                            strFileContent = strFileContent.Replace("[INFORMATIONPACKREQUEST]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Please send me Information Pack:&nbsp; <span style=\"font-weight: normal;\">" + model.informationpackrequest + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[INFORMATIONPACKREQUEST]", "");
                        }
                        if (!string.IsNullOrEmpty(model.gsbrochurerequest))
                        {
                            strFileContent = strFileContent.Replace("[GSBROCHUREREQUEST]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Yes, I'd like to receive the Greater Springfield Learning City Brochure:&nbsp; <span style=\"font-weight: normal;\">" + model.gsbrochurerequest + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[GSBROCHUREREQUEST]", "");
                        }
                        if (!string.IsNullOrEmpty(model.gsupdaterequest))
                        {
                            strFileContent = strFileContent.Replace("[GSUPDATEREQUEST]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Yes, I'd like to receive the latest Greater Springfield updates:&nbsp; <span style=\"font-weight: normal;\">" + model.gsupdaterequest + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[GSUPDATEREQUEST]", "");
                        }
                        if (!string.IsNullOrEmpty(model.gsopportunitiesrequest))
                        {
                            strFileContent = strFileContent.Replace("[GSOPPORTUNITIESREQUEST]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Yes, I would like to discuss opportunities for my business/education facility:&nbsp; <span style=\"font-weight: normal;\">" + model.gsopportunitiesrequest + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[GSOPPORTUNITIESREQUEST]", "");
                        }
                        if (!string.IsNullOrEmpty(model.learningcoalitioncommittee))
                        {
                            strFileContent = strFileContent.Replace("[LEARNINGCOALITIONCOMMITTEE]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Yes, I would like to make contact with the Learning Coalition Committee:&nbsp; <span style=\"font-weight: normal;\">" + model.learningcoalitioncommittee + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[LEARNINGCOALITIONCOMMITTEE]", "");
                        }
                        if (!string.IsNullOrEmpty(model.gsreceivelatestnews))
                        {
                            strFileContent = strFileContent.Replace("[GSRECEIVELATESTNEWS]", "<p style=\"font-size: 12px; font-weight: bold; color: #000000; line-height: 12px;\">Yes, I would like to receive the latest news on Greater Springfield:&nbsp; <span style=\"font-weight: normal;\">" + model.gsreceivelatestnews + "</span></p>");
                        }
                        else
                        {
                            strFileContent = strFileContent.Replace("[GSRECEIVELATESTNEWS]", "");
                        }
                    }



                    strFile.Close();
                    #endregion
                    strBody = strFileContent;
                    string mailto = ConfigurationManager.AppSettings["bccEmail"];
                    mailres = CommonMethod.MailSend(strMailTo: mailto, strMailSubject: strMailSubject, strMailBody: strBody, blnAllowMultipleBCC: true, sendBCC: true, blnSendToAdmin: false);
                    //mailres = "failed";
                    string strSubscriberRes = "";
                    if (model.formname == "golf-and-country-club-enquiry")
                    {
                        strSubscriberRes = AddWithCustomFields_GolfAndCountry(model);
                    }
                    else
                    {
                        if ((model.gsgeneralenews ?? "") != "" && Convert.ToString(model.gsgeneralenews ?? "").ToLower() == "yes")
                        {
                            strSubscriberRes = AddWithCustomFields(model);
                        }
                    }
                    if (mailres == "failed")
                    {
                        if (res == "noteinsertsuccess")
                        {
                            res = "insertsuccessSendingmailfailed";
                        }
                        else if (res == "noteupdatesuccess")
                        {
                            res = "updatesuccessSendingmailfailed";
                        }
                        else if (strSubscriberRes == "errorforsubscriberadd")
                        {
                            res = "updatesuccessSendingmailfailedsubscriberfailed";
                        }
                    }
                    ViewBag.response = res;
                    TempData["SuccessStage"] = res;
                    return RedirectToAction("enews", "Home");
                }
            }
            return View();
        }
        #endregion

        #region Add Subscriber List in Greater Springfield URL USING API                
        public string AddWithCustomFields(eNewsModel model)
        {
            string res = "";
            string ListId = "Greater Springfield";
            ApiKeyAuthenticationDetails auth = new ApiKeyAuthenticationDetails(ConfigurationManager.AppSettings["GreaterSpringfield_APIKey"]);
            Client Client1 = new Client(auth, ConfigurationManager.AppSettings["GreaterSpringfield_ClientID"]);
            var cientList = Client1.Lists();
            if (cientList != null && cientList.Count() > 0)
            {
                ListId = cientList.Where(s => s.Name.ToLower().Trim() == ("Greater Springfield").ToLower().Trim()).Select(s => s.ListID).FirstOrDefault();
            }
            Subscriber subscriber = new Subscriber(auth, ListId);
            List<SubscriberCustomField> customFields = new List<SubscriberCustomField>();

            try
            {
                string newSubscriberID = subscriber.Add(model.email, (model.firstname + " " + model.lastname), customFields, true, ConsentToTrack.Yes);
                if (!string.IsNullOrEmpty(newSubscriberID))
                {
                    res = "successforsubscriberadd";
                }
            }
            catch
            {
                res = "errorforsubscriberadd";
            }
            return res;
        }
        public string AddWithCustomFields_GolfAndCountry(eNewsModel model)
        {
            string res = "";
            string ListId = "Greater Springfield";


            ApiKeyAuthenticationDetails auth = new ApiKeyAuthenticationDetails(ConfigurationManager.AppSettings["GreaterSpringfield_APIKey_CampaignMonitor"]);
            Client Client1 = new Client(auth, ConfigurationManager.AppSettings["GreaterSpringfield_ClientID_CampaignMonitor"]);
            var cientList = Client1.Lists();
            string newSubscriberID = "";
            if (cientList != null && cientList.Count() > 0)
            {
                List<string> lstenquirytopic = new List<string>();

                List<string> listGolfGeneralArray = "General Enquiry|Corporate, Tours and Group Golf|Memmbership|Lessons|Golf Shop|Driving Range".ToLower().Trim().Split('|').ToList();
                List<string> listFEWDArray = "Functions and Events|General Enquiry".ToLower().Trim().Split('|').ToList();

                string strenquirytopic = model.enquirytopic.Trim().ToLower();
                var objlistGolfGeneralArray = listGolfGeneralArray.Where(x => strenquirytopic.Contains(x)).Select(s => s).FirstOrDefault();
                if (objlistGolfGeneralArray != null)
                {
                    lstenquirytopic.Add(ConfigurationManager.AppSettings["GreaterSpringfield_GolfGeneral"]);
                }
                var objlistFEWDArray = listFEWDArray.Where(x => strenquirytopic.Contains(x)).Select(s => s).FirstOrDefault();
                if (objlistFEWDArray != null)
                {
                    lstenquirytopic.Add(ConfigurationManager.AppSettings["GreaterSpringfield_FEWD"]);
                }


                if (lstenquirytopic != null && lstenquirytopic.Count > 0)
                {
                    foreach (var lst in lstenquirytopic)
                    {
                        ListId = cientList.Where(s => s.Name.ToLower().Trim() == lst).Select(s => s.ListID).FirstOrDefault();
                        if (!string.IsNullOrEmpty(ListId))
                        {
                            Subscriber subscriber = new Subscriber(auth, ListId);
                            List<SubscriberCustomField> customFields = new List<SubscriberCustomField>();
                            newSubscriberID = subscriber.Add(model.email, (model.fullname), customFields, true, ConsentToTrack.Yes);
                        }
                    }
                }
            }

            try
            {
                if (!string.IsNullOrEmpty(newSubscriberID))
                {
                    res = "successforsubscriberadd";
                }
            }
            catch
            {
                res = "errorforsubscriberadd";
            }
            return res;
        }

        #endregion

        #region MYOB Code
        public IApiConfiguration _configurationCloud;
        public IApiConfiguration _configurationCloudFor_Pk;
        public IOAuthKeyService _oAuthKeyService;
        public IOAuthKeyService _oAuthKeyServiceFor_Pk;
        public const string iApiConfiguration = "MyConfiguration";
        public const string iOAuthKeyService = "MyOAuthKeyService";
        public ActionResult myobAccRight2max(string key = "", string myobid = "", string myobCompany = "", string code = "", string state = "")
        {
            #region Get From WebConfig            
            string redirect_uri = ConfigurationManager.AppSettings["RedirectUri"];
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
            string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
            string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
            myobCompany = ConfigurationManager.AppSettings["MyObCompanyInvoice"];
            #endregion

            List<SaleInvoice> listResponse = new List<SaleInvoice>();

            string[] strParameter = new string[] { };
            _oAuthKeyService = new OAuthKeyService();
            if (Session[iOAuthKeyService] != null)
            {
                _oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
                if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                {
                    _oAuthKeyService.OAuthResponse = null;
                }
            }
            else
            {
                _oAuthKeyService.OAuthResponse = null;
                strParameter = new string[] { key, myobid, myobCompany };

                //_oAuthKeyService.OAuthResponse = null;
            }

            _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);
            SessionManager.MyConfiguration = _configurationCloud;
            if (_oAuthKeyService.OAuthResponse == null || !string.IsNullOrEmpty(code))
            {
                if (!string.IsNullOrEmpty(code))
                {

                    //var oauthService = new OAuthService(_configurationCloud);
                    //_oAuthKeyService.OAuthResponse = oauthService.GetTokens(code);
                    _oAuthKeyService = setSessionToken(code);
                    Session[iOAuthKeyService] = _oAuthKeyService;
                    //SessionManager.MyOAuthKeyService = _oAuthKeyService;
                    if (!string.IsNullOrEmpty(state))
                    {
                        strParameter = state.Split(',');
                        key = strParameter[0];
                        myobid = strParameter[1];
                        myobCompany = strParameter[2];
                    }

                    listResponse = GetSaleInvoiceList(key, myobid, myobCompany);
                }
                else
                {

                    Response.Redirect("https://secure.myob.com/oauth2/account/authorize?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code&scope=CompanyFile&state=" + string.Join(",", strParameter));
                }
            }
            else
            {

                listResponse = GetSaleInvoiceList(key, myobid, myobCompany);
            }

            return View(listResponse);
        }
        public List<SaleInvoice> GetSaleInvoiceList(string companyName, string myobid, string myobCompany)
        {
            string cf_guid = "";
            string cf_uri = "";
            ViewBag.client_id = null;
            ViewBag.myobid = null;
            ViewBag.myobCompany = null;
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string cftoken = CommonMethod.GetEncode("Administrator:");

            SimpleOAuthKeyService keystore = new SimpleOAuthKeyService();
            List<SaleInvoice> listResponse = new List<SaleInvoice>();
            List<CustomerPaymentModel> listCustomerPaymentModel = new List<CustomerPaymentModel>();
            List<DropdownCustomerModel> lstCustomerResponse = new List<DropdownCustomerModel>();


            ViewBag.key = companyName;
            ViewBag.myobid = myobid;
            ViewBag.myobCompany = myobCompany;

            #region Get Comapny Guid Id From MyOb Account
            var cfsCloud = new CompanyFileService(_configurationCloud, null, _oAuthKeyService);
            var cf = cfsCloud.GetRange();
            if (cf != null && cf.Length > 0)
            {
                foreach (var item in cf.ToList())
                {
                    if (item.Name.ToLower().Trim() == myobCompany.ToLower().Trim())
                    {
                        cf_guid = item.Id.ToString();
                        cf_uri = item.Uri.ToString();
                    }

                }
            }
            else
            {

            }

            #endregion

            #region Update MyObId In CRM
            if (!string.IsNullOrEmpty(companyName) && !string.IsNullOrEmpty(myobid))
            {
                string res = EsconaTutorialCRMReference.UpdateMyObIdInCompany(companyName, myobid);
                if (res == "success")
                {
                }
                else if (res == "alreadyexistmyobID")
                {
                }
                else if (res == "invalidcompanyname")
                {
                }
            }
            #endregion

            #region Invoice API CALL
            if (!string.IsNullOrEmpty(myobid))
            {
                JArray JArraySaleInvoice = new JArray();
                int count = 1;

                string SaleInvoicefilter = string.Format("?$filter=Customer/DisplayID eq'{0}' and Status eq 'Open'", myobid);
                JArraySaleInvoice = CommonMethod.MakeAccountRightAPICall(cf_uri + "/Sale/Invoice/" + SaleInvoicefilter + "", _oAuthKeyService.OAuthResponse.AccessToken, client_id, cftoken);
                if (JArraySaleInvoice != null && JArraySaleInvoice.Count > 0)
                {
                    listResponse = JArraySaleInvoice.ToObject<List<SaleInvoice>>();

                    if (listResponse != null && listResponse.Count > 0)
                    {
                        foreach (var i in listResponse)
                        {
                            i.strDate = Convert.ToDateTime(i.Date).ToString("dd/MM/yyyy");
                            if (i.PromisedDate != null)
                            {
                                i.strPromisedDate = Convert.ToDateTime(i.PromisedDate).ToString("dd/MM/yyyy");
                            }
                            if (!string.IsNullOrEmpty(i.Number))
                            {
                                if (count != 0)
                                {
                                    string CustomerPaymentfilter = string.Format("?$filter=Customer/DisplayID eq'{0}' and Invoices/any(x: x/Number eq '{1}')", myobid, i.Number);
                                    var lstCRList = CommonMethod.MakeAccountRightAPICall(cf_uri + "/Sale/CustomerPayment/" + CustomerPaymentfilter + "", _oAuthKeyService.OAuthResponse.AccessToken, client_id, cftoken);
                                    if (lstCRList != null && lstCRList.Count > 0)
                                    {
                                        listCustomerPaymentModel = lstCRList.ToObject<List<CustomerPaymentModel>>();
                                        if (listCustomerPaymentModel != null && listCustomerPaymentModel.Count > 0)
                                        {
                                            foreach (var lstCR in listCustomerPaymentModel)
                                            {
                                                if (lstCR.Date != null)
                                                {
                                                    lstCR.strDate = Convert.ToDateTime(lstCR.Date).ToString("dd/MM/yyyy");
                                                }
                                            }
                                            i.CustomerPaymentModel = listCustomerPaymentModel;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }
            #endregion

            return listResponse;
        }
        public string UpdateIntoCRM(string myobid)
        {
            string res = "sessionout";
            string cf_uri = "";
            #region Get From WebConfig            
            string redirect_uri = ConfigurationManager.AppSettings["RedirectUri"];
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
            string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
            string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
            string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];
            #endregion
            string cf_guid = "";
            string cftoken = CommonMethod.GetEncode("Administrator:");
            _oAuthKeyService = new OAuthKeyService();
            List<CustomerInfo> listCustomerInfoModel = new List<CustomerInfo>();

            _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);

            try
            {
                if (Session[iOAuthKeyService] != null)
                {
                    _oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
                    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                    {
                        _oAuthKeyService.OAuthResponse = null;
                        res = "sessionout";
                    }
                    else
                    {
                        if (IsUpdatedtoCRM)
                        {
                            res = "alreadyupdated";
                        }
                        else
                        {
                            #region Get Comapny Guid Id From MyOb Account
                            var cfsCloud = new CompanyFileService(_configurationCloud, null, _oAuthKeyService);
                            var cf = cfsCloud.GetRange();
                            if (cf != null && cf.Length > 0)
                            {
                                foreach (var item in cf.ToList())
                                {
                                    if (item.Name.ToLower().Trim() == myobCompany.ToLower().Trim())
                                    {
                                        cf_guid = item.Id.ToString();
                                        cf_uri = item.Uri.ToString();
                                    }
                                }
                            }
                            #endregion
                            if (!string.IsNullOrEmpty(cf_guid))
                            {   //var lstCRList = CommonMethod.MakeAccountRightAPICall("https://ar2.api.myob.com/accountright/" + cf_guid + "/Contact/Customer/", _oAuthKeyService.OAuthResponse.AccessToken, client_id, cftoken);
                                var lstCRList = CommonMethod.MakeAccountRightAPICall(cf_uri + "/Contact/Customer/", _oAuthKeyService.OAuthResponse.AccessToken, client_id, cftoken);
                                if (lstCRList != null && lstCRList.Count > 0)
                                {
                                    listCustomerInfoModel = lstCRList.ToObject<List<CustomerInfo>>();
                                    if (listCustomerInfoModel != null && listCustomerInfoModel.Count > 0)
                                    {
                                        listCustomerInfoModel = (from c in listCustomerInfoModel
                                                                 where (c.CompanyName ?? "") != "" && (c.DisplayId ?? "") != ""
                                                                 && (c.CurrentBalance ?? 0) > 0 && (c.SellingDetails.Credit.PastDue ?? 0) > 0
                                                                 select c).ToList();

                                        foreach (var lci in listCustomerInfoModel)
                                        {
                                            string ret = EsconaTutorialCRMReference.UpdateCompanyUDFBasedOnMYOBCustomerID(lci);
                                            if (ret == "success")
                                            {
                                            }
                                        }
                                    }
                                }
                                //IsUpdatedtoCRM = true;
                                res = "sucess";
                            }
                        }
                    }
                }
                else
                {
                    res = "sessionout";
                    _oAuthKeyService.OAuthResponse = null;
                }
            }
            catch (Exception ex)
            {

            }
            return res;
        }
        #endregion

        #region MYOB 
        public ActionResult SyncMyOb2CRM(string code = "")
        {
            //#region Update Based On Previous Day
            //ViewBag.synPreviousDayProcessStatus = "False";
            //if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"])))
            //{
            //    XmlDocument doc = new XmlDocument();
            //    doc.Load(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"]));
            //    XmlNode myNode_Status = doc.SelectSingleNode("/Process/Status");
            //    ViewBag.synPreviousDayProcessStatus= myNode_Status.InnerText;
            //}
            //#endregion

            #region Get From WebConfig            
            string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUri"];
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
            string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
            string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
            string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];
            int expirein = Convert.ToInt32(ConfigurationManager.AppSettings["expirein"]);
            #endregion

            List<SaleInvoice> listResponse = new List<SaleInvoice>();

            string[] strParameter = new string[] { };
            _oAuthKeyService = new OAuthKeyService();
            if (Request.Cookies["AccessToken"] != null)
            {
                //if (Session[iOAuthKeyService] != null)
                //{
                //_oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
                //sb.Clear();
                //sb.AppendLine("=====Current Session================");
                //sb.AppendLine("New Token Generated");
                //sb.AppendLine("Current Expired time: " + _oAuthKeyService.OAuthResponse.ReceivedTime + "");
                //sb.AppendLine("ExpiresIn: " + _oAuthKeyService.OAuthResponse.ExpiresIn + "");
                //sb.AppendLine("HasExpired: " + _oAuthKeyService.OAuthResponse.HasExpired + "");
                //sb.AppendLine("=====================");
                //CommonMethod.LogFile(sb, false);
                //if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                //{
                _oAuthKeyService.OAuthResponse = null;
                //_oAuthKeyService = CommonMethod.RefreshToken();
                //_oAuthKeyService.OAuthResponse = null;
                //}
            }
            else
            {
                _oAuthKeyService.OAuthResponse = null;
            }

            _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);
            if (_oAuthKeyService.OAuthResponse == null || !string.IsNullOrEmpty(code))
            {
                if (!string.IsNullOrEmpty(code))
                {

                    _oAuthKeyService = setSessionToken(code);
                    //_oAuthKeyService.OAuthResponse.ReceivedTime =  _oAuthKeyService.OAuthResponse.ReceivedTime.AddMinutes(-5);
                    _oAuthKeyService.OAuthResponse.ExpiresIn = expirein;
                    //Session[iOAuthKeyService] = _oAuthKeyService;


                    //CommonMethod.LogFile(sb, false);
                }
                else
                {

                    Response.Redirect("https://secure.myob.com/oauth2/account/authorize?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code&scope=CompanyFile&state=" + string.Join(",", strParameter));
                }
            }
            else
            {

            }
            return View();
        }
        public ActionResult SyncMyOb2CrmDateFilter(string code = "")
        {
            //#region Update Based On Previous Day
            //ViewBag.synPreviousDayProcessStatus = "False";
            //if (System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"])))
            //{
            //    XmlDocument doc = new XmlDocument();
            //    doc.Load(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"]));
            //    XmlNode myNode_Status = doc.SelectSingleNode("/Process/Status");
            //    ViewBag.synPreviousDayProcessStatus= myNode_Status.InnerText;
            //}
            //#endregion

            #region Get From WebConfig            
            string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRMDateFilter_RedirectUri"];
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
            string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
            string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
            string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];
            int expirein = Convert.ToInt32(ConfigurationManager.AppSettings["expirein"]);
            #endregion

            List<SaleInvoice> listResponse = new List<SaleInvoice>();

            string[] strParameter = new string[] { };
            _oAuthKeyService = new OAuthKeyService();
            if (Request.Cookies["AccessToken"] != null)
            {
                //if (Session[iOAuthKeyService] != null)
                //{
                //_oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
                //sb.Clear();
                //sb.AppendLine("=====Current Session================");
                //sb.AppendLine("New Token Generated");
                //sb.AppendLine("Current Expired time: " + _oAuthKeyService.OAuthResponse.ReceivedTime + "");
                //sb.AppendLine("ExpiresIn: " + _oAuthKeyService.OAuthResponse.ExpiresIn + "");
                //sb.AppendLine("HasExpired: " + _oAuthKeyService.OAuthResponse.HasExpired + "");
                //sb.AppendLine("=====================");
                //CommonMethod.LogFile(sb, false);
                //if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                //{
                _oAuthKeyService.OAuthResponse = null;
                //_oAuthKeyService = CommonMethod.RefreshToken();
                //_oAuthKeyService.OAuthResponse = null;
                //}
            }
            else
            {
                _oAuthKeyService.OAuthResponse = null;
            }

            _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);
            if (_oAuthKeyService.OAuthResponse == null || !string.IsNullOrEmpty(code))
            {
                if (!string.IsNullOrEmpty(code))
                {

                    _oAuthKeyService = setSessionToken(code);
                    //_oAuthKeyService.OAuthResponse.ReceivedTime =  _oAuthKeyService.OAuthResponse.ReceivedTime.AddMinutes(-5);
                    _oAuthKeyService.OAuthResponse.ExpiresIn = expirein;
                    //Session[iOAuthKeyService] = _oAuthKeyService;


                    //CommonMethod.LogFile(sb, false);
                }
                else
                {

                    Response.Redirect("https://secure.myob.com/oauth2/account/authorize?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code&scope=CompanyFile&state=" + string.Join(",", strParameter));
                }
            }
            else
            {

            }
            return View();
        }
        public IOAuthKeyService setSessionToken(string code = "")
        {
            if (!string.IsNullOrEmpty(code))
            {
                var oauthService = new OAuthService(_configurationCloud);
                _oAuthKeyService.OAuthResponse = oauthService.GetTokens(code);

                HttpCookie cookie_AccessToken = new HttpCookie("AccessToken", _oAuthKeyService.OAuthResponse.AccessToken);
                HttpCookie cookie_RefreshToken = new HttpCookie("RefreshToken", _oAuthKeyService.OAuthResponse.RefreshToken);
                HttpCookie cookie_ReceivedTime = new HttpCookie("ReceivedTime", _oAuthKeyService.OAuthResponse.ReceivedTime.ToString());
                cookie_AccessToken.Expires = DateTime.Now.AddHours(3);
                cookie_RefreshToken.Expires = DateTime.Now.AddHours(3);
                cookie_ReceivedTime.Expires = DateTime.Now.AddHours(3);
                Response.Cookies.Add(cookie_AccessToken);
                Response.Cookies.Add(cookie_RefreshToken);
                Response.Cookies.Add(cookie_ReceivedTime);
                return _oAuthKeyService;
            }
            else
            {
                return null;
            }
        }

        public ActionResult GetPartialCustomerInfo(bool allowProcess = false, bool FilterData = false, bool Refresh = false, bool IsBasedOnCompanyNameMatch = false, bool IsBasedOnMyObIdMatch = false, string FilterDate = "", string FilterCustomerName = "", string FilterCustomerNameList = "", bool SyncdForToday = false)
        {
            try
            {

                //f8ac4516-dbca-4090-9745-9716bfab057c
                #region Get From WebConfig            
                //string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUri"];
                string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
                string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
                string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
                string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
                string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];


                int companynameupdatecnt = 0;
                int companycreatecnt = 0;
                int myobupdatecnt = 0;
                int myobalreadyexistcnt = 0;
                int myobnotfoundcnt = 0;
                int companynotfoundcnt = 0;
                int udfdatafieldsupdatecnt = 0;
                int Customercnt = 0;
                #endregion
                string cf_guid = "";
                string cf_uri = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717";
                string FromDate = "";
                string ToDate = "";
                StringBuilder sb = new StringBuilder();
                int errorcnt = 0;
                List<string> lstErrorComp = new List<string>();
                string cftoken = CommonMethod.GetEncode("Administrator:");
                HttpCookie cookie_AccessToken = Request.Cookies["AccessToken"];
                List<CustomerInfo> listCustomerInfoModel = new List<CustomerInfo>();

                if (!string.IsNullOrEmpty(cf_uri))
                {
                    //Refresh = false;
                    //IsBasedOnCompanyNameMatch = false;
                    //IsBasedOnMyObIdMatch = false;
                    cftoken = "";
                    string filter = "";
                    string filterBody = "";
                    if (!string.IsNullOrEmpty(FilterDate))
                    {
                        if (FilterDate.Contains("-"))
                        {
                            string[] strFilterDateArry = FilterDate.Split('-');
                            if (strFilterDateArry != null && strFilterDateArry.Length > 0)
                            {
                                FromDate = Convert.ToDateTime(strFilterDateArry[0]).ToString("yyyy-MM-dd");
                                ToDate = Convert.ToDateTime(strFilterDateArry[1]).ToString("yyyy-MM-dd");
                                filterBody += "LastModified ge datetime'" + FromDate + "' and LastModified le datetime'" + ToDate + "'";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(FilterCustomerName))
                    {
                        if (!string.IsNullOrEmpty(filterBody))
                        {
                            filterBody += " and ";
                        }
                        if (FilterCustomerName.Contains("'"))
                        {
                            FilterCustomerName = FilterCustomerName.Replace("'", "''");
                        }
                        filterBody += "substringof(tolower('" + HttpUtility.UrlEncode(FilterCustomerName) + "'), tolower(CompanyName)) eq true";
                    }
                    if (!string.IsNullOrEmpty(filterBody))
                    {
                        filter = "?$filter=" + filterBody + "&$top=1000";
                    }
                    List<string> strCompanyNameFilter = new List<string>();
                    ViewBag.TodayCompanyName = "";
                    #region Today Data Sync ONly
                    if (SyncdForToday)
                    {
                        string TodayDate = DateTime.UtcNow.AddHours(10).ToString("yyyy-MM-dd");
                        string strUrl = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/GeneralLedger/JournalTransaction/?$filter=DateOccurred ge datetime'" + TodayDate + "' and DateOccurred le datetime'" + TodayDate + "'";
                        List<GeneralLedgerJournalTransaction.Item> listjournaltransactionmodel = new List<GeneralLedgerJournalTransaction.Item>();
                        //cookie_AccessToken.Value
                        var jt = CommonMethod.MakeAccountRightAPICall(strUrl, cookie_AccessToken.Value, client_id, "");
                        if (jt != null && jt.Count > 0)
                        {
                            listjournaltransactionmodel = jt.ToObject<List<GeneralLedgerJournalTransaction.Item>>();
                            if (listjournaltransactionmodel != null && listjournaltransactionmodel.Count > 0)
                            {
                                foreach (var item in listjournaltransactionmodel)
                                {
                                    CustomerSaleInvoiceItem objInvModel = new CustomerSaleInvoiceItem();
                                    CustomerInfo objCustinfomodel = new CustomerInfo();
                                    if (item.JournalType != null)
                                    {
                                        if (item.JournalType == "Sale")
                                        {
                                            strUrl = item.SourceTransaction.Uri.ToString();
                                            var obj = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, cookie_AccessToken.Value, client_id, "");
                                            if (obj != null)
                                            {
                                                objInvModel = obj.ToObject<CustomerSaleInvoiceItem>();
                                                if (objInvModel.Customer != null)
                                                {
                                                    if (objInvModel.Customer.Name != null)
                                                    {
                                                        var existName = (from c in strCompanyNameFilter
                                                                         where c.ToLower().Trim() == objInvModel.Customer.Name.ToLower().Trim()
                                                                         select c).FirstOrDefault();
                                                        if (existName == null)
                                                        {
                                                            strCompanyNameFilter.Add(objInvModel.Customer.Name);
                                                            objCustinfomodel.CompanyName = objInvModel.Customer.Name;
                                                            objCustinfomodel.Uid = objInvModel.Customer.UID;
                                                            listCustomerInfoModel.Add(objCustinfomodel);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        ToDate = Convert.ToDateTime(TodayDate).AddDays(1).ToString("yyyy-MM-dd");
                        strUrl = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/Sale/Order?$filter=Date ge datetime'" + TodayDate + "T00:00:00' and Date le datetime'" + ToDate + "T00:00:00'&$top=1000";

                        List<EnquiryInsertToCRM.Models.Item> lstCustomerSaleInvoice = new List<EnquiryInsertToCRM.Models.Item>();
                        var itemCustSaleInv = CommonMethod.MakeAccountRightAPICall(strUrl, cookie_AccessToken.Value, client_id, "");
                        if (itemCustSaleInv != null && itemCustSaleInv.Count > 0)
                        {
                            lstCustomerSaleInvoice = itemCustSaleInv.ToObject<List<EnquiryInsertToCRM.Models.Item>>();
                            if (lstCustomerSaleInvoice != null && lstCustomerSaleInvoice.Count > 0)
                            {
                                foreach (var item in lstCustomerSaleInvoice)
                                {
                                    CustomerSaleInvoiceItem objInvModel = new CustomerSaleInvoiceItem();
                                    CustomerInfo objCustinfomodel = new CustomerInfo();
                                    if (item != null)
                                    {
                                        if (item.Customer != null)
                                        {
                                            if (item.Customer.Name != null)
                                            {
                                                var existName = (from c in strCompanyNameFilter
                                                                 where c.ToLower().Trim() == item.Customer.Name.ToLower().Trim()
                                                                 select c).FirstOrDefault();
                                                if (existName == null)
                                                {
                                                    strCompanyNameFilter.Add(item.Customer.Name);
                                                    objCustinfomodel.CompanyName = item.Customer.Name;
                                                    objCustinfomodel.Uid = item.Customer.UID;
                                                    listCustomerInfoModel.Add(objCustinfomodel);
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        if (strCompanyNameFilter != null && strCompanyNameFilter.Count > 0)
                        {
                            ViewBag.TodayCompanyName = string.Join(",", strCompanyNameFilter);
                        }
                    }
                    #endregion
                    if (SyncdForToday == false)
                    {
                        var lstCRList = CommonMethod.MakeAccountRightAPICall(cf_uri + "/Contact/Customer/", cookie_AccessToken.Value, client_id, cftoken, filter);
                        if (lstCRList != null && lstCRList.Count > 0)
                        {
                            listCustomerInfoModel = lstCRList.ToObject<List<CustomerInfo>>();
                            if (listCustomerInfoModel != null && listCustomerInfoModel.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(FilterCustomerNameList))
                                {
                                    FilterCustomerNameList = FilterCustomerNameList.Replace("[REPLACEANDCHAR]", "&");
                                    if (FilterCustomerNameList.Contains("\\n"))
                                    {

                                        FilterCustomerNameList = FilterCustomerNameList.Replace(@"""", @"\""").Replace(@"\""", @"").Replace("\\n", "\n");

                                        strCompanyNameFilter = FilterCustomerNameList.Split('\n').ToList();

                                    }
                                    else if (FilterCustomerNameList.Contains(","))
                                    {
                                        strCompanyNameFilter = FilterCustomerNameList.Split(',').ToList();
                                    }
                                    else
                                    {
                                        strCompanyNameFilter.Add(FilterCustomerNameList.Replace(@"""", @"\""").Replace(@"\""", @""));
                                    }
                                }
                                if (strCompanyNameFilter != null && strCompanyNameFilter.Count > 0)
                                {
                                    listCustomerInfoModel = (from c in listCustomerInfoModel
                                                             where strCompanyNameFilter.Contains(c.CompanyName)
                                                             select c).ToList();
                                    //var  listCustomerInfoModel1 = listCustomerInfoModel.Where(c => !strCompanyNameFilter.Any(folder => c.CompanyName.ToLower().Trim().Contains(folder))).ToList();

                                }
                                else
                                {
                                    listCustomerInfoModel = (from c in listCustomerInfoModel
                                                                 //where (c.CompanyName ?? "") != "" && (c.DisplayId ?? "") != ""
                                                             select c).ToList();
                                }


                                Customercnt = listCustomerInfoModel.Count;
                                ViewBag.Customercnt = listCustomerInfoModel.Count;
                                if (allowProcess)
                                {
                                    if (Refresh || IsBasedOnCompanyNameMatch || IsBasedOnMyObIdMatch)
                                    {
                                        sb.Clear();
                                        sb.AppendLine("=================================================================");
                                        sb.AppendLine("allowProcess:" + allowProcess);
                                        sb.AppendLine("FilterCustomerName:" + FilterCustomerName);
                                        sb.AppendLine("Date:" + DateTime.UtcNow.AddHours(10));
                                        CommonMethod.LogFile(sb, false);
                                        //Refresh = false;
                                        //IsBasedOnCompanyNameMatch = false;
                                        //IsBasedOnMyObIdMatch = false;
                                        string MYOBUID_UDF = RigonCRMReference.getMyOBUid_UDFKey();
                                        int counter = 0;
                                        foreach (var lci in listCustomerInfoModel)
                                        {
                                            counter++;
                                            CommonMethod.RefreshToken_BasedOnCookies();
                                            #region Update MyObId In CRM Based On Company Name Match
                                            if (!string.IsNullOrEmpty(lci.CompanyName) && !string.IsNullOrEmpty(lci.Uid))
                                            {
                                                string res = "";
                                                if (Refresh)
                                                {
                                                    string checkCompExistOrNot = CreateCompanyInCRM(lci);
                                                    if (checkCompExistOrNot == "companycreate")
                                                    {
                                                        companycreatecnt += 1;
                                                    }
                                                    else
                                                    {
                                                        res = RigonCRMReference.UpdateCompanyNameOrMyObIdInCRM(lci.CompanyName, lci.Uid);
                                                    }
                                                }
                                                else if (IsBasedOnCompanyNameMatch)
                                                {
                                                    string checkCompExistOrNot = CreateCompanyInCRM(lci);
                                                    if (checkCompExistOrNot == "companycreate")
                                                    {
                                                        companycreatecnt += 1;
                                                    }
                                                    else
                                                    {
                                                        res = RigonCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, MYOBUID_UDF);
                                                    }
                                                }
                                                else if (IsBasedOnMyObIdMatch)
                                                {
                                                    string checkCompExistOrNot = CreateCompanyInCRM(lci);
                                                    if (checkCompExistOrNot == "companycreate")
                                                    {
                                                        companycreatecnt += 1;
                                                    }
                                                    else
                                                    {
                                                        var str1 = RigonCRMReference.UpdateCompanyNameOrMyObIdInCRM(lci.CompanyName, lci.Uid);
                                                        if (str1 == "alreadyexistmyobID")
                                                        {
                                                            RigonCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, MYOBUID_UDF);
                                                        }
                                                        //RigonCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, MYOBUID_UDF);
                                                    }
                                                    res = RigonCRMReference.UpdateCompanyUDFBasedOnMYOBCustomerID(lci, cf_uri, cookie_AccessToken.Value, client_id, cftoken);
                                                    if (res == "success")
                                                    {
                                                        res = "udfdatafieldsupdate";
                                                    }
                                                    else if (res == "sessionexpired")
                                                    {
                                                        res = "sessionexpired";
                                                    }
                                                    else if (res == "error")
                                                    {
                                                        res = "error";
                                                        errorcnt += 1;
                                                        lstErrorComp.Add(lci.CompanyName);
                                                    }
                                                    else
                                                    {
                                                        res = "myobnotfound";
                                                    }
                                                }
                                                if (res == "companynameupdate")
                                                {
                                                    companynameupdatecnt += 1;
                                                }
                                                else if (res == "myobupdate")
                                                {
                                                    myobupdatecnt += 1;
                                                }
                                                else
                                                if (res == "alreadyexistmyobID")
                                                {
                                                    myobalreadyexistcnt += 1;
                                                }
                                                else
                                                if (res == "invalidcompanyname")
                                                {
                                                    companynotfoundcnt += 1;
                                                }
                                                else
                                                if (res == "udfdatafieldsupdate")
                                                {
                                                    udfdatafieldsupdatecnt += 1;
                                                }
                                                else
                                                if (res == "myobnotfound")
                                                {
                                                    myobnotfoundcnt += 1;
                                                }
                                            }
                                            #endregion

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Request.IsAjaxRequest())
                {
                    if (FilterData)
                    {
                        return PartialView(listCustomerInfoModel);
                    }
                    string strErrorCompList = "";
                    if (lstErrorComp != null && lstErrorComp.Count > 0)
                    {
                        strErrorCompList = string.Join(", ", lstErrorComp);
                    }
                    var jsonData = new
                    {
                        CustomerInfocnt = Customercnt,
                        companynameupdate = companynameupdatecnt,
                        companycreate = companycreatecnt,
                        myobupdate = myobupdatecnt,
                        myobalreadyexist = myobalreadyexistcnt,
                        companynotfound = companynotfoundcnt,
                        myobnotfound = myobnotfoundcnt,
                        udfdatafieldsupdate = udfdatafieldsupdatecnt,
                        errorCompcnt = errorcnt,
                        errorCompList = strErrorCompList
                    };

                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                return PartialView(listCustomerInfoModel);
            }
            catch (Exception)
            {

                throw;
            }
        }
        public ActionResult GetPartialCustomerInfoDateFilter(bool allowProcess = false, bool FilterData = false, bool Refresh = false, bool IsBasedOnCompanyNameMatch = false, bool IsBasedOnMyObIdMatch = false, string FilterDate = "", string FilterCustomerName = "", string FilterCustomerNameList = "", bool SyncdForToday = false)
        {
            try
            {

                //f8ac4516-dbca-4090-9745-9716bfab057c
                #region Get From WebConfig            
                //string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUri"];
                string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
                string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
                string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
                string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
                string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];


                int companynameupdatecnt = 0;
                int companycreatecnt = 0;
                int myobupdatecnt = 0;
                int myobalreadyexistcnt = 0;
                int myobnotfoundcnt = 0;
                int companynotfoundcnt = 0;
                int udfdatafieldsupdatecnt = 0;
                
                int Customercnt = 0;
                #endregion
                string cf_guid = "";
                string cf_uri = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717";
                string FromDate = "";
                string ToDate = "";
                DateTime dtFromDate = DateTime.Now;
                DateTime dtToDate = DateTime.Now;
                StringBuilder sb = new StringBuilder();
                int errorcnt = 0;
                List<string> lstErrorComp = new List<string>();

                string cftoken = CommonMethod.GetEncode("Administrator:");
                HttpCookie cookie_AccessToken = Request.Cookies["AccessToken"];
                List<CustomerInfo> listCustomerInfoModel = new List<CustomerInfo>();

                if (!string.IsNullOrEmpty(cf_uri))
                {
                    //Refresh = false;
                    //IsBasedOnCompanyNameMatch = false;
                    //IsBasedOnMyObIdMatch = false;
                    cftoken = "";
                    string filter = "";
                    string filterBody = "";
                    if (!string.IsNullOrEmpty(FilterDate))
                    {
                        if (FilterDate.Contains("-"))
                        {
                            string[] strFilterDateArry = FilterDate.Split('-');
                            if (strFilterDateArry != null && strFilterDateArry.Length > 0)
                            {
                                FromDate = Convert.ToDateTime(strFilterDateArry[0]).ToString("yyyy-MM-dd");
                                dtFromDate = Convert.ToDateTime(strFilterDateArry[0]);
                                ToDate = Convert.ToDateTime(strFilterDateArry[1]).ToString("yyyy-MM-dd");
                                dtToDate = Convert.ToDateTime(strFilterDateArry[1]);
                            }
                        }
                    }
                    else
                    {
                        if (SyncdForToday)
                        {
                            return PartialView(listCustomerInfoModel);
                        }
                    }

                    if (!string.IsNullOrEmpty(FilterCustomerName))
                    {
                        if (!string.IsNullOrEmpty(filterBody))
                        {
                            filterBody += " and ";
                        }
                        if (FilterCustomerName.Contains("'"))
                        {
                            FilterCustomerName = FilterCustomerName.Replace("'", "''");
                        }
                        filterBody += "substringof(tolower('" + HttpUtility.UrlEncode(FilterCustomerName) + "'), tolower(CompanyName)) eq true";
                    }
                    if (!string.IsNullOrEmpty(filterBody))
                    {
                        filter = "?$filter=" + filterBody + "&$top=1000";
                    }
                    List<string> strCompanyNameFilter = new List<string>();
                    ViewBag.TodayCompanyName = "";
                    #region Today Data Sync ONly
                    if (SyncdForToday)
                    {
                        string strUrl = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/GeneralLedger/JournalTransaction/?$filter=DateOccurred ge datetime'" + FromDate + "' and DateOccurred le datetime'" + ToDate + "'";
                        List<GeneralLedgerJournalTransaction.Item> listjournaltransactionmodel = new List<GeneralLedgerJournalTransaction.Item>();
                        var jt = CommonMethod.MakeAccountRightAPICall(strUrl, cookie_AccessToken.Value, client_id, "");
                        if (jt != null && jt.Count > 0)
                        {
                            listjournaltransactionmodel = jt.ToObject<List<GeneralLedgerJournalTransaction.Item>>();
                            if (listjournaltransactionmodel != null && listjournaltransactionmodel.Count > 0)
                            {
                                foreach (var item in listjournaltransactionmodel)
                                {
                                    CustomerSaleInvoiceItem objInvModel = new CustomerSaleInvoiceItem();
                                    CustomerInfo objCustinfomodel = new CustomerInfo();
                                    if (item.JournalType != null)
                                    {
                                        if (item.JournalType == "Sale")
                                        {
                                            strUrl = item.SourceTransaction.Uri.ToString();
                                            var obj = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, cookie_AccessToken.Value, client_id, "");
                                            if (obj != null)
                                            {
                                                objInvModel = obj.ToObject<CustomerSaleInvoiceItem>();
                                                if (objInvModel.Customer != null)
                                                {
                                                    if (objInvModel.Customer.Name != null)
                                                    {
                                                        var existName = (from c in strCompanyNameFilter
                                                                         where c.ToLower().Trim() == objInvModel.Customer.Name.ToLower().Trim()
                                                                         select c).FirstOrDefault();
                                                        if (existName == null)
                                                        {
                                                            strCompanyNameFilter.Add(objInvModel.Customer.Name);
                                                            objCustinfomodel.CompanyName = objInvModel.Customer.Name;
                                                            objCustinfomodel.Uid = objInvModel.Customer.UID;
                                                            listCustomerInfoModel.Add(objCustinfomodel);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        ToDate = Convert.ToDateTime(dtToDate).AddDays(1).ToString("yyyy-MM-dd");
                        strUrl = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/Sale/Order?$filter=Date ge datetime'" + FromDate + "T00:00:00' and Date le datetime'" + ToDate + "T00:00:00'&$top=1000";

                        List<EnquiryInsertToCRM.Models.Item> lstCustomerSaleInvoice = new List<EnquiryInsertToCRM.Models.Item>();
                        var itemCustSaleInv = CommonMethod.MakeAccountRightAPICall(strUrl, cookie_AccessToken.Value, client_id, "");
                        if (itemCustSaleInv != null && itemCustSaleInv.Count > 0)
                        {
                            lstCustomerSaleInvoice = itemCustSaleInv.ToObject<List<EnquiryInsertToCRM.Models.Item>>();
                            if (lstCustomerSaleInvoice != null && lstCustomerSaleInvoice.Count > 0)
                            {
                                foreach (var item in lstCustomerSaleInvoice)
                                {
                                    CustomerSaleInvoiceItem objInvModel = new CustomerSaleInvoiceItem();
                                    CustomerInfo objCustinfomodel = new CustomerInfo();
                                    if (item != null)
                                    {
                                        if (item.Customer != null)
                                        {
                                            if (item.Customer.Name != null)
                                            {
                                                var existName = (from c in strCompanyNameFilter
                                                                 where c.ToLower().Trim() == item.Customer.Name.ToLower().Trim()
                                                                 select c).FirstOrDefault();
                                                if (existName == null)
                                                {
                                                    strCompanyNameFilter.Add(item.Customer.Name);
                                                    objCustinfomodel.CompanyName = item.Customer.Name;
                                                    objCustinfomodel.Uid = item.Customer.UID;
                                                    listCustomerInfoModel.Add(objCustinfomodel);
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                        if (strCompanyNameFilter != null && strCompanyNameFilter.Count > 0)
                        {
                            ViewBag.TodayCompanyName = string.Join(",", strCompanyNameFilter);
                        }
                    }
                    #endregion
                    if (SyncdForToday == false)
                    {
                        var lstCRList = CommonMethod.MakeAccountRightAPICall(cf_uri + "/Contact/Customer/", cookie_AccessToken.Value, client_id, cftoken, filter);
                        if (lstCRList != null && lstCRList.Count > 0)
                        {
                            listCustomerInfoModel = lstCRList.ToObject<List<CustomerInfo>>();
                            if (listCustomerInfoModel != null && listCustomerInfoModel.Count > 0)
                            {
                                if (!string.IsNullOrEmpty(FilterCustomerNameList))
                                {
                                    FilterCustomerNameList = FilterCustomerNameList.Replace("[REPLACEANDCHAR]", "&");
                                    if (FilterCustomerNameList.Contains("\\n"))
                                    {

                                        FilterCustomerNameList = FilterCustomerNameList.Replace(@"""", @"\""").Replace(@"\""", @"").Replace("\\n", "\n");

                                        strCompanyNameFilter = FilterCustomerNameList.Split('\n').ToList();

                                    }
                                    else if (FilterCustomerNameList.Contains(","))
                                    {
                                        strCompanyNameFilter = FilterCustomerNameList.Split(',').ToList();
                                    }
                                    else
                                    {
                                        strCompanyNameFilter.Add(FilterCustomerNameList.Replace(@"""", @"\""").Replace(@"\""", @""));
                                    }
                                }
                                if (strCompanyNameFilter != null && strCompanyNameFilter.Count > 0)
                                {
                                    listCustomerInfoModel = (from c in listCustomerInfoModel
                                                             where strCompanyNameFilter.Contains(c.CompanyName)
                                                             select c).ToList();
                                    //var  listCustomerInfoModel1 = listCustomerInfoModel.Where(c => !strCompanyNameFilter.Any(folder => c.CompanyName.ToLower().Trim().Contains(folder))).ToList();

                                }
                                else
                                {
                                    listCustomerInfoModel = (from c in listCustomerInfoModel
                                                                 //where (c.CompanyName ?? "") != "" && (c.DisplayId ?? "") != ""
                                                             select c).ToList();
                                }


                                Customercnt = listCustomerInfoModel.Count;
                                ViewBag.Customercnt = listCustomerInfoModel.Count;
                                if (allowProcess)
                                {
                                    if (Refresh || IsBasedOnCompanyNameMatch || IsBasedOnMyObIdMatch)
                                    {
                                        sb.Clear();
                                        sb.AppendLine("=================================================================");
                                        sb.AppendLine("allowProcess:" + allowProcess);
                                        sb.AppendLine("FilterCustomerName:" + FilterCustomerName);
                                        sb.AppendLine("Date:" + DateTime.UtcNow.AddHours(10));
                                        CommonMethod.LogFile(sb, false);
                                        //Refresh = false;
                                        //IsBasedOnCompanyNameMatch = false;
                                        //IsBasedOnMyObIdMatch = false;
                                        string MYOBUID_UDF = RigonCRMReference.getMyOBUid_UDFKey();
                                        int counter = 0;
                                        foreach (var lci in listCustomerInfoModel)
                                        {
                                            counter++;
                                            CommonMethod.RefreshToken_BasedOnCookies();
                                            #region Update MyObId In CRM Based On Company Name Match
                                            if (!string.IsNullOrEmpty(lci.CompanyName) && !string.IsNullOrEmpty(lci.Uid))
                                            {
                                                string res = "";
                                                if (Refresh)
                                                {
                                                    string checkCompExistOrNot = CreateCompanyInCRM(lci);
                                                    if (checkCompExistOrNot == "companycreate")
                                                    {
                                                        companycreatecnt += 1;
                                                    }
                                                    else
                                                    {
                                                        res = RigonCRMReference.UpdateCompanyNameOrMyObIdInCRM(lci.CompanyName, lci.Uid);
                                                    }
                                                }
                                                else if (IsBasedOnCompanyNameMatch)
                                                {
                                                    string checkCompExistOrNot = CreateCompanyInCRM(lci);
                                                    if (checkCompExistOrNot == "companycreate")
                                                    {
                                                        companycreatecnt += 1;
                                                    }
                                                    else
                                                    {
                                                        res = RigonCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, MYOBUID_UDF);
                                                    }
                                                }
                                                else if (IsBasedOnMyObIdMatch)
                                                {
                                                    string checkCompExistOrNot = CreateCompanyInCRM(lci);
                                                    if (checkCompExistOrNot == "companycreate")
                                                    {
                                                        companycreatecnt += 1;
                                                    }
                                                    else
                                                    {
                                                        var str1 = RigonCRMReference.UpdateCompanyNameOrMyObIdInCRM(lci.CompanyName, lci.Uid);
                                                        if (str1 == "alreadyexistmyobID")
                                                        {
                                                            RigonCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, MYOBUID_UDF);
                                                        }
                                                        //RigonCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, MYOBUID_UDF);
                                                    }
                                                    res = RigonCRMReference.UpdateCompanyUDFBasedOnMYOBCustomerID(lci, cf_uri, cookie_AccessToken.Value, client_id, cftoken);
                                                    if (res == "success")
                                                    {
                                                        res = "udfdatafieldsupdate";
                                                    }
                                                    else if (res == "sessionexpired")
                                                    {
                                                        res = "sessionexpired";
                                                    }
                                                    else if (res == "error")
                                                    {
                                                        res = "error";
                                                        errorcnt += 1; 
                                                        lstErrorComp.Add(lci.CompanyName);
                                                    }
                                                    else
                                                    {
                                                        res = "myobnotfound";
                                                    }
                                                }

                                                if (res == "companynameupdate")
                                                {
                                                    companynameupdatecnt += 1;
                                                }
                                                else if (res == "myobupdate")
                                                {
                                                    myobupdatecnt += 1;
                                                }
                                                else
                                                if (res == "alreadyexistmyobID")
                                                {
                                                    myobalreadyexistcnt += 1;
                                                }
                                                else
                                                if (res == "invalidcompanyname")
                                                {
                                                    companynotfoundcnt += 1;
                                                }
                                                else
                                                if (res == "udfdatafieldsupdate")
                                                {
                                                    udfdatafieldsupdatecnt += 1;
                                                }
                                                else
                                                if (res == "myobnotfound")
                                                {
                                                    myobnotfoundcnt += 1;
                                                }
                                            }
                                            #endregion

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                if (Request.IsAjaxRequest())
                {
                    if (FilterData)
                    {
                        return PartialView(listCustomerInfoModel);
                    }
                    string strErrorCompList = "";
                    if (lstErrorComp != null && lstErrorComp.Count > 0)
                    {
                        strErrorCompList = string.Join(", ", lstErrorComp);
                    }
                    var jsonData = new
                    {
                        CustomerInfocnt = Customercnt,
                        companynameupdate = companynameupdatecnt,
                        companycreate = companycreatecnt,
                        myobupdate = myobupdatecnt,
                        myobalreadyexist = myobalreadyexistcnt,
                        companynotfound = companynotfoundcnt,
                        myobnotfound = myobnotfoundcnt,
                        udfdatafieldsupdate = udfdatafieldsupdatecnt,
                        errorCompcnt = errorcnt,
                        errorCompList = strErrorCompList
                    };

                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                return PartialView(listCustomerInfoModel);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string CreateCompanyInCRM(CustomerInfo objCustomerInfo)
        {

            string response = "alreadyexist";
            AbEntryKeyModel model = new AbEntryKeyModel();
            model = RigonCRMReference.ReadExistingCompanyNameOrMYOBID(objCustomerInfo.CompanyName, "");
            if (string.IsNullOrEmpty(model.Key))
            {
                string key = RigonCRMReference.CreateCompany(objCustomerInfo);
                if (!string.IsNullOrEmpty(key))
                {
                    response = "companycreate";
                }
            }

            //CommonMethod.LogFile(sb, false);
            return response;
        }

        //public bool IsExpiredToken()
        //{
        //    _oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
        //    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public IOAuthKeyService RefreshToken()
        //{
        //    

        //    string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUri"];
        //    string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
        //    string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
        //    string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
        //    string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
        //    string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];

        //    _oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
        //    _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);
        //    var oauthService = new OAuthService(_configurationCloud);
        //    _oAuthKeyService.OAuthResponse = oauthService.RenewTokens(_oAuthKeyService.OAuthResponse);

        //    sb.Clear();
        //    sb.AppendLine("");
        //    sb.AppendLine("");
        //    sb.AppendLine("#1  Call Refresh Token Fn" + DateTime.Now + "");
        //    sb.AppendLine("");
        //    sb.AppendLine("New Access Token: " + _oAuthKeyService.OAuthResponse.AccessToken);
        //    sb.AppendLine("");
        //    sb.AppendLine("New Refresh Token: " + _oAuthKeyService.OAuthResponse.RefreshToken);
        //    sb.AppendLine("");
        //    sb.AppendLine("");

        //    CommonMethod.LogFile(sb, false);
        //    return _oAuthKeyService;
        //}

        #endregion

        public void testData(string str)
        {
            JArray lstSaleInvoice = new JArray();
            List<string> strCompanyNameFilter = new List<string>();
            List<CustomerInfo> listCustomerInfoModel = new List<CustomerInfo>();
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string strToken = "AAEAAGwAk2VYCCwzx2d6-FOtKKXEbfTfLiIv61x6Jh1RxmC5uA7lK5ZiP2SjFQA2A6-m-nbYY1fVrwOMm0Fxba7HBPPL7ONKROEANgdEr4qcLCeFxkp6x64eBLiUDPfoDtiW9NH8UWlQpSw_TsJWkgAB9_65RWhiMSsCI6zP_nNT41fCLF0ynIjJ08QJmS30uGvrkjzbmepzAupJ3kAaHa_JbNzxPhLLsfE-v1VgqaXdIHV_kWChHsC7nUgmBasow5PQtvcq4_HhBbXwaRzOdmmxY0PWr3UH-8dtE5RGsUgOIFZJcWxVYXTlbCVW52qVF_HJDn-tzeKZap-1jgZ7S45J9RqkAQAAAAEAAAMElxY16wZNcxoas25xTeI5LdfFHlDHkkJywItP_32XQ6lmN3LiGfcwljTSYCmCY8VhIKscbAUHHitQTWM1LGvLpJS3TQhSNbQIlphqm6UbGFNwKOdIS-s9IHMF9mT1ZGasp1XiK7KSms9UkCy0REVoyVLwj5-FOhGYJJkHOqRcT_RJO7bUCIsdYLbqrJo6HWd_G4nyg3vjiBKfPJESdyz9FHjG21QW9QzYRtYNx1IgxYpLdksxG0SS8s3va9I7uLFX8Omxg4R-i8dsesJcqoXuMf7-6_N8bAa51UginnXNaI9OKkJA_ZoP26n_G7PIoOE5lmdNOuphuBy836cH8t1NwoBjjO6uJJcxQ9yIZZHIArfp5_Ayt9_tC6LHpDD6vDiEMvuRXEM5SR_XGS35ui-GdTw8lprUAW5wYVZW6ogRv1xviL0BBXWZcjjDQvjx5oBOoCOkCQZGtW_D8iA5HMxojYzCop-nf29Lak8SkkRY8--lmyaMGkVfKx5YlGYSkDJDZh3zxDz2NeG4RKG12p6IS6lKHSA1_SuNmHaGQuKS";
            string strKey = ConfigurationManager.AppSettings["DeveloperKey"];
            string strUrl = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/GeneralLedger/JournalTransaction/?$filter=DateOccurred ge datetime'2020-05-16' and DateOccurred le datetime'2020-05-16'&$top=1000";
            List<GeneralLedgerJournalTransaction.Item> listjournaltransactionmodel = new List<GeneralLedgerJournalTransaction.Item>();
            var jt = CommonMethod.MakeAccountRightAPICall(strUrl, strToken, client_id, "");
            if (jt != null && jt.Count > 0)
            {
                listjournaltransactionmodel = jt.ToObject<List<GeneralLedgerJournalTransaction.Item>>();
                if (listjournaltransactionmodel != null && listjournaltransactionmodel.Count > 0)
                {
                    foreach (var item in listjournaltransactionmodel)
                    {
                        CustomerSaleInvoiceItem objInvModel = new CustomerSaleInvoiceItem();
                        CustomerInfo objCustinfomodel = new CustomerInfo();
                        if (item.JournalType != null)
                        {
                            if (item.JournalType == "Sale")
                            {
                                strUrl = item.SourceTransaction.Uri.ToString();
                                var obj = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, strToken, client_id, "");
                                if (obj != null)
                                {
                                    objInvModel = obj.ToObject<CustomerSaleInvoiceItem>();
                                    if (objInvModel.Customer != null)
                                    {
                                        if (objInvModel.Customer.Name != null)
                                        {
                                            var existName = (from c in strCompanyNameFilter
                                                             where c.ToLower().Trim() == objInvModel.Customer.Name.ToLower().Trim()
                                                             select c).FirstOrDefault();
                                            if (existName == null)
                                            {
                                                strCompanyNameFilter.Add(objInvModel.Customer.Name);
                                                objCustinfomodel.CompanyName = objInvModel.Customer.Name;
                                                objCustinfomodel.Uid = objInvModel.Customer.UID;
                                                listCustomerInfoModel.Add(objCustinfomodel);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            strUrl = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/Sale/Order?$filter=Date ge datetime'2020-05-15T00:00:00' and Date le datetime'2020-05-16T00:00:00'&$top=1000";
            List<EnquiryInsertToCRM.Models.Item> lstCustomerSaleInvoice = new List<EnquiryInsertToCRM.Models.Item>();
            var itemCustSaleInv = CommonMethod.MakeAccountRightAPICall(strUrl, strToken, client_id, "");
            if (itemCustSaleInv != null && itemCustSaleInv.Count > 0)
            {
                lstCustomerSaleInvoice = itemCustSaleInv.ToObject<List<EnquiryInsertToCRM.Models.Item>>();
                if (lstCustomerSaleInvoice != null && lstCustomerSaleInvoice.Count > 0)
                {
                    foreach (var item in lstCustomerSaleInvoice)
                    {
                        CustomerSaleInvoiceItem objInvModel = new CustomerSaleInvoiceItem();
                        CustomerInfo objCustinfomodel = new CustomerInfo();
                        if (item != null)
                        {
                            if (item.Customer != null)
                            {
                                if (item.Customer.Name != null)
                                {
                                    var existName = (from c in strCompanyNameFilter
                                                     where c.ToLower().Trim() == objInvModel.Customer.Name.ToLower().Trim()
                                                     select c).FirstOrDefault();
                                    if (existName == null)
                                    {
                                        strCompanyNameFilter.Add(item.Customer.Name);
                                        objCustinfomodel.CompanyName = item.Customer.Name;
                                        objCustinfomodel.Uid = item.Customer.UID;
                                        listCustomerInfoModel.Add(objCustinfomodel);
                                    }
                                }
                            }
                        }

                    }
                }
            }
            if (strCompanyNameFilter != null && strCompanyNameFilter.Count > 0)
            {
                ViewBag.TodayCompanyName = string.Join(",", strCompanyNameFilter);
            }
            //_oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
            //if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
            //{
            //    return "Expired "+DateTime.Now;
            //}
            //else
            //{
            //    return "Not Expired "+ DateTime.Now;
            //}




            //CommonMethod.MailSend(str, "Test", "Hello, Test Mail", true, true,false);
            //return CommonMethod.GetEncode(str);
            //sb.Clear();
            //sb.AppendLine("#1 cftokenAdministrator:"+ cftokenAdministrator);
            //sb.AppendLine("#2 cftokenAR:" + cftokenAR);
            //CommonMethod.LogFile(sb, false);
        }
        public string testData2()
        {
            _oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
            if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
            {
                return "Expired";
            }
            else
            {
                return _oAuthKeyService.OAuthResponse.AccessToken;
            }

            //CommonMethod.MailSend(str, "Test", "Hello, Test Mail", true, true,false);
            //return CommonMethod.GetEncode(str);
            //sb.Clear();
            //sb.AppendLine("#1 cftokenAdministrator:"+ cftokenAdministrator);
            //sb.AppendLine("#2 cftokenAR:" + cftokenAR);
            //CommonMethod.LogFile(sb, false);
        }
        public static void testData3(bool status)
        {
            if (!System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"])))
            {
                XmlTextWriter xmlwriter = new XmlTextWriter(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"]), Encoding.UTF8);
                xmlwriter.Formatting = Formatting.Indented;
                xmlwriter.WriteStartDocument();
                xmlwriter.WriteStartElement("Process");
                xmlwriter.WriteElementString("Status", status.ToString());
                xmlwriter.WriteEndElement();
                xmlwriter.WriteEndDocument();
                xmlwriter.Flush();
                xmlwriter.Close();
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"]));
                XmlNode myNode_Status = doc.SelectSingleNode("/Process/Status");
                myNode_Status.InnerText = status.ToString();
                doc.Save(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"]));
            }
        }
        public string testData6(int id = 0)
        {
            HttpCookie reqCookies = HttpContext.Request.Cookies["AccessToken"];
            if (id == 1)
            {
                CommonMethod.RefreshToken_BasedOnCookies();
            }
            return reqCookies.Value;
        }
        public string testData9(int id = 0)
        {
            //var str = CommonMethod.GetEncode("Nick:coffee1");
            HttpCookie reqCookies = HttpContext.Request.Cookies["AccessToken_Pk"];
            if (id == 1)
            {
                CommonMethod.RefreshTokenForPk_BasedOnCookies();
            }
            return reqCookies.Value;
        }

        public string testData7()
        {
            // string cftoken = CommonMethod.GetEncode("Administrator:");
            // List<AbEntryFieldInfo> getMonthUdfList = RigonCRMReference.getMonthUdfList();
            if (CommonMethod.IsExpiredToken_BasedOnCookies())
            {
                return "true";
            }
            else
            {

                return "false";
            }


            //if (HttpContext.Request.Cookies["AccessToken"] != null)
            //{
            //    return "Not null";
            //}
            //else
            //{
            //    return "Null";
            //}
            //if (CommonMethod.IsExpiredToken_BasedOnCookies())
            //{
            //    CommonMethod.RefreshToken_Test();
            //}
        }
        public string testData8()
        {
            this.ControllerContext.HttpContext.Response.Cookies.Clear();
            if (HttpContext.Request.Cookies["AccessToken"] != null)
            {
                System.Web.HttpCookie AccessTokenCookie = new System.Web.HttpCookie("AccessToken");
                return "Not Null:" + AccessTokenCookie.Value;
            }
            else
            {
                return "Null";
            }

            //if (HttpContext.Request.Cookies["AccessToken"] != null)
            //{
            //    return "Not null";
            //}
            //else
            //{
            //    return "Null";
            //}
            //if (CommonMethod.IsExpiredToken_BasedOnCookies())
            //{
            //    CommonMethod.RefreshToken_Test();
            //}
        }
        public void testData4()
        {
            #region Get From WebConfig            
            string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUri"];
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string client_secret = ConfigurationManager.AppSettings["DeveloperSecret"];
            string TokensFilePath = ConfigurationManager.AppSettings["TokensFilePath"];
            string XMLFilePath = ConfigurationManager.AppSettings["XMLFilePath"];
            string myobCompany = ConfigurationManager.AppSettings["MyObCompany"];

            #endregion           
            string cf_guid = "";
            string cf_uri = "";


            string cftoken = CommonMethod.GetEncode("Administrator:");

            List<CustomerInfo> listCustomerInfoModel = new List<CustomerInfo>();
            _oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];

            #region Get Comapny Guid Id From MyOb Account
            _configurationCloud = new ApiConfiguration(client_id, client_secret, redirect_uri);
            var cfsCloud = new CompanyFileService(_configurationCloud, null, _oAuthKeyService);
            var cf = cfsCloud.GetRange();
            if (cf != null && cf.Length > 0)
            {
                foreach (var item in cf.ToList())
                {
                    if (item.Name.ToLower().Trim() == myobCompany.ToLower().Trim())
                    {
                        cf_guid = item.Id.ToString();
                        cf_uri = item.Uri.ToString();
                    }

                }
                //  CommonMethod.LogFile(sb, false);
            }
            #endregion            
            if (!string.IsNullOrEmpty(cf_guid))
            {
                cftoken = "";
                string filter = "";
                string filterBody = "";

                if (!string.IsNullOrEmpty(filterBody))
                {
                    filter = "?$filter=" + filterBody + "&$top=1000";
                }

                var lstCRList = CommonMethod.MakeAccountRightAPICall(cf_uri + "/Contact/Customer/", _oAuthKeyService.OAuthResponse.AccessToken, client_id, cftoken, filter);

                if (lstCRList != null && lstCRList.Count > 0)
                {
                    listCustomerInfoModel = lstCRList.ToObject<List<CustomerInfo>>();
                    if (listCustomerInfoModel != null && listCustomerInfoModel.Count > 0)
                    {
                        listCustomerInfoModel = (from c in listCustomerInfoModel
                                                     //where (c.CompanyName ?? "") != "" && (c.DisplayId ?? "") != ""
                                                 select c).ToList();
                        int i = 0;
                        foreach (var lci in listCustomerInfoModel)
                        {
                            i++;
                            if (!string.IsNullOrEmpty(lci.CompanyName) && !string.IsNullOrEmpty(lci.Uid))
                            {
                                //RigonCRMReference.UpdateCompanyUDFBasedOnMYOBCustomerID1(lci, cf_uri, _oAuthKeyService.OAuthResponse.AccessToken, client_id, cftoken);
                            }
                        }
                    }
                }
            }
        }
        public void testData1()
        {
            //RigonCRMReference.GetFieldList();
            //
            //sb.AppendLine(""+DateTime.Now+" - ABC Company 123");
            //CommonMethod.LogFileForLastSaleDate(sb,true);

            //CommonMethod.RefreshToken();
            //fd6ae3e9-e0cf-418f-8386-1e16d4764b68
            //74570a60-5d5a-4ea7-aa96-26586739f84f
            //984192fc-8ec2-41d0-b39d-fc56f44af304
            //a3065be7-9e25-4176-848b-f1ca901abdaf
            //984192fc-8ec2-41d0-b39d-fc56f44af304
            //0e683598-dc94-4755-ba5f-0f5d52e34913
            //6185379c-2a07-4d75-bcd5-ca2ede7a5717
            //11f266c9-71ec-45fc-ab0e-332fffb63428

            //Run Only This Start
            string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            string strUrl = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/Contact/Customer/88cf4743-43b4-4f34-8464-b70877ccbb60";
            string strToken = "AAEAAJOrqsudbifNlhfFpB74eBod3NTlXlDnxE3A78vgcqF9a8a0BRSrEw9v7ItRlICUXZy429saIz0Vku1p4bMkRWH1zs9e6J5kWpuonhnAOHUgA3ALmjf8LP-xuvfBEL9cq788wqg0aO-lquAZKeLMEvAvb0Wfe3ilMwwoQBtXAQXcbxhITy9_aHL1QCAhzRWT1nyZDrvGwUuwVKx5iUMtGppxGRVzQC60e6EnUWMRLJNzkYpOUOznUtv8kT8MGshn71R3La_vPOIMVLPP48a6bkMpE89ph6G6svFZKxMQfnpLMCvHaaEGy9use8pu1Ru1lR0WENrZ2v_BtuqYf0ppY9ekAQAAAAEAAGMkdsSih0ogbm1tAeYxdJsaliWx60SMpRir448tDl4EyRxrorx3wu6kMcdAI8ul1t6Tlc9Aj4hh-tHNBO3x6yu0xngz9VantQJgIGoSDiuAkk0m8G2g9Hayta74Jy6dz7X4dd-wmdRo_Y46sf4rRBqbEV2dIlOzsAA_nQYpsezBh_j7B0rWFYWZAXnWQVQiYiogPvBrQP5aZFJ1QGgulYyEf9hVxGr5SGqwMtVZAC9MthoGM5vNLXV-Pu3_Gkm4iXzQrIOb7ItwefxL0gJDfolT38JDfXnHyR8zsQCfaKgWfmr6kVMre1_CXUPNajY308Uox_Zut0UxUH9-BsxHldQSAK5VSI8vUWqkiEqfHZe5A2C-FVhSeemGQTIP_7Y2OfqYKEbGBHwYoQCP014GitOAX2158bILbUILIiEnNL2jOoc6Y_lSBP17nloPDQ7tnmdxE5BLfmrXDFPymT_RtpxJnihn5AGEtD_n6QPb9ZeTwBz0V_R86xcXOsJW0S7qceQ9yaY5DplWMLTgAM1v37zpVQxUMPaGH2ATctAGrYbB";
            string strKey = ConfigurationManager.AppSettings["DeveloperKey"];
            CustomerInfo listCustomerInfoModel = new CustomerInfo();
            var jt = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, strToken, strKey, "");
            listCustomerInfoModel = jt.ToObject<CustomerInfo>();
            RigonCRMReference.UpdateCompanyUDFBasedOnMYOBCustomerID(listCustomerInfoModel, "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717", strToken, strKey, "");
            //Run Only This End

            //if (!System.IO.File.Exists(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"])))
            //{
            //    XmlTextWriter xmlwriter = new XmlTextWriter(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"]), Encoding.UTF8);
            //    xmlwriter.Formatting = Formatting.Indented;
            //    xmlwriter.WriteStartDocument();
            //    xmlwriter.WriteStartElement("Accesscode");
            //    xmlwriter.WriteElementString("code", "100");
            //    xmlwriter.WriteElementString("createdate", DateTime.Now.ToString());
            //    xmlwriter.WriteEndElement();
            //    xmlwriter.WriteEndDocument();
            //    xmlwriter.Flush();
            //    xmlwriter.Close();
            //}
            //else
            //{
            //    // instantiate XmlDocument and load XML from file
            //    XmlDocument doc = new XmlDocument();
            //    doc.Load(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"]));

            //    // get a list of nodes - in this case, I'm selecting all <AID> nodes under
            //    // the <GroupAIDs> node - change to suit your needs

            //    XmlNode myNode = doc.SelectSingleNode("/Accesscode/code");
            //    var strongstr = myNode.InnerText;
            //    myNode.InnerText = "CODEHERE update " + DateTime.Now.ToString();

            //    XmlNode myNodecreatedate = doc.SelectSingleNode("/Accesscode/createdate");
            //    var strongstrmyNodecreatedate = myNodecreatedate.InnerText;
            //    myNodecreatedate.InnerText = DateTime.Now.ToString();
            //    // loop through all AID nodes


            //    // save the XmlDocument back to disk
            //    doc.Save(System.Web.HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["XMLFilePath"]));
            //}

            //CustomerInfo CustomerInfo = new CustomerInfo();
            //CustomerInfo.CompanyName = "Test1' dummy";
            //CustomerInfo.Uri = "e8acsadd0c2-4esbf-4145s-as000-6f15b5885ac2dfsd";
            //RigonCRMReference.CreateCompany(CustomerInfo);

            //string client_id = ConfigurationManager.AppSettings["DeveloperKey"];
            //string strUrl = "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717/Contact/Customer/984192fc-8ec2-41d0-b39d-fc56f44af304";
            ////////string filter = "?$filter=LastModified ge datetime'2019-08-19' and LastModified le datetime'2019-08-21'&$top=2";
            //string strToken = "AAEAACLxsG0Sn-m4eF7LvNNJ7Qw9SiTh3oXa8a66LxuDqPBAF7VLjCikOLmh6O--x1C5uziGfcoBBW7bBP_Rxh2CZXRDTC8G-HYSBsMBE6x2vG3a9I1WGP_QvVQNL8UbuNr6P8apRYlr7X4vjwLtTIwzBiwykc7Y6l5nTThe5r-6enEwdx4pm6XqpeJYJ7G-qEaHKAWJJAtPlKokLpCoGF_ViNtcBGOzRDpIzSmanKBFvsyzW38ply1NlNMc2xGouKV41HBIQAVlSsG_RQmkH9Ggql6RRLLnIW96EyaAMOuU7XSfvA2niwYeEnngnx0zWirKr0bd_33t7Ln7EYIYqKwBipekAQAAAAEAAKtAoCQbaIFliq0yj_JDvhBkKmlvIseEJzxYJTJZ6nQqkrS60ot9nMSyifJeGHtkUaMPV5wFArWnU3vCI6AHEFW5Anx63GC6KA2eGSworpOXzRMmMtHWA_6w7fo3gKsRo7YlWpJMHx1LDIidqpLeD7ZB4_Zx-EAG1oZ0WLaJwvhcTofESCICKExUdzn3BS9i99IU_-2iHz6n_iqTTtk6YUl2GXTF4hFu3sk0ktAfAJ8o0nvk-he_s7ufLM4BLjp7OXk549GJv28-WvM10DzO_wrXrrc34kwDo95gqGHVcdajD9N8jf-Bnky3VzMBOSgxD6l-a7TBOHKllgl-_976p2RgjfmE52n5WFYvv6znHl-BJq-vUWyRQtyQHRvbIQNVkA3uMk61HRYIYfM3nPvIZLyZYJ_1daYEgJ7GApBDtSTANTSxVno0uAUiRQBdqW_d_liPxKR2P6H5H05kkkNrAoWNy04nmLx0m82kg2r5HbWwh3OqaVz0U_j3JTAimaAMRNGIhPIvGMzo6bgQ3Z37SJwkkBtIRmFYOTaNe_H1iVHQ";
            //////_oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
            //string strKey = ConfigurationManager.AppSettings["DeveloperKey"];
            //CustomerInfo listCustomerInfoModel = new CustomerInfo();
            //////objCustomerInfo.Uid = "e8acd0c2-4ebf-4145-a000-6f15b5885ac2";

            //var jt = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, strToken, strKey, "");
            //listCustomerInfoModel = jt.ToObject<CustomerInfo>();

            ////foreach (var lci in listCustomerInfoModel)
            ////{
            ////    //string checkCompExistOrNot = CreateCompanyInCRM(lci);
            ////    //if (checkCompExistOrNot == "companycreate")
            ////    //{

            ////    //}
            ////    //else
            ////    //{
            ////    //    RigonCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, "Udf/$TYPEID(95)");
            ////    //}

            ////}
            //RigonCRMReference.UpdateCompanyUDFBasedOnMYOBCustomerID(listCustomerInfoModel, "https://ar1.api.myob.com/accountright/6185379c-2a07-4d75-bcd5-ca2ede7a5717", strToken, strKey, "");
            //_oAuthKeyService = (IOAuthKeyService)Session[iOAuthKeyService];
            //sb.Clear();            
            //sb.AppendLine("");
            //sb.AppendLine("before #1  Call Refresh Token Fn" + DateTime.Now + "");
            //sb.AppendLine("");
            //sb.AppendLine("before Access Token: " + _oAuthKeyService.OAuthResponse.AccessToken);
            //sb.AppendLine("");
            //sb.AppendLine("before Refresh Token: " + _oAuthKeyService.OAuthResponse.RefreshToken);
            //sb.AppendLine("");
            //CommonMethod.LogFile(sb, false);
            //_oAuthKeyService = CommonMethod.RefreshToken();
            //sb.Clear();


            //sb.AppendLine("");
            //sb.AppendLine("after #1  Call Refresh Token Fn" + DateTime.Now + "");
            //sb.AppendLine("");
            //sb.AppendLine("after New Access Token: " + _oAuthKeyService.OAuthResponse.AccessToken);
            //sb.AppendLine("");
            //sb.AppendLine("after New Refresh Token: " + _oAuthKeyService.OAuthResponse.RefreshToken);
            //sb.AppendLine("");            
            //CommonMethod.LogFile(sb, false);
            //sb.Clear();
            //sb.AppendLine("#1 cftokenAdministrator:"+ cftokenAdministrator);
            //sb.AppendLine("#2 cftokenAR:" + cftokenAR);
            //CommonMethod.LogFile(sb, false);
        }
        #region Union Member Update Details
        public ActionResult UnionMemberUpdateDetails(string ClientID)
        {
            //USCRMReference.GetAbEntryProperties();
            //Udf/$TYPEID(656) -- Direct Offers From Union Shopper

            //ViewBag.pageheader = "Union Member Update Details";            
            UnionMemberModel model = new UnionMemberModel();
            if (!string.IsNullOrEmpty(ClientID ?? ""))
            {
                model = USCRMReference.ReadCompanyClientIdExisting(ClientID);
                if (model.IsInvalidClientId)
                {
                    model.lstUnionID = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(4)");
                    model.lstDirectOffers = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(656)");
                }
            }
            else
            {
                model.lstUnionID = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(4)");
                model.lstDirectOffers = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(656)");
            }
            if (model.lstUnionID != null && model.lstUnionID.Count > 0)
            {
                List<string> strFilter = new List<string>();
                strFilter.Add("INACTIVE");
                model.lstUnionID = model.lstUnionID.Where(c => !strFilter.Any(folder => c.Value.Contains(folder))).ToList();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public string UnionMemberFormSubmit(UnionMemberModel model)
        {
            string res = "";
            string mailres = "";

            if (ModelState.IsValid)
            {
                res = USCRMReference.CreateOrUpdateIndividual(model);
            }
            else
            {
                res = "invalidformdata";
            }
            return res;
        }

        public JsonResult UnionMemberUpdateDetails_Php(string ClientID, string Email)
        {
            //USCRMReference.GetAbEntryProperties();
            //Udf/$TYPEID(656) -- Direct Offers From Union Shopper

            //ViewBag.pageheader = "Union Member Update Details";            
            UnionMemberModel model = new UnionMemberModel();
            if (!string.IsNullOrEmpty(ClientID ?? "") || !string.IsNullOrEmpty(Email ?? ""))
            {
                if (!string.IsNullOrEmpty(ClientID ?? ""))
                {
                    model = USCRMReference.ReadCompanyClientIdExisting(ClientID);
                }
                else if (!string.IsNullOrEmpty(Email ?? ""))
                {
                    model = USCRMReference.ReadCompanyEmailExisting(Email);
                }
                if (model.IsInvalidClientId)
                {
                    model.lstUnionID = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(4)");
                    model.lstDirectOffers = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(656)");
                }
            }
            else
            {
                model.lstUnionID = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(4)");
                model.lstDirectOffers = USCRMReference.GetAbEntryGetFieldOptions("Udf/$TYPEID(656)");
            }
            if (model.lstUnionID != null && model.lstUnionID.Count > 0)
            {
                List<string> strFilter = new List<string>();
                strFilter.Add("INACTIVE");
                model.lstUnionID = model.lstUnionID.Where(c => !strFilter.Any(folder => c.Value.Contains(folder))).ToList();
            }
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region PK CRM
        public ActionResult SyncMyOb2NicknCRM(string code = "")
        {
            #region Get From WebConfig            
            string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUriForPK"];
            string client_id = ConfigurationManager.AppSettings["DeveloperKeyFoPK"];
            string client_secret = ConfigurationManager.AppSettings["DeveloperSecretFoPK"];
            string myobCompany = ConfigurationManager.AppSettings["MyObCompanyFoPK"];
            #endregion
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendLine("Code: " + code);
            //CommonMethod.LogFile_PK(sb, false);
            sb.Clear();
            List<SaleInvoice> listResponse = new List<SaleInvoice>();

            string[] strParameter = new string[] { };
            _oAuthKeyServiceFor_Pk = new OAuthKeyService();
            if (Request.Cookies["AccessToken_Pk"] != null)
            {
                _oAuthKeyServiceFor_Pk.OAuthResponse = null;
            }
            else
            {
                _oAuthKeyServiceFor_Pk.OAuthResponse = null;
            }

            _configurationCloudFor_Pk = new ApiConfiguration(client_id, client_secret, redirect_uri);
            if (_oAuthKeyServiceFor_Pk.OAuthResponse == null || !string.IsNullOrEmpty(code))
            {
                if (!string.IsNullOrEmpty(code))
                {
                    _oAuthKeyServiceFor_Pk = setSessionTokenFor_Pk(code);
                }
                else
                {
                    Response.Redirect("https://secure.myob.com/oauth2/account/authorize?client_id=" + client_id + "&redirect_uri=" + redirect_uri + "&response_type=code&scope=CompanyFile");
                }
            }
            else
            {

            }
            return View();
        }
        public ActionResult GetPkPartialCustomerInfo(bool FilterData = false, bool Refresh = false, bool IsBasedOnCompanyNameMatch = false, bool IsBasedOnMyObIdMatch = false, string FilterDate = "", string FilterCustomerName = "", string FilterCustomerNameList = "")
        {
            try
            {
                #region Get From WebConfig            
                string redirect_uri = ConfigurationManager.AppSettings["SyncMyOb2CRM_RedirectUriForPK"];
                string client_id = ConfigurationManager.AppSettings["DeveloperKeyFoPK"];
                string client_secret = ConfigurationManager.AppSettings["DeveloperSecretFoPK"];
                string myobCompany = ConfigurationManager.AppSettings["MyObCompanyFoPK"];

                int companynameupdatecnt = 0;
                int companycreatecnt = 0;
                int myobupdatecnt = 0;
                int myobalreadyexistcnt = 0;
                int myobnotfoundcnt = 0;
                int companynotfoundcnt = 0;
                int udfdatafieldsupdatecnt = 0;
                int Customercnt = 0;
                #endregion
                string cf_guid = "";
                string cf_uri = "https://ar1.api.myob.com/accountright/d64e5b25-0551-4e2d-a279-1cd6d4f7e6f1";
                string FromDate = "";
                string ToDate = "";
                StringBuilder sb = new StringBuilder();

                int errorcnt = 0;
                List<string> lstErrorComp = new List<string>();
                sb.Clear();
                sb.AppendLine("=================================================================");
                sb.AppendLine("#1");
                //CommonMethod.LogFile_PK(sb, false);
                string cftoken = CommonMethod.GetEncode("Nick:coffee1");
                HttpCookie cookie_AccessToken = Request.Cookies["AccessToken_Pk"];
                List<CustomerInfo> listCustomerInfoModel = new List<CustomerInfo>();
                if (!string.IsNullOrEmpty(cf_uri))
                {
                    //Refresh = false;
                    //IsBasedOnCompanyNameMatch = false;
                    //IsBasedOnMyObIdMatch = false;
                    //cftoken = "";
                    string filter = "";
                    string filterBody = "";
                    if (!string.IsNullOrEmpty(FilterDate))
                    {
                        if (FilterDate.Contains("-"))
                        {
                            string[] strFilterDateArry = FilterDate.Split('-');
                            if (strFilterDateArry != null && strFilterDateArry.Length > 0)
                            {
                                FromDate = Convert.ToDateTime(strFilterDateArry[0]).ToString("yyyy-MM-dd");
                                ToDate = Convert.ToDateTime(strFilterDateArry[1]).ToString("yyyy-MM-dd");
                                filterBody += "LastModified ge datetime'" + FromDate + "' and LastModified le datetime'" + ToDate + "'";
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(FilterCustomerName))
                    {
                        if (!string.IsNullOrEmpty(filterBody))
                        {
                            filterBody += " and ";
                        }
                        if (FilterCustomerName.Contains("'"))
                        {
                            FilterCustomerName = FilterCustomerName.Replace("'", "''");
                        }
                        filterBody += "substringof(tolower('" + HttpUtility.UrlEncode(FilterCustomerName) + "'), tolower(CompanyName)) eq true";
                    }
                    if (!string.IsNullOrEmpty(filterBody))
                    {
                        filter = "?$filter=" + filterBody + "&$top=1000";
                    }

                    var lstCRList = CommonMethod.MakeAccountRightAPICall(cf_uri + "/Contact/Customer/", cookie_AccessToken.Value, client_id, cftoken, filter);

                    if (lstCRList != null && lstCRList.Count > 0)
                    {
                        listCustomerInfoModel = lstCRList.ToObject<List<CustomerInfo>>();
                        if (listCustomerInfoModel != null && listCustomerInfoModel.Count > 0)
                        {
                            List<string> strCompanyNameFilter = new List<string>();
                            if (!string.IsNullOrEmpty(FilterCustomerNameList))
                            {
                                sb.Clear();
                                sb.AppendLine("#2.9");
                                sb.AppendLine("#FilterCustomerNameList: " + FilterCustomerNameList);
                                //CommonMethod.LogFile_PK(sb, false);
                                FilterCustomerNameList = FilterCustomerNameList.Replace("[REPLACEANDCHAR]", "&");
                                if (FilterCustomerNameList.Contains("\\n"))
                                {

                                    FilterCustomerNameList = FilterCustomerNameList.Replace(@"""", @"\""").Replace(@"\""", @"").Replace("\\n", "\n");

                                    strCompanyNameFilter = FilterCustomerNameList.Split('\n').ToList();
                                }
                                else if (FilterCustomerNameList.Contains(","))
                                {
                                    strCompanyNameFilter = FilterCustomerNameList.Split(',').ToList();
                                }
                                else
                                {
                                    strCompanyNameFilter.Add(FilterCustomerNameList.Replace(@"""", @"\""").Replace(@"\""", @""));
                                }
                            }
                            if (strCompanyNameFilter != null && strCompanyNameFilter.Count > 0)
                            {
                                listCustomerInfoModel = (from c in listCustomerInfoModel
                                                         where strCompanyNameFilter.Contains(c.CompanyName)
                                                         select c).ToList();
                                //var  listCustomerInfoModel1 = listCustomerInfoModel.Where(c => !strCompanyNameFilter.Any(folder => c.CompanyName.ToLower().Trim().Contains(folder))).ToList();

                            }
                            else
                            {
                                listCustomerInfoModel = (from c in listCustomerInfoModel
                                                             //where (c.CompanyName ?? "") != "" && (c.DisplayId ?? "") != ""
                                                         select c).ToList();
                            }


                            Customercnt = listCustomerInfoModel.Count;
                            sb.Clear();
                            sb.AppendLine("#5 counter=" + Customercnt);
                            //CommonMethod.LogFile_PK(sb, false);

                            ViewBag.Customercnt = listCustomerInfoModel.Count;
                            sb.Clear();
                            sb.AppendLine("#6 Refresh=" + Refresh.ToString());
                            sb.AppendLine("#7 IsBasedOnCompanyNameMatch=" + IsBasedOnCompanyNameMatch.ToString());
                            sb.AppendLine("#8 IsBasedOnMyObIdMatch=" + IsBasedOnMyObIdMatch.ToString());
                            //CommonMethod.LogFile_PK(sb, false);
                            if (Refresh || IsBasedOnCompanyNameMatch || IsBasedOnMyObIdMatch)
                            {
                                try
                                {
                                    sb.Clear();
                                    sb.AppendLine("#6 Refresh=" + Refresh.ToString());
                                    sb.AppendLine("#7 IsBasedOnCompanyNameMatch=" + IsBasedOnCompanyNameMatch.ToString());
                                    sb.AppendLine("#8 IsBasedOnMyObIdMatch=" + IsBasedOnMyObIdMatch.ToString());
                                    //CommonMethod.LogFile_PK(sb, false);
                                    string MYOBUID_UDF = PKCRMReference.getMyOBUid_UDFKey();
                                    int counter = 0;
                                    foreach (var lci in listCustomerInfoModel)
                                    {
                                        sb.Clear();
                                        sb.AppendLine("#9 counter:" + counter);
                                        //CommonMethod.LogFile_PK(sb, false);
                                        counter++;
                                        CommonMethod.RefreshTokenForPk_BasedOnCookies();

                                        #region Update MyObId In CRM Based On Company Name Match
                                        if (!string.IsNullOrEmpty(lci.CompanyName) && !string.IsNullOrEmpty(lci.Uid))
                                        {
                                            string res = "";
                                            if (Refresh)
                                            {
                                                string checkCompExistOrNot = CreateCompanyInPkCRM(lci);
                                                if (checkCompExistOrNot == "companycreate")
                                                {
                                                    companycreatecnt += 1;
                                                }
                                                else
                                                {
                                                    res = PKCRMReference.UpdateCompanyNameOrMyObIdInCRM(lci.CompanyName, lci.Uid);
                                                }
                                            }
                                            else if (IsBasedOnCompanyNameMatch)
                                            {
                                                string checkCompExistOrNot = CreateCompanyInPkCRM(lci);
                                                if (checkCompExistOrNot == "companycreate")
                                                {
                                                    companycreatecnt += 1;
                                                }
                                                else
                                                {
                                                    res = PKCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, MYOBUID_UDF);
                                                }
                                            }
                                            else if (IsBasedOnMyObIdMatch)
                                            {
                                                //StringBuilder sb = new StringBuilder();
                                                sb.Clear();
                                                sb.AppendLine("Processing For Start");
                                                sb.AppendLine("----------------------");
                                                sb.AppendLine("CompanyName:" + lci.CompanyName);
                                                sb.AppendLine("DisplayID:" + lci.DisplayId);
                                                sb.AppendLine("counter:" + counter);
                                                sb.AppendLine("");
                                                //CommonMethod.LogFile_PK(sb, false);
                                                string checkCompExistOrNot = CreateCompanyInPkCRM(lci);
                                                if (checkCompExistOrNot == "companycreate")
                                                {
                                                    companycreatecnt += 1;
                                                }
                                                else
                                                {
                                                    var str1 = PKCRMReference.UpdateCompanyNameOrMyObIdInCRM(lci.CompanyName, lci.Uid);
                                                    if (str1 == "alreadyexistmyobID")
                                                    {
                                                        PKCRMReference.UpdateMyObIdInCompany(lci.CompanyName, lci.Uid, MYOBUID_UDF);
                                                    }
                                                }
                                                res = PKCRMReference.UpdateCompanyUDFBasedOnMYOBCustomerID(lci, cf_uri, cookie_AccessToken.Value, client_id, cftoken);
                                                if (res == "success")
                                                {
                                                    res = "udfdatafieldsupdate";
                                                }
                                                else if (res == "sessionexpired")
                                                {
                                                    res = "sessionexpired";
                                                }
                                                else if (res == "error")
                                                {
                                                    res = "error";
                                                    errorcnt += 1;
                                                    lstErrorComp.Add(lci.CompanyName);
                                                }
                                                else
                                                {
                                                    res = "myobnotfound";
                                                }

                                                sb.Clear();
                                                sb.AppendLine("Processing For Done");
                                                sb.AppendLine("----------------------");
                                                sb.AppendLine("Res:" + res);
                                                sb.AppendLine("CompanyName:" + lci.CompanyName);
                                                sb.AppendLine("DisplayID:" + lci.DisplayId);
                                                sb.AppendLine("counter:" + counter);
                                                sb.AppendLine("");
                                                sb.AppendLine("========================================================================");
                                                sb.AppendLine("");
                                                //CommonMethod.LogFile_PK(sb, false);
                                            }

                                            if (res == "companynameupdate")
                                            {
                                                companynameupdatecnt += 1;
                                            }
                                            else if (res == "myobupdate")
                                            {
                                                myobupdatecnt += 1;
                                            }
                                            else
                                            if (res == "alreadyexistmyobID")
                                            {
                                                myobalreadyexistcnt += 1;
                                            }
                                            else
                                            if (res == "invalidcompanyname")
                                            {
                                                companynotfoundcnt += 1;
                                            }
                                            else
                                            if (res == "udfdatafieldsupdate")
                                            {
                                                udfdatafieldsupdatecnt += 1;
                                            }
                                            else
                                            if (res == "myobnotfound")
                                            {
                                                myobnotfoundcnt += 1;
                                            }
                                        }
                                        #endregion

                                    }
                                }
                                catch (Exception ex)
                                {
                                    CommonMethod.LogFile_PK(sb, true);
                                }
                            }
                        }
                    }
                }
                if (Request.IsAjaxRequest())
                {
                    if (FilterData)
                    {
                        return PartialView(listCustomerInfoModel);
                    }
                    string strErrorCompList = "";
                    if (lstErrorComp != null && lstErrorComp.Count > 0)
                    {
                        strErrorCompList = string.Join(", ", lstErrorComp);
                    }
                    var jsonData = new
                    {
                        CustomerInfocnt = Customercnt,
                        companynameupdate = companynameupdatecnt,
                        companycreate = companycreatecnt,
                        myobupdate = myobupdatecnt,
                        myobalreadyexist = myobalreadyexistcnt,
                        companynotfound = companynotfoundcnt,
                        myobnotfound = myobnotfoundcnt,
                        udfdatafieldsupdate = udfdatafieldsupdatecnt,
                        errorCompcnt = errorcnt,
                        errorCompList = strErrorCompList
                    };

                    return Json(jsonData, JsonRequestBehavior.AllowGet);
                }
                return PartialView(listCustomerInfoModel);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IOAuthKeyService setSessionTokenFor_Pk(string code = "")
        {
            if (!string.IsNullOrEmpty(code))
            {
                var oauthService = new OAuthService(_configurationCloudFor_Pk);
                _oAuthKeyServiceFor_Pk.OAuthResponse = oauthService.GetTokens(code);

                HttpCookie cookie_AccessToken_Pk = new HttpCookie("AccessToken_Pk", _oAuthKeyServiceFor_Pk.OAuthResponse.AccessToken);
                HttpCookie cookie_RefreshToken_Pk = new HttpCookie("RefreshToken_Pk", _oAuthKeyServiceFor_Pk.OAuthResponse.RefreshToken);
                HttpCookie cookie_ReceivedTime_Pk = new HttpCookie("ReceivedTime_Pk", _oAuthKeyServiceFor_Pk.OAuthResponse.ReceivedTime.ToString());
                cookie_AccessToken_Pk.Expires = DateTime.Now.AddHours(3);
                cookie_RefreshToken_Pk.Expires = DateTime.Now.AddHours(3);
                cookie_ReceivedTime_Pk.Expires = DateTime.Now.AddHours(3);
                Response.Cookies.Add(cookie_AccessToken_Pk);
                Response.Cookies.Add(cookie_RefreshToken_Pk);
                Response.Cookies.Add(cookie_ReceivedTime_Pk);
                return _oAuthKeyServiceFor_Pk;
            }
            else
            {
                return null;
            }
        }
        public string CreateCompanyInPkCRM(CustomerInfo objCustomerInfo)
        {
            string response = "alreadyexist";
            AbEntryKeyModel model = new AbEntryKeyModel();
            model = PKCRMReference.ReadExistingCompanyNameOrMYOBID(objCustomerInfo.CompanyName, "");
            if (string.IsNullOrEmpty(model.Key))
            {
                string key = PKCRMReference.CreateCompany(objCustomerInfo);
                if (!string.IsNullOrEmpty(key))
                {
                    response = "companycreate";
                }
            }
            //CommonMethod.LogFile(sb, false);
            return response;
        }
        #endregion


    }
}