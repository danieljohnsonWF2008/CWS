using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using WF.Common.Configuration;
using System.Configuration;

namespace WF.Common.EventLogging
{
    public class FlatFilePublisher : EventPublisher
    {
        private const string MessageHeader = "------------------------------ Event Start -----------------------------------";
        private const string MessageFooter = "------------------------------ Event End -----------------------------------";
        private string _FilePath = "";

        public FlatFilePublisher(ConfigErrorHandler objConfig): base(objConfig)
        {
            this.Initialize();
        }

        public override bool PublishEvent(EventDetails objEventDetails)
        {
            bool RtnVal = false;
            try
            {
                if (_FilePath != null && _FilePath != String.Empty)
                {
                    string FileName = _FilePath + DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + ".log";
                    RtnVal = SaveToDisk(FileName, CreateMessage(objEventDetails));   
                }
                else
                {
                    RtnVal = false;
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
                
                if (!string.IsNullOrEmpty(base.Configuration.FilePath))
                    _FilePath = base.Configuration.FilePath;
                
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
                sb.AppendLine(MessageHeader);
                sb.AppendLine("Environment: " + strEnvironment);
                sb.AppendLine("Server Name: " + objEventDetails.ServerName);
                sb.AppendLine("Event Time: " + objEventDetails.EventTime.ToString());
                sb.AppendLine("Event Message: " + objEventDetails.EventMessage);
                sb.AppendLine("Event Details: " + objEventDetails.EventDetail);
                sb.AppendLine("Stack Trace: " + objEventDetails.EventTrace);
                sb.AppendLine("Event Level: " + Enum.Parse(typeof(EventType), objEventDetails.EventCategory.ToString()).ToString());
                sb.AppendLine("Class Name: " + objEventDetails.ClassName);
                sb.AppendLine("Method Name: " + objEventDetails.MethodName);
                sb.AppendLine(MessageFooter);
                sb.AppendLine(Environment.NewLine);
            }
            catch (Exception ex)
            {
                sb.Append("Error while creating Text for Flat File Publishing.Message:" + ex.Message + ", Trace" + ex.StackTrace);
            }
            return sb.ToString();
        }

        private bool SaveToDisk(string FileName, string Message)
        {
            bool RtnVal = false;
            TextWriter tw = null;
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(FileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(FileName));     
                }
                if (File.Exists(FileName))
                {
                    tw = File.AppendText(FileName);
                }
                else
                {
                    tw = File.CreateText(FileName);
                }
                tw.Write(Message);
                tw.Flush();
                RtnVal = true;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (tw != null)
                    tw.Close();
            }
            return RtnVal;
        }
    }
}
