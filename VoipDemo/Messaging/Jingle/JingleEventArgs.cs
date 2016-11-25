using System;

namespace Messaging.Jingle
{
	public class JingleEventArgs : EventArgs
	{


		public string From {
			get;
			set;
		}

		public JingleEventArgs (string from)
		{
			this.From = from;
		}
	}
}

