using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Diagnostics;
using WF.Common.Configuration;

namespace WF.Common.EventLogging
{
    [Serializable]
    public class EventLogger
    {
        private ConfigEnvironment _Config;
        private List<IEventPublisher> _EventPublisherList;
        private EventDetails _lastEventOccurred;

        public EventLogger(ConfigEnvironment oConfig)
        {
            _Config = oConfig;
 
            if (this.Initialize())
            {
                this.GetEventPublisher();
            }
        }

        public EventDetails LastEventOccurred
        {
            get
            {
                return _lastEventOccurred; 
            }
        }

        public void ClearLastEventOccurred()
        {
            _lastEventOccurred = null;
        }

        private bool Publish(EventType _EventType, string EventMessage, string EventDetails, string EventTrace, string ClassName, string MethodName)
        {
            bool RtnVal = true;
            try
            {
                _lastEventOccurred = null; 
                EventDetails objEventDetails = new EventDetails();
                objEventDetails.EventMessage = EventMessage;
                objEventDetails.EventDetail = EventDetails;
                objEventDetails.EventTrace = EventTrace;
                objEventDetails.ClassName = ClassName;
                objEventDetails.MethodName = MethodName;
                objEventDetails.EventCategory = _EventType;
                _lastEventOccurred = objEventDetails; 
                foreach (EventPublisher _EventPublisher in _EventPublisherList)
                {
                    if (_EventPublisher.LogEventTypes.Contains(_EventType))
                    {
                        try
                        {
                            if (!_EventPublisher.PublishEvent(objEventDetails))
                            {
                                RtnVal = false;
                            }
                        }
                        catch
                        {
                            throw;
                        }
                    }
                }
            }
            catch
            {
                RtnVal = false;                
            }
            return RtnVal; 
        }
        private bool Publish(EventDetails objEventDetails)
        {
            bool RtnVal = true;
            try
            {
                _lastEventOccurred = objEventDetails;
                foreach (EventPublisher _EventPublisher in _EventPublisherList)
                {
                    if (_EventPublisher.LogEventTypes.Contains(objEventDetails.EventCategory))
                    {
                        try
                        {
                            if (!_EventPublisher.PublishEvent(objEventDetails))
                            {
                                RtnVal = false;
                            }
                        }
                        catch
                        {
                            throw;
                        }
                    }
                }
            }
            catch
            {
                RtnVal = false;
            }
            return RtnVal; 
        }
        public bool PublishEvent(EventDetails objEventDetails)
        {
            return Publish(objEventDetails);
        }

        public bool PublishEvent(EventType _EventType, string EventMessage, string EventDetails, string EventTrace)
        {
            return Publish(_EventType, EventMessage, EventDetails, EventTrace, GetClassName(2), GetMethodName(2));
        }

        
        public bool PublishEvent(EventType _EventType, string EventMessage, string EventDetails, string EventTrace, string ClassName, string MethodName)
        {
            return Publish(_EventType, EventMessage, EventDetails, EventTrace, ClassName, MethodName);
        }

        public bool PublishEvent(Exception ex, string EventDetail)
        {
            EventDetails objEventDetails = new EventDetails();
            objEventDetails.EventMessage = ex.Message;
            objEventDetails.EventDetail = EventDetail;
            objEventDetails.ApplicationName = ex.Source;
            objEventDetails.EventTrace = ex.StackTrace;
            objEventDetails.ClassName = GetClassName(2);
            objEventDetails.MethodName = GetMethodName(2);
            objEventDetails.EventCategory = EventType.Error;
            return this.Publish(objEventDetails);
        }

        public bool PublishEvent(Exception ex)
        {
            return PublishEvent(ex, "");
        }

        public string GetClassName(int frameIndex)
        {
            string RtnVal = "";

            try
            {
                RtnVal = new StackTrace().GetFrame(frameIndex).GetMethod().DeclaringType.AssemblyQualifiedName;
            }
            catch (Exception ex)
            {
                string ErrorMessage = "Unable to retrieve class name from stack trace : Error is " + ex.Message;
                throw (new Exception(ErrorMessage, ex));
            }
            return RtnVal;
        }


        public string GetMethodName(int frameIndex)
        {
            string RtnVal = "";

            try
            {
                RtnVal = new StackTrace().GetFrame(frameIndex).GetMethod().Name;
            }
            catch (Exception ex)
            {
                string ErrorMessage = "Unable to retrieve method name from stack trace : Error is " + ex.Message;
                throw (new Exception(ErrorMessage, ex));
            }
            return RtnVal;
        }

        private bool Initialize()
        {
            bool RtnVal = false;

            try
            {
                _EventPublisherList = new List<IEventPublisher>();
                RtnVal = true;
            }
            catch
            {
                throw;
            }

            return RtnVal;
        }

        private bool GetEventPublisher()
        {
            bool RtnVal = false;
            try
            {
                if (_Config == null)
                    return false;

                foreach (ConfigErrorHandler oHandler in _Config.ErrorHandlers)
                {
                    switch (oHandler.HandlerType.ToUpper())
                    {
                        case "EMAIL":
                            _EventPublisherList.Add(new EmailPublisher(oHandler));
                            break;
                        case "FLATFILE":
                            _EventPublisherList.Add(new FlatFilePublisher(oHandler));
                            break;
                        case "CUSTOM":
                            if (!string.IsNullOrEmpty(oHandler.CustomPublisher))
                            {
                                _EventPublisherList.Add(GetCustomDBPublisher(oHandler.CustomPublisher, oHandler));
                            }
                            break;
                    }
                }
                if (_EventPublisherList.Count > 0) { RtnVal = true; }
                      
            }
            catch
            {
                throw; 
            }
            return RtnVal; 
        }

        protected string GenerateTraceInformation(Exception AppEx)
        {
            string RtnVal = "";
            bool TestForInnerEx = true;
            Exception CurEx = null;

            if (AppEx != null)
            {
                RtnVal = (AppEx != null) ? this.GenerateTraceInfoHeader(AppEx) : "";

                CurEx = AppEx;

                while (TestForInnerEx)
                {
                    if (CurEx.InnerException != null)
                    {
                        RtnVal += "\n";
                        RtnVal += " : " + CurEx.InnerException.Message;
                        CurEx = CurEx.InnerException;
                    }
                    else
                    {
                        TestForInnerEx = false;
                    }
                }
            }
            else
            {
                RtnVal = "Error [detail] information was not sent to publishing function";
            }

            return RtnVal;
        }

        protected string GenerateTraceInfoHeader(Exception AppEx)
        {
            string RtnVal = "";

            if (AppEx != null)
            {
                RtnVal = "Machine Name:" + System.Environment.MachineName + "; " + "\n";
                RtnVal += "Error Message:" + AppEx.Message + "\n";
                RtnVal += "Error Details:" + AppEx.StackTrace;
            }
            else
            {
                RtnVal = "Error [header] information was not sent to publishing function";
            }
            return RtnVal;
        }
        protected EventPublisher GetCustomDBPublisher(string customDBClassDetails, ConfigErrorHandler oConfig)
        {
            string[] aryClassDetails = customDBClassDetails.Split(',');
            object[] args = new object[1];
            args[0] = oConfig;
            System.Runtime.Remoting.ObjectHandle handle = Activator.CreateInstance(aryClassDetails[1],
                                                                        aryClassDetails[0],
                                                                        true,
                                                                        0,
                                                                        null,
                                                                        args,
                                                                        null,
                                                                        null);
            return (EventPublisher)handle.Unwrap();
        }
    }
}
