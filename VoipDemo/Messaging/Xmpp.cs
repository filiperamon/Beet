using System;

using Matrix;
using Matrix.Xmpp;
using Matrix.Xml;
using Matrix.Xmpp.Client;

using System.Linq;
using System.Collections.Generic;

using Matrix.Xmpp.XData;
using AppCore;

namespace Messaging
{
	public class Xmpp
	{
		private XmppClient xmppClient;
		private RosterManager rosterManager;
		private DiscoManager discoManager;
		private PresenceManager presenceManager;
		private MucManager mucManager;

		// TODO: make these private
		public PubSubManager pubsubManager;
		public OpenPGP openPGP;

		private const string _mucServer = AppCore.Utils._MUS_SERVER;
		private const string _pubsubServer = AppCore.Utils._PUB_SUB_SERVER;

		// connection info
		private string _jid;
		private string _username;
		private string _serverAddress;
		private string _password;

		/// <summary>
		/// Create XMPP object with connection info
		/// </summary>
		/// <param name="user">XMPP Username</param>
		/// <param name="server">XMPP Server address</param>
		/// <param name="password">XMPP password</param>
		public Xmpp(string user, string server, string password, string matrix_linc)
		{
			// XMPP login/authentication information
			var androidID = Android.Provider.Settings.Secure.GetString(Android.App.Application.Context.ContentResolver, Android.Provider.Settings.Secure.AndroidId);

			this._username = user;
			this._jid = user + "@" + server + '/' + androidID;
			this._serverAddress = server;
			this._password = password;

			Log.Info(matrix_linc);

			Matrix.License.LicenseManager.SetLicense(matrix_linc);




			// set up general XMPP events 
			this.xmppClient = new XmppClient(this._username, this._serverAddress, androidID, this._password);
			this.xmppClient.OnBeforeSendPresence += XmppClient_OnBeforeSendPresence;
			this.xmppClient.OnRosterEnd += XmppClient_OnRosterEnd;
			this.xmppClient.OnMessage += XmppClient_OnMessage;

			this.xmppClient.OnIq += XmppClient_OnIq;
			this.xmppClient.OnSendXml += XmppClient_OnSendXml;
			this.xmppClient.OnReceiveXml += XmppClient_OnReceiveXml;
			this.xmppClient.OnPresence += XmppClient_OnPresence;
			this.xmppClient.OnRosterItem += XmppClient_OnRosterItem;
			// connection errors
			this.xmppClient.OnBindError += XmppClient_OnBindError;
			this.xmppClient.OnAuthError += XmppClient_OnAuthError;
			this.xmppClient.OnError += XmppClient_OnError;
			this.xmppClient.OnClose += XmppClient_OnClose;
			this.xmppClient.OnStreamError += XmppClient_OnStreamError;
			this.xmppClient.OnXmlError += XmppClient_OnXmlError;

			this.xmppClient.OnLogin += XmppClient_OnLogin;           

			this.xmppClient.XPathFilter.XmlNamespaceManager.AddNamespace("JC", "jabber:client");
			this.xmppClient.XPathFilter.XmlNamespaceManager.AddNamespace("JI", "urn:xmpp:jingle:1");
			//this.xmppClient.XPathFilter.XmlNamespaceManager.AddNamespace("QU", "http://jabber.org/protocol/disco#info");

			// set up callbacks for specific Jingle events
			this.xmppClient.XPathFilter.Add("/JC:iq/JI:jingle[@action='session-initiate']", SessionInitiateCallback);
			this.xmppClient.XPathFilter.Add("/JC:iq/JI:jingle[@action='session-accept']", SessionAcceptCallback);
			this.xmppClient.XPathFilter.Add("/JC:iq/JI:jingle[@action='transport-info']", TransportInfoCallback);
			this.xmppClient.XPathFilter.Add("/JC:iq/JI:jingle[@action='session-terminate']", SessionTerminateCallback);

			Factory.RegisterElement<Jingle.JingleSdp>("urn:xmpp:jingle:1", "jingle");
			Factory.RegisterElement<Jingle.ContentSdp>("urn:xmpp:jingle:1", "content");
			Factory.RegisterElement<Jingle.FingerprintSdp>("urn:xmpp:tmp:jingle:apps:dtls:0", "fingerprint");

			//Factory.RegisterElement<PubsubEvent>("http://jabber.org/protocol/pubsub#event", "event");
		}

		private void PubsubManager_OnEvent(object sender, Matrix.Xmpp.Client.MessageEventArgs e)
		{
			Log.Info("pep_onevent " + e.Message.From + " " + e.Message.Value);

			if (e.Message.Value != "")
			{
				string jid = e.Message.From;
				System.Xml.Linq.XNamespace ns = "urn:xmpp:openpgp:0";
				try
				{
					string pubkey = (string)
						(
						 from el in e.Message.Descendants(ns + "pubkey")
						 select el
						 ).First();
					this.openPGP.AddPublicKey(jid, pubkey);
				}
				catch (Exception ex)
				{
					Log.Error(e.Message.Value + " resulted in " + ex.Message);
				}
			}
		}

		public void MucManager()
		{
			this.mucManager = new MucManager(xmppClient);

		}

		public void CreateRoom(string rooname, string jidUser)
		{
			Jid jidroom = new Jid(rooname + "/Legion");
			this.mucManager.EnterRoom(jidroom, "mytest");
		}

		public void InviteUserRoom(string rooname, string jidfriend)
		{
			this.mucManager.Invite(jidfriend + "/Legion", rooname + "/Legion", "You went added at a new Group.");
		}

		public void AcceptInvit(string roomname, string jidUser)
		{
			this.mucManager.EnterRoom(roomname, roomname);
		}

		public void FindAllGroup(string jid)
		{

		}

		#region Jingle & Cap Discovery Events
		void SessionAcceptCallback(object sender, XPathEventArgs e)
		{
			try
			{
				Log.Info("<SessionAccept");

				Iq iqIn = (Iq)e.Stanza;
				Messaging.Jingle.JingleSdp jingleIn = null;

				// jingleIn = (Messaging.Jingle.JingleSdp) iqIn.Query;
				jingleIn = iqIn.Element<Messaging.Jingle.JingleSdp>();

				this.SendAck(iqIn.From, iqIn.Id);
				// This event needs to get passed back up to IceLink.Conference.OnReceiveOfferAnswer
				if (this.OnReceiveSessionAccept != null)
				{
					Log.Info("Raising OnReceiveSessionAccept");
					this.OnReceiveSessionAccept(this, new Messaging.Jingle.JingleSdpEventArgs(iqIn.From, jingleIn.ToSdp()));
				}
				else
				{
					Log.Info("No OnReceiveSessionAccept handler");
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		void SessionInitiateCallback(object sender, XPathEventArgs e)
		{
			try
			{
				Iq iqIn = (Iq)e.Stanza;
				if (iqIn.Type != IqType.Error)
				{
					Log.Info("<SessionInitiate");


					Messaging.Jingle.JingleSdp jingleIn = null;

					// jingleIn = (Messaging.Jingle.JingleSdp) iqIn.Query;
					jingleIn = iqIn.Element<Messaging.Jingle.JingleSdp>();

					this.SendAck(iqIn.From, iqIn.Id);
					if (this.OnReceiveSessionInitiate != null)
					{
						this.OnReceiveSessionInitiate(this, new Messaging.Jingle.JingleSdpEventArgs(iqIn.From, jingleIn.ToSdp()));
					}
				}
				else
				{
					Log.Error("SessionInitiateCallback Error");
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		void SessionTerminateCallback(object sender, XPathEventArgs e)
		{
			try
			{
				Log.Info("<SessionTerminate");

				Iq iqIn = (Iq)e.Stanza;

				this.SendAck(iqIn.From, iqIn.Id);

				if (this.OnReceiveSessionTerminate != null)
				{
					this.OnReceiveSessionTerminate(this, new Messaging.Jingle.JingleEventArgs(iqIn.From));
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}

		}

		void TransportInfoCallback(object sender, XPathEventArgs e)
		{
			Log.Info("<TransportInfo");
		}
		#endregion

		#region Internal Xmpp Events

		private void XmppClient_OnXmlError(object sender, ExceptionEventArgs e)
		{
			Log.Info("OnXmlError " + e.ToString());
		}

		private void XmppClient_OnStreamError(object sender, StreamErrorEventArgs e)
		{
			Log.Info("OnStreamError " + e.ToString());
		}

		private void XmppClient_OnBindError(object sender, IqEventArgs e)
		{
			Log.Info("OnBindError " + e.ToString());
		}

		public void XmppClient_OnClose(object sender, Matrix.EventArgs e)
		{
			Log.Debug("OnClose");
			if (this.OnClose != null)
			{
				this.OnClose(this, e);
			}
		}

		public void XmppClient_OnAuthError(object sender, Matrix.Xmpp.Sasl.SaslEventArgs e)
		{
			Log.Debug("OnAuthError");
			if (this.OnAuthError != null)
			{
				this.OnAuthError(this, e.Failure.Condition);
			}
		}

		void XmppClient_OnRosterEnd(object sender, Matrix.EventArgs e)
		{
			Log.Debug("XmppClient_OnRosterEnd");

			this.presenceManager = new PresenceManager(this.xmppClient);
			this.presenceManager.OnSubscribe += PresenceManager_OnSubscribe;
			this.presenceManager.OnSubscribed += PresenceManager_OnSubscribed;

			this.rosterManager = new RosterManager(this.xmppClient);

			this.mucManager = new MucManager(this.xmppClient);
			this.mucManager.OnInvite += MucManager_OnInvite;

			this.pubsubManager = new PubSubManager(this.xmppClient);
			this.pubsubManager.OnEvent += PubsubManager_OnEvent;
			this.pubsubManager.Subscribe(_pubsubServer, "urn:xmpp:openpgp:0", this._jid);

			this.discoManager = new DiscoManager(this.xmppClient);

			this.openPGP = new OpenPGP(this, this._jid);

			if (this.OnConnected != null)
			{
				this.OnConnected(this, e);
			}
		}

		void XmppClient_OnRosterItem(object sender, Matrix.Xmpp.Roster.RosterEventArgs e)
		{
			Log.Debug("XmppClient_OnRosterItem");
			if (this.OnRosterItem != null)
			{
				this.OnRosterItem(this, e);
			}

		}

		private void XmppClient_OnLogin(object sender, Matrix.EventArgs e)
		{

		}

		void XmppClient_OnBeforeSendPresence(object sender, Matrix.Xmpp.Client.PresenceEventArgs e)
		{
			Log.Debug("XmppClient_OnBeforeSendPresence");


			/*
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
            this.discoManager.AddFeature("urn:xmpp:openpgp:im:0");
            */

			// These could be used to find out server items / capabilities.  not interesting for now.  we check elsewhere for individual clients.
			// this.discoManager.DiscoverItems(this.xmppClient.XmppDomain, new System.EventHandler<Matrix.Xmpp.Client.IqEventArgs>(DiscoItemsResult));               
			//this.discoManager.DiscoverInformation(this.xmppClient.XmppDomain);  

		}

		public void MucManager_OnInvite(object sender, Matrix.Xmpp.Client.MessageEventArgs e)
		{
			Log.Info("Received an invite!!! " + e.Message.ToString() + " subject: " + e.Message.Subject);
			string jid = e.Message.From;

			this.mucManager.EnterRoom(e.Message.From, this._jid);

			if (this.OnInvit != null) {
				this.OnInvit(sender, new MessageEventArgs(e.Message.From, e.Message.XMucUser.FirstElement.FirstAttribute.Value));
			}
			//if (this.OnJoinGroup != null)
			//{
			//	this.OnJoinGroup(this, jid.Split('@')[0]);
			//}
			// set up public event
		}

		private void PresenceManager_OnSubscribed(object sender, Matrix.Xmpp.Client.PresenceEventArgs e)
		{
			Log.Info("OnSubscribed from " + e.Presence.From);

			// pubsubManager.Subscribe("", "urn:xmpp:openpgp:0", e.Presence.From);
		}

		private void PresenceManager_OnSubscribe(object sender, Matrix.Xmpp.Client.PresenceEventArgs e)
		{
			if (this.OnSubscribeRequest != null)
			{
				this.OnSubscribeRequest(this, new SubscribeRequestEventArgs(e.Presence.From, e.Presence.Status));
			}
		}

		void DiscoItemsResult(object sender, IqEventArgs e)
		{
			var query = e.Iq.Element<Matrix.Xmpp.Disco.Items>();
			if (query != null)
			{
				foreach (var itm in query.GetItems())
				{
					//_dm.DiscoverInformation(itm.Jid, itm.Node);
					System.Threading.Thread.Sleep(200);
					Log.Info("Disco: " + itm.Jid);
					this.discoManager.DiscoverInformation(itm.Jid, itm.Node, new EventHandler<IqEventArgs>(DiscoInfoResult));
				}
			}
		}

		void DiscoGroupsResult(object sender, IqEventArgs e)
		{
			var query = e.Iq.Element<Matrix.Xmpp.Disco.Items>();
			if (query != null)
			{
				//
				int i = 0;
				int c = query.GetItems().Count();
				string[] rooms = new string[c];

				foreach (var itm in query.GetItems())
				{
					rooms[i] = itm.Jid;
					i++;
					//_dm.DiscoverInformation(itm.Jid, itm.Node);
					//System.Threading.Thread.Sleep(200);                    
					//this.discoManager.DiscoverInformation(itm.Jid, itm.Node, new EventHandler<IqEventArgs>(DiscoInfoResult));
				}

				if (OnListGroup != null)
				{
					this.OnListGroup(this, rooms);
				}
			}
		}

		void DiscoInfoResult(object sender, IqEventArgs e)
		{
			var query = e.Iq.Element<Matrix.Xmpp.Disco.Info>();
			if (query != null)
			{
				foreach (var id in query.GetIdentities())
				{
					if (id.Category == "pubsub")
					{
						Log.Info(e.Iq.From);
						DiscoPubSubNodes(e.Iq.From, null);
					}
					Log.Info("DiscoInfo " + id.Category);

				}
			}
		}

		void DiscoPubSubNodes(Jid jid, string node)
		{
			this.discoManager.DiscoverItems(jid, node, new EventHandler<IqEventArgs>(DiscoPubSubNodesResult));
		}

		void DiscoPubSubNodesResult(object sender, IqEventArgs e)
		{
			var query = e.Iq.Element<Matrix.Xmpp.Disco.Items>();
			if (query != null)
			{
				foreach (var item in query.GetItems())
				{
					// this seems recursive?
					//Log.Info("ASDF "  + item.Name + " " + item.Node + " " + item.Jid);
					DiscoPubSubNodes(item.Jid, item.Node);
				}
			}
		}

		void XmppClient_OnPresence(object sender, Matrix.Xmpp.Client.PresenceEventArgs e)
		{
			// when we see that someone is online then we see initiate discovery of capabilities
			if (e.Presence.Type != PresenceType.Unavailable)
			{
				// test if we have a key already
				// this.pubsubManager.RequestAllItems(_pubsubServer, "urn:xmpp:openpgp:0", new EventHandler<IqEventArgs>(DiscoPubSubRequestAllItemsResult));

				// this.discoManager.DiscoverInformation(e.Presence.From, new System.EventHandler<Matrix.Xmpp.Client.IqEventArgs>(DiscoInfoResult));
			}
			Log.Info("OnPresence " + e.Presence.From);

			// also raise event to update contact list
			if (this.OnPresence != null)
			{
				this.OnPresence(this, new PresenceEventArgs(e.Presence.From, e.Presence.Type));
			}
		}

		void DiscoPubSubRequestAllItemsResult(object sender, IqEventArgs e)
		{
			Log.Info("DiscoPubSubRequestAllItemsResult " + e.Iq.ToString());
		}

		void XmppClient_OnReceiveXml(object sender, TextEventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem(delegate
			{
				Log.Verbose("Recv " + e.Text);

			}, null);

		}

		void XmppClient_OnSendXml(object sender, TextEventArgs e)
		{
			Log.Verbose("SendXml " + e.Text);
		}

		void XmppClient_OnIq(object sender, IqEventArgs e)
		{
			Log.Verbose("RecvIq  " + e.Iq.ToString());

		}

		void XmppClient_OnError(object sender, ExceptionEventArgs e)
		{
			Log.Error(string.Format("OnError " + e.Exception.ToString()));
		}

		void XmppClient_OnMessage(object sender, Matrix.Xmpp.Client.MessageEventArgs e)
		{

			//System.Threading.ThreadPool.QueueUserWorkItem(delegate
			//{
				if (e.Message.Body != null && e.Message.Body.Length > 0 && e.Message.Type != MessageType.Error)
				{
					string messageBody = "";
					System.Xml.Linq.XNamespace ns = "urn:xmpp:openpgp:0";
					string pgpData = (string)
						(
						 from el in e.Message.Descendants(ns + "openpgp")
						 select el
						 ).FirstOrDefault();
					if (pgpData != null)
					{
						messageBody = this.openPGP.Decrypt(pgpData);
						Log.Info("encrypted message " + e.Message.From + " " + messageBody);
					}
					else
					{
						messageBody = e.Message.Body;
						Log.Info("unencrypted message " + e.Message.From + " " + messageBody);
					}

					if (e.Message.XMucUser != null && this.OnInvit != null)
					{
						this.OnInvit(this, new MessageEventArgs(e.Message.From, e.Message.XMucUser.FirstElement.FirstAttribute.Value));
					}
					else if (this.OnMessage != null && e.Message.Body != null)
					{
						var from = e.Message.From.ToString().Split('/');
						if (from[1] != null && from[1].Equals(_username))
						{
							return;
						}
						else { 
						    this.OnMessage(this, new MessageEventArgs(e.Message.From.ToString().Split('@')[0], e.Message.Body));
						}

					}

				}
				else if (e.Message.Type == MessageType.GroupChat && e.Message.Subject != null && this.OnSubjectGroup != null)
				{
					this.OnSubjectGroup(this, new MessageEventArgs(e.Message.From, e.Message.Subject));
				}
				else if (e.Message.Type == MessageType.Error)
				{
					Log.Error("OnMessage error " + e.Message.From + " " + e.Message.Body);
					Log.Error(e.Message.ToString());
				}
				
			//}, null);



		}
		#endregion

		#region Public methods
		/// <summary>
		/// Connect to XMPP server using authentication information from constructor
		/// </summary>
		public void Connect()
		{
			Log.Info(">Connect");
			this.xmppClient.Open();
		}

		/// <summary>
		/// Add new contact by jid
		/// </summary>
		/// <param name="jid"></param>
		public void Add(string jid)
		{
			rosterManager.Add(jid);

			presenceManager.Subscribe(jid);
		}

		/// <summary>
		/// Add new contact with request reason
		/// </summary>
		/// <param name="jid"></param>
		/// <param name="reason">Explain why you want the user to accept your add request</param>
		public void Add(string jid, string reason)
		{
			rosterManager.Add(jid);

			presenceManager.Subscribe(jid, reason);
		}

		public void Remove(string jid)
		{
			RosterManager rm = new RosterManager(this.xmppClient);
			rm.Remove(jid);
			return;
			/*
            pubsubManager.Unsubscribe("", "urn:xmpp:openpgp:0", jid, delegate {
                presenceManager.Unsubscribe(jid);

                RosterManager rm = new RosterManager(this.xmppClient);
                rm.Remove(jid);
            });
            */
		}

		/// <summary>
		/// Accept a friend request
		/// </summary>
		/// <param name="jid"></param>
		public void Approve(string jid)
		{
			Log.Info("Approving friend request " + jid);
			presenceManager.ApproveSubscriptionRequest(jid);

			Log.Info("Subscribing to " + jid);
			presenceManager.Subscribe(jid);


		}

		/// <summary>
		/// Deny a friend request
		/// </summary>
		/// <param name="jid"></param>
		public void Deny(string jid)
		{
			presenceManager.DenySubscriptionRequest(jid);
		}

		/// <summary>
		/// Search for user's to add by email address
		/// </summary>
		/// <param name="email"></param>
		/// <param name="callback"></param>
		public async void Search(string email, EventHandler<IqEventArgs> callback)
		{
			// query to list searchable fields
			// openfire responds w/first last nick email/
			/*
            var s2 = new Iq() {
                To = "search.messaging.beetmessenger.com",
                Type = IqType.Get,
                Query = new Matrix.Xmpp.Search.Search()
            };
            */
			var search = new Matrix.Xmpp.Search.Search();
			search.Email = email;

			var s = new Iq()
			{
				To = "search.messaging.beetmessenger.com",
				Type = IqType.Set,
				Query = search,
			};

			s.GenerateId();

			this.xmppClient.IqFilter.SendIq(s, callback);
		}

		public void Close()
		{
			this.xmppClient.Close();
		}

		/// <summary>
		/// Sends a message
		/// </summary>
		/// <param name="jid">Recipient jid</param>
		/// <param name="message">Message body</param>
		public void Send(string jid, string message)
		{
			Matrix.Xmpp.Client.Message m = new Matrix.Xmpp.Client.Message(jid, Matrix.Xmpp.MessageType.Chat, message);
			this.xmppClient.Send(m);
		}

		public void SendGroup(string groupName, string message)
		{
			Matrix.Xmpp.Client.Message m = new Matrix.Xmpp.Client.Message(groupName + "@" + _mucServer , Matrix.Xmpp.MessageType.GroupChat, message);
			this.xmppClient.Send(m);
		}

		/// <summary>
		/// XMPP this will send request to the server and trigger OnListGroup with a list of accessible groups
		/// </summary>
		public void ListGroup()
		{
			this.discoManager.DiscoverItems(_mucServer, new EventHandler<IqEventArgs>(DiscoGroupsResult));
		}

		/// <summary>
		/// Sends a pgp encrypted message
		/// </summary>
		/// <param name="jid">Recipient jid</param>
		/// <param name="message">Message body</param>
		public void SendPgp(string jid, string message)
		{
			Matrix.Xmpp.Client.Message m = new Matrix.Xmpp.Client.Message(jid, Matrix.Xmpp.MessageType.Chat, "This message is encrypted using OpenPGP.");

			try
			{
				var encryptedMessage = this.openPGP.Encrypt(jid, message);
				m.Add(new XmppXElement("urn:xmpp:hints", "store"));
				m.Add(new XmppXElement("urn:xmpp:openpgp:0", "openpgp")
				{ Value = encryptedMessage });

				this.xmppClient.Send(m);
			}
			catch (System.IO.FileNotFoundException e)
			{
				Log.Error(e.Message);
				this.Send(jid, message);
			}

		}

		public void SendPresence(Matrix.Xmpp.Show status)
		{
			var presence = new Presence { Show = status };
			this.xmppClient.Send(presence);
		}
		/// <summary>
		/// Send request for VOIP/Video/Audio call
		/// </summary>
		/// <param name="to">recipient jid</param>
		/// <param name="sdp">SDP string from Icelink</param>
		public void SendSessionInitiate(string to, string sdp)
		{
			try
			{
				Iq iq = new Iq();
				iq.To = to;
				//iq.From = this._jid;
				iq.Type = Matrix.Xmpp.IqType.Set;
				Matrix.Xmpp.Jingle.Jingle jingle = Jingle.JingleSdp.FromSdp(sdp);
				jingle.Action = Matrix.Xmpp.Jingle.Action.SessionInitiate;
				jingle.GenerateSid();
				iq.Add(jingle);

				//iq.Add(new Matrix.Xmpp.Jingle.Jingle());
				//xmppClient.Send(new Matrix.Xmpp.Client.Message(to, Matrix.Xmpp.MessageType.Chat, jingle.ToString()));
				xmppClient.Send(iq);
				Log.Info(">session-initiate " + to);
			}
			catch (Exception e)
			{
				Log.Error("Can't SendSessionInitiate " + e.ToString());
			}
		}

		/// <summary>
		/// Accept VOIP/Video/Audio call.  Should only be used after a SessionInitiate was received.
		/// </summary>
		/// <param name="to">recipient jid</param>
		/// <param name="sdp">Response sdp</param>
		public void SendSessionAccept(string to, string sdp)
		{
			Iq iq = new Iq();
			iq.To = to;
			iq.Type = Matrix.Xmpp.IqType.Set;
			Matrix.Xmpp.Jingle.Jingle jingle = Jingle.JingleSdp.FromSdp(sdp);
			jingle.Action = Matrix.Xmpp.Jingle.Action.SessionAccept;
			jingle.GenerateSid();
			iq.Add(jingle);
			xmppClient.Send(iq);
			Log.Info(">session-accept " + to);
		}

		/// <summary>
		/// Disconnect VOIP/Video/Audio call.
		/// </summary>
		/// <param name="to">recipient jid</param>
		public void SendSessionTerminate(string to)
		{
			Matrix.Xmpp.Jingle.Jingle jIq = new Matrix.Xmpp.Jingle.Jingle();

			jIq.Action = Matrix.Xmpp.Jingle.Action.SessionTerminate;
			jIq.GenerateSid();
			string defaultNs = "urn:xmpp:jingle:1";

			Matrix.Xml.XmppXElement eX = new Matrix.Xml.XmppXElement(defaultNs, "reason");
			eX.Add(new Matrix.Xml.XmppXElement(defaultNs, "success"));
			jIq.Add(eX);

			Iq denyIq = new Iq();
			denyIq.To = to;
			denyIq.From = this._jid;
			denyIq.Type = Matrix.Xmpp.IqType.Set;
			denyIq.Add(jIq);

			xmppClient.Send(denyIq);
			Log.Info(">session-terminate");
		}

		/// <summary>
		/// Acknowledge jingle message, usually handled automatically
		/// </summary>
		/// <param name="to">recipieint jid</param>
		/// <param name="id">message id</param>
		public void SendAck(string to, string id)
		{
			try
			{
				// send ack
				Iq iqOut = new Iq();
				iqOut.To = to;
				iqOut.From = this._jid;
				iqOut.Type = Matrix.Xmpp.IqType.Result;
				iqOut.Id = id;

				xmppClient.Send(iqOut);
				Log.Debug(">ack");
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		/// <summary>
		/// Retrieves vCard with current users profile information
		/// </summary>
		/// <param name="callback"></param>
		public void GetMyVCard(EventHandler<IqEventArgs> callback)
		{
			try
			{
				var vIq = new VcardIq { Type = IqType.Get };
				xmppClient.IqFilter.SendIq(vIq, callback);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		/// <summary>
		/// Stores vCard with current users profile information ie avatar, nickname, first/last
		/// </summary>
		/// <param name="vCard"></param>
		public void SetMyVCard(Matrix.Xmpp.Vcard.Vcard vCard)
		{
			try
			{
				var vIq = new VcardIq { Type = Matrix.Xmpp.IqType.Set };
				vIq.Vcard = vCard;
				xmppClient.IqFilter.SendIq(vIq, null);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		/// <summary>
		/// Retrieves vCard with another users profile information
		/// </summary>
		/// <param name="to">jid of user</param>
		/// <param name="callback">delegate or callback function for result</param>
		public void GetVCard(string to, EventHandler<IqEventArgs> callback)
		{
			try
			{
				var vIq = new VcardIq { To = to, Type = IqType.Get };
				xmppClient.IqFilter.SendIq(vIq, callback);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		/// <summary>
		/// Change the local user's password.  Make sure to check verify their old password first using cached client login password and enforce password restrictions (a-zA-Z0-9\-_{6,20})
		/// </summary>
		/// <param name="password"></param>
		public void ChangePassword(string password)
		{
			xmppClient.ChangePassword(password);
		}

		public void CloseSession()
		{
			xmppClient.Close();
		}

		public string CreateGroup()
		{
			string roomJid = Guid.NewGuid().ToString();
			Log.Info("Join room " + roomJid);


			JoinGroup(roomJid);

			return roomJid;

			// handle 
			//this.xmppClient.MessageFilter.Add(roomJid, new BareJidComparer(), MessageCallback);

			// Setup new Presence Callback using the PresenceFilter
			//this.xmppClient.PresenceFilter.Add(roomJid, new BareJidComparer(), PresenceCallback);
		}

		public void InviteGroup(string groupName, string userJid)
		{

			string groupJid = groupName + "@" + _mucServer;
			Log.Info("inviting " + userJid + " to " + groupJid);
			this.mucManager.Invite(userJid, groupJid);
		}

		public async void AddGroup(string groupName)
		{
			string jid = groupName + "@" + _mucServer;
			Log.Info("joining group " + jid + " as " + this._jid);
			Presence x = await mucManager.EnterRoomAsync(jid, this._jid.Split('@')[0]);
			var mucX = x.MucUser;
			//set 
			if (mucX.HasStatus(201))
			{
				await mucManager.RequestInstantRoomAsync(jid);
				Log.Info("Accepted MUC defaults.");
			}

		}

        public void DestroyGroup(string groupName)
        {
			string jid = "";
			if (groupName.Contains("@"))
			{
				jid = groupName;
			}
			else { 
				jid = groupName + "@" + _mucServer;
			}
            
            mucManager.DestroyRoom(groupName);
        }

        public void ChangeGroupSubject(string groupName, string subject)
        {
            string jid = groupName + "@" + _mucServer;
            mucManager.ChangeSubject(jid, subject);
        }

		public async void JoinGroup(string groupName)
		{
			try
			{
				string jid = null;
				if (groupName.Contains("@"))
				{
					jid = groupName;
				}
				else { 
					jid = groupName + "@" + _mucServer;
				}

				Log.Info("joining group " + jid + " as " + this._jid);
				Presence x = await mucManager.EnterRoomAsync(jid, this._jid.Split('@')[0]);
				var mucX = x.MucUser;

				// set 
				if (mucX.HasStatus(201))
				{
					await mucManager.RequestInstantRoomAsync(jid);
					Log.Info("Accepted MUC defaults.");
				}

				// setup room as persistent and members only
				var iq = await mucManager.RequestRoomConfigurationAsync(jid);
				if (iq.Type == IqType.Result) // only proceed on result
				{
					// Step 2 and 3, parsing the current config and
					// creating the result is done in the same loop here.
					var xdata = iq.Query.Element<Data>();
					var xDataResult = new Data
					{
						Type = FormType.Submit
					};

					foreach (var field in xdata.GetFields())
					{
						var retField = new Field()
						{
							Type = field.Type, // keep the type
							Var = field.Var // keep the var
						};

                        // we are changing the muc#roomconfig_persistentroom only
                        // other fields get copied only with the existing values            
                        if (field.Var == "muc#roomconfig_persistentroom")
                            retField.AddValue(true);
                        else if (field.Var == "muc#roomconfig_membersonly")
                            retField.AddValue(true);
                        else if (field.Var == "muc#roomconfig_publicroom")
                            retField.AddValue(false);
                        else if (field.Var == "muc#roomconfig_changesubject")
                            retField.AddValue(true);
                        else
                            retField.AddValues(field.GetValues().ToArray());

						xDataResult.AddField(retField);
					}

					// Step 4, submit the changed configuration back to the server (room)
					var submitIq = await mucManager.SubmitRoomConfigurationAsync(jid, xDataResult);
					if (submitIq.Type == IqType.Result)
						Log.Info("success");
					else
						Log.Info("something went wrong");
				}
				//this.mucManager.EnterRoom(jid, null);
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
		}

		public string GetNameRoom(string Jid) {
			JoinGroup(Jid);
			return null;
		}

		public void LeaveGroup(string groupName)
		{
			Log.Info("Leaving " + groupName);
			this.mucManager.ExitRoom(groupName + "@" + _mucServer, null);
		}
		#endregion

		#region Public events
		// going to need Jingle Incoming Call event

		// going to need a bunch of p2p bullshit 


		public delegate void ConnectedEventHandler(object sender, System.EventArgs e);
		/// <summary>
		/// XMPP successfully connected
		/// </summary>
		public event ConnectedEventHandler OnConnected;

		public delegate void CloseEventHandler(object sender, System.EventArgs e);
		/// <summary>
		/// XMPP connection lost
		/// </summary>
		public event ConnectedEventHandler OnClose;

		public delegate void OnAuthErrorEventHandler(object sender, Matrix.Xmpp.Sasl.FailureCondition e);
		/// <summary>
		/// XMPP authentication error
		/// </summary>
		public event OnAuthErrorEventHandler OnAuthError;

		public delegate void PresenceEventHandler(object sender, PresenceEventArgs e);
		public event PresenceEventHandler OnPresence;

		/*
        public delegate void RosterEventHandler(object sender, System.EventArgs e);
        public event RosterEventHandler OnRoster;
        */
		public delegate void MessageEventHandler(object sender, MessageEventArgs e);
		/// <summary>
		/// XMPP text message received
		/// </summary>
		public event MessageEventHandler OnMessage;
        /// <summary>
        /// MUC Group Subject changed
        /// </summary>
        public event MessageEventHandler OnSubjectGroup;

		/// <summary>
		/// XMPP invit received
		/// </summary>
		public event MessageEventHandler OnInvit;

		public delegate void RosterEventArgs(object sender, Matrix.Xmpp.Roster.RosterEventArgs e);

		public event RosterEventArgs OnRosterItem;

		public delegate void ReceiveCandidateEventHandler(object sender, Messaging.Jingle.JingleEventArgs e);
		/// <summary>
		/// XMPP received an SDP candidate
		/// </summary>
		public event ReceiveCandidateEventHandler OnReceiveCandidate;

		public delegate void ReceiveSessionInitiateHandler(object sender, Messaging.Jingle.JingleSdpEventArgs e);
		/// <summary>
		/// XMPP received incoming call request
		/// </summary>
		public event ReceiveSessionInitiateHandler OnReceiveSessionInitiate;

		public delegate void ReceiveSessionAcceptHandler(object sender, Messaging.Jingle.JingleSdpEventArgs e);
		/// <summary>
		/// XMPP received incoming call answer
		/// </summary>
		public event ReceiveSessionAcceptHandler OnReceiveSessionAccept;

		public delegate void ReceiveSessionTerminateHandler(object sender, Messaging.Jingle.JingleEventArgs e);
		/// <summary>
		/// XMPP received hangup
		/// </summary>
		public event ReceiveSessionTerminateHandler OnReceiveSessionTerminate;

		public delegate void SubscribeRequestHandler(object sender, Messaging.SubscribeRequestEventArgs e);
		/// <summary>
		/// XMPP incoming friend/subscribe request, need to approve or deny
		/// </summary>
		public event SubscribeRequestHandler OnSubscribeRequest;

		public delegate void JoinGroupHandler(object sender, string roomJid);
		/// <summary>
		/// XMPP joined a group chat after invitation
		/// </summary>
		public event JoinGroupHandler OnJoinGroup;

		public delegate void ListGroupHandler(object sender, string[] rooms);
		/// <summary>
		/// XMPP List of all groups that are not hidden from you
		/// </summary>
		public event ListGroupHandler OnListGroup;

		#endregion
	}
}
