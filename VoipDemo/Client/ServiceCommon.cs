
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AppCore;
using FM.IceLink;
using Matrix.Xmpp.Compression;
using Messaging;

namespace Legion.Client
{
	
	public class ServiceCommon 
	{

		public AppPreferences appPreferences { get; private set; }
		public String Username { get; set; }
		public String Password { get; set; }
		public String Email { get; set; }
		public AppCore.User friend { get; set; }
		public ProgressBar progress;
		public static bool VISIBLE = true;
		public static bool INVISIBLE = false;
		InputMethodManager inputManager;
		private App App { get; set; }
		public static bool SignallingStarted;
		public Activity myActivity { get; set; }
		public AppCore.Message message = new AppCore.Message();
		public List<AppCore.Message> msgs;
		public Dictionary<string, AppCore.Message> recentConversationsUserName;
		public readonly string PHONE_CALL = "PHONE_CALL";
		public readonly string VIDEO_CALL = "VIDEO_CALL";
		private string jsonUser;
		public bool isEnableSubScript { get; set; }
		public List<string> JidsFromServer { get; set; }
		public List<string> jidsPhone { get; set; }
		private string lastBodyMsg = "";
		private UserService userService = new UserService();
		private List<AppCore.User> usersRecents;
		public List<AppCore.User> userLastContacts;

		//T Not receive msg another user 
		// in current chat user
		public string inChatJid;

		protected void OnCreateInit()
		{
			
			App = App.Instance;
			appPreferences = new AppPreferences(Application.Context);
			Username = appPreferences.getValueKey(AppPreferences.USERNAME);
			Password = appPreferences.getValueKey(AppPreferences.PASSWORD);
			//inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
			myActivity = null;
			JidsFromServer = listJidsRoster();
			usersRecents = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
			userLastContacts = listContacts();
			//if (AppUtils.JIdsOnline == null) {
			//	AppUtils.JIdsOnline = new List<string>();
			//}

		}

		protected void loadAppUserNamePass(string username, string password)
		{
			this.App.Username = username;
			this.App.Password = password;
		}

		protected void SingOutApp()
		{
			if (this.App.Xmpp != null)
				this.App.Xmpp = null;
		}

		protected void StartSignalling()
		{
			SignallingStarted = true;
			App.StartSignalling((error) =>
			{
				if (error != null)
				{
					Alert(error);
				}
			});
		}

		public bool isExistUser()
		{
			string userJson = appPreferences.getValueKey(AppPreferences.USER);
			Username = appPreferences.getValueKey(AppPreferences.USERNAME);
			Password = appPreferences.getValueKey(AppPreferences.PASSWORD);

			if (Username == null || Password == null)
			{
				return false;
			}
			else if (Username != null && Password != null)
			{
				this.App.Username = Username;
				this.App.Password = Password;
				return true;
			}
			else {

				if (userJson != null)
				{
					var userObj = Newtonsoft.Json.Linq.JObject.Parse(userJson);
					Username = (string)userObj["username"];
					Email = (string)userObj["email"];
					Password = (string)userObj["password"];
					this.App.Username = Username;
					this.App.Password = Password;

					return true;
				}
				else
				{
					return false;
				}


			}

		}

		public void newConnection(Activity myActivity)
		{
			this.App.Xmpp = null;
			loadXmppConnect(myActivity);
		}

		public Xmpp createXmpp(string idkey)
		{
			return new Xmpp(Username, WServiceUrl.XMPP_URL_LEGION, Password, idkey);
		}

		protected void loadXmppConnect(Activity myActivity)
		{

			this.myActivity = myActivity;
			// add common XMPP events
			if (this.App.Xmpp == null)
			{
				this.App.Xmpp = createXmpp("123");
				// connected means we have our online contacts and everything is ready to go
				this.App.Xmpp.OnConnected += Xmpp_OnConnected;
				this.App.Xmpp.OnMessage += Xmpp_OnMessage;
				this.App.Xmpp.OnPresence += Xmpp_OnPresence;
				this.App.Xmpp.OnClose += XmppClient_OnClose;
				//this.App.Xmpp.OnAuthError += XmppClient_OnAuthError;
				this.App.Xmpp.OnRosterItem += XmppClient_OnRosterItem;
				this.App.Xmpp.OnJoinGroup += Xmpp_OnJoinGroup;
				this.App.Xmpp.OnListGroup += Xmpp_OnListGroup;
				this.App.Xmpp.OnSubjectGroup += Xmpp_OnSubjectGroup;
				this.App.Xmpp.OnSubscribeRequest += Xmpp_OnSubscribeRequest;

				if (isEnableSubScript)
				{
					// Incoming VOIP session
					this.App.Xmpp.OnReceiveSessionInitiate += Xmpp_OnReceiveSessionInitiate;

				}

				this.App.Xmpp.OnInvit += OnInvit;


				// connect to xmpp
				this.App.Xmpp.Connect();
			}
			else {

				this.App.Xmpp.OnPresence += Xmpp_OnPresence;
				this.App.Xmpp.OnConnected += Xmpp_OnConnected;
				this.App.Xmpp.OnInvit += OnInvit;
				this.App.Xmpp.OnClose += XmppClient_OnClose;
				//this.App.Xmpp.OnAuthError += XmppClient_OnAuthError;
				this.App.Xmpp.OnRosterItem += XmppClient_OnRosterItem;
				this.App.Xmpp.OnJoinGroup += Xmpp_OnJoinGroup;
				this.App.Xmpp.OnListGroup += Xmpp_OnListGroup;
				this.App.Xmpp.OnSubjectGroup += Xmpp_OnSubjectGroup;

				if (isEnableSubScript)
				{
					//	// Incoming VOIP session
					this.App.Xmpp.OnReceiveSessionInitiate += Xmpp_OnReceiveSessionInitiate;
					//	// Friend request to say yes or no
					this.App.Xmpp.OnSubscribeRequest += Xmpp_OnSubscribeRequest;
				}

				//if (myActivity.GetType() == typeof(ChatActivity))
				//{
				this.App.Xmpp.OnMessage += Xmpp_OnMessage;
				//}
				//else { 

				//}
			}
		}



		protected virtual void XmppClient_OnClose(object sender, EventArgs e)
		{
			Log.Debug("OnClose");
			//this.App.Xmpp = null;
			//this.App.Xmpp.CloseSession();
			loadXmppConnect(myActivity);
		}

		protected virtual void XmppClient_OnAuthError(object sender, FailureCondition e)
		{
			//Log.Debug("Failure");
			//RunOnUiThread(() =>
			//			  showMsg("Ops! Error in your Username or Password.")
			//			 );
			this.App.Xmpp = null;
		}

		public void showMsg(string msg)
		{
			//VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
			//Toast.MakeText(myActivity.ApplicationContext,
			//			   msg, ToastLength.Long).Show();
		}

		protected virtual void XmppClient_OnRosterItem(object sender, Matrix.Xmpp.Roster.RosterEventArgs e)
		{
			List<AppCore.User> uses = LegionUtils.getListJson(appPreferences.getValueKey(AppPreferences.CONTACTS));
			uses.Add(new AppCore.User(e.RosterItem.Jid.Bare, e.RosterItem.Jid.Bare.Split('@')[0]));
			LegionUtils.saveFoundContacts(uses, appPreferences, AppPreferences.CONTACTS);

		}

		protected virtual void Xmpp_OnListGroup(object sender, string[] rooms)
		{

			//         foreach (string room in rooms)
			//{
			//	Console.WriteLine("hey its " + room);
			//	///JoinOnGroups(room);
			//}

		}

		protected virtual void Xmpp_OnSubjectGroup(object sender, MessageEventArgs e)
		{
			Console.WriteLine("my name " + e.Body);
			// List<AppCore.User> groups = LegionUtils.getListJson(AppPreferences.GROUPS_TEMP);
			//foreach (AppCore.User u in groups) {                                    
			//     var index = groups.FindIndex(c => c.Jid.Split('@')[0] == u.Jid.Split('@')[0]);
			//    groups[index].username = e.Body;
			//     groups[index].Nick = e.Body;
			///  }
			//   LegionUtils.saveFoundGroups(groups, appPreferences, AppPreferences.GROUPS_TEMP);
		}

		public void GetXmppListGroup()
		{
			this.App.Xmpp.ListGroup();
		}

		public void JoinOnGroups(string JidGroup)
		{
			this.App.Xmpp.JoinGroup(JidGroup);
		}

		protected virtual void Xmpp_OnConnected(object sender, EventArgs e)
		{
			//UpdateListView("Connected to XMPP server");
			GetXmppListGroup();

			/// Demonstrate Avatar/Profile/User Info
			/// This whole section can be removed

			// First create a vCard with our information that we want to set
			/*
            var vCard = new Matrix.Xmpp.Vcard.Vcard();
            vCard.Fullname = "Lloyd Evetts";
            vCard.Nickname = "Lloyd Username";
            // avatar photo usage
            // https://gitlab.com/matrix-xmpp/samples/blob/master/csharp/SilverlightMuc/ConferenceRoster.xaml.cs#L134
            // vCard.Photo = 

            // send vcard to server to store
            App.Instance.Xmpp.SetMyVCard(vCard);
            */

			// retrieve vcard with our stored information
			/*
            App.Instance.Xmpp.GetMyVCard(delegate(object s, Matrix.Xmpp.Client.IqEventArgs eIq) {
                var vc = eIq.Iq.Query as Matrix.Xmpp.Vcard.Vcard;
                UpdateListView("<GetMyVCard: " + vc.Nickname);
            });
            */

			// retrieve vcard with another user's information
			/*
            App.Instance.Xmpp.GetVCard("testuser2@messaging.legiontech.org", delegate (object s, Matrix.Xmpp.Client.IqEventArgs eIq)
            {
                var vc = eIq.Iq.Query as Matrix.Xmpp.Vcard.Vcard;
                UpdateListView("<GetVCard: " + vc.Nickname);
            });
            */

			// search for user by email address.
			/*
            App.Instance.Xmpp.Search("llloyd@gmail.com", delegate (object s, Matrix.Xmpp.Client.IqEventArgs eIq)
            {
                var iq = eIq.Iq;
                if (iq.Type == Matrix.Xmpp.IqType.Result)
                {
                    var searchQuery = iq.Query as Matrix.Xmpp.Search.Search;
                    foreach (var sItem in searchQuery.GetItems())
                    {
                        Log.Info("search result: " + sItem.Jid + "\n");
                    }                    
                }                
            });
            */

			/// End demonstration section
		}

		public virtual void Xmpp_OnMessage(object sender, MessageEventArgs e)
		{
			if (userLastContacts == null || userLastContacts.Count == 0)
			{
				return;
			}

			if (myActivity.GetType() == typeof(ChatActivity))
			{

			}
			//RunOnUiThread(() =>
			setUserRecentChat(e.From, e.Body);
			msgFriend(sender, e);
			//	);
			//}

			//save ready msg to count
			string lastMsg = appPreferences.getValueKey("LAST_MSG" + e.From);
			bool isSaveRecMsg = true;
			if (lastMsg != null)
			{
				if (lastMsg.Equals(e.Body))
				{
					isSaveRecMsg = false;
				}
			}
			if (LegionUtils.GetUserNameClear(e.From) != null && isSaveRecMsg == true)
			{
				appPreferences.saveKey("LAST_MSG" + e.From, e.Body);
				int countMsg = appPreferences.getValueKeyMSGCount(LegionUtils.GetUserNameClear(e.From));
				appPreferences.saveKeyMSGCount(LegionUtils.GetUserNameClear(e.From), countMsg + 1);
			}


		}

		public void setUserRecentChat(string From, string msgem)
		{

			if (usersRecents.Any(u => u.Jid.Split('@')[0].Equals(From.Split('@')[0])))
			{
				int index = usersRecents.FindIndex(c => c.Jid.Split('@')[0].Equals(From.Split('@')[0]));
				usersRecents = userService.moveItemToFirst(usersRecents, index);
				setLastMsgInPhone(index, msgem);
			}
			else {
				if (friend == null)
				{
					int indexF = userLastContacts.FindIndex(c => c.Jid.Split('@')[0].Equals(From.Split('@')[0]));
					friend = userLastContacts[indexF];
				}
				usersRecents.Add(friend);
				int index = usersRecents.FindIndex(c => c.Jid.Split('@')[0].Equals(From.Split('@')[0]));
				usersRecents = userService.moveItemToFirst(usersRecents, index);
				setLastMsgInPhone(index, msgem);
			}

			saveSeachGroups(usersRecents);


		}

		public void setLastMsgInPhone(int index, string msgem)
		{
			usersRecents[index].Phone = LegionUtils.Truncate(msgem, 26);
		}

		protected virtual void OnInvit(object sender, MessageEventArgs e)
		{

			//RunOnUiThread(() =>
		   //{
			   List<AppCore.User> groups = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
			   string invitFrom = e.From;
			   string invitGroup = e.Body;

			   AppCore.User g = new AppCore.User();
			   g.Jid = invitGroup;
			   g.username = invitGroup;
			   g.isGroup = true;
			   groups.Add(g);
			   saveSeachGroups(groups);

		  // });


		}



		public void msgFriend(object sender, MessageEventArgs e)
		{

			string userNameFriend = LegionUtils.GetUserNameClear(e.From);
			AppCore.Message m = null;
			Bitmap bit = null;
			//byte[] bytes = LegionUtils.base64ToByte(e.Body);
			//if (bytes != null)
			//{
			//	bit = BitmapFactory.DecodeByteArray(bytes, 0, bytes.Length);
			//}
			if (bit == null)
			{
				if (e.From.Contains(AppCore.Utils._MUS_SERVER))
				{
					m = new AppCore.Message(e.From, e.Body, DateTime.Now, false, true);
				}
				else {
					m = new AppCore.Message(e.From, e.Body, DateTime.Now, false);
				}

			}
			else {
				string fileName = LegionUtils.SaveBitmapGalerry(bit);
				if (e.From.Contains(AppCore.Utils._MUS_SERVER))
				{
					m = new AppCore.Message(e.From, e.Body, DateTime.Now, false, true, fileName, true);
				}
				else {
					m = new AppCore.Message(e.From, e.Body, DateTime.Now, false, fileName, true);
				}
			}

			this.message = m;
			if (this.msgs == null)
			{
				msgs = getMsgs(e.From);
			}
			this.msgs.Add(m);
			saveMsg(userNameFriend);

			if (friend != null)
			{
				if (inChatJid != null)
				{
					if (e.From.Equals(inChatJid))
					{
						RefrashRecycleView(friend);
					}
				}


			}

		}

		public void saveMsg(string username)
		{
			string tmpMsgs = LegionUtils.parceToGson(msgs);
			appPreferences.saveKey(username, tmpMsgs);
			saveRecentUser(username);
		}

		void saveRecentUser(string username)
		{
			UserService useruserService = new UserService();
			if (recentConversationsUserName == null)
			{
				recentConversationsUserName = useruserService.getRecentUsers(appPreferences);
			}

			if (recentConversationsUserName.TryGetValue(username, out message))
			{
				recentConversationsUserName.Remove(username);
			}

			Dictionary<string, AppCore.Message> reTm = new Dictionary<string, AppCore.Message>();
			foreach (var item in recentConversationsUserName)
			{
				reTm.Add(item.Key, item.Value);
			}
			reTm.Add(username, msgs[msgs.Count - 1]);
			string recentUserConv = LegionUtils.parceToGson(reTm);
			appPreferences.saveKey(AppPreferences.CONVERSA_BY_JID, recentUserConv);
		}

		protected virtual void RefrashRecycleView(AppCore.User friend)
		{

		}

		/// <summary>
		/// XMPP presence notificatino, user is online/offline/away
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public virtual void Xmpp_OnPresence(object sender, PresenceEventArgs e)
		{

			RunOnUiThread(() =>
			{
				if (e.Status == Matrix.Xmpp.PresenceType.Subscribe)
				{
					Activity actualActivity;

					if (myActivity.Window.DecorView.IsShown)
					{
						actualActivity = myActivity;
					}
					else { return; }
				}


			});

		}

		/// <summary>
		/// Handle incoming call
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void Xmpp_OnReceiveSessionInitiate(object sender, Messaging.Jingle.JingleSdpEventArgs e)
		{
			//UpdateListView("<Incoming call");
			Activity actualActivity;
			if (myActivity.Window.DecorView.IsShown)
			{
				actualActivity = myActivity;

			}
			else { return; }

			App.Jid = e.From;
			App.Sdp = e.Sdp;
			var sdp = SDPMessage.Parse(e.Sdp);

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
			}

			// Prompt to answer call
			//this.RunOnUiThread(() =>
			//{
				new AlertDialog.Builder(actualActivity)
				.SetPositiveButton("Yes", (s, args) =>
				{

					Intent intent = null;
					if (Signalling.IS_CALL_VIDEO)
					{
						//intent = new Intent(ApplicationContext, typeof(VideoActivity));
					}
					else {
						//intent = new Intent(ApplicationContext, typeof(VoiceCallActivity));
					}

					intent.PutExtra("callType", "incoming");
					intent.PutExtra("userChat", App.Jid);
					//StartActivity(intent);
				})
				.SetNegativeButton("No", (s, args) =>
				{
					App.Instance.Xmpp.SendSessionTerminate(e.From);
				})
				.SetTitle("Incoming Call")
				.SetMessage("Would you like to answer?")
				.Show();
			//});
		}

		/// <summary>
		/// Example of how to handle an incoming friend request
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void Xmpp_OnSubscribeRequest(object sender, SubscribeRequestEventArgs e)
		{

			List<AppCore.User> friedsReqs = LegionUtils.getListJson(
				appPreferences.getValueKey(AppPreferences.FRIEND_REQUEST));
			friedsReqs.Add(new AppCore.User(e.From, e.From.Split('@')[0]));

			LegionUtils.saveFoundContacts(friedsReqs, appPreferences, AppPreferences.FRIEND_REQUEST);

		}

		public void alertSub(Activity actualActivity, AppCore.User u)
		{
			List<AppCore.User> uses = LegionUtils.getListJson(appPreferences.getValueKey(AppPreferences.CONTACTS));

			new AlertDialog.Builder(actualActivity)
				.SetPositiveButton("Yes", (s, args) =>
				{
					Log.Info("Accept friend request");
					this.App.Xmpp.Approve(u.Jid);
					uses.Add(u);
					LegionUtils.saveFoundContacts(uses, appPreferences, AppPreferences.CONTACTS);
					LegionUtils.saveFoundContacts(new List<AppCore.User>(), appPreferences, AppPreferences.FRIEND_REQUEST);
					appPreferences.saveKey(AppPreferences.FIRST_ACCESS, true);
				})
				.SetNegativeButton("No", (s, args) =>
				{
					Log.Info("Deny friend request");
					this.App.Xmpp.Deny(u.Jid);
				})
				.SetTitle("Friend Request")
						   .SetMessage("Would you like to add " + u.Jid + " as a friend?")
				.Show();
		}


		/// <summary>
		/// Update UI listview
		/// </summary>
		/// <param name="msg"></param>
		protected virtual void UpdateListView(string msg)
		{
			//RunOnUiThread(() =>
			//{
				Messaging.Log.Info(msg);

				//((ArrayAdapter)HistoryListView.Adapter).Add(msg);
				//HistoryListView.SetSelection(HistoryListView.Adapter.Count - 1);
			//});
		}

		public List<AppCore.Message> getMsgs(string username)
		{
			List<AppCore.Message> ms =
				LegionUtils.getListMessageJson(appPreferences.getValueKey(username));
			if (ms == null || ms.Equals(""))
			{
				return new List<AppCore.Message>();
			}
			else
			{
				return ms;
			}

		}

		protected virtual void Alert(string format, params object[] args)
		{
			//RunOnUiThread(() =>
			//{
				if (!IsFinishing)
				{
					var alert = new AlertDialog.Builder(this);
					alert.SetMessage(string.Format(format, args));
					alert.SetPositiveButton("OK", (sender, e) => { });
					alert.Show();
				}
			//});
		}

		protected virtual void XmppSend(string to, string message)
		{
			this.App.Xmpp.Send(to, message);
		}

		protected virtual void XmppSendGroup(string to, string message)
		{
			this.App.Xmpp.SendGroup(to, message);
		}

		public void SetSessionID(string sessionId)
		{
			this.App.SessionId = sessionId;
		}

		public void SetJID(string JId)
		{
			this.App.Jid = JId;
		}

		public void CloseSession()
		{
			//this.App.Xmpp.CloseSession();
			//this.App.Xmpp = null;    
		}

		public string AddNewGroup()
		{
			return this.App.Xmpp.CreateGroup();
		}

		public void ChangeGroupSubject(string groupJId, string groupName)
		{
			this.App.Xmpp.ChangeGroupSubject(groupJId, groupName);
		}

		public void Xmpp_OnJoinGroup(object sender, string roomJid)
		{
			Messaging.Log.Info("joined a room " + roomJid);
			if (roomJid != null)
			{
				List<AppCore.User> userGroups = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
				AppCore.User group = new AppCore.User();
				group.Jid = roomJid + "@" + AppCore.Utils._MUS_SERVER;
				group.isGroup = true;

				if (userGroups.Any(x => !x.Jid.Equals(group.Jid)))
				{
					userGroups.Add(group);
					saveSeachGroups(userGroups);
				}

			}
		}

		public void XmppGetGroupName(string Jid)
		{
			this.App.Xmpp.GetNameRoom(Jid);
		}

		public void invictFriend(List<AppCore.User> Users, string groupJid)
		{
			foreach (AppCore.User user in Users)
			{
				this.App.Xmpp.InviteGroup(groupJid, user.username + "@" + GetString(Resource.String.jid));
			}
		}

		public void sendMsgGroup(string groupJid, string msg)
		{
			this.App.Xmpp.SendGroup(groupJid, msg);
		}

		public List<string> listJidsRoster()
		{
			if (appPreferences.getValueKey(AppPreferences.JIDSFROM_SERVER) != null)
			{
				return LegionUtils.getListStringJson(appPreferences.getValueKey(AppPreferences.JIDSFROM_SERVER));
			}
			else {
				return new List<string>();
			}
		}

		public List<User> listContacts()
		{
			List<User> uses = new List<User>();
			List<User> usReturnList = new List<User>();
			jidsPhone = new List<string>();
			if (appPreferences.getValueKey(AppPreferences.CONTACTS) != null)
			{

				uses = LegionUtils.getListJson(appPreferences.getValueKey(AppPreferences.CONTACTS));
				foreach (User u in uses)
				{

					if (!jidsPhone.Contains(u.Jid))
					{
						jidsPhone.Add(u.Jid);
						usReturnList.Add(u);
					}

				}

			}

			uses = usReturnList;
			return uses;
		}

		public void saveSeachGroups(List<AppCore.User> groups)
		{
			LegionUtils.saveFoundGroups(groups, appPreferences, AppPreferences.GROUPS_TEMP);
		}

		public List<AppCore.User> getGroupsTempToList(string APP_PREF)
		{
			string jsonGroup = appPreferences.getValueKey(APP_PREF);
			List<AppCore.User> usersGroups = null;
			if (jsonGroup != null || jsonGroup != "")
			{
				usersGroups = LegionUtils.getListJson(jsonGroup);

			}
			else {
				usersGroups = new List<User>();
			}
			return usersGroups;
		}

		public void removeGroup(User groupName)
		{
			this.App.Xmpp.DestroyGroup(groupName.Jid);

			AppCore.User user = new AppCore.User();
			user.users = getGroupsTempToList(AppPreferences.GROUPS_TEMP);

			var itemToRemove = user.users.Single(r => r.Jid.Equals(groupName.Jid));
			user.users.Remove(itemToRemove);

			user.users.Remove(groupName);
			saveSeachGroups(user.users);
		}
	}

}

