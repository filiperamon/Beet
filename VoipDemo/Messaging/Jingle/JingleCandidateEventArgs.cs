using System;
using Matrix.Xmpp.Jingle.Candidates;

namespace Messaging.Jingle
{
	sealed class JingleCandidateEventArgs : JingleEventArgs
	{
		public JingleCandidateEventArgs (string from, CandidateIceUdp candidate) : base(from)
		{
			this.Candidate = candidate;
		}

		public CandidateIceUdp Candidate {
			get;
			set;
		}
	}
}

