using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Configuration;
using System.Web.Hosting;
using Microsoft.Win32;
using System.Security.Permissions;
using WF.Common.Configuration;
using WF.Common.SMTP;

[assembly: RegistryPermissionAttribute(SecurityAction.RequestMinimum,
ViewAndModify = "HKEY_CURRENT_USER", Unrestricted = true)]
namespace WF.Common.EventLogging
{
    [Serializable]
    public class EmailPublisher:EventPublisher
    {
        private string EmailToAddress = "EmployeeSurveillance@wellsfargo.com";
        private string EmailFromAddress = "EmployeeSurveillance@wellsfargo.com";

        private int MAX_EMAILS_PER_HOUR = 1;
        private int EMAILS_INTERVAL = 15;

        public EmailPublisher(ConfigErrorHandler objConfig): base(objConfig)
        {
            this.Initialize(); 
        }

        public override bool PublishEvent(EventDetails objEventDetails)
        {
           
            bool RtnVal = false;
            
            try
            {
                string strEnvironment = "";
                try
                {
                    strEnvironment = System.Environment.GetEnvironmentVariable("APP_ENVIRONMENT");
                    if (!string.IsNullOrEmpty(strEnvironment))  
                        strEnvironment = strEnvironment + ": ";
                }
                catch
                {
                    //DO NOTHING
                }
                if (isOKToSendEmail())
                {
                    RtnVal = SendMail(strEnvironment + objEventDetails.ApplicationName, CreateMessage(objEventDetails));
                }
            }
            catch
            {
                throw;
            }
            return RtnVal;
        }

        private void Initialize()
        {
            try
            {
                SetPublisherEventTypes();

                if (!string.IsNullOrEmpty(base.Configuration.FromAccount))
                {
                    this.EmailFromAddress = base.Configuration.FromAccount;
                }
                if (!string.IsNullOrEmpty(base.Configuration.ToAccount))
                {
                    this.EmailToAddress = base.Configuration.ToAccount;
                }
            }
            catch
            {
                throw;
            }
        }

        private string CreateMessage(EventDetails objEventDetails)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                string strEnvironment = System.Environment.GetEnvironmentVariable("APP_ENVIRONMENT");
                sb.AppendLine("Environment: " + strEnvironment);
                sb.AppendLine("Server Name: " + objEventDetails.ServerName);
                sb.AppendLine("Event Time: " + objEventDetails.EventTime.ToString());
                sb.AppendLine("Class Name: " + objEventDetails.ClassName);
                sb.AppendLine("Method Name: " + objEventDetails.MethodName);
                sb.AppendLine("Event Message: " + objEventDetails.EventMessage + "\n");
                sb.AppendLine("Event Details: " + objEventDetails.EventDetail + "\n");
                sb.AppendLine("Stack Trace: " + objEventDetails.EventTrace + "\n");
                sb.AppendLine("Event Level: " + Enum.Parse(typeof(EventType), objEventDetails.EventCategory.ToString()).ToString());
                return sb.ToString(); 
            }
            catch (Exception ex)
            {
                sb.AppendLine("Error While creating Message body. Error Message:" + ex.Message + ", Error Details: " + ex.StackTrace);                    
            }
            return sb.ToString(); 
        }

        private bool SendMail(string MessageSubject, string MessageBody)
        {
            bool RtnVal = false;
            try
            {
                Mail oMail = new Mail();
                if(!string.IsNullOrEmpty(base.Configuration.MailServer))
                { oMail.MailServer = base.Configuration.MailServer; }
                
                oMail.Subject = MessageSubject;
                oMail.Body = MessageBody;
                oMail.Recipients = this.EmailToAddress;
                oMail.From = this.EmailFromAddress;

                if (oMail.SendMail())
                { throw new Exception(oMail.ErrorMsg); }

                RtnVal = true;   
            }
            catch
            {
                throw;
            }
            return RtnVal; 
        }

        /// <summary>
        /// THis method is a built in throttle to prevent spamming of email
        /// </summary>
        /// <returns></returns>
        private bool isOKToSendEmail()
        {
            bool blnResult = false;
            if(HostingEnvironment.IsHosted){return true;}
                
            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks);

            RegistryKey regkey = GetAppSettingsKey("Software\\" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            string LastSentMail = ((string)regkey.GetValue("LastSentMail"));
            string SentCount = ((string)regkey.GetValue("SentCount"));
            if (LastSentMail != "" && ts.Subtract(new TimeSpan(Convert.ToInt64(LastSentMail))).TotalMinutes < EMAILS_INTERVAL)
            {
                if (SentCount != "")
                {
                    if (Convert.ToInt32(SentCount) < MAX_EMAILS_PER_HOUR)
                    {
                        int i = Convert.ToInt32(SentCount);
                        i++;
                        blnResult = true;
                        regkey.SetValue("SentCount", i.ToString());
                    }
                }
                else
                {
                    regkey.SetValue("SentCount", "1");
                    blnResult = true;

                }
            }
            else
            {
                regkey.SetValue("LastSentMail", DateTime.Now.Ticks.ToString());
                regkey.SetValue("SentCount", "1");
                blnResult = true;
            }

            return blnResult;
        }

        #region Registry Methods
        public object GetRegistryValue(string Key)
        {
            RegistryKey regkey = GetAppSettingsKey("Software\\" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            return (regkey.GetValue(Key));
        }
        public void SetRegistryValue(string Key, object value)
        {
            RegistryKey regkey = GetAppSettingsKey("Software\\" + System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            regkey.SetValue(Key, value);
        }

        private RegistryKey GetAppSettingsKey(string RegPath)
        {

            RegistryKey regResult = Registry.CurrentUser;
            string[] strRegPath = RegPath.Split("\\".ToCharArray());
            foreach (string strSubKey in strRegPath)
            {
                regResult = GetRegKey(regResult, strSubKey);
            }

            return regResult;
        }

        private RegistryKey GetRegKey(RegistryKey CurrentKey, string strSubKey)
        {
            RegistryKey regResult;
            string[] strNames = CurrentKey.GetSubKeyNames();

            bool blnKeyFound = false;
            foreach (string KeyName in strNames)
            {
                if (KeyName == strSubKey)
                {
                    blnKeyFound = true;
                    break;
                }
            }

            if (blnKeyFound)
            { regResult = CurrentKey.OpenSubKey(strSubKey, true); }
            else
            { regResult = CurrentKey.CreateSubKey(strSubKey); }

            return regResult;
        }
        #endregion
    }
}
