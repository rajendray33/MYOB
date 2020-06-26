using System.Configuration;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;
using MYOB.AccountRight.SDK;
using MYOB.AccountRight.SDK.Contracts;
using Newtonsoft.Json;

namespace EnquiryInsertToCRM.Models
{
    public class OAuthKeyService : IOAuthKeyService
    {
        private string CsTokensFile = ConfigurationManager.AppSettings["TokensFilePath"];

        public OAuthTokens _tokens;

        /// <summary>
        /// On creation read any settings from file
        /// </summary>
        /// <remarks></remarks>
        public OAuthKeyService()
        {
           // ReadFromFile();
        }

        #region IOAuthKeyService Members

        /// <summary>
        /// Implements the property for OAuthResponse which holdes theTokens
        /// </summary>
        /// <value>object containing OAuthTokens</value>
        /// <returns>Contracts.OAuthTokens</returns>
        /// <remarks>Saves to isolated storage when set</remarks>
        public OAuthTokens OAuthResponse
        {
            get { return _tokens; }
            set
            {
                _tokens = value;
                //SaveToFile();
            }
        }
        #endregion

        /// <summary>
        /// Method to read Tokens from Isolated storage
        /// </summary>
        /// <remarks></remarks>
        private void ReadFromFile()
        {
            try
            {
                // Get an isolated store for user and application                 
                if (File.Exists(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile)))
                {

                    var isoStream = new FileStream(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile), FileMode.Open, FileAccess.Read);
                    //var isoStream = new IsolatedStorageFileStream(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile), FileMode.Open,
                    //                                                              FileAccess.Read, FileShare.Read);
                    var reader = new StreamReader(isoStream);
                    // Read the data.

                    _tokens = JsonConvert.DeserializeObject<OAuthTokens>(reader.ReadToEnd());
                    if (_tokens != null && _tokens.HasExpired == true)
                    {
                        _tokens = null;

                    }
                    reader.Close();
                    isoStream.Dispose();
                }
                else
                {

                    _tokens = null;


                }
            }
            catch (FileNotFoundException)
            {

                // Expected exception if a file cannot be found. This indicates that we have a new user.
                _tokens = null;
            }
        }


        /// <summary>
        /// Method to save tokens to isolated storage
        /// </summary>
        /// <remarks></remarks>
        private void SaveToFile()
        {
          
            if (File.Exists(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile)))
            {
                
                File.Delete(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile));
            }
            using (StreamWriter sw = File.CreateText(System.Web.HttpContext.Current.Server.MapPath(CsTokensFile)))
            {
                
                sw.Write(JsonConvert.SerializeObject(_tokens));
                sw.Dispose();
                sw.Close();
               
            }
            
           
            //// Get an isolated store for user and application 
            //IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(
            //    IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null);

            //// Create a file
            //var isoStream = new IsolatedStorageFileStream(CsTokensFile, FileMode.OpenOrCreate,
            //                                              FileAccess.Write, isoStore);
            //isoStream.SetLength(0);
            ////Position to overwrite the old data.

            //// Write tokens to file
            //var writer = new StreamWriter(isoStream);
            //writer.Write(JsonConvert.SerializeObject(_tokens));
            //writer.Close();

            //isoStore.Dispose();
            //isoStore.Close();
        }
    }
}
