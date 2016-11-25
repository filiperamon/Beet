using System;

namespace Messaging.Jingle
{
	public class JingleSdpEventArgs : Jingle.JingleEventArgs
	{
		public JingleSdpEventArgs (string from, string sdp) : base(from)
		{
			this.Sdp = sdp;
		}

		public string Sdp {
			get;
			set;
		}
	}
}

