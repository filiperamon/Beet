using System;

namespace Messaging
{
	public class MessageEventArgs
	{
		public string From {
			get;
			set;
		}

		public string Body {
			get;
			set;
		}
		public MessageEventArgs (string from, string body)
		{
			this.From = from;
			this.Body = body;
		}
	}
}

