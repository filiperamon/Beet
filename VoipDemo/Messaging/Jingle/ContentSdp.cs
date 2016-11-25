using System;

using Matrix;
using Matrix.Xmpp;
using Matrix.Xml;
using Matrix.Xmpp.Client;

using Console = System.Console;


namespace Messaging.Jingle
{
	//[Matrix.Attributes.XmppTag (Name = "content", Namespace = "urn:xmpp:jingle:1")]
	sealed class ContentSdp : Matrix.Xml.XmppXElement
	{

		public XmppXElement Description {
			get { return Element <XmppXElement> (); }
			set { Replace (value); }
		}

		public new string Name {
			get { return GetAttribute("name"); }
			set { SetAttribute ("name", value); }
		}

		public Matrix.Xmpp.Jingle.Transports.TransportIceUdp TransportIceUdp {			
			get { return Element <Matrix.Xmpp.Jingle.Transports.TransportIceUdp> (); }
			set { Replace (value); }
		}

		public string Creator {
			get { return GetAttribute("creator"); }
			set { SetAttribute ("creator", value); }
		}

		//
		// Constructors
		//
		public ContentSdp () : base("urn:xmpp:jingle:1", "content") {
			//
		}
	}
}

