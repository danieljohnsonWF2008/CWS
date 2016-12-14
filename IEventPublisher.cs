using System;
namespace WF.Common.EventLogging
{
    public interface IEventPublisher
    {
        bool PublishEvent(EventDetails objEventDetails);
    }
}
