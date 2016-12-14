using System;
using System.Collections.Generic;
using System.Text;

namespace WF.Common.EventLogging
{
    [Serializable]
    public class EventDetails
    {

        private string _EventMessage;
        private string _EventDetail;
        private string _EventTrace;
        private string _ClassName;
        private string _MethodName;
        private EventType _EventCategory;
        //private int _TicketId;
        //private int _IssueId;
        private DateTime _EventTime = DateTime.Now;
        private string _ServerName = System.Environment.MachineName;

        //public int TicketId
        //{
        //    get { return _TicketId; }
        //    set { _TicketId = value; }
        //}

        //public int IssueId
        //{
        //    get { return _IssueId; }
        //    set { _IssueId = value; }
        //}

        public string ApplicationName { get; set; }

        public string EventMessage
        {
            get { return _EventMessage; }
            set { _EventMessage = value; }
        }       

        public string EventDetail
        {
            get { return _EventDetail; }
            set { _EventDetail = value; }
        }       

        public string EventTrace
        {
            get { return _EventTrace; }
            set { _EventTrace = value; }
        }       

        public string ClassName
        {
            get { return _ClassName; }
            set { _ClassName = value; }
        }

        public string MethodName
        {
            get { return _MethodName; }
            set { _MethodName = value; }
        }        

        public EventType EventCategory
        {
            get { return _EventCategory; }
            set { _EventCategory = value; }
        }

        public DateTime EventTime
        {
            get { return _EventTime; }
        }

        public string ServerName
        {
            get { return _ServerName; }
        }
    }
}
