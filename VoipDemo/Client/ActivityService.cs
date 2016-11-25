using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Messaging;
using Android.Net;
using FM.IceLink;

namespace Legion.Client
{
    
    class ActivityService
    {
		public App App { get; private set; }
		private Context context;
		public static String Jid { get; set; }
		public static String Sdp { get; set; }
        public static String Username { get; set; }
        public static String Password { get; set; }
        public static String Email { get; set; }
        public static String Matrix_linc {get; set;}
		public static bool SignallingStarted;
		public AppPreferences appPreferences { get; private set; }
		private UserService userService;
        private ConnectivityManager connectivityManager;
        public static bool IS_TRY_CONNECT = false;
		List<AppCore.User> userRecentsAndGroups;
		public static List<string> LIST_USERS_ONLINE { get; set;}
        public static bool IsXmppInProgress = false;

        public static ActivityService GetInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ActivityService();
					LIST_USERS_ONLINE = new List<string>();
                }
                return instance;
            }
			 
        }

        private static ActivityService instance;
        private ActivityService() { }

        public void init(Context context)
        {
            App = App.Instance;
            this.context = context;
            connectivityManager = (ConnectivityManager) 
            context.GetSystemService(Context.ConnectivityService);
			
            if (appPreferences == null) {
				appPreferences = new AppPreferences(Application.Context);
			}
			if (userService == null) { 
				userService = new UserService(appPreferences);
			}
			userRecentsAndGroups = LegionUtils.getGroupsTempToList(AppPreferences.GROUPS_TEMP, appPreferences);

			if (Username != null && Password != null) {
                if (IsOnline())
                {
                    if (IS_TRY_CONNECT)
                    {
                        if (App.Xmpp != null)
                        {
                            App.Xmpp.Connect();
                        } else
                        {
                            loadXmppConnect();
                        }
                    }
                    else {
                        loadXmppConnect();
                    }                 
                    
                }
                else {

                    BaseAppCompatActivity o = (BaseAppCompatActivity)context;
                    o.Alert("You are Offline!");                  
                    
                }
                
			}

			StartSignalling();

        }

		public void XmppToNull() {
			App.Xmpp = null;
			App = null;
		}

		public void CloseSession() { 
			App.Xmpp.Close();
			XmppToNull();
            IsXmppInProgress = false;
        }

        protected void loadXmppConnect()
            {

                if (IsXmppInProgress) {
                    return;
                }
                // add common XMPP events
                if (App.Xmpp == null)
                 {
                    IsXmppInProgress = true;
                    this.App.Xmpp = createXmpp();
                    // connected means we have our online contacts and everything is ready to go
                    this.App.Xmpp.OnConnected += Xmpp_OnConnected;                    
				    this.App.Xmpp.OnAuthError += XmppClient_OnAuthError;
				    this.App.Xmpp.OnRosterItem += XmppClient_OnRosterItem;
				    this.App.Xmpp.OnSubscribeRequest += Xmpp_OnSubscribeRequest;
					this.App.Xmpp.OnJoinGroup += Xmpp_OnJoinGroup;
					this.App.Xmpp.OnListGroup += Xmpp_OnListGroup;	
				    this.App.Xmpp.OnSubjectGroup += Xmpp_OnSubjectGroup;
                    this.App.Xmpp.OnClose += XmppClient_OnClose;
                    this.App.Xmpp.OnMessage += Xmpp_OnMessage;
				    this.App.Xmpp.OnInvit += Xmpp_OnInvit;
					this.App.Xmpp.OnReceiveSessionInitiate += Xmpp_OnReceiveSessionInitiate;
                    this.App.Xmpp.OnPresence += Xmpp_OnPresence;
                //this.App.Xmpp.OnClose += XmppClient_OnClose;
                //this.App.Xmpp.OnAuthError += XmppClient_OnAuthError;



                // if (isEnableSubScript)
                //{
                // Incoming VOIP session
                //  this.App.Xmpp.OnReceiveSessionInitiate += Xmpp_OnReceiveSessionInitiate;

                //}

                //this.App.Xmpp.OnInvit += OnInvit;

                // connect to xmpp
                this.App.Xmpp.Connect();


                }
            }

            public Xmpp createXmpp()
            {
                return new Xmpp(Username, WServiceUrl.XMPP_URL_LEGION, Password, Matrix_linc);
            }

			public void StartSignalling()
			{
                if (this.App.Xmpp == null) {
                    return;
                }
				SignallingStarted = true;                
				App.StartSignalling((error) =>
				{
					if (error != null)
					{
					    BaseAppCompatActivity o = (BaseAppCompatActivity)context;
						o.Alert(error);
					}
				});
			}

            public void Xmpp_OnConnected(object sender, EventArgs e)
            {
				if (appPreferences.isFisrtAcess(AppPreferences.FIRST_ACCESS)) {
				     this.App.Xmpp.ListGroup();
				}
			    if (context.GetType() == typeof(SplashAuthVideo))
			    {
				    loginGroups();
				    SplashAuthVideo sp = (SplashAuthVideo)context;
				    sp.Xmpp_OnConnected(sender, e);
			    }
			    else if (context.GetType() == typeof(LoginActivity))
			    {
				    LoginActivity lg = (LoginActivity)context;
				    lg.Xmpp_OnConnected(sender, e);
			    }
			    else if (context.GetType() == typeof(SessionActivity)) {
				    loginGroups();
			    }
				
            }

			void loginGroups() { 
				if (userRecentsAndGroups != null && userRecentsAndGroups.Count > 0)
				{
					var list = userRecentsAndGroups.FindAll(x => x.isGroup == true);
					foreach (AppCore.User u in list)
					{
						this.App.Xmpp.JoinGroup(u.Jid);
					}
				}
			}

			public void XmppClient_OnAuthError(object sender, Matrix.Xmpp.Sasl.FailureCondition e) {
				if (context.GetType() == typeof(LoginActivity))
				{
					LoginActivity lg = (LoginActivity)context;
					lg.XmppClient_OnAuthError(sender, e);
				}
			}
			
            public void Xmpp_OnMessage(object sender, MessageEventArgs e)
            {
                
			    if (appPreferences.isFisrtAcess (AppPreferences.FIRST_ACCESS) == true) {
					return;
				}
                if (context.GetType() == typeof(SessionActivity))
                {
                    SessionActivity session = (SessionActivity)context;
                    session.Xmpp_OnMessage(session, e);
                }
                else if (context.GetType() == typeof(ChatActivity)) {
                    ChatActivity chat = (ChatActivity)context;
                    chat.Xmpp_OnMessage(sender, e);
                }
                else if (context.GetType() == typeof(ContacPhoneActivity))
                {
                    ContacPhoneActivity cp = (ContacPhoneActivity)context;
                    cp.Xmpp_OnMessage(sender, e);
                }

				if (context.GetType() != typeof(ChatActivity)) {
					BaseAppCompatActivity o = (BaseAppCompatActivity)context;
					o.CountMessage(sender, e);
				}
           }

			public void XmppClient_OnRosterItem(object sender, Matrix.Xmpp.Roster.RosterEventArgs e)
			{
				List<AppCore.User> uses = LegionUtils.getListJson(appPreferences.getValueKey(AppPreferences.CONTACTS));
				uses.Add(new AppCore.User(e.RosterItem.Jid.Bare, e.RosterItem.Jid.Bare.Split('@')[0]));
				LegionUtils.saveFoundContacts(uses, appPreferences, AppPreferences.CONTACTS);
				
				if (context.GetType() == typeof(ContacPhoneActivity))
				{
					ContacPhoneActivity cpa = (ContacPhoneActivity)context;
					cpa.XmppClient_OnRosterItem(sender, e);
				}
			}

			public void Xmpp_OnSubscribeRequest(object sender, SubscribeRequestEventArgs e)
			{

				ThreadPool.QueueUserWorkItem(o =>
			 	{ 
					List<AppCore.User> friedsReqs = LegionUtils.getListJson(
						appPreferences.getValueKey(AppPreferences.FRIEND_REQUEST));
					friedsReqs.Add(new AppCore.User(e.From, e.From.Split('@')[0]));

					LegionUtils.saveFoundContacts(friedsReqs, appPreferences, AppPreferences.FRIEND_REQUEST);
				});
			}

			public void Xmpp_OnJoinGroup(object sender, string roomJid)
			{
			   Messaging.Log.Info("joined a room " + roomJid);
			   if (roomJid != null)
				{
				List<AppCore.User> userGroups = LegionUtils.getGroupsTempToList(AppPreferences.GROUPS_TEMP, appPreferences);
					AppCore.User group = new AppCore.User();
					group.Jid = roomJid + "@" + AppCore.Utils._MUS_SERVER;
					group.isGroup = true;

					if (userGroups.Any(x => x.Jid != (group.Jid)))
					{
						userGroups.Add(group);
						if (context != null) {
						    BaseAppCompatActivity o = (BaseAppCompatActivity)context;
							o.saveSeachGroups(userGroups);
						}

					}

				}	
			}

			public void Xmpp_OnListGroup(object sender, string[] rooms)
			{ 
				if (rooms.Length == 0)
				{
					return;
				}

				List<AppCore.User> groups = new List<AppCore.User>();
				foreach (string room in rooms)
				{
					AppCore.User gUser = userService.turnGroupToUserToList(room, room);
					groups.Add(gUser);

					    //if (room.Equals(rooms[rooms.Length - 1]))
					    //{
						    if (context != null)
						    {
							    BaseAppCompatActivity o = (BaseAppCompatActivity)context;
							    o.saveSeachGroups(groups);
							    this.App.Xmpp.JoinGroup(room);
						    }
							
						//}
					}				
			}

			public void Xmpp_OnSubjectGroup(object sender, MessageEventArgs e)
			{
				Console.WriteLine("my name " + e.Body);
				setNameFromGroup(e.Body, e.From);
			}

			public void setNameFromGroup(string groupName, string From) { 
				List<AppCore.User> groups = LegionUtils.getGroupsTempToList(AppPreferences.GROUPS_TEMP, appPreferences);
				var index = groups.FindIndex(c => c.Jid.Split('@')[0] == From.Split('@')[0]);
				groups[index].username = groupName;
				groups[index].Nick = groupName;
				
				LegionUtils.saveFoundGroups(groups, appPreferences, AppPreferences.GROUPS_TEMP);
				
				if (context.GetType() == typeof(SessionActivity))
				{
					SessionActivity o = (SessionActivity)context;
                    o.updateUIListGroupsUsers(groups);
				}
				
			}
            
            public bool IsOnline() {
                NetworkInfo activeConnection = connectivityManager.ActiveNetworkInfo;
                bool isConnect = (activeConnection != null) && activeConnection.IsConnected;
                if (isConnect == false) {
                    App.Xmpp = null;
                }
                return isConnect;        
            }

            public void XmppClient_OnClose(object sender, EventArgs e)
            {
                Log.Debug("OnClose");
                IS_TRY_CONNECT = true;                               
            }

			public void Xmpp_OnInvit(object sender, MessageEventArgs e) {
				if (context.GetType() == typeof(SessionActivity))
				{
					SessionActivity o = (SessionActivity)context;
					o.OnInvit(sender, e);
				} else
				{
					BaseAppCompatActivity o = (BaseAppCompatActivity)context;
					o.OnInvit(sender, e);
				}
			}

		/// <summary>
		/// XMPP presence notificatino, user is online/offline/away
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void Xmpp_OnPresence(object sender, PresenceEventArgs e)
		{

			//RunOnUiThread(() =>
			//{
			if (e.Status == Matrix.Xmpp.PresenceType.Subscribe)
			{

			}
			else if (e.Status == Matrix.Xmpp.PresenceType.Available) { 
				LIST_USERS_ONLINE.Add(e.From);
			}


			//});

		}

			/// <summary>
			/// Handle incoming call
			/// </summary>
			/// <param name="sender"></param>
			/// <param name="e"></param>
			protected virtual void Xmpp_OnReceiveSessionInitiate(object sender, Messaging.Jingle.JingleSdpEventArgs e)
			{
				
				foreach (string jidWithHash in LIST_USERS_ONLINE)
				{
					if (jidWithHash.Contains(e.From))
					{
						e.From = jidWithHash;
					}
				}

				this.App.Jid = e.From;
				this.App.Sdp = e.Sdp;
				var sdp = SDPMessage.Parse(e.Sdp);

                /*
				foreach (var md in sdp.MediaDescriptions)
				{
					if (md.Media.MediaType == SDPMediaType.Video)
					{
						foreach (var ma in md.MediaAttributes)
						{

							if (ma is SDPReceiveOnlyAttribute)
							{
								Log.Info("ma is SDPReceiveOnlyAttribute ");
							}
							if (ma is SDPInactiveAttribute)
							{
								Log.Info("ma is SDPInactiveAttribute");
							}
							if (ma is SDPSendOnlyAttribute)
							{
								Log.Info("ma is SDPSendOnlyAttribute");
							}
							if (ma is SDPSendReceiveAttribute)
							{
								Log.Info("ma is SDPSendReceiveAttribute");
							}

							//if (ma is SDPReceiveOnlyAttribute || ma is SDPInactiveAttribute)
							//{

							//	Signalling.IS_CALL_VIDEO = false;
							//	// Remote participant is NOT sending video.
							//	// ReceiveOnly: not sending video, but willing to receive video
							//	// Inactive: not sending video, and doesn't want to receive video either
							//}
							//else if (ma is SDPSendOnlyAttribute || ma is SDPSendReceiveAttribute)
							//{
							//	//is call audio
							//	Signalling.IS_CALL_VIDEO = true;
							//	// Remote participant IS sending video.
							//	// SendOnly: sending video, but doesn't want to receive video
							//	// SendReceive: sending video, and willing to receive video
							//}
						}
					}
				} */

				if (context != null)
				{
					BaseAppCompatActivity o = (BaseAppCompatActivity)context;
					o.Xmpp_OnReceiveSessionInitiate(sender, e);
				}


			}
        

    }
}