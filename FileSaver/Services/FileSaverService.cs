using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Http;
using System.ServiceModel.Web;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Hosting;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Text;

namespace FileSaver
{
    public class FileSaverService : IFileSaverService
    {
        #region Constants
        public const string UserName = "Langdon";
        public const string PassWord = "ChangeMe123!";
        public const string Error = "Invalid: {0}";
        #endregion

        [WebInvoke(UriTemplate = "", Method = "POST")]
        public string InsertDeclaration(Declaration dec)
        {
            if (dec == null)
                throw new WebFaultException(HttpStatusCode.BadRequest);

            var x = SaveToFile(dec.inputData, dec.fileName);

            return x;
        }

        [WebInvoke(UriTemplate = "/send-object", Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped)]
        public string SerializeObject (Message serializableObject)
        {
            var headerCredentials = GetCredentialsFromHeader();
            var userCredentials = CheckUserCredentials(headerCredentials);
            if (!userCredentials.Username || !userCredentials.Password)
                throw new WebFaultException<string>(string.Format(Error, userCredentials.Error), HttpStatusCode.Unauthorized);                

            if (serializableObject == null)
                throw new WebFaultException(HttpStatusCode.BadRequest);
            
            try
            {
                var message = serializableObject.ToString();
                XDocument doc;
                using (StringReader s = new StringReader(message))
                {
                    doc = XDocument.Load(s);
                    var companyReference = doc.Root.Element("CompanyReference").Value;
                    var date = DateTime.Now;
                    string combiDate = date.ToString("yyyyMMddHHmmss");
                    doc.Save(HostingEnvironment.MapPath("/" + ConfigurationManager.AppSettings["FilePath"] + "/" + companyReference + "_" + combiDate + ".xml"));
                    return "File has been saved";
                }

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
            
        }

        [WebInvoke(UriTemplate="/system-check", Method="GET")]
        public SystemCheck checkSystem()
        {
            SystemCheck check = new SystemCheck();
            check.SystemOnline = true;
            check.FolderPath = HostingEnvironment.MapPath("/" + ConfigurationManager.AppSettings["FilePath"] + "/");
            try
            {
                // Attempt to get a list of security permissions from the folder. 
                // This will raise an exception if the path is read only or do not have access to view the permissions. 
                System.Security.AccessControl.DirectorySecurity ds = Directory.GetAccessControl(check.FolderPath);
                check.CorrectFolderRights =  true;
            }
            catch (Exception e)
            {
                check.CorrectFolderRights =  false;
                check.Errors = e.ToString();
            }
            return check;
        }

        #region Private Functions

        private string SaveToFile (string inputData, string fileName) {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(inputData);
                StreamWriter path = new StreamWriter(HostingEnvironment.MapPath("/" + ConfigurationManager.AppSettings["FilePath"] + "/" + fileName + ".xml"));
                xmlDoc.Save(path);
                path.Close();
                return "File has been saved";
            }
            catch (Exception e)
            {
                return e.ToString();
            }
        }

        private static Credentials CheckUserCredentials(string credentials)
        {
            Credentials cred = new Credentials();

            if (string.IsNullOrWhiteSpace(credentials) || credentials.StartsWith("Basic ") == false)
            {
                cred.Error = "No authentication available";
                return cred;
            }

            Match UserInfo = Regex.Match(credentials, @"Basic (?<Base>.*)?");

            if (UserInfo.Success)
            {
                byte[] converted = Convert.FromBase64String(UserInfo.Groups["Base"].Value);
                string userPass = Encoding.UTF8.GetString(converted);
                Match userAndPass = Regex.Match(userPass, @"(?<User>.*)\:(?<Pass>.*)");
                if (userAndPass.Success)
                {
                    cred.Username = UserName.Equals(userAndPass.Groups["User"].Value);
                    cred.Password = PassWord.Equals(userAndPass.Groups["Pass"].Value);
                }

                if (!cred.Username)
                {
                    cred.Error = "Username";
                } 
                else if (!cred.Password)
                {
                    cred.Error = "Password";
                }
                else if (!cred.Username && !cred.Password)
                {
                    cred.Error = "Username and password";
                }
            }

            return cred;
        }

        private string GetCredentialsFromHeader()
        {
            var headers = WebOperationContext.Current.IncomingRequest.Headers;
            return headers[HttpRequestHeader.Authorization];
        }


        #endregion
    }
}