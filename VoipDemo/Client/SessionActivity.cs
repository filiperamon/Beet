using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Graphics;
using Layout = Android.Resource.Layout;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

using FM.IceLink.WebRTC;
using Android.Text;

using Messaging;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using AppCore;
using System.Threading.Tasks;

namespace Legion.Client
{
	[Activity(Label = "Legion")]
	public class SessionActivity : BaseAppCompatActivity
	{
        ActivityService ActivityService;
        private RecyclerView mRecyclerView;
		private RecyclerView.LayoutManager mLayoutManager;
		private UserAdapter userAdapter;
		private User user;
		private UserService userService;
		private Dictionary<string, AppCore.Message> recentConversationsUserName;
		private RelativeLayout relativeWelcome;
		public List<AppCore.Message> msgs;
		private List<Group> allGroups;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.Session);
			relativeWelcome = (RelativeLayout)FindViewById(Resource.Id.relativWelcome);
			userService = new UserService(appPreferences);
			isEnableSubScript = true;

			var toolbar = loadToolBar(Resource.String.conversations);
			toolbar.SetNavigationIcon(Resource.Drawable.config);

			mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
			mLayoutManager = new LinearLayoutManager(this);
			mRecyclerView.SetLayoutManager(mLayoutManager);

            //loadListUserRecent();

            //load friends and Groups in cash data
            user = new AppCore.User();
			user.users = getGroupsTempToList(AppPreferences.GROUPS_TEMP);

			if (appPreferences.isFisrtAcess(AppPreferences.FIRST_ACCESS_SESSION))
			{
				appPreferences.saveKey(AppPreferences.FIRST_ACCESS_SESSION, false);
				ListGroupsOnStart();
			}
			else { 

				System.Threading.ThreadPool.QueueUserWorkItem(o =>
				{
					updateListCardUi();
					ListGroupsOnStart();

					if (user.users.Count > 0)
					{
						updateListCardUi();
					}

				});

			}

			verifySubscriptFriend();

		}

		protected override void OnStart()
		{
			base.OnStart();
		}

		public override void Xmpp_OnMessage(object sender, MessageEventArgs e)
		{
			base.Xmpp_OnMessage(sender, e);
			msgFriend(sender, e);
			RunOnUiThread(() => updateIndexUserRecentMensages(e)
			);
		}

		public void updateIndexUserRecentMensages(MessageEventArgs e)
		{

            if (user.users.Any(u => u.Jid.Split('@')[0].Equals(e.From.Split('@')[0])))
            {
                setRecentUser(user.users, e);
            }
            else {
                List<AppCore.User> userGroups = LegionUtils.getGroupsTempToList(AppPreferences.GROUPS_TEMP, appPreferences);
                setRecentUser(userGroups, e);
            }
		}

        protected void setRecentUser(List<AppCore.User> list, MessageEventArgs e) {
            int index = list.FindIndex(c => c.Jid.Split('@')[0].Equals(e.From.Split('@')[0]));
            user.users[index].Phone = e.Body;
            updateListCardUi();
        }

		protected  override void OnResume()
		{
			base.OnResume();
			user.users = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
			updateListCardUi();
		}

		public void ListGroupsOnStart() {
			//if (IsFinishing)
			//{
			//	return;
			//}

			//if (user.users.Count > 0)
			//{
			//	foreach (var userRoom in user.users) {
			//		JoinOnGroups(userRoom.Jid);
				//}
			//}

		}
       
        public void loadListUserRecent() {
			//string jsonGroupsList = appPreferences.getValueKey(AppPreferences.GROUPS);
			//if (jsonGroupsList == "" || jsonGroupsList == null)
			//{
			//	allGroups = new List<Group>();
			//}
			//else {
			//	allGroups = LegionUtils.getListGroupJson(jsonGroupsList);
			//}

			recentConversationsUserName = userService.getRecentUsers(appPreferences);

			user = new User();
			user.users = usersByRecentConversation(recentConversationsUserName);

			if (user.users != null || user.users.Count != 0)
			{
				updateListCardUi();
			}

		}


		public void updateListCardUi()
		{ 
			
			this.RunOnUiThread(() =>
			{
				if (user.users.Count > 0)
				{
					relativeWelcome.Visibility = ViewStates.Gone;
					relativeWelcome.RefreshDrawableState();
				}
                updateListCardUiList();
            });
		}

        public void updateListCardUiList() {
            
            mLayoutManager = new LinearLayoutManager(this);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            userAdapter = new UserAdapter(userService.removeNullUsersJidUnsername(user), this);
            userAdapter.ItemClick += OnItemClick;
            mRecyclerView.SetAdapter(userAdapter);
            userAdapter.NotifyDataSetChanged();
            mRecyclerView.RefreshDrawableState();
        }

        public void updateUIListGroupsUsers(List<AppCore.User> list) {
            user.users = list;
            updateListCardUi();
        }

        void startBack()
        {
            //Finish();
            StartActivity(new Intent(ApplicationContext, typeof(SessionActivity)));
            OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
        }

        protected override void OnDestroy()
		{
			base.OnDestroy();
			saveSeachGroups(user.users);
		}

		public void saveSeachGroupsUpdateUi(List<AppCore.User> groups) {
			VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
			saveSeachGroups(groups);
		}

		protected override void Xmpp_OnListGroup(object sender, string[] rooms)
		{
			
			List<AppCore.User> groups = new List<AppCore.User>();
			foreach (string room in rooms)
			{
				AppCore.User gUser = userService.turnGroupToUserToList(room, room);
				groups.Add(gUser);
				user.users.Add(gUser);
				JoinOnGroup(room);
			}

			if (groups.Count > 0) {
				saveSeachGroups(groups);
			}

			updateListCardUi();

		}

		void addGroupToListUser(AppCore.User user) {
			if(user.users.Any(x => !x.Jid.Equals(user.Jid))) {
				user.users.Add(user);
			}
		}


		public void Xmpp_OnConnected(object sender, EventArgs e)
		{

			//loadGroups();

			//List<AppCore.User> listGroupTmp = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
			//if (listGroupTmp != null)
			//{
			//	foreach (AppCore.User u in listGroupTmp)
			//	{
			//		XmppGetGroupName(u.Jid);
			//	}
			//}

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
        }

		protected override void OnRestart()
		{
			base.OnRestart();
			//loadListUserRecent();
		}
        
		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			
			switch (item.ItemId) {
				case Android.Resource.Id.Home:
					startConfig();
					return true;
				case Resource.Id.action_add:
					startContactsPhone();
					return true;
				default:
					return base.OnOptionsItemSelected(item);	
			}

		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{ 
			this.MenuInflater.Inflate(Resource.Menu.actionbar_main, menu);
			return base.OnCreateOptionsMenu(menu);
		}

		void OnItemClick(object sender, int position){}

		void startContactsPhone() { 
			//Finish();
			StartActivity(new Intent(ApplicationContext, typeof(ContacPhoneActivity)));
			OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
		}

		void startConfig()
		{
			//Finish();
			StartActivity(new Intent(ApplicationContext, typeof(ConfigurationActivity)));
			OverridePendingTransition(Resource.Animator.slide_out_right, Resource.Animator.stable);
		}

		List<User> usersByRecentConversation(Dictionary<string, AppCore.Message> recents) {
			List<User> usersFriend = new List<User>();

			if (recents != null)
			{

				foreach (var item in recents.Reverse())
				{
					AppCore.Message m = item.Value;
					User friend = new User();

					friend.username = item.Key;
					if (item.Value.GroupId != null)
					{
						friend.isGroup = true;

					}
					else {
						friend.Jid = friend.username;
					}

					//In Session Phone assumed callback menssages
					if (LegionUtils.IsBase64String(m.Msg))
					{
						m.Msg = "@ Photo";
					}
					friend.Phone = LegionUtils.Truncate(m.Msg, 26);
					string photo = appPreferences.getValueKey(friend.username + "_ICON");
					if (photo != null || photo != "")
					{
						friend.Photo = photo;
					}
					usersFriend.Add(friend);
				}
			}

			// add groups list conversations
			List<AppCore.User> listGroupTmp = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
			if (listGroupTmp != null) {
				
				//var varUser = listGroupTmp.Where(y => usersFriend.Any(z => z.Jid == y.Jid));

				List<string> jidstmps = new List<string>();
				foreach (AppCore.User u in usersFriend)
				{
					jidstmps.Add(u.Jid);
				}

				foreach (AppCore.User g in listGroupTmp) {
					if (!jidstmps.Contains(g.Jid)) {
						usersFriend.Add(g);
					}
				}

			}

			return usersFriend;
		}

        // set name at group
		protected override void Xmpp_OnSubjectGroup(object sender, MessageEventArgs e)
		{
	
			if (user.users.Any(u => u.Jid.Split('@')[0].Equals(e.From.Split('@')[0])))
				{
				int index = user.users.FindIndex(c => c.Jid.Split('@')[0].Equals(e.From.Split('@')[0]));
					user.users[index].username = e.Body;
					user.users[index].Nick = e.Body;

					
					updateListCardUi();
				}
            
        }

		void editIndexGroupFromCard(AppCore.User userIndex) {

            if (user.users.Count > 0)
            {
                var index = user.users.FindIndex(c => (c.isGroup == true) && (c.Jid.Split('@')[0] == userIndex.Jid.Split('@')[0]));
                user.users[index].username = userIndex.username;
                user.users[index].Nick = userIndex.Nick;
            }          
			
			updateListCardUi();
		}



		void msgFriend(object sender, MessageEventArgs e)
		{
			string friend = e.From;
			int index = e.From.LastIndexOf("@");
			if (index > 0)
				friend = e.From.Substring(0, index);
				
			this.msgs = getMsgs(friend);
			AppCore.Message m = new AppCore.Message(friend, e.Body, DateTime.Now, false);
			this.msgs.Add(m);
			saveMsg(friend, m);
			//RefrashRecycleView();
		}

		public void saveMsg(string friend, AppCore.Message message)
		{
			string tmpMsgs = LegionUtils.parceToGson(msgs);
			appPreferences.saveKey(friend, tmpMsgs);
			saveRecentUser(friend, message);
		}

		public List<AppCore.Message> getMsgs(string friend)
		{
			List<AppCore.Message> ms =
				LegionUtils.getListMessageJson(appPreferences.getValueKey(friend));
			if (ms == null)
			{
				return new List<AppCore.Message>();
			}
			else {
				return ms;
			}

		}

		void loadGroups()
		{
			List<AppCore.User> listGroupTmp = getGroupsTempToList(AppPreferences.GROUPS_TEMP);
			if (listGroupTmp != null)
			{
				foreach (AppCore.User u in listGroupTmp)
				{
					XmppGetGroupName(u.Jid);
				}
			}
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


		void verifySubscriptFriend() { 
			List<AppCore.User> friedsReqs = LegionUtils.getListJson(
				appPreferences.getValueKey(AppPreferences.FRIEND_REQUEST));

			if (friedsReqs.Count > 0) {
				foreach (AppCore.User u in friedsReqs) {
					RunOnUiThread(() =>
					{
						alertSub(this, u);
					});
				} 
				  
			}
		}

		public override void OnBackPressed()
		{
			
		}
	}

}
