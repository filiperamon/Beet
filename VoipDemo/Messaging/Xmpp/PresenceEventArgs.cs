using System;

namespace Messaging
{
	public class PresenceEventArgs
	{
		public Matrix.Xmpp.PresenceType Status {
			get;
			set;
		}

		public string From {
			get;
			set;
		}
		public PresenceEventArgs (string from, Matrix.Xmpp.PresenceType status)
		{
			this.From = from;
			this.Status = status;
		}
	}
}

