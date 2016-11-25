using System;

using Matrix;
using Matrix.Xmpp;
using Matrix.Xml;
using Matrix.Xmpp.Client;

using Console = System.Console;

namespace Messaging.Jingle
{
	//[Matrix.Attributes.XmppTag (Name = "fingerprint", Namespace = "urn:xmpp:tmp:jingle:apps:dtls:0")]
	sealed class FingerprintSdp : Matrix.Xml.XmppXElement
	{
		public FingerprintSdp() : base("urn:xmpp:tmp:jingle:apps:dtls:0", "fingerprint") {
			//
		}

		public string Hash {
			get { return GetAttribute("hash"); }
			set { SetAttribute ("hash", value); }
		}
	}

}

