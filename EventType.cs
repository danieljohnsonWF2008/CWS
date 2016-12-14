using System;
using System.Collections.Generic;
using System.Text;

namespace WF.Common.EventLogging
{
    public enum EventType
    {
        Fatal = 0,
        Critical = 1,
        Error = 2,
        Warning = 3,
        Audit = 4,
        Debug = 5,
        All = 6,
        Status = 7
    }

    public enum PublishSink
    {
        Email = 0,
        Database = 1,
        FlatFile = 2
    }
}
