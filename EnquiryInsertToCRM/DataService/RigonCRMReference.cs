using EnquiryInsertToCRM.DataService;
using EnquiryInsertToCRM.RigonServiceReferenceData;
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
using MYOB.AccountRight.SDK;
using System.Web;

namespace EnquiryInsertToCRM.DataService
{
    public static class RigonCRMReference
    {

        public static IOAuthKeyService _oAuthKeyService;
        public const string iOAuthKeyService = "MyOAuthKeyService";
        public static string strLastSaleDate = "";
        public static string strCompanyName = "";
        public static List<string> NotExistKeyInUdfTable = new List<string>();
        public static List<string> strPreviousValue = new List<string>();
        public static List<string> NotExistItemInUdfTable = new List<string>();
        public static string sysFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
        public static string FinancialYearCRMFolder = ConfigurationManager.AppSettings["FinancialYearCRMFolder"] + "\\";
        public static string UDFFieldsName = "MYOB Phone,MYOB IsInactive,MYOB Sales Person,MYOB Identifiers," + FinancialYearCRMFolder + "MYOB Last Sale Date,MYOB Products bought,Unused Fields\\MYOB Items bought," +
            "Unused Fields\\MYOB Customer Since,Unused Fields\\MYOB Current Balance,Unused Fields\\MYOB Last Sales Update,MYOB Last Sale Amount," + FinancialYearCRMFolder + "2014," + FinancialYearCRMFolder + "2015," + FinancialYearCRMFolder + "2016," +
            "" + FinancialYearCRMFolder + "2017," + FinancialYearCRMFolder + "2018," + FinancialYearCRMFolder + "2019," + FinancialYearCRMFolder + "2020," + FinancialYearCRMFolder + "2021," + FinancialYearCRMFolder + "2022," +
            "" + FinancialYearCRMFolder + "MYOB Sales YTD,Unused Fields\\MYOB Highest Invoice,MYOB Note,Unused Fields\\MYOB Balance 0-30 days,Unused Fields\\MYOB Balance 30-60 days,Unused Fields\\MYOB Balance over 60 days,Unused Fields\\MYOB Card Record ID," +
            "Unused Fields\\MYOB Contact Name,MYOB Credit Limit,Unused Fields\\MYOB Email,Unused Fields\\MYOB Last Balance update,Unused Fields\\MYOB Total Paid,ABN,Pharmacy,MYOB UID,Marketing\\Type of Customer," +
            "Unused Fields\\MYOB Overdue Balance,Unused Fields\\Last Payment Date,Contact\\Sales Email,Contact\\Sales Contact,Contact\\Sales Phone";
        public static string GetToken()
        {
            string token = string.Empty;
            //initialize a string variable to store the return value
            //construct the JSON request using Newtonsoft.Json.Linq.JObject
            JObject authRequest = new JObject(
                new JProperty("Database", ConfigurationManager.AppSettings["RigonMaximizerDatabase"]),
                new JProperty("UID", ConfigurationManager.AppSettings["RigonMaximizerUID"]),
                new JProperty("Password", ConfigurationManager.AppSettings["RigonMaximizerPassword"])
              );

            //instantiate the WCF service client ("MaximizerWebDataService" is the service namespace)
            using (RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient())
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
            string MYOBUID_UDF = getMyOBUid_UDFKey();
            List<JProperty> lstFilterJProperty = new List<JProperty>();
            List<JProperty> lstRequestJProperty = new List<JProperty>();

            if (!string.IsNullOrEmpty(companyname))
            {
                lstFilterJProperty.Add(new JProperty("CompanyName", (companyname.Trim() ?? "")));
                lstRequestJProperty.Add(new JProperty(MYOBUID_UDF, 1));
            }
            else if (!string.IsNullOrEmpty(myobID))
            {
                lstFilterJProperty.Add(new JProperty(MYOBUID_UDF, (myobID.Trim() ?? "")));
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
            using (RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient())
            {
                Stream response = client.AbEntryRead(new MemoryStream(Encoding.UTF8.GetBytes(createRequest)));
                JToken createResponse = JToken.Parse(new StreamReader(response).ReadToEnd());
                //check the return value of the response

                //CommonMethod.LogFile(sb, false);
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
                                model.MyObID = Convert.ToString(arrayAppointment[0][MYOBUID_UDF] ?? "");
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
        public static string UpdateCompanyNameOrMyObIdInCRM(string companyname, string myobUID)
        {
            string res = "";
            string MYOBUID_UDF = getMyOBUid_UDFKey();
            StringBuilder sb = new StringBuilder();
            try
            {
                if (CommonMethod.IsExpiredToken_BasedOnCookies())
                {
                    _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                }
                string ParentKey = "";
                string rtnRes = "";
                bool isCompanyName = false;
                bool ismyobupdate = false;
                AbEntryKeyModel model = new AbEntryKeyModel();
                NotesModel objNoteModel = new NotesModel();
                string dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ssZ");
                List<string> strMyObID = new List<string>();
                List<JProperty> lstRequestJProperty = new List<JProperty>();

                if (!string.IsNullOrEmpty(myobUID))
                {
                    model = ReadExistingCompanyNameOrMYOBID("", myobUID);
                    if (string.IsNullOrEmpty(model.Key ?? ""))
                    {
                        model = ReadExistingCompanyNameOrMYOBID(companyname, "");
                        if (string.IsNullOrEmpty(model.Key))
                        {
                            return "invalidcompanyname";
                        }
                        else
                        {
                            lstRequestJProperty.Add(new JProperty(MYOBUID_UDF, myobUID));
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

                    RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient();
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
                                    sb.AppendLine("<div style='margin-bottom:5px;'><b>MYOB UID: </b>" + myobUID + "</div>");
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
                //                CommonMethod.LogFile(sb, false);
            }

            return res;
        }

        //Function check for Company already exist based on Company name.
        public static AbEntryKeyModel ReadCompanyNameExisting(string companyname, string MYOBUID_UDF)
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
                                new JProperty("Key",
                                        new JObject(
                                            new JProperty("Value", 1),
                                            new JProperty("ID", 1)
                                      )
                                ),
                                new JProperty("CompanyName", 1),
                                new JProperty(MYOBUID_UDF, 1)
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
            using (RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient())
            {
                Stream response = client.AbEntryRead(new MemoryStream(Encoding.UTF8.GetBytes(createRequest)));
                JToken createResponse = JToken.Parse(new StreamReader(response).ReadToEnd());
                //check the return value of the response
                if (createResponse.Value<int>("Code") == 0)
                {
                    JObject ResponseData = (JObject)createResponse["AbEntry"];
                    if (ResponseData != null)
                    {
                        var arrayData = (JArray)ResponseData["Data"];
                        if (arrayData != null && arrayData.Count > 0)
                        {
                            model.Key = Convert.ToString(arrayData[0]["Key"]["Value"] ?? "");
                            model.MyObID = Convert.ToString(arrayData[0][MYOBUID_UDF] ?? "");
                        }
                    }
                }
            }
            return model;
        }

        public static string UpdateMyObIdInCompany(string companyname, string myobID, string MYOBUID_UDF)
        {
            string res = "";

            try
            {
                //if (CommonMethod.IsExpiredToken())
                //{
                //    
                //    sb.Clear();
                //    sb.AppendLine("#1 GetPartialCustomerInfo Call Session Token Expired " + companyname + "");
                //    CommonMethod.LogFile(sb, false);
                //    _oAuthKeyService = CommonMethod.RefreshToken();
                //}
                if (CommonMethod.IsExpiredToken_BasedOnCookies())
                {
                    _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                }

                string ParentKey = "";
                string rtnRes = "";
                AbEntryKeyModel model = new AbEntryKeyModel();
                NotesModel objNoteModel = new NotesModel();
                StringBuilder strb = new StringBuilder();
                string dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ssZ");
                List<string> strMyObID = new List<string>();
                if (!string.IsNullOrEmpty(companyname) && !string.IsNullOrEmpty(myobID))
                {
                    model = ReadCompanyNameExisting(companyname, MYOBUID_UDF);
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
                                new JProperty(MYOBUID_UDF, string.Join(",", strMyObID))
                            ))
                        ))
                    ).ToString();

                        RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient();
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

                                    strb.AppendLine("<div style='margin-bottom:5px;'><b>MYOB UID: </b>" + myobID + "</div>");
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
                            res = "myobupdate";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder();
                sb.Clear();
                sb.AppendLine("");
                sb.AppendLine("#CRM Step error:" + ex.Message);
                sb.AppendLine("");
                //CommonMethod.LogFile(sb, false);
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

            RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient();
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
            using (RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient())
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
        public static string UpdateCompanyUDFBasedOnMYOBCustomerID(CustomerInfo objCustomerInfo, string cf_uri = "", string AccessToken = "", string client_id = "", string cftoken = "")
        {
            StringBuilder sb = new StringBuilder();
            List<CustomerPaymentModel> listCustomerPaymentModel = new List<CustomerPaymentModel>();
            List<CustomerSaleInvoiceItem> lstCustSaleInv = new List<CustomerSaleInvoiceItem>();
            CustomerSaleInvoice custSaleInvObjforFy = new CustomerSaleInvoice();
            List<FinancialYearModel> lstFyear = new List<FinancialYearModel>();
            List<Item> invoiceListItemforFy = new List<Item>();
            List<Item> invoiceListItemForAgeingRpt = new List<Item>();
            CustomerSaleInvoice custSaleInvObjForAgeingRpt = new CustomerSaleInvoice();
            string MYOBUID_UDFKey = "";
            strLastSaleDate = "";
            strCompanyName = "";
            double? db0To30 = 0, db31To60 = 0, db60Plus = 0;
            DateTime? CustLastPaymentDate = null;
            double? TotalInvoiceDebitColumn = 0, TotalInvoiceCreditColumn = 0, TotalPaid = 0;
            string res = "";
            try
            {
                if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                {
                    _oAuthKeyService = new OAuthKeyService();
                    _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
                    _oAuthKeyService.OAuthResponse.AccessToken = AccessToken;
                }
                else
                {
                    if (CommonMethod.IsExpiredToken_BasedOnCookies())
                    {

                        _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                    }
                    else
                    {

                    }
                    //if (HttpContext.Current.Session[iOAuthKeyService] != null)
                    //{
                    //    sbNew.Append("1# true");
                    //    CommonMethod.LogFile(sbNew, true);
                    //    _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
                    //    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                    //    {
                    //        _oAuthKeyService = CommonMethod.RefreshToken();
                    //    }
                    //}
                    //else
                    //{
                    //    sbNew.Append("1# false and return ");
                    //    CommonMethod.LogFile(sbNew, true);
                    //    return res = "sessionexpired";
                    //}
                }

                if (string.IsNullOrEmpty(objCustomerInfo.Uid))
                {
                    return res = "";
                }
                List<JProperty> lstJProperty = new List<JProperty>();
                List<JProperty> lstReadJProperty = new List<JProperty>();
                List<AbEntryFieldInfo> udfFieldList = new List<AbEntryFieldInfo>();
                StringBuilder strbNotetext = new StringBuilder();
                AbEntryKeyModel model = new AbEntryKeyModel();
                List<GeneralLedgerJournalTransaction.JournalTransactionModel> lstJournalTransactionModel = new List<GeneralLedgerJournalTransaction.JournalTransactionModel>();
                strCompanyName = objCustomerInfo.CompanyName;
                #region Create Json Format for Get UDF field            
                udfFieldList = GetFieldList();
                if (udfFieldList != null && udfFieldList.Count > 0)
                {
                    foreach (var item in udfFieldList)
                    {
                        if (item != null)
                        {
                            lstReadJProperty.Add(new JProperty(item.UniqueKey, 1));
                        }
                    }
                    var objUdf = udfFieldList.Select(s => s).Where(s => s.Name.ToLower().Trim() == "myob uid".Trim()).FirstOrDefault();
                    if (objUdf != null)
                    {
                        MYOBUID_UDFKey = objUdf.UniqueKey;
                    }
                }
                #endregion
                #region Get Monthly Sale UDF List


                #endregion

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
                                    lstReadJProperty
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
                                                            new JProperty(MYOBUID_UDFKey, (objCustomerInfo.Uid.Trim() ?? ""))
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
                using (RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient())
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
                                string Key = Convert.ToString(arrayAppointment[0]["Key"] ?? "");
                                if (!string.IsNullOrEmpty(Key))
                                {
                                    #region MYOB Last Sale Date, MYOB Last Sale Amount, MYOB Highest Invoice, MYOB Products bought & MYOB Items bought 
                                    CustomerSaleInvoice custSaleInvObj = new CustomerSaleInvoice();
                                    List<Item> invoiceListItem = new List<Item>();
                                    List<Line> linesItem = new List<Line>();
                                    List<Customer> CustomerItem = new List<Customer>();
                                    string strUrl = "";
                                    strUrl = cf_uri + "/Sale/Invoice/?$filter=Customer/UID eq guid'" + objCustomerInfo.Uid + "'&$top=1000";
                                    //strUrl = cf_uri + "/Sale/Invoice/?$filter=Customer/UID eq guid'" + objCustomerInfo.Uid + "' and (CustomerPurchaseOrderNumber eq null or CustomerPurchaseOrderNumber eq '')&$top=1000";

                                    do
                                    {
                                        if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                                        {
                                            _oAuthKeyService = new OAuthKeyService();
                                            _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
                                            _oAuthKeyService.OAuthResponse.AccessToken = AccessToken;
                                        }
                                        else
                                        {
                                            if (CommonMethod.IsExpiredToken_BasedOnCookies())
                                            {
                                                _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                                            }
                                            //if (HttpContext.Current.Session[iOAuthKeyService] != null)
                                            //{
                                            //    _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
                                            //    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                                            //    {
                                            //        _oAuthKeyService = CommonMethod.RefreshToken();
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    return res = "sessionexpired";
                                            //}
                                        }
                                        //JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, AccessToken, client_id, "");
                                        JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, CommonMethod.GetAccessToken(), client_id, "");
                                        if (jt1 != null)
                                        {
                                            CustomerSaleInvoice tempCustInv = jt1.ToObject<CustomerSaleInvoice>();
                                            invoiceListItem.AddRange(tempCustInv.Items);
                                            if (!string.IsNullOrEmpty(tempCustInv.NextPageLink))
                                            {
                                                strUrl = tempCustInv.NextPageLink;
                                            }
                                            else
                                            {
                                                strUrl = "";
                                                custSaleInvObj.Items = invoiceListItem;
                                            }
                                        }
                                        else
                                        {
                                            strUrl = "";
                                        }

                                    } while (!string.IsNullOrEmpty(strUrl));
                                    if (custSaleInvObj != null && custSaleInvObj.Items != null)
                                    {
                                        int counterI = 0;
                                        foreach (var item in custSaleInvObj.Items.Where(s => s.Status == "Open").OrderByDescending(s => s.Date))
                                        {
                                            counterI++;

                                            if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                                            {
                                                _oAuthKeyService = new OAuthKeyService();
                                                _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
                                                _oAuthKeyService.OAuthResponse.AccessToken = AccessToken;
                                            }
                                            else
                                            {

                                                //sbNew.AppendLine("# custSaleInvObj.Items :" + item.Number);
                                                //CommonMethod.LogFile(sbNew, false);
                                                if (CommonMethod.IsExpiredToken_BasedOnCookies())
                                                {
                                                    _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                                                }
                                                //if (HttpContext.Current.Session[iOAuthKeyService] != null)
                                                //{
                                                //    sbNew.AppendLine("# Session Not Null :" + item.Number);
                                                //    _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
                                                //    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                                                //    {
                                                //        _oAuthKeyService = CommonMethod.RefreshToken();
                                                //    }
                                                //}
                                                //else
                                                //{
                                                //    sbNew.AppendLine("# custSaleInvObj.Items Session Null :" + item.Number);
                                                //    CommonMethod.LogFile(sbNew, false);
                                                //    return res = "sessionexpired";
                                                //}
                                            }
                                            //if (!string.IsNullOrEmpty(item.Date))
                                            //{
                                            //    string[] strDateArray = item.Date.Split('/');
                                            //    item.Date_Converted = Convert.ToDateTime(strDateArray[1] + "/" + strDateArray[0] + "/" + strDateArray[2]);
                                            //}

                                            //JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(item.Uri, AccessToken, client_id, "");
                                            JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(item.Uri, CommonMethod.GetAccessToken(), client_id, "");
                                            if (jt1 != null)
                                            {
                                                CustomerSaleInvoiceItem custSaleInvItemObj = jt1.ToObject<CustomerSaleInvoiceItem>();
                                                if (custSaleInvItemObj != null && custSaleInvItemObj.Lines != null)
                                                {
                                                    lstCustSaleInv.Add(custSaleInvItemObj);
                                                    linesItem.AddRange(custSaleInvItemObj.Lines.ToList());
                                                }
                                            }

                                            strUrl = "";
                                            strUrl = cf_uri + "/GeneralLedger/JournalTransaction?$filter=DisplayID eq '" + item.Number + "'&$top=1000";
                                            if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                                            {
                                                _oAuthKeyService = new OAuthKeyService();
                                                _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
                                                _oAuthKeyService.OAuthResponse.AccessToken = AccessToken;
                                            }
                                            else
                                            {
                                                if (CommonMethod.IsExpiredToken_BasedOnCookies())
                                                {
                                                    _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                                                }
                                                //if (HttpContext.Current.Session[iOAuthKeyService] != null)
                                                //{
                                                //    sbNew.AppendLine("# lstJournalTransactionModel :" + item.Number);
                                                //    CommonMethod.LogFile(sbNew, false);
                                                //    _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
                                                //    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                                                //    {
                                                //        _oAuthKeyService = CommonMethod.RefreshToken();
                                                //    }
                                                //}
                                                //else
                                                //{
                                                //    sbNew.AppendLine("# lstJournalTransactionModel Session Null :" + item.Number);
                                                //    CommonMethod.LogFile(sbNew, false);
                                                //    return res = "sessionexpired";
                                                //}
                                            }
                                            //JToken jt2 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, AccessToken, client_id, "");
                                            JToken jt2 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, CommonMethod.GetAccessToken(), client_id, "");
                                            if (jt2 != null)
                                            {
                                                GeneralLedgerJournalTransaction.JournalTransactionModel objJournalTransactionModel = jt2.ToObject<GeneralLedgerJournalTransaction.JournalTransactionModel>();
                                                lstJournalTransactionModel.Add(objJournalTransactionModel);
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Journal Transaction Model                                   
                                    if (lstJournalTransactionModel != null && lstJournalTransactionModel.Count > 0)
                                    {
                                        foreach (var lstJTM in lstJournalTransactionModel)
                                        {
                                            if (lstJTM != null)
                                            {
                                                if (lstJTM.Items != null && lstJTM.Items.Count > 0)
                                                {
                                                    foreach (var itm in lstJTM.Items)
                                                    {
                                                        //if (itm.DisplayId == "00062230")
                                                        //{
                                                        if (itm.Lines != null && itm.Lines.Count > 0)
                                                        {
                                                            foreach (var lstLines in itm.Lines)
                                                            {
                                                                if (lstLines != null)
                                                                {
                                                                    if (lstLines.Amount > 0)
                                                                    {
                                                                        if (lstLines.IsCredit)
                                                                        {
                                                                            TotalInvoiceDebitColumn += lstLines.Amount;
                                                                        }
                                                                        else
                                                                        {
                                                                            //TotalInvoiceCreditColumn += lstLines.Amount;
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                        //}
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Calculate Total Paid(Sum of all invoices(Debit column)-Current Balance)
                                    if (TotalInvoiceDebitColumn > 0)
                                    {
                                        if (objCustomerInfo.CurrentBalance > 0)
                                        {
                                            TotalPaid = Math.Round((TotalInvoiceDebitColumn - objCustomerInfo.CurrentBalance ?? 0), 2);
                                        }
                                    }
                                    #endregion
                                    #region Current FY Related UDF Like 2020, 2019, YTD

                                    strUrl = cf_uri + "/Sale/Invoice/?$filter=Customer/UID eq guid'" + objCustomerInfo.Uid + "'&$top=1000";
                                    do
                                    {
                                        if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                                        {
                                            _oAuthKeyService = new OAuthKeyService();
                                            _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
                                            _oAuthKeyService.OAuthResponse.AccessToken = AccessToken;
                                        }
                                        else
                                        {
                                            if (CommonMethod.IsExpiredToken_BasedOnCookies())
                                            {
                                                _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                                            }
                                            //if (HttpContext.Current.Session[iOAuthKeyService] != null)
                                            //{
                                            //    _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
                                            //    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                                            //    {
                                            //        _oAuthKeyService = CommonMethod.RefreshToken();
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    return res = "sessionexpired";
                                            //}
                                        }
                                        //JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, AccessToken, client_id, "");
                                        JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, CommonMethod.GetAccessToken(), client_id, "");
                                        if (jt1 != null)
                                        {
                                            CustomerSaleInvoice tempCustInv = jt1.ToObject<CustomerSaleInvoice>();
                                            invoiceListItemforFy.AddRange(tempCustInv.Items);
                                            if (!string.IsNullOrEmpty(tempCustInv.NextPageLink))
                                            {
                                                strUrl = tempCustInv.NextPageLink;
                                            }
                                            else
                                            {
                                                strUrl = "";
                                                custSaleInvObjforFy.Items = invoiceListItemforFy;
                                            }
                                        }
                                        else
                                        {
                                            strUrl = "";
                                        }
                                    } while (!string.IsNullOrEmpty(strUrl));
                                    if (custSaleInvObjforFy != null && custSaleInvObjforFy.Items != null)
                                    {
                                        if (custSaleInvObjforFy.Items != null && custSaleInvObjforFy.Items.Count > 0)
                                        {
                                            lstFyear = CommonMethod.GetFinancialYearList();
                                            if (lstFyear != null && lstFyear.Count > 0)
                                            {
                                                if (ConfigurationManager.AppSettings["UpdateOnlyCurrentYear"] == "1")
                                                {
                                                    lstFyear = lstFyear.Where(s => s.IsCurrentFY == true).ToList();
                                                }
                                                #region FY List   
                                                if (lstFyear != null && lstFyear.Count > 0)
                                                {
                                                    foreach (var lf in lstFyear)
                                                    {
                                                        var fyInvList = (from c in custSaleInvObjforFy.Items
                                                                         where ((c.Date >= lf.fyStartDate) && (c.Date <= lf.fyEndDate))
                                                                         select c).ToList();
                                                        if (fyInvList != null && fyInvList.Count > 0)
                                                        {
                                                            double dbTotal = 0;
                                                            foreach (var iv in fyInvList)
                                                            {
                                                                if (iv.InvoiceType == "Service")
                                                                {
                                                                    if (iv.TotalTax != null && iv.Subtotal != null && iv.Subtotal < 0)
                                                                    {
                                                                        double TotalTax = Convert.ToDouble(iv.TotalTax);
                                                                        double Subtotal = Convert.ToDouble(iv.Subtotal);
                                                                        if (TotalTax < 0 && Subtotal < 0)
                                                                        {
                                                                            iv.Subtotal = Subtotal + (TotalTax * -1);
                                                                        }
                                                                    }
                                                                }
                                                                dbTotal += iv.Subtotal;
                                                            }
                                                            lf.value = Math.Round(dbTotal, 2);
                                                        }
                                                    }
                                                }
                                                #endregion
                                                #region Monthly Sale
                                                //lstFyear = CommonMethod.GetFinancialYearList();
                                                var objModel = lstFyear.Where(s => s.IsCurrentFY == true).FirstOrDefault();
                                                if (objModel != null)
                                                {
                                                    if (udfFieldList != null && udfFieldList.Count > 0)
                                                    {
                                                        DateTime lastDayOfMonth = new DateTime();
                                                        DateTime firstDayOfMonth = new DateTime();
                                                        DateTime NextMonthStartDate = new DateTime(objModel.fyStartDate.Year, objModel.fyStartDate.Month, 1);
                                                        do
                                                        {
                                                            firstDayOfMonth = NextMonthStartDate;
                                                            lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
                                                            NextMonthStartDate = new DateTime(((lastDayOfMonth.Month == 12) ? lastDayOfMonth.Year + 1 : lastDayOfMonth.Year), ((lastDayOfMonth.Month == 12) ? 1 : lastDayOfMonth.Month + 1), 1);
                                                            var objMonthWiseUDF = (from c in udfFieldList
                                                                                   where c.Month == firstDayOfMonth.Month && (c.udfFor??"") == ""
                                                                                   select c).FirstOrDefault();
                                                            if (objMonthWiseUDF != null)
                                                            {
                                                                var currentMonthInvoiceList = (from c in custSaleInvObjforFy.Items
                                                                                               where c.Date >= firstDayOfMonth && c.Date <= lastDayOfMonth
                                                                                               select c).ToList();
                                                                if (currentMonthInvoiceList != null && currentMonthInvoiceList.Count > 0)
                                                                {
                                                                    objMonthWiseUDF.Value = Math.Round((from c in currentMonthInvoiceList
                                                                                                        where c.Date >= firstDayOfMonth && c.Date <= lastDayOfMonth
                                                                                                        select c.TotalAmount).Sum(), 2);
                                                                }
                                                                else
                                                                {
                                                                    objMonthWiseUDF.Value = null;
                                                                }
                                                            }
                                                        } while (objModel.fyEndDate != lastDayOfMonth);
                                                    }
                                                }
                                                #endregion
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Find Customer Payment Last Date 

                                    strUrl = "";
                                    strUrl = cf_uri + "/Sale/CustomerPayment?$filter=Customer/UID eq guid'" + objCustomerInfo.Uid + "'&$top=1000";
                                    if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                                    {
                                        _oAuthKeyService = new OAuthKeyService();
                                        _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
                                        _oAuthKeyService.OAuthResponse.AccessToken = AccessToken;
                                    }
                                    else
                                    {
                                        if (CommonMethod.IsExpiredToken_BasedOnCookies())
                                        {
                                            _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                                        }
                                        //if (HttpContext.Current.Session[iOAuthKeyService] != null)
                                        //{
                                        //    _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
                                        //    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                                        //    {
                                        //        _oAuthKeyService = CommonMethod.RefreshToken();
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    return res = "sessionexpired";
                                        //}
                                    }
                                    //var lstCRList = CommonMethod.MakeAccountRightAPICall(strUrl, AccessToken, client_id, "");

                                    var lstCRList = CommonMethod.MakeAccountRightAPICall(strUrl, CommonMethod.GetAccessToken(), client_id, "");
                                    if (lstCRList != null && lstCRList.Count > 0)
                                    {
                                        listCustomerPaymentModel = lstCRList.ToObject<List<CustomerPaymentModel>>();
                                        if (listCustomerPaymentModel != null && listCustomerPaymentModel.Count > 0)
                                        {
                                            var objCrModel = (from c in listCustomerPaymentModel
                                                              orderby c.Date descending
                                                              select c).FirstOrDefault();
                                            if (objCrModel != null)
                                            {
                                                if (objCrModel.Date != null)
                                                {
                                                    CustLastPaymentDate = objCrModel.Date;
                                                }
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Ageing Method MYOB Balance 0-30 days, MYOB Balance 30-60 days, MYOB Balance over 60 days 

                                    strUrl = "";
                                    strUrl = cf_uri + "/Sale/Invoice/?$filter=Customer/UID eq guid'" + objCustomerInfo.Uid + "' and (CustomerPurchaseOrderNumber eq null or CustomerPurchaseOrderNumber eq '') and Status eq 'Open'&$top=1000";
                                    do
                                    {
                                        if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                                        {
                                            _oAuthKeyService = new OAuthKeyService();
                                            _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
                                            _oAuthKeyService.OAuthResponse.AccessToken = AccessToken;
                                        }
                                        else
                                        {
                                            if (CommonMethod.IsExpiredToken_BasedOnCookies())
                                            {
                                                _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                                            }
                                            //if (HttpContext.Current.Session[iOAuthKeyService] != null)
                                            //{
                                            //    _oAuthKeyService = (IOAuthKeyService)HttpContext.Current.Session[iOAuthKeyService];
                                            //    if (_oAuthKeyService.OAuthResponse != null && _oAuthKeyService.OAuthResponse.HasExpired == true)
                                            //    {
                                            //        _oAuthKeyService = CommonMethod.RefreshToken();
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    return res = "sessionexpired";
                                            //}
                                        }
                                        //JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, AccessToken, client_id, "");
                                        JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, CommonMethod.GetAccessToken(), client_id, "");
                                        if (jt1 != null)
                                        {
                                            CustomerSaleInvoice objCustInv = jt1.ToObject<CustomerSaleInvoice>();
                                            invoiceListItemForAgeingRpt.AddRange(objCustInv.Items);
                                            if (!string.IsNullOrEmpty(objCustInv.NextPageLink))
                                            {
                                                strUrl = objCustInv.NextPageLink;
                                            }
                                            else
                                            {
                                                strUrl = "";
                                                custSaleInvObjForAgeingRpt.Items = invoiceListItemForAgeingRpt;
                                            }
                                        }
                                        else
                                        {
                                            strUrl = "";
                                        }
                                    } while (!string.IsNullOrEmpty(strUrl));
                                    if (custSaleInvObjForAgeingRpt != null && custSaleInvObjForAgeingRpt.Items != null)
                                    {
                                        foreach (var agI in custSaleInvObjForAgeingRpt.Items)
                                        {
                                            DateTime InvDate = Convert.ToDateTime(agI.Date.ToShortDateString());
                                            DateTime CurrentDate = Convert.ToDateTime(DateTime.Now.ToShortDateString());
                                            double days = (CurrentDate - InvDate).TotalDays;
                                            if (days >= 0 && days <= 30)
                                            {
                                                db0To30 += Math.Round(agI.TotalAmount, 2);
                                            }
                                            else if (days >= 31 && days <= 60)
                                            {
                                                db31To60 += Math.Round(agI.TotalAmount, 2);
                                            }
                                            else if (days >= 61)
                                            {
                                                db60Plus += Math.Round(agI.TotalAmount, 2);
                                            }
                                        }
                                    }
                                    #endregion
                                    #region Orders
                                    
                                    CustomerSaleInvoice CustomerSaleOrder = new CustomerSaleInvoice();
                                    List<Item> orderListItem = new List<Item>();
                                    strUrl = "";
                                    DateTime dtDate = DateTime.Now;
                                    DateTime stStartDate = new DateTime(dtDate.Year, dtDate.Month, 1);
                                    DateTime stEndDate = stStartDate.AddMonths(1).AddDays(-1);
                                    double dbTotalAmount = 0;
                                    string strStartMonth = stStartDate.Month>=9? stStartDate.Month.ToString(): "0"+stStartDate.Month.ToString();
                                    string strStartDay = stStartDate.Day >= 9 ? stStartDate.Day.ToString() : "0" + stStartDate.Day.ToString();

                                    string strEndMonth = stEndDate.Month >= 9 ? stEndDate.Month.ToString() : "0" + stEndDate.Month.ToString();
                                    string strEndDay = stEndDate.Day >= 9 ? stEndDate.Day.ToString() : "0" + stEndDate.Day.ToString();

                                    strUrl = cf_uri + "/Sale/Order?$filter=Date ge datetime'"+ stStartDate.Year + "-"+ strStartMonth + "-"+ strStartDay + "T00:00:00' and Date le datetime'" + stEndDate.Year + "-" + strEndMonth + "-" + strEndDay + "T00:00:00' and Status eq 'Open' and Customer/UID eq guid'" + objCustomerInfo.Uid + "'&$top=1000";
                                    do
                                    {
                                        if (HttpContext.Current.Request.Url.ToString().ToLower().Contains("localhost"))
                                        {
                                            _oAuthKeyService = new OAuthKeyService();
                                            _oAuthKeyService.OAuthResponse = new MYOB.AccountRight.SDK.Contracts.OAuthTokens();
                                            _oAuthKeyService.OAuthResponse.AccessToken = AccessToken;
                                        }
                                        else
                                        {
                                            if (CommonMethod.IsExpiredToken_BasedOnCookies())
                                            {
                                                _oAuthKeyService = CommonMethod.RefreshToken_BasedOnCookies();
                                            }
                                        }
                                        JToken jt1 = CommonMethod.MakeAccountRightAPICall_SingleItemReturn(strUrl, CommonMethod.GetAccessToken(), client_id, "");
                                        if (jt1 != null)
                                        {
                                            CustomerSaleInvoice tempCustOrder = jt1.ToObject<CustomerSaleInvoice>();
                                            orderListItem.AddRange(tempCustOrder.Items);
                                            if (!string.IsNullOrEmpty(tempCustOrder.NextPageLink))
                                            {
                                                strUrl = tempCustOrder.NextPageLink;
                                            }
                                            else
                                            {
                                                strUrl = "";
                                                CustomerSaleOrder.Items = orderListItem;
                                            }
                                        }
                                        else
                                        {
                                            strUrl = "";
                                        }
                                    } while (!string.IsNullOrEmpty(strUrl));
                                    if (CustomerSaleOrder != null && CustomerSaleOrder.Items != null)
                                    {   
                                        foreach (var od in CustomerSaleOrder.Items)
                                        {
                                            dbTotalAmount += od.TotalAmount;
                                        }
                                    }
                                    if (dbTotalAmount > 0)
                                    {
                                        var objUdf = (from c in udfFieldList
                                                      where c.Month == stStartDate.Month && c.udfFor == "orders"
                                                      select c).FirstOrDefault();
                                        if (objUdf != null)
                                        {
                                            objUdf.Value = dbTotalAmount;
                                        }
                                    }
                                    #endregion

                                    //CommonMethod.LogFile(sb, false);

                                    if (linesItem != null && linesItem.Count > 0)
                                    {
                                        foreach (var li in linesItem)
                                        {
                                            if (li.Item != null)
                                            {
                                                //sb.AppendLine("");
                                                //sb.AppendLine("Number:" + li.Item.Number);
                                                //sb.AppendLine("Name:" + li.Item.Name);
                                                //sb.AppendLine("");
                                                CustomerItem.Add(li.Item);
                                            }
                                        }
                                    }
                                    //CommonMethod.LogFile(sb, false);

                                    #region Create Json Format for Get UDF field                                
                                    if (udfFieldList != null && udfFieldList.Count > 0)
                                    {
                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><span style='color: red;'> <b>Note </b>: Marked with '*' is not present in table items.</span></div>");
                                        foreach (var item in udfFieldList)
                                        {

                                            if (item.Name == "MYOB Note")
                                            {
                                                if (!string.IsNullOrEmpty(objCustomerInfo.Notes))
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.Notes ?? ""));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.Notes + "</div>");
                                                }
                                            }
                                            else if (item.Month != null && item.Month > 0 && (item.udfFor ?? "") == "")
                                            {
                                                if (item.Value != null)
                                                {
                                                    decimal dcValue = CommonMethod.TryParseDecimal(Convert.ToString(item.Value));
                                                    if (dcValue > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, item.Value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + ("$" + item.Value) + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Month != null && item.Month > 0 && (item.udfFor ?? "") == "orders")
                                            {
                                                if (item.Value != null)
                                                {
                                                    decimal dcValue = CommonMethod.TryParseDecimal(Convert.ToString(item.Value));
                                                    if (dcValue > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, item.Value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + ("$" + item.Value) + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Contact Name")
                                            {
                                                if (objCustomerInfo.Addresses != null && objCustomerInfo.Addresses.Count > 0)
                                                {
                                                    if (!string.IsNullOrEmpty(objCustomerInfo.Addresses[0].ContactName))
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.Addresses[0].ContactName));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.Addresses[0].ContactName + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "MYOB Phone")
                                            {
                                                if (objCustomerInfo.Addresses != null && objCustomerInfo.Addresses.Count > 0)
                                                {
                                                    if (!string.IsNullOrEmpty(objCustomerInfo.Addresses[0].Phone1))
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.Addresses[0].Phone1));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.Addresses[0].Phone1 + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Email")
                                            {
                                                if (objCustomerInfo.Addresses != null && objCustomerInfo.Addresses.Count > 0)
                                                {
                                                    if (!string.IsNullOrEmpty(objCustomerInfo.Addresses[0].Email))
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.Addresses[0].Email));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.Addresses[0].Email + "</div>");
                                                    }
                                                }
                                                //if (objCustomerInfo.CustomField3 != null && !string.IsNullOrEmpty(objCustomerInfo.CustomField3.Value))
                                                //{
                                                //    lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.CustomField3.Value ?? ""));
                                                //    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.CustomField3.Value ?? "" + "</div>");
                                                //}
                                            }
                                            else if (item.Name == "MYOB IsInactive")
                                            {
                                                lstJProperty.Add(new JProperty(item.UniqueKey, (objCustomerInfo.IsActive == true ? "N" : "Y")));
                                                strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + (objCustomerInfo.IsActive == true ? "N" : "Y") + "</div>");
                                            }
                                            else if (item.Name == "MYOB Identifiers")
                                            {
                                                if (objCustomerInfo.Identifiers != null && objCustomerInfo.Identifiers.Length > 0)
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.Identifiers[0].Value));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.Identifiers[0].Value + "</div>");
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Last Sales Update")
                                            {
                                                if (custSaleInvObj != null && custSaleInvObj.Items != null)
                                                {
                                                    var lastSaleInvoice = (from c in custSaleInvObj.Items
                                                                           orderby c.Date descending
                                                                           select c).FirstOrDefault();
                                                    if (lastSaleInvoice != null)
                                                    {
                                                        string dtUtcNow = lastSaleInvoice.Date.ToString("yyyy-MM-ddT00:00:00");
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, dtUtcNow));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + dtUtcNow + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\Last Payment Date")
                                            {
                                                if (CustLastPaymentDate != null)
                                                {
                                                    string strCustLastPaymentDate = Convert.ToDateTime(CustLastPaymentDate).ToString("yyyy-MM-ddT00:00:00");
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, strCustLastPaymentDate));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + strCustLastPaymentDate + "</div>");
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Last Balance update")
                                            {
                                                if (CustLastPaymentDate != null)
                                                {
                                                    string strCustLastPaymentDate = Convert.ToDateTime(CustLastPaymentDate).ToString("yyyy-MM-ddT00:00:00");
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, strCustLastPaymentDate));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + strCustLastPaymentDate + "</div>");
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Card Record ID")
                                            {
                                                //if (objCustomerInfo.DisplayId != null && objCustomerInfo.DisplayId != "*None")
                                                //{
                                                //    lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.DisplayId));
                                                //    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.DisplayId + "</div>");
                                                //}
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Total Paid")
                                            {
                                                if (TotalPaid > 0)
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, TotalPaid));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + TotalPaid + "</div>");
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Highest Invoice")
                                            {
                                                if (custSaleInvObj != null && custSaleInvObj.Items != null)
                                                {
                                                    var lastSaleInvoice = (from c in custSaleInvObj.Items
                                                                           orderby c.TotalAmount descending
                                                                           select c).FirstOrDefault();
                                                    if (lastSaleInvoice != null)
                                                    {
                                                        if (Math.Round(lastSaleInvoice.Subtotal, 2) > 0)
                                                        {
                                                            lstJProperty.Add(new JProperty(item.UniqueKey, Math.Round(lastSaleInvoice.Subtotal, 2)));
                                                            strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + Math.Round(lastSaleInvoice.Subtotal, 2) + "</div>");
                                                        }
                                                    }
                                                }

                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Balance 0-30 days")
                                            {
                                                if (db0To30 > 0)
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, db0To30));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + db0To30 + "</div>");
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Balance 30-60 days")
                                            {
                                                if (db31To60 > 0)
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, db31To60));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + db31To60 + "</div>");
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Balance over 60 days")
                                            {
                                                if (db60Plus > 0)
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, db60Plus));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + db60Plus + "</div>");
                                                }
                                            }
                                            else if (item.Name == "MYOB Products bought")
                                            {
                                                //if (item.Type == "array")
                                                //{
                                                //    if (item.UniqueKey.Contains("Udf"))
                                                //    {
                                                //        if (arrayAppointment != null)
                                                //        {
                                                //            //if (arrayAppointment[0][item.UniqueKey] != null)
                                                //            //{
                                                //            //    var PreviousValue = (arrayAppointment[0][item.UniqueKey]).Value<JArray>();
                                                //            //    strPreviousValue = fnStringToArray(PreviousValue);
                                                //            //}
                                                //            if (CustomerItem != null && CustomerItem.Count > 0)
                                                //            {
                                                //                List<string> ProductNameList = (from c in CustomerItem
                                                //                                                select c.Name).ToList();
                                                //                var resResult = fnGetTableSelectedItemId(item.UniqueKey, string.Join(",", ProductNameList));
                                                //                if (resResult != null)
                                                //                {
                                                //                    lstJProperty.Add(resResult);
                                                //                }
                                                //                if (NotExistKeyInUdfTable != null && NotExistKeyInUdfTable.Count > 0)
                                                //                {
                                                //                    foreach (var nfC in CustomerItem.Where(s => NotExistKeyInUdfTable.Contains(s.Name)).ToList())
                                                //                    {
                                                //                        nfC.Name = nfC.Name + " <label style='color: red;'> *</label>";
                                                //                    }
                                                //                }
                                                //                strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + string.Join(", ", CustomerItem.Select(s => s.Name).ToList()) + "</div>");
                                                //            }
                                                //        }
                                                //    }
                                                //}
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Items bought")
                                            {
                                                //if (item.Type == "array")
                                                //{
                                                //    if (item.UniqueKey.Contains("Udf"))
                                                //    {
                                                //        if (arrayAppointment != null)
                                                //        {
                                                //            //if (arrayAppointment[0][item.UniqueKey] != null)
                                                //            //{
                                                //            //    var PreviousValue = (arrayAppointment[0][item.UniqueKey]).Value<JArray>();
                                                //            //    strPreviousValue = fnStringToArray(PreviousValue);
                                                //            //}
                                                //            if (CustomerItem != null && CustomerItem.Count > 0)
                                                //            {
                                                //                List<string> itemBoughtList = (from c in CustomerItem
                                                //                                               select c.Number).ToList();
                                                //                var resResult = fnGetTableSelectedItemId(item.UniqueKey, string.Join(",", itemBoughtList));
                                                //                if (resResult != null)
                                                //                {
                                                //                    lstJProperty.Add(resResult);
                                                //                }
                                                //                if (NotExistKeyInUdfTable != null && NotExistKeyInUdfTable.Count > 0)
                                                //                {
                                                //                    foreach (var nfC in CustomerItem.Where(s => NotExistKeyInUdfTable.Contains(s.Number)).ToList())
                                                //                    {
                                                //                        nfC.Number = nfC.Number + " <label style='color: red;'> *</label>";
                                                //                    }
                                                //                }
                                                //                strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + string.Join(", ", CustomerItem.Select(s => s.Number).ToList()) + "</div>");
                                                //            }
                                                //        }
                                                //    }
                                                //}
                                            }
                                            else if (item.Name == "ABN")
                                            {
                                                if (objCustomerInfo.SellingDetails != null && !string.IsNullOrEmpty(objCustomerInfo.SellingDetails.Abn))
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.SellingDetails.Abn));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.SellingDetails.Abn + "</div>");
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "MYOB Last Sale Date")
                                            {
                                                if (custSaleInvObj != null && custSaleInvObj.Items != null)
                                                {
                                                    var lastSaleInvoice = (from c in custSaleInvObj.Items
                                                                           orderby c.Date descending
                                                                           select c).FirstOrDefault();
                                                    if (lastSaleInvoice != null)
                                                    {
                                                        string dtUtcNow = lastSaleInvoice.Date.ToString("yyyy-MM-ddT00:00:00");
                                                        strLastSaleDate = lastSaleInvoice.Date.ToString();
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, dtUtcNow));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + dtUtcNow + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "MYOB Credit Limit")
                                            {
                                                if (objCustomerInfo.SellingDetails != null && objCustomerInfo.SellingDetails.Credit != null)
                                                {
                                                    if (objCustomerInfo.SellingDetails.Credit.Limit != null)
                                                    {
                                                        if (Convert.ToDouble(string.Format("{0:0.00}", objCustomerInfo.SellingDetails.Credit.Limit)) > 0)
                                                        {
                                                            lstJProperty.Add(new JProperty(item.UniqueKey, Convert.ToDouble(string.Format("{0:0.00}", objCustomerInfo.SellingDetails.Credit.Limit))));
                                                            strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + string.Format("{0:0.00}", objCustomerInfo.SellingDetails.Credit.Limit) + "</div>");
                                                        }
                                                    }
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Current Balance")
                                            {
                                                if (objCustomerInfo.CurrentBalance != null)
                                                {
                                                    string strCurrentBalance = string.Format("{0:0.00}", objCustomerInfo.CurrentBalance);
                                                    if (!string.IsNullOrEmpty(strCurrentBalance))
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.CurrentBalance));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + strCurrentBalance + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2014")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2015")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2016")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2017")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2018")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2019")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2020")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2021")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "2022")
                                            {
                                                var objFY = GetFyValue(lstFyear, item.Name);
                                                if (objFY != null)
                                                {
                                                    if (objFY.value != null && objFY.value > 0)
                                                    {
                                                        lstJProperty.Add(new JProperty(item.UniqueKey, objFY.value));
                                                        strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                    }
                                                }
                                            }
                                            else if (item.Name == "MYOB Last Sale Amount")
                                            {
                                                if (custSaleInvObj != null && custSaleInvObj.Items != null)
                                                {
                                                    var lastSaleInvoice = (from c in custSaleInvObj.Items
                                                                           orderby c.Date descending
                                                                           select c).FirstOrDefault();
                                                    if (lastSaleInvoice != null)
                                                    {
                                                        if (lastSaleInvoice.TotalAmount > 0)
                                                        {
                                                            lstJProperty.Add(new JProperty(item.UniqueKey, Math.Round(lastSaleInvoice.TotalAmount, 2)));
                                                            strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + Math.Round(lastSaleInvoice.TotalAmount, 2) + "</div>");
                                                        }
                                                    }
                                                }
                                            }
                                            else if (item.Name == "MYOB Sales Person")
                                            {
                                                if (objCustomerInfo.SellingDetails != null && objCustomerInfo.SellingDetails.SalesPerson != null)
                                                {
                                                    if (item.Type == "array")
                                                    {
                                                        if (item.UniqueKey.Contains("Udf") && !string.IsNullOrEmpty(objCustomerInfo.SellingDetails.SalesPerson.Name ?? ""))
                                                        {
                                                            var resResult = fnGetTableSelectedItemId(item.UniqueKey, objCustomerInfo.SellingDetails.SalesPerson.Name);
                                                            if (resResult != null)
                                                            {
                                                                lstJProperty.Add(resResult);
                                                                strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.SellingDetails.SalesPerson.Name + "</div>");
                                                            }
                                                            else
                                                            {
                                                                strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.SellingDetails.SalesPerson.Name + "<span style='color: red;'> (Note*: Table item list not present this value)<span></div>");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Customer Since")
                                            {

                                            }
                                            else if (item.Name == "Pharmacy")
                                            {
                                                string strValue = "";
                                                if (item.Type == "array")
                                                {
                                                    if (item.UniqueKey.Contains("Udf"))
                                                    {
                                                        //if (objCustomerInfo.SellingDetails.SalesPerson != null && !string.IsNullOrEmpty(objCustomerInfo.SellingDetails.SalesPerson.Name))
                                                        //{
                                                        if (objCustomerInfo.Identifiers != null && objCustomerInfo.Identifiers.Length > 0)
                                                        {
                                                            if (objCustomerInfo.Addresses != null && objCustomerInfo.Addresses.Count > 0)
                                                            {
                                                                if (!string.IsNullOrEmpty(objCustomerInfo.Addresses[0].State))
                                                                {
                                                                    strValue = objCustomerInfo.Addresses[0].State;
                                                                }
                                                            }
                                                            if (objCustomerInfo.Identifiers[0].Label == "Pharmacy")
                                                            {
                                                                //strValue = objCustomerInfo.Identifiers[0].Value + "-" + strValue;
                                                            }
                                                        }
                                                        if (!string.IsNullOrEmpty(strValue))
                                                        {
                                                            var resResult = fnGetTableSelectedItemId(item.UniqueKey, strValue);
                                                            if (resResult != null)
                                                            {
                                                                lstJProperty.Add(resResult);
                                                                strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + strValue + "</div>");
                                                            }
                                                            else
                                                            {
                                                                strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + strValue + "<span style='color: red;'> *<span></div>");
                                                            }
                                                        }
                                                        //}
                                                    }
                                                }
                                            }
                                            else if (item.Name == "" + FinancialYearCRMFolder + "MYOB Sales YTD")
                                            {
                                                if (ConfigurationManager.AppSettings["UpdateUDF_MYOBSalesYTD"] == "1")
                                                {
                                                    if (lstFyear != null && lstFyear.Count > 0)
                                                    {
                                                        var objFY = (from c in lstFyear
                                                                     where c.IsCurrentFY == true
                                                                     select c).FirstOrDefault();
                                                        if (objFY != null)
                                                        {
                                                            if (objFY.value != null && objFY.value > 0)
                                                            {
                                                                lstJProperty.Add(new JProperty(item.UniqueKey, Math.Round(objFY.value, 2)));
                                                                strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + objFY.value + "</div>");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (item.Name == "Marketing\\Type of Customer")
                                            {
                                                if (item.Type == "array")
                                                {
                                                    if (item.UniqueKey.Contains("Udf"))
                                                    {
                                                        if (arrayAppointment != null)
                                                        {
                                                            //if (arrayAppointment[0][item.UniqueKey] != null)
                                                            //{
                                                            //    var PreviousValue = (arrayAppointment[0][item.UniqueKey]).Value<JArray>();
                                                            //    strPreviousValue = fnStringToArray(PreviousValue);
                                                            //}
                                                            if (objCustomerInfo.Identifiers != null && objCustomerInfo.Identifiers.Length > 0)
                                                            {
                                                                var resResult = fnGetTableSelectedItemId(item.UniqueKey, objCustomerInfo.Identifiers[0].Label);
                                                                if (resResult != null)
                                                                {
                                                                    lstJProperty.Add(resResult);
                                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.Identifiers[0].Label + "</div>");
                                                                }
                                                                else
                                                                {
                                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.Identifiers[0].Label + "<span style='color: red;'> *<span></div>");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                            else if (item.Name == "Unused Fields\\MYOB Overdue Balance")
                                            {
                                                if (objCustomerInfo.SellingDetails != null && objCustomerInfo.SellingDetails.Credit != null)
                                                {
                                                    if (objCustomerInfo.SellingDetails.Credit.PastDue != null)
                                                    {
                                                        if (Math.Round(objCustomerInfo.SellingDetails.Credit.PastDue ?? 0, 2) > 0)
                                                        {
                                                            lstJProperty.Add(new JProperty(item.UniqueKey, Math.Round(objCustomerInfo.SellingDetails.Credit.PastDue ?? 0, 2)));
                                                            strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>$" + string.Format("{0:0.00}", objCustomerInfo.SellingDetails.Credit.PastDue) + "</div>");
                                                        }
                                                    }
                                                }
                                            }
                                            else if (item.Name == "Contact\\Sales Email")
                                            {
                                                if (objCustomerInfo.CustomField3 != null)
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.CustomField3.Value));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.CustomField3.Value + "</div>");
                                                }
                                            }
                                            else if (item.Name == "Contact\\Sales Contact")
                                            {
                                                if (objCustomerInfo.CustomField1 != null)
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.CustomField1.Value));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.CustomField1.Value + "</div>");
                                                }
                                            }
                                            else if (item.Name == "Contact\\Sales Phone")
                                            {
                                                if (objCustomerInfo.CustomField2 != null)
                                                {
                                                    lstJProperty.Add(new JProperty(item.UniqueKey, objCustomerInfo.CustomField2.Value));
                                                    strbNotetext.AppendLine("<div style='margin-bottom:5px;'><b>" + item.Name + ": </b>" + objCustomerInfo.CustomField2.Value + "</div>");
                                                }
                                            }

                                        }
                                    }
                                    #endregion
                                    res = fnCompanyUDFBasedOnMYOBCustomerID(lstJProperty, Key, strbNotetext);
                                    if (res != "badrequestfornote" || res != "badrequestfornote")
                                    {
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

                }
            }
            catch (Exception ex)
            {
                sb.Clear();
                sb.AppendLine("UpdateCompanyUDFBasedOnMYOBCustomerID fn error!");
                sb.AppendLine("--------------");
                sb.AppendLine(ex.ToString());
                CommonMethod.LogFile(sb, false);
            }
            return res;
        }


        public static string fnCompanyUDFBasedOnMYOBCustomerID(List<JProperty> lstJProperty, string key, StringBuilder strbNotetext)
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
                             new JProperty("Key", key),
                             new JProperty("Type", "Company"),
                             lstJProperty
                         ))
                     ))
                 ).ToString();
            //instantiate the WCF service client
            using (RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient())
            {
                System.IO.Stream response = null;
                if (!string.IsNullOrEmpty(key))
                {
                    response = client.AbEntryUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
                }
                JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

                if (responseJSON.Value<int>("Code") != 0)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("-----------------------------Error----------------------------------------");
                    sb.AppendLine("Error: " + strCompanyName);
                    sb.AppendLine(Convert.ToString(responseJSON["Msg"][0]["Message"]));
                    CommonMethod.LogFile(sb, false);
                    sb.AppendLine("---------------------------------------------------------------------");
                    res = "badrequestabentry";
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
                            //strb.AppendLine("<div style='margin-bottom:5px;'><b>Total Balance: </b>" + objCustomerInfo.CurrentBalance + "</div>");
                            //strb.AppendLine("<div style='margin-bottom:5px;'><b>Overdue Balance: </b>" + objCustomerInfo.SellingDetails.Credit.PastDue + "</div>");
                            if (strbNotetext != null)
                            {
                                objNoteModel.Text = "";
                                objNoteModel.Text = Convert.ToString(strbNotetext);
                            }
                            objNoteModel.DateTime = dtUtcNow;
                        }
                        //Note
                        if (objNoteModel.Text.Length > 50000)
                        {
                            IEnumerable<string> noteTextChunk = CommonMethod.Split(objNoteModel.Text, 50000);
                            if (noteTextChunk != null && noteTextChunk.Count() > 0)
                            {
                                foreach (var item in noteTextChunk)
                                {
                                    objNoteModel.Text = item;
                                    res = CreateOrUpdateNote(objNoteModel);
                                }
                            }
                        }
                        else
                        {
                            res = CreateOrUpdateNote(objNoteModel);
                        }
                        if (!string.IsNullOrEmpty(strCompanyName) && !string.IsNullOrEmpty(strLastSaleDate))
                        {
                            StringBuilder sbLsd = new StringBuilder();
                            sbLsd.AppendLine(strLastSaleDate + " - " + strCompanyName);
                            CommonMethod.LogFileForLastSaleDate(sbLsd, true);
                        }
                        if (!string.IsNullOrWhiteSpace(res))
                        {
                            if (res == "badrequest")
                            {
                                res = "badrequestfornote";
                            }
                        }
                    }
                    //CommonMethod.LogFile(sb, false);
                    res = "success";
                }
            }
            return res;
        }
        public static void GetAbEntryProperties_new()
        {
            string token = GetToken();
            List<DropdownModel> referencemodelList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("Schema", new JObject(
                    new JProperty("Data", new JObject(
                        new JProperty("Key", "Udf/$TYPEID(82)"),//"Key":"Udf/$TYPEID(309)",
                        new JProperty("Type", "EnumField<StringItem>"),
                        new JProperty("AppliesTo", new JArray("Company", "Contact")),//"Company","Contact"
                        new JProperty("Items",
                        new JArray(new JObject()
                            {
                                { "Key", null },
                                { "Value", "F" }
                            }
                        )
                    )
                  ))
                ))
            ).ToString();

            RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient();
            System.IO.Stream response = client.FieldOptionsCreate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                //return null;
            }
            else
            {
                //JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                //var model2 = Data["Udf/$TYPEID(217)"];
                //var Referencearray = Data["Udf/$TYPEID(217)"].Value<JArray>();
                //if (Referencearray != null && Referencearray.Count > 0)
                //{
                //    referencemodelList = Referencearray.ToObject<List<DropdownModel>>();
                //}
            }
            //return referencemodelList;
        }

        public static string GetAbGetFieldInfo()
        {
            //build the request string by constructing a JSON object
            string createRequest = new JObject(

               new JProperty("Token", GetToken()),
                                    new JProperty("Schema",
                                        new JObject(
                                            new JProperty("Scope",
                                                new JObject(
                                                    new JProperty("Fields",
                                                        new JObject(
                                                            new JProperty("Key", 1)
                                                            )
                                                        )
                                                    )
                                                ),
                                            new JProperty("Criteria",
                                                new JObject(
                                                    new JProperty("SearchQuery",
                                                        new JObject(
                                                            new JObject(
                                                                new JProperty("$EQ",
                                                                    new JObject(
                                                                        new JProperty("name", "TestUDF")
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

            //instantiate the WCF service client
            using (RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient())
            {
                //call Get AbEntry Properties method of the WCF client and parse the result
                Stream response = client.AbEntryGetFieldInfo(new MemoryStream(Encoding.UTF8.GetBytes(createRequest)));
                //Stream response1 = client.FieldOptionsCreate(;
                JToken createResponse = JToken.Parse(new StreamReader(response).ReadToEnd());

                //check the return value of the response
                if (createResponse.Value<int>("Code") == 0)
                {
                    return Convert.ToString(createResponse["AbEntry"]["Data"]["properties"]);
                }
                return "";
            }
        }

        public static void GetReferenceByList()
        {
            string token = GetToken();
            List<DropdownModel> referencemodelList = new List<DropdownModel>();
            string requestString = new JObject(
                new JProperty("Token", token),
                new JProperty("Schema", new JObject(
                    new JProperty("Data", new JObject(
                        new JProperty("Key", null),
                        new JProperty("Type", "EnumField<StringItem>"),
                        new JProperty("Name", "TestUDF"),
                        new JProperty("Items",
                        new JArray(new JObject()
                            {
                                { "Key", null },
                                { "Value", "C" }
                            }
                        )
                    )
                  ))
                ))
            ).ToString();

            RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient();
            System.IO.Stream response = client.SchemaUpdate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                //there was a problem with the request
                //return null;
            }
            else
            {
                //JObject Data = (JObject)responseJSON["AbEntry"]["Data"];
                //var model2 = Data["Udf/$TYPEID(217)"];
                //var Referencearray = Data["Udf/$TYPEID(217)"].Value<JArray>();
                //if (Referencearray != null && Referencearray.Count > 0)
                //{
                //    referencemodelList = Referencearray.ToObject<List<DropdownModel>>();
                //}
            }
            //return referencemodelList;
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
            using (RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient())
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
            List<AbEntryFieldInfo> lstMonthlySaleList = getMonthUdfList();
            List<AbEntryFieldInfo> lstFyOrdersList = getFY_OrdersUdfList();
            //List<AbEntryFieldInfo> lstMonthlySaleUDFName = getMonthUdfList();
            List<string> lstUDFFieldsName = UDFFieldsName.Split(',').ToList();
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
                    var allUDfList = lstAbEntryFieldInfo;
                    lstAbEntryFieldInfo = lstAbEntryFieldInfo.Where(s => lstUDFFieldsName.Contains(s.Name)).ToList();

                    if (lstMonthlySaleList != null && lstMonthlySaleList.Count > 0)
                    {
                        foreach (var lmst in lstMonthlySaleList)
                        {
                            var objModelMonth = (from c in allUDfList
                                                 where c.Name == lmst.Name
                                                 select c).FirstOrDefault();
                            if (objModelMonth != null)
                            {
                                objModelMonth.udfFor = "";
                                objModelMonth.Month = lmst.Month;
                                lstAbEntryFieldInfo.Add(objModelMonth);
                            }
                        }
                    }

                    if (lstFyOrdersList != null && lstFyOrdersList.Count > 0)
                    {
                        foreach (var lmst in lstFyOrdersList)
                        {
                            var objModelMonth = (from c in allUDfList
                                                 where c.Name == lmst.Name
                                                 select c).FirstOrDefault();
                            if (objModelMonth != null)
                            {
                                objModelMonth.udfFor = lmst.udfFor;
                                objModelMonth.Month = lmst.Month;
                                lstAbEntryFieldInfo.Add(objModelMonth);
                            }
                        }
                    }
                }
            }

            return lstAbEntryFieldInfo;
        }
        public static List<AbEntryFieldInfo> getMonthUdfList()
        {
            var lstFyear = CommonMethod.GetFinancialYearList();
            List<string> lstMonthlySaleUDFName = new List<string>();
            List<AbEntryFieldInfo> lstAbEntryFieldInfo = new List<AbEntryFieldInfo>();
            string currentYear = "";
            string MonthlySalesCRMFolder = ConfigurationManager.AppSettings["MonthlySalesCRMFolder"] + "\\";
            if (lstFyear != null && lstFyear.Count > 0)
            {
                var objCurrentFY = (from c in lstFyear
                                    where c.IsCurrentFY == true
                                    select c).FirstOrDefault();
                if (objCurrentFY != null)
                {
                    currentYear = objCurrentFY.year.ToString();
                }
                //if (Convert.ToInt32(currentYear) >= 2020)
                //{
                    string strPath = ConfigurationManager.AppSettings["FYMonthlySalesCRMFolder"];
                    strPath = strPath.Replace("[REPLACEYEAR]", (objCurrentFY.year - 1).ToString());
                    MonthlySalesCRMFolder = strPath + "\\";
                //}
                DateTime dtStartDate = objCurrentFY.fyStartDate;
                int j = 1;
                for (int i = 1; i <= 12; i++)
                {
                    if (dtStartDate.Month == 12)
                    {
                        dtStartDate = dtStartDate.AddMonths(-11).AddYears(1);
                        j = 1;
                    }
                    if (j > 1)
                    {
                        dtStartDate = dtStartDate.AddMonths(1);
                    }
                    AbEntryFieldInfo objModel = new AbEntryFieldInfo();
                    //var str = MonthlySalesCRMFolder + currentYear + " " + System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(i).Substring(0, 3).ToUpper();
                    objModel.Name = MonthlySalesCRMFolder + dtStartDate.Year + " " + System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(dtStartDate.Month).Substring(0, 3).ToUpper();
                    objModel.Month = dtStartDate.Month;
                    objModel.Year = dtStartDate.Year;
                    lstAbEntryFieldInfo.Add(objModel);
                    j++;
                }
            }
            return lstAbEntryFieldInfo;
        }
        public static List<AbEntryFieldInfo> getFY_OrdersUdfList()
        {
            var lstFyear = CommonMethod.GetFinancialYearList();
            List<string> lstMonthlySaleUDFName = new List<string>();
            List<AbEntryFieldInfo> lstAbEntryFieldInfo = new List<AbEntryFieldInfo>();
            string currentYear = "";
            string MonthlySalesCRMFolder = "";
            if (lstFyear != null && lstFyear.Count > 0)
            {
                var objCurrentFY = (from c in lstFyear
                                    where c.IsCurrentFY == true
                                    select c).FirstOrDefault();
                if (objCurrentFY != null)
                {
                    currentYear = objCurrentFY.year.ToString();
                }

                string strPath = ConfigurationManager.AppSettings["FYOrdersCRMFolder"];
                strPath = strPath.Replace("[REPLACEYEAR]", (objCurrentFY.year - 1).ToString());
                MonthlySalesCRMFolder = strPath + "\\";

                DateTime dtStartDate = objCurrentFY.fyStartDate;
                int j = 1;
                for (int i = 1; i <= 12; i++)
                {
                    if (dtStartDate.Month == 12)
                    {
                        dtStartDate = dtStartDate.AddMonths(-11).AddYears(1);
                        j = 1;
                    }
                    if (j > 1)
                    {
                        dtStartDate = dtStartDate.AddMonths(1);
                    }
                    AbEntryFieldInfo objModel = new AbEntryFieldInfo();
                    //var str = MonthlySalesCRMFolder + currentYear + " " + System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(i).Substring(0, 3).ToUpper();
                    objModel.Name = MonthlySalesCRMFolder + dtStartDate.Year + " " + System.Globalization.DateTimeFormatInfo.CurrentInfo.GetMonthName(dtStartDate.Month).Substring(0, 3).ToUpper();
                    objModel.Month = dtStartDate.Month;
                    objModel.Year = dtStartDate.Year;
                    objModel.udfFor = "orders";
                    lstAbEntryFieldInfo.Add(objModel);
                    j++;
                }
            }
            return lstAbEntryFieldInfo;
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

            RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient();
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
        public static JProperty fnGetTableSelectedItemId(string UniqueKey, string Value)
        {
            NotExistKeyInUdfTable.Clear();
            List<DropdownModel> tempModelList = new List<DropdownModel>();
            tempModelList = GetAbEntryGetFieldOptions(UniqueKey);
            if (tempModelList != null && tempModelList.Count > 0)
            {
                if (!string.IsNullOrEmpty(Value))
                {
                    string[] arryItem = Value.Split(',');
                    List<string> ayyId = new List<string>();
                    if (arryItem != null && arryItem.Length > 0)
                    {
                        foreach (var ari in arryItem)
                        {
                            string modelItemId = (from c in tempModelList
                                                  where ari.ToLower().Trim().Contains(c.Value.ToLower().Trim())
                                                  select c.Key).FirstOrDefault();
                            if (!string.IsNullOrEmpty(modelItemId))
                            {
                                ayyId.Add(modelItemId);
                            }
                            else
                            {
                                NotExistKeyInUdfTable.Add(ari);
                            }
                        }
                    }
                    if (ayyId != null && ayyId.Count > 0)
                    {
                        //if (strPreviousValue != null && strPreviousValue.Count > 0)
                        //{
                        //    strPreviousValue = (from i in strPreviousValue
                        //                        where !ayyId.Contains(i)
                        //                        select i).ToList();
                        //    ayyId.AddRange(strPreviousValue);
                        //}
                        return new JProperty(UniqueKey, new JArray(ayyId.ToArray()));
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        public static List<string> fnStringToArray(JArray ArrayValue)
        {
            strPreviousValue.Clear();
            if (ArrayValue != null && ArrayValue.Count > 0)
            {
                return ArrayValue.ToObject<List<string>>();
            }
            else { return new List<string>(); }
        }
        #region Create Company
        public static string CreateCompany(CustomerInfo objCustomerInfo)
        {

            StringBuilder divNoteText = new StringBuilder();

            divNoteText.Clear();
            string MYOBUID_UDF = getMyOBUid_UDFKey();
            List<JProperty> lstJProperty = new List<JProperty>();
            divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>CompanyName:</b> " + objCustomerInfo.CompanyName + "</div>");
            if (objCustomerInfo.Addresses != null && objCustomerInfo.Addresses.Count > 0)
            {
                if (!string.IsNullOrEmpty(objCustomerInfo.Addresses[0].Phone1))
                {
                    lstJProperty.Add(new JProperty("Phone", objCustomerInfo.Addresses[0].Phone1));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Phone:</b> " + objCustomerInfo.Addresses[0].Phone1 + "</div>");
                }
                if (!string.IsNullOrEmpty(objCustomerInfo.Addresses[0].Email))
                {
                    lstJProperty.Add(new JProperty("Email", objCustomerInfo.Addresses[0].Email));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Email:</b> " + objCustomerInfo.Addresses[0].Email + "</div>");
                }
                if (!string.IsNullOrEmpty(objCustomerInfo.Uid))
                {
                    lstJProperty.Add(new JProperty(MYOBUID_UDF, objCustomerInfo.Uid));
                    divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Uid:</b> " + objCustomerInfo.Uid + "</div>");
                }
                lstJProperty.Add(
                new JProperty("Address",
                       new JObject()
                           {
                                { "Key", null },
                                { "Description", "Main Address" },
                                { "AddressLine1", objCustomerInfo.Addresses[0].Street},
                                { "City", objCustomerInfo.Addresses[0].City},
                                { "ZipCode", objCustomerInfo.Addresses[0].PostCode},
                                { "Country", objCustomerInfo.Addresses[0].Country},
                                { "StateProvince", objCustomerInfo.Addresses[0].State},
                                { "Default", true }
                           }
                       ));
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>AddressLine1:</b> " + objCustomerInfo.Addresses[0].Street + "</div>");
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>City:</b> " + objCustomerInfo.Addresses[0].City + "</div>");
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>ZipCode:</b> " + objCustomerInfo.Addresses[0].PostCode + "</div>");
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>Country:</b> " + objCustomerInfo.Addresses[0].Country + "</div>");
                divNoteText.AppendLine("<div style='margin-bottom:5px;'><b>StateProvince:</b> " + objCustomerInfo.Addresses[0].State + "</div>");
            }
            string requestString = new JObject(
                new JProperty("Token", GetToken()),
                new JProperty("AbEntry", new JObject(
                    new JProperty("Data", new JObject(
                        new JProperty("Key", null),
                        new JProperty("Type", "Company"),
                        new JProperty("CompanyName", objCustomerInfo.CompanyName),
                        lstJProperty
                    ))
                ))
            ).ToString();

            RigonServiceReferenceData.DataClient client = new RigonServiceReferenceData.DataClient();
            System.IO.Stream response = client.AbEntryCreate(new System.IO.MemoryStream(Encoding.UTF8.GetBytes(requestString)));
            JToken responseJSON = JToken.Parse(new System.IO.StreamReader(response).ReadToEnd());

            if (responseJSON.Value<int>("Code") != 0)
            {
                return null;
            }
            else
            {
                NotesModel objNoteModel = new NotesModel();
                string dtUtcNow = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:MM:ssZ");
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
                        objNoteModel.DateTime = dtUtcNow;
                    }
                    //Note
                    string rtnRes = CreateOrUpdateNote(objNoteModel);

                    if (!string.IsNullOrWhiteSpace(rtnRes))
                    {
                        if (rtnRes == "badrequest")
                        {
                        }
                    }
                }
                return Data.Value<String>("Key");
            }
        }

        public static FinancialYearModel GetFyValue(List<FinancialYearModel> lstFyear, string key)
        {
            FinancialYearModel model = new FinancialYearModel();
            if (lstFyear != null && lstFyear.Count > 0)
            {

                model = (from c in lstFyear
                         where c.key == key
                         select c).FirstOrDefault();
            }
            return model;
        }
        #endregion

        public static string getMyOBUid_UDFKey()
        {
            List<AbEntryFieldInfo> udfFieldList = new List<AbEntryFieldInfo>();
            string udfkey = "";
            udfFieldList = RigonCRMReference.GetFieldList();
            if (udfFieldList != null && udfFieldList.Count > 0)
            {
                var objUdf = udfFieldList.Select(s => s).Where(s => s.Name.ToLower().Trim() == "myob uid".Trim()).FirstOrDefault();
                if (objUdf != null)
                {
                    udfkey = objUdf.UniqueKey;
                }
            }
            return udfkey;
        }

    }
}




