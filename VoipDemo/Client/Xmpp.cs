using System;

using Matrix;
using Matrix.Xmpp;
using Matrix.Xml;
using Matrix.Xmpp.Client;

using Console = System.Console;

namespace Xamarin.Android.Conference.WebRTC
{
	public class Xmpp
	{
		private XmppClient xmppClient;
		private DiscoManager discoManager;

		// this is a hack to keep track of proof-of-concept JID
		// in practice there should be a dictionary with everyone online
		private string session;
		private string jid = "testuser@messaging.legion.mobi/Matrix-for-Android";
		private string serverAddress = "messaging.legion.mobi";

		public Xmpp ()
		{	
			string lic = @"eJxkkNtymzAQhl8lk1tPKxk7bulsNCUEH2hDnBinhzsFyVS2DlgSGHj6uo2T
9HCzs/9+e/hn4bMouHb8rFVSu8tzWr5xZuMP1PIP8gmdE1haw+rCLxhZ+ZoJ
A+i1Anc11V74jgwBveQQ184bxS2BjCpOkobKmnpjAf3WEBtVUd09A2H02ckK
oGcGiaJCEkcldx//cPaWHZue2LH55dC6YtTzpK2E5dfHjAR4+A6PRhjQfwgW
7porQ7ytj7tOAn7Fv+cneByEgP4BsBKlpr62nDRW4u0mYnr88N42WShz68JJ
vp9Nhfg+YFnP8SzFQ16l0QAVbb+e9zm9Sq2cq/L+bpGWj1mX2cjhPGjyfCel
vl+28U5N+ot0UFRdl1xt40+Tbj8PivVqa5hW8T46oD6QxfgwKujtDH0NlZLt
Q/NjNsWNUmx5860ow4vpI92NG54sN5vbG/0lugT06hvQ6d3kpwA=";			

			Matrix.License.LicenseManager.SetLicense(lic);

			this.xmppClient = new XmppClient ("testuser", "messaging.legion.mobi", "P@ssw0rd");
			this.xmppClient.OnBeforeSendPresence += XmppClient_OnBeforeSendPresence;
			this.xmppClient.OnRosterEnd += XmppClient_OnRosterEnd;
			this.xmppClient.OnMessage += XmppClient_OnMessage;
			this.xmppClient.OnError += XmppClient_OnError;
			this.xmppClient.OnIq += XmppClient_OnIq;
			this.xmppClient.OnSendXml += XmppClient_OnSendXml;
			this.xmppClient.OnReceiveXml += XmppClient_OnReceiveXml;
			this.xmppClient.OnPresence += XmppClient_OnPresence;

			this.xmppClient.XPathFilter.XmlNamespaceManager.AddNamespace("JC", "jabber:client");            
			this.xmppClient.XPathFilter.XmlNamespaceManager.AddNamespace("JI", "urn:xmpp:jingle:1");
			this.xmppClient.XPathFilter.XmlNamespaceManager.AddNamespace("QU", "http://jabber.org/protocol/disco#info");
			this.xmppClient.XPathFilter.Add ("/JC:iq[@type='get']/QU:query", ServiceDiscovery);
			this.xmppClient.XPathFilter.Add ("/JC:iq/JI:jingle[@action='session-initiate']", SessionInitiateCallback); 
			this.xmppClient.XPathFilter.Add ("/JC:iq/JI:jingle[@action='transport-info']", TransportInfoCallback); 
			this.xmppClient.XPathFilter.Add ("/JC:iq/JI:jingle[@action='session-terminate']", SessionTerminateCallback); 
		}

		#region Jingle & Cap Discovery Events
		void SessionInitiateCallback(object sender, XPathEventArgs e)
		{
			Console.WriteLine ("<SessionInitiate");

			Iq iqIn = (Iq)e.Stanza;

			Messaging.Jingle.JingleSdp jingleIn = (Messaging.Jingle.SdpJingle) iqIn.Query;

			this.SendAck (iqIn.From, iqIn.Id);
			string sdp;


			if (this.OnOfferAnswer != null) {
				this.OnOfferAnswer (this, new System.EventArgs ());
			}
		}

		void SessionTerminateCallback(object sender, XPathEventArgs e)
		{
			Console.WriteLine ("<SessionTerminate");

			Iq iqIn = (Iq)e.Stanza;

			// send ack
			this.SendAck(iqIn.From, iqIn.Id);
		}

		void TransportInfoCallback(object sender, XPathEventArgs e)
		{
			Console.WriteLine ("<TransportInfo");
		}

		void ServiceDiscovery(object sender, XPathEventArgs e)
		{			
			Console.WriteLine ("<ServiceDiscovery");

			Matrix.Xmpp.Client.Iq xIq = (Matrix.Xmpp.Client.Iq) e.Stanza;

			Matrix.Xmpp.Client.Iq iq = new Matrix.Xmpp.Client.Iq();
			iq.From = this.jid;
			iq.To = "testuser2@messaging.legion.mobi/" + this.session;
			iq.Type = Matrix.Xmpp.IqType.Result;
			iq.Id = xIq.Id;
			string defaultNs = "http://jabber.org/protocol/disco#info";
			iq.Query = new Matrix.Xml.XmppXElement(defaultNs, "query", new System.Xml.Linq.XAttribute("node", xIq.Query.GetAttribute("node")));
			iq.Query.Add(new Matrix.Xml.XmppXElement (defaultNs, "identity", new System.Xml.Linq.XAttribute ("category", "client"), new System.Xml.Linq.XAttribute("name", "legion-0.1"), new System.Xml.Linq.XAttribute("type", "pc")));

			iq.Query.Add(new Matrix.Xml.XmppXElement (defaultNs, "feature", new System.Xml.Linq.XAttribute ("var", "http://jabber.org/protocol/caps")));
			iq.Query.Add(new Matrix.Xml.XmppXElement (defaultNs, "feature", new System.Xml.Linq.XAttribute ("var", "http://jabber.org/protocol/disco#info")));
			iq.Query.Add(new Matrix.Xml.XmppXElement (defaultNs, "feature", new System.Xml.Linq.XAttribute ("var", "urn:ietf:rfc:3264")));
			iq.Query.Add(new Matrix.Xml.XmppXElement (defaultNs, "feature", new System.Xml.Linq.XAttribute ("var", "urn:xmpp:jingle:apps:dtls:0")));
			iq.Query.Add(new Matrix.Xml.XmppXElement (defaultNs, "feature", new System.Xml.Linq.XAttribute ("var", "urn:xmpp:jingle:transports:ice-udp:1")));
			iq.Query.Add(new Matrix.Xml.XmppXElement (defaultNs, "feature", new System.Xml.Linq.XAttribute ("var", "urn:xmpp:jingle:apps:rtp:audio")));
			iq.Query.Add(new Matrix.Xml.XmppXElement (defaultNs, "feature", new System.Xml.Linq.XAttribute ("var", "urn:xmpp:jingle:apps:rtp:video")));

			this.xmppClient.Send(iq);
		}
		#endregion

		#region Internal Xmpp Events
		void XmppClient_OnRosterEnd (object sender, Matrix.EventArgs e)
		{
			Console.WriteLine ("XmppClient_OnRosterEnd");
			if (this.OnConnected != null) {
				this.OnConnected (this, new System.EventArgs ());
			}
		}

		void XmppClient_OnBeforeSendPresence (object sender, PresenceEventArgs e)
		{
			Console.WriteLine ("XmppClient_OnBeforeSendPresence");
			this.discoManager = new DiscoManager(this.xmppClient);
			this.discoManager.AutoSendCaps = true;
			this.discoManager.AutoDiscover = true;
			this.discoManager.AutoReplyToDiscoInfo = true;
			this.discoManager.AddIdentity(new Matrix.Xmpp.Disco.Identity("pc", "legion-0.1", "client"));
			this.discoManager.AddFeature("urn:ietf:rfc:3264");
			this.discoManager.AddFeature("urn:xmpp:jingle:apps:dtls:0");
			this.discoManager.AddFeature("urn:xmpp:jingle:transports:ice-udp:1");
			this.discoManager.AddFeature("urn:xmpp:jingle:apps:rtp:audio");
			this.discoManager.AddFeature("urn:xmpp:jingle:apps:rtp:video");			
			this.discoManager.AddFeature("http://jabber.org/protocol/caps");
			this.discoManager.AddFeature("http://jabber.org/protocol/disco#info");
			//this.discoManager.DiscoverItems(this.xmppClient.XmppDomain, new System.EventHandler<Matrix.Xmpp.Client.IqEventArgs>(DiscoItemsResult));				
			this.discoManager.DiscoverItems(this.xmppClient.XmppDomain);				
		}

		void XmppClient_OnPresence (object sender, PresenceEventArgs e)
		{
			Console.WriteLine("OnPresence");
			try {
				if (e.Presence.From.User == "testuser2")
				{
					// hack for purposes of demo
					this.session = e.Presence.From.Resource;

					// here we send our presence directly which triggers them asking us for caps
					Console.WriteLine("Sending presence direct");
					Matrix.Xmpp.Client.Presence p = new Matrix.Xmpp.Client.Presence();
					p.From = "testuser@messaging.legion.mobi/Matrix-for-Android";
					p.To = "testuser2@messaging.legion.mobi/"+ this.session;
					p.Caps = new Matrix.Xmpp.Capabilities.Caps();
					p.Caps.Node = "http://legion.mobi/";
					p.Caps.SetVersion(new Matrix.Xmpp.Disco.Info(""));
					xmppClient.Send(p);
				}
			}
			catch (System.Exception ex) {
				Console.WriteLine(ex.ToString());
			}
		}

		void XmppClient_OnReceiveXml (object sender, TextEventArgs e)
		{
			Console.WriteLine ("Recv " + e.Text);
		}

		void XmppClient_OnSendXml (object sender, TextEventArgs e)
		{
			Console.WriteLine ("Send " + e.Text);
		}

		void XmppClient_OnIq (object sender, IqEventArgs e)
		{
			Console.WriteLine(e.Iq.ToString());
		}

		void XmppClient_OnError (object sender, ExceptionEventArgs e)
		{
			//TODO: event update ui
			Console.WriteLine(string.Format("err " + e.Exception.Message));
		}

		void XmppClient_OnMessage (object sender, MessageEventArgs e)
		{
			if (e.Message.Body != null && e.Message.Body.Length > 0)
			{
				// TODO: event update ui
				Console.WriteLine(string.Format("<<< {0}", e.Message.Body));
			}

		}
		#endregion

		#region Public methods
		public void Connect()
		{
			this.xmppClient.Open ();
		}


		// going to need Initiate Jingle Call

		public void Send(string jid, string message)
		{

			Matrix.Xmpp.Client.Message m = new Matrix.Xmpp.Client.Message(jid, Matrix.Xmpp.MessageType.Chat, message);
			this.xmppClient.Send(m);
		}

		private int i = 1;
		public void SendTest()
		{
			string s = string.Format ("hello world {0}!", i++);
			Send ("testuser2@messaging.legion.mobi/" + this.session, s);			
		}

		public void SendSessionTerminate(string to, string sid)
		{
			Matrix.Xmpp.Jingle.Jingle jIq = new Matrix.Xmpp.Jingle.Jingle ();

			jIq.Action = Matrix.Xmpp.Jingle.Action.SessionTerminate;
			jIq.Sid = sid;
			string defaultNs = "urn:xmpp:jingle:1";

			Matrix.Xml.XmppXElement eX = new Matrix.Xml.XmppXElement (defaultNs, "reason");
			eX.Add(new Matrix.Xml.XmppXElement(defaultNs, "success"));
			jIq.Add(eX);	

			Iq denyIq = new Iq ();
			denyIq.To = to;
			denyIq.From = this.jid;
			denyIq.Type = Matrix.Xmpp.IqType.Set;
			denyIq.Add (jIq);

			Console.Write (denyIq.ToString ());
			xmppClient.Send (denyIq);
		}

		public void SendAck(string to, string id)
		{
			// send ack
			Iq iqOut = new Iq ();
			iqOut.To = to;
			iqOut.From = this.jid;
			iqOut.Type = Matrix.Xmpp.IqType.Result;
			iqOut.Id = id;

			xmppClient.Send (iqOut);
		}

		public void SendSessionAccept()
		{
		}
		#endregion

		#region Public events
		// going to need Jingle Incoming Call event

		// going to need a bunch of p2p bullshit 

		public delegate void ConnectedEventHandler(object sender, System.EventArgs e);
		public event ConnectedEventHandler OnConnected;

		public delegate void MessageEventHandler(object sender, System.EventArgs e);
		public event MessageEventHandler OnMessage;

		public delegate void CandidateEventHandler(object sender, System.EventArgs e);
		public event CandidateEventHandler OnCandidate;

		public delegate void OfferAnswerHandler(object sender, System.EventArgs e);
		public event OfferAnswerHandler OnOfferAnswer;
		#endregion
	}
}

