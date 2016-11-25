using System;

namespace Messaging
{
    public class SubscribeRequestEventArgs
    {
        public SubscribeRequestEventArgs(string from, string reason)
        {
            From = from;
            Reason = reason;
        }

        public string From
        {
            get;
            set;
        }

        public string Reason
        {
            get;
            set;
        }
    }
}