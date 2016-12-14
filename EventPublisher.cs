using System;
using System.Collections.Generic;
using System.Text;
using WF.Common.Configuration;
using System.Configuration;

namespace WF.Common.EventLogging
{
    public abstract class EventPublisher : EventLogging.IEventPublisher
    {
        private ConfigErrorHandler _Configuration;
        private List<EventType> _LogEventTypes;


        public EventPublisher(ConfigErrorHandler objConfig)
        {
            _Configuration = objConfig;
            _LogEventTypes = new List<EventType>(); 
        }

        public abstract bool PublishEvent(EventDetails objEventDetails);

        protected ConfigErrorHandler Configuration
        {
            get { return _Configuration; }           
        }

        public List<EventType> LogEventTypes
        {
            get { return _LogEventTypes; }
        }

        protected void SetPublisherEventTypes()
        {
            if (!string.IsNullOrEmpty(_Configuration.LogEventType))
            {
                string[] LogEventTypes = _Configuration.LogEventType.Split(',');
                foreach (string LogEventType in LogEventTypes)
                {
                    EventType evt = (EventType)Enum.Parse(typeof(EventType), LogEventType, true);
                    this.LogEventTypes.Add(evt);
                }
            }
        }
    }
}
