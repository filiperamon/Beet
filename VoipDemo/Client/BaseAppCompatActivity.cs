
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Messaging;
using Log = Messaging.Log;
using Android.Graphics;
using Android.Util;
using Android.Provider;
using Android.Graphics.Drawables;
using Android.Views.InputMethods;
using System.Threading;
using System.Threading.Tasks;
using Matrix.Xmpp.Sasl;
using AppCore;
using FM.IceLink;
using Android.Net;

namespace Legion.Client
{
	[Activity(Label = "BaseAppCompatActivity")]
	public abstract class BaseAppCompatActivity : AppCompatActivity
	{
		ActivityService ActivityService;
		public AppPreferences appPreferences { get; private set; }
		public String Username { get; set; }
		public String Password { get; set; }
		public String Email { get; set; }
		public AppCore.User friend { get; set; }
		public ProgressBar progress;
		public static bool VISIBLE = true;
		public static bool INVISIBLE = false;
		InputMethodManager inputManager;
		public static bool SignallingStarted;
		public Activity myActivity { get; set; }
		public AppCore.Message message = new AppCore.Message();
		public List<AppCore.Message> msgs;
		public Dictionary<string, AppCore.Message> recentConversationsUserName;
		public RecyclerView rvList { get; set; }
		public LinearLayoutManager mLayoutManager { get; set; }
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

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
            
			appPreferences = new AppPreferences(Application.Context);
			Username = appPreferences.getValueKey(AppPreferences.USERNAME);
            loadNewActivity();                               
        }

       

        public void loadComponentsInit() {
            Password = appPreferences.getValueKey(AppPreferences.PASSWORD);
            inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);
            //JidsFromServer = listJidsRoster();
            usersRecents = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
            userLastContacts = listContacts();

            ActivityService.Username = Username;
            ActivityService.Password = Password;
            ActivityService.Matrix_linc = GetString(Resource.String.matrix_linc);
            loadConnectionXmpp();
        }
        public void loadNewActivity() {
            if (this.GetType() == typeof(SplashAuthVideo)) {
                if (Username != null)
                {
                    SplashAuthVideo s = (SplashAuthVideo)this;
                    s.StartActivity(new Intent(this.ApplicationContext, typeof(SessionActivity)));
                }
            }
            
            loadComponentsInit();                 
        }

		public void loadConnectionXmpp() {
			ActivityService.GetInstance.init(this);
		}

		public void loadAppUserNamePass(string username, string password)
		{
			ActivityService.Username = username;
			ActivityService.Password = password;
		}

		protected void SingOutApp()
		{
			ActivityService.GetInstance.XmppToNull();
		}

		protected void CloseSession() {
			ActivityService.GetInstance.CloseSession();
		}

		protected void StartSignalling()
		{
			//SignallingStarted = true;
			//App.StartSignalling((error) =>
			//{
			//	if (error != null)
			//	{
			//		Alert(error);
			//	}
			//});
		}

		public Toolbar loadToolBar(int title)
		{
			Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);
			TextView tvToo = (TextView)toolbar.FindViewById(Resource.Id.toolbar_title);
			tvToo.Text = (GetString(title));
			tvToo.Typeface = Typeface.DefaultBold;

			return toolbar;
		}

		//public Xmpp createXmpp()
		//{
		//	return new Xmpp(Username, WServiceUrl.XMPP_URL_LEGION, Password, GetString(Resource.String.matrix_linc));
		//}

		public override bool OnKeyUp(Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Menu)
			{

				StartActivity(new Intent(ApplicationContext, typeof(ConfigurationActivity)));
				OverridePendingTransition(Resource.Animator.slide_out_right, Resource.Animator.stable);
				return true;
			}
			return base.OnKeyUp(keyCode, e);
		}

		public void VisibleProgress(int id, bool isVisible)
		{
			if (progress == null)
			{
				progress = (ProgressBar)FindViewById(id);
			}

			if (isVisible)
			{
				progress.Visibility = ViewStates.Visible;
			}
			else {
				progress.Visibility = ViewStates.Gone;
			}

		}

		protected override void OnResume()
		{
			base.OnResume();
		}

		public void hidenKeyBoard()
		{
			inputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
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
				ActivityService.Username = Username;
				ActivityService.Username = Password;
				return true;
			}
			else {

				if (userJson != null)
				{
					var userObj = Newtonsoft.Json.Linq.JObject.Parse(userJson);
					Username = (string)userObj["username"];
					Email = (string)userObj["email"];
					Password = (string)userObj["password"];
					ActivityService.Username = Username;
					ActivityService.Username = Password;

					return true;
				}
				else
				{
					return false;
				}


			}

		}

		protected override void OnStart()
		{
			base.OnStart();
            
        }



		protected virtual void XmppClient_OnClose(object sender, EventArgs e)
		{
			Log.Debug("OnClose");
		}

		//protected virtual void XmppClient_OnAuthError(object sender, FailureCondition e)
		//{
		//	Log.Debug("Failure");
		//	RunOnUiThread(() =>
		//				  showMsg("Ops! Error in your Username or Password.")
		//				 );
		//	this.App.Xmpp = null;
		//}

		public void showMsg(string msg)
		{
			VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
			Toast.MakeText(myActivity.ApplicationContext,
						   msg, ToastLength.Long).Show();
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

		protected virtual void Xmpp_OnSubjectGroup(object sender, MessageEventArgs e) {
			Console.WriteLine("my name " + e.Body);
           // List<AppCore.User> groups = LegionUtils.getListJson(AppPreferences.GROUPS_TEMP);
            //foreach (AppCore.User u in groups) {                                    
           //     var index = groups.FindIndex(c => c.Jid.Split('@')[0] == u.Jid.Split('@')[0]);
            //    groups[index].username = e.Body;
           //     groups[index].Nick = e.Body;
          ///  }
         //   LegionUtils.saveFoundGroups(groups, appPreferences, AppPreferences.GROUPS_TEMP);
        }

		public void GetXmppListGroup() { 
			//this.App.Xmpp.ListGroup();
		}

		public void JoinOnGroup(string JidGroup) {
			ActivityService.GetInstance.App.Xmpp.JoinGroup(JidGroup);
		}

		protected virtual void Xmpp_OnConnected(object sender, EventArgs e)
		{
            //UpdateListView("Connected to XMPP server");
            //GetXmppListGroup();

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

		public void closeXmpp() {
			//this.App.Xmpp.Close();
		}

		public virtual void Xmpp_OnMessage(object sender, MessageEventArgs e)
		{
           
			if (userLastContacts == null || userLastContacts.Count == 0) {
				return;
			}
			setUserRecentChat(e.From, e.Body);

		}

		public void CountMessage(object sender, MessageEventArgs e) { 
			if (LegionUtils.GetUserNameClear(e.From) != null)
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
				setLastMsgInPhone(usersRecents,0, msgem);
			}
			else {
				if (friend == null) {
                    if (userLastContacts.Any(u => u.Jid.Split('@')[0].Equals(From.Split('@')[0])))
                    {
                        int indexF = userLastContacts.FindIndex(c => c.Jid.Split('@')[0].Equals(From.Split('@')[0]));
                        friend = userLastContacts[indexF];
                    }
                    else {
                        friend = new User();
                        friend.Jid = From + "@" + GetString(Resource.String.jid);
                        friend.username = From.Split('@')[0];
                        friend.Nick = friend.username;
                        friend.isGroup = false;
                    }                    
				}
				usersRecents.Add(friend);
				int index = usersRecents.FindIndex(c => c.Jid.Split('@')[0].Equals(From.Split('@')[0]));
				usersRecents = userService.moveItemToFirst(usersRecents, index);
				setLastMsgInPhone(usersRecents, 0, msgem);
			}

			saveSeachGroups(usersRecents);

		}

		public void setLastMsgInPhone(List<AppCore.User> list,int index, string msgem)
		{
			list[index].Phone = LegionUtils.Truncate(msgem, 26);
		}

		public virtual void OnInvit(object sender, MessageEventArgs e)
		{

			RunOnUiThread(() =>
		   {
			   List<AppCore.User> groups = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
			   string invitFrom = e.From;
			   string managerGroup = e.Body;

			   AppCore.User g = new AppCore.User();
			   g.Jid = invitFrom;
			   g.username = invitFrom;
			   g.isGroup = true;
			   groups.Add(g);
			  
			   //JoinOnGroup(invitFrom);

				int index = groups.FindIndex(c => c.Jid.Split('@')[0].Equals(e.From.Split('@')[0]));
			    groups = userService.moveItemToFirst(groups, index);
				setLastMsgInPhone(groups, 0, "@Joined the Group");
				saveSeachGroups(groups);

				if (this.GetType() == typeof(SessionActivity)) {
					SessionActivity s = (SessionActivity) this;
				    s.updateUIListGroupsUsers(groups);
				}
		   });

		}

		public void saveMsg(string friend, AppCore.Message message)
		{
			string tmpMsgs = LegionUtils.parceToGson(msgs);
			appPreferences.saveKey(friend, tmpMsgs);
			saveRecentUser(friend, message);
		}

		void saveRecentUser(string friend, AppCore.Message message)
		{

			recentConversationsUserName = userService.getRecentUsers(appPreferences);
			if (recentConversationsUserName.TryGetValue(friend, out message))
			{
				recentConversationsUserName.Remove(friend);
			}

			Dictionary<string, AppCore.Message> reTm = new Dictionary<string, AppCore.Message>();
			foreach (var item in recentConversationsUserName)
			{
				reTm.Add(item.Key, item.Value);
			}
			reTm.Add(friend, msgs[msgs.Count - 1]);
			string recentUserConv = LegionUtils.parceToGson(reTm);
			appPreferences.saveKey(AppPreferences.CONVERSA_BY_JID, recentUserConv);
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
			if (recentConversationsUserName == null) { 
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
		public virtual void Xmpp_OnReceiveSessionInitiate(object sender, Messaging.Jingle.JingleSdpEventArgs e)
		{
			
			// Prompt to answer call
			this.RunOnUiThread(() =>
			{
				new AlertDialog.Builder(this)
				.SetPositiveButton("Yes", (s, args) =>
				{

					Intent intent = new Intent(ApplicationContext, typeof(VideoActivity));
					//}
					//else {
					//	intent = new Intent(ApplicationContext, typeof(VoiceCallActivity));
					//}

					intent.PutExtra("callType", "incoming");
					intent.PutExtra("userChat", e.From);
					StartActivity(intent);
				})
				.SetNegativeButton("No", (s, args) =>
				{
					ActivityService.GetInstance.App.Xmpp.SendSessionTerminate(e.From);
				})
				.SetTitle("Incoming Call")
				.SetMessage("Would you like to answer?")
				.Show();
			});
		}

		/// <summary>
		/// Example of how to handle an incoming friend request
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Xmpp_OnSubscribeRequest(object sender, SubscribeRequestEventArgs e)
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
					ActivityService.GetInstance.App.Xmpp.Approve(u.Jid);
				    uses.Add(u);
					LegionUtils.saveFoundContacts(uses, appPreferences, AppPreferences.CONTACTS);
				    LegionUtils.saveFoundContacts(new List<AppCore.User>(), appPreferences, AppPreferences.FRIEND_REQUEST);
					appPreferences.saveKey(AppPreferences.FIRST_ACCESS, true);
				})
				.SetNegativeButton("No", (s, args) =>
				{
					Log.Info("Deny friend request");
				    ActivityService.GetInstance.App.Xmpp.Deny(u.Jid);
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
			RunOnUiThread(() =>
			{
				Messaging.Log.Info(msg);

				//((ArrayAdapter)HistoryListView.Adapter).Add(msg);
				//HistoryListView.SetSelection(HistoryListView.Adapter.Count - 1);
			});
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

		public virtual void Alert(string format, params object[] args)
		{
			RunOnUiThread(() =>
			{

					var alert = new AlertDialog.Builder(this);
					alert.SetMessage(string.Format(format, args));
					alert.SetPositiveButton("OK", (sender, e) => { });
					alert.Show();
				
			});
		}

		protected virtual void XmppSend(string to, string message)
		{
			ActivityService.GetInstance.App.Xmpp.Send(to, message);
		}

		protected virtual void XmppSendGroup(string to, string message)
		{            
            ActivityService.GetInstance.App.Xmpp.SendGroup(to, message);
        }

		public void SetSessionID(string sessionId)
		{            
            ActivityService.GetInstance.App.SessionId = sessionId;
        }

		public void SetJID(string JId)
		{
			ActivityService.GetInstance.App.Jid = JId;
		}


		public string AddNewGroup()
		{
			return ActivityService.GetInstance.App.Xmpp.CreateGroup();
		}

		public void ChangeGroupSubject(string groupJId, string groupName)
		{
			ActivityService.GetInstance.App.Xmpp.ChangeGroupSubject(groupJId, groupName);
		}

		public void Xmpp_OnJoinGroup(object sender, string roomJid)
		{
			Messaging.Log.Info("joined a room " + roomJid);
			if (roomJid != null)
			{
				List<AppCore.User> userGroups = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
				AppCore.User group = new AppCore.User();
				group.Jid = roomJid + "@"+ AppCore.Utils._MUS_SERVER;
				group.isGroup = true;

				if (userGroups.Any(x => !x.Jid.Equals(group.Jid)))
				{
					userGroups.Add(group);
					saveSeachGroups(userGroups);
				}

			}
		}

		public void XmppGetGroupName(string Jid) {
			ActivityService.GetInstance.App.Xmpp.GetNameRoom(Jid);
		}

		public void invictFriend(List<AppCore.User> Users, string groupJid) {
			foreach (AppCore.User user in Users) {
				ActivityService.GetInstance.App.Xmpp.InviteGroup(groupJid, user.username +
																 "@" + GetString(Resource.String.jid));
			}
		}

		public void sendMsgGroup(string groupJid, string msg) {
			ActivityService.GetInstance.App.Xmpp.SendGroup(groupJid, msg);

		}

		public List<string> listJidsRoster() {
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
			return LegionUtils.getGroupsTempToList(APP_PREF, appPreferences);
		}

        public void removeGroup(User groupName)
        {
            //this.App.Xmpp.DestroyGroup(groupName.Jid);

            AppCore.User user = new AppCore.User();
            user.users = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
            
			var itemToRemove = user.users.Single(r => r.Jid.Equals(groupName.Jid));
			user.users.Remove(itemToRemove);

			user.users.Remove(groupName);
            saveSeachGroups(user.users);
        }
	}
}

