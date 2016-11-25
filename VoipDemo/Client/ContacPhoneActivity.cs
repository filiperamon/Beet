
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Android.Graphics;

using Android.Support.V7.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using SearchView = Android.Support.V7.Widget.SearchView;
using AppCore;
using Android.Text;
using Messaging;
using Matrix.Xmpp.Client;
using Xamarin.Contacts;
using Android.Support.V4.App;
using Android;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using Android.Content.PM;

namespace Legion.Client
{
	[Activity(Label = "ContacPhoneActivity")]
	public class ContacPhoneActivity :  BaseAppCompatActivity
	{

        ActivityService ActivityService;
        private App App;
        private static bool SignallingStarted;
		public static readonly AddressBook addressBook = new AddressBook(Application.Context) { PreferContactAggregation = true };
		private UserService userService;

		private RecyclerView mRecyclerView;
		private RecyclerView.LayoutManager mLayoutManager;
		private UserAdapter userAdapter;
		private User user;
		private SearchView search;
		private List<User> filterUser = new List<User>();
        private XmppFacade xmppFace;
		private int totalContact;
		private View layout;
		private int totalContactReady = 0;
		private int countJidsAlredSync = 0;
		private List<User> lastContacts = new List<User>();
		List<User> contatcsBooks = null;
		bool alredySync = false;

		static readonly int REQUEST_CONTACTS = 1;
		static string[] PERMISSIONS_CONTACT = {
			Manifest.Permission.ReadContacts,
			Manifest.Permission.WriteContacts, Manifest.Permission.Camera,
					Manifest.Permission.RecordAudio

		};

        protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.ContactsPhone);
			layout = FindViewById(Resource.Id.sample_main_layout);
			userService = new UserService(appPreferences);
			isEnableSubScript = true;
            var toolbar = loadToolBar(Resource.String.contacts);
            toolbar.SetNavigationIcon(Resource.Drawable.config);
            toolbar.SetNavigationIcon(Resource.Drawable.back);
			mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);

            user = new User();
			user.users = listContacts();

			showProgress();
			
			syncOnStart();
            searchInList();

			App = App.Instance;
			xmppFace = new XmppFacade();
        	
			//loadXmppConnect(this);

			if ((int)Build.VERSION.SdkInt < 23)
			{
				addressBook.RequestPermission().ContinueWith(t =>
			   {
				   if (!t.Result)
				   {
					   Console.WriteLine("{0}", "Permission denied, check your manifest");
					   Toast.MakeText(Application.Context, "Permission denied, check your manifest", ToastLength.Short).Show();
					   return;
				   }

			   });
			}
			else {
				ShowContacts(layout);
			}
            
        }

        private void syncOnStart()
        {
			totalContactReady = 0;
			if (user.users.Count > 0)
			{
				if (appPreferences.isFisrtAcess(AppPreferences.FIRST_ACCESS))
				{
					syncContact();
				}
				else {                    
                    updateListCardUi();                    
                }
			}
			else {
				updateListCardUi();
			}
        }

        public void loadXmppConnect()
        {
			//syncContact();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					startBack();
					return true;
				case Resource.Id.action_add_contact:
					startContacts();
					return true;
				case Resource.Id.action_add_group:
					StartActivity(new Intent(ApplicationContext, typeof(GroupAddActivity)));
					return true;
                case Resource.Id.action_sync_contact:
                    //updateContatcs();
                    syncContact();
                    return true;
                default:
					return base.OnOptionsItemSelected(item);
			}

		}


		public override bool OnCreateOptionsMenu(IMenu menu)
		{

			this.MenuInflater.Inflate(Resource.Menu.contact_menu, menu);

			return base.OnCreateOptionsMenu(menu);
		}

		public override void Xmpp_OnPresence(object sender, Messaging.PresenceEventArgs e) {
	
		}

		void updateListUserAdapter(User user) { 
			mLayoutManager = new LinearLayoutManager(this);
			mRecyclerView.SetLayoutManager(mLayoutManager);
			userAdapter = new UserAdapter(user, this);
			mRecyclerView.SetAdapter(userAdapter);
			userAdapter.NotifyDataSetChanged();
		}

		void startBack()
		{
			//Finish();                        
			StartActivity(new Intent(ApplicationContext, typeof(SessionActivity)));
			OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
		}

		public override void OnBackPressed()
		{
			startBack();
		}

		void searchInList() {
			
			search = (SearchView)FindViewById(Resource.Id.search);
			search.QueryTextChange += (s, e) => {
				string ch = ((SearchView)s).Query.ToUpper();
				for (int i = 0; i < user.users.Count; i++) {
					string text = "";
					if (user.users[i].username != null)
					{
						text = user.users[i].username.ToUpper();
					}
					else if (user.users[i].Jid != null) { 
						text = user.users[i].Jid.ToUpper();
					}
					else {
						return;
					}
				 
                    if (text.Contains(ch))
                    {
                        if (!filterUser.Contains(user.users[i])) {
                            filterUser.Add(user.users[i]);
                        }                        
                    }
                    else {
                        filterUser.Remove(user.users[i]);
                    }
				}

				User u = new User();
				u.users = filterUser;
				mLayoutManager = new LinearLayoutManager(this);
				mRecyclerView.SetLayoutManager(mLayoutManager);
				userAdapter = new UserAdapter(u, this);
				mRecyclerView.SetAdapter(userAdapter);
				userAdapter.NotifyDataSetChanged();
				    
			};

		}

		private void Xmpp_OnConnected(object sender, EventArgs e)
        {
            
        }

        private void syncContact() {
            List<User> vCards = new List<User>();
            if (lastContacts != null && lastContacts.Count > 0) {
               
				foreach (AppCore.User u in lastContacts) {
					if (user.users.Any(c => c.Jid != u.Jid)) {
						user.users.Add(u);
					}
				}
            }

            foreach (User contac in user.users)
            { 
                if(contac.Jid == null || contac.Jid == "")
                {
                    return;
                }
				App.Instance.Xmpp.GetVCard(contac.Jid, delegate (object s, Matrix.Xmpp.Client.IqEventArgs eIq)
				{

					var vc = eIq.Iq.Query as Matrix.Xmpp.Vcard.Vcard;
					vc.Jid = contac.Jid;

                    if (vc.Jid != null  && vc.Jid != "") {
                        vCards.Add(userService.getValidUser(vc, GetString(Resource.String.jid)));
                        if (vCards.Count >= user.users.Count)
                        {
                            LegionUtils.saveFoundContacts(vCards, appPreferences, AppPreferences.CONTACTS);
                            this.user.users = vCards;
                            updateListCardUi();
                            appPreferences.saveKey(AppPreferences.FIRST_ACCESS, false);
                        }
                    }				

					

				});

            }
        }

		void updateListCardUi() {
			this.RunOnUiThread(() => {
				
				mLayoutManager = new LinearLayoutManager(this);
				mRecyclerView.SetLayoutManager(mLayoutManager);
				userAdapter = new UserAdapter(userService.removeRepetUsers(user), this);
				mRecyclerView.SetAdapter(userAdapter);
				userAdapter.NotifyDataSetChanged();
				mRecyclerView.RefreshDrawableState();

				VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
			});
        }


		void startContacts()
		{
			//Create a new intent for choosing a contact
			var contactPickerIntent = new Intent(Intent.ActionInsert);
			contactPickerIntent.SetType(Android.Provider.ContactsContract.Contacts.ContentType);
			StartActivityForResult(contactPickerIntent, 101);
		}
		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);
			
             updateContatcs();

		}

		public void updateContatcs() {

			totalContactReady = 0;
            showProgress();			

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
				contatcsBooks = LegionUtils.readContacts(addressBook);
                if (contatcsBooks == null) {
                    updateListCardUi();
                }
                processContacts(contatcsBooks);

            });
		}

	    void processContacts(List<User> contatcs)
		{
			// search JId by email exist
			if (contatcs.Count > 0)
			{
                foreach (AppCore.User u in contatcs) {

                    showProgress();
                    searchContactByEmail(u.email);
                }                
            }
			else {
				updateListCardUi();
			}
		}
        
        public void XmppClient_OnRosterItem(object sender, Matrix.Xmpp.Roster.RosterEventArgs e) {
            updateListCardUi();           
        }

		 void searchContactByEmail(string email)
		{
			if (email == null || email.Equals("")){return;}

            try
			{
                App.Xmpp.Search(email, delegate (object s, Matrix.Xmpp.Client.IqEventArgs eIq)
                {
                       var iq = eIq.Iq;
                    if (iq.Type == Matrix.Xmpp.IqType.Result)
                    {
                        var searchQuery = iq.Query as Matrix.Xmpp.Search.Search;

                        foreach (var sItem in searchQuery.GetItems())
                        {
                            Log.Info("search result: " + sItem.Jid + "\n");
                            User userCard = new User();
                            userCard.Jid = sItem.Jid;
                            userCard.Nick = sItem.Nick;
                            userCard.First = sItem.First;
                            userCard.Last = sItem.Last;
                            
                            if (userCard.Jid != Username + "@" + GetString(Resource.String.jid))
                            {

                                lastContacts.Add(userCard);
                                this.App.Xmpp.Add(userCard.Jid);
                                updateListCardUi();
                            }
                            else {
                                updateListCardUi();
                            }

                        }

                    }
                    else if (iq.Type == Matrix.Xmpp.IqType.Error)
                    {
                        updateListCardUi();
                    }
                    else {
                        updateListCardUi();
                    }

                    updateListCardUi();
                });

			}
			catch (Exception e) {
				
			}

		}

		void addUserXmpp() {
			foreach(AppCore.User u in user.users) { 
				this.App.Xmpp.Add(u.Jid);
			}

		}


		void validaSaveContactNext()
		{
	        Log.Info("Load Contacts Complete");
	        if (user.users.Count > 0)
	        {
		        LegionUtils.saveFoundContacts(user.users.Distinct().ToList(), appPreferences, AppPreferences.CONTACTS);
		        updateComplete();
	        }
	        else {
		        updateComplete();
	        }

		}

		public void updateComplete() {

			this.user.users = LegionUtils.getListJson(appPreferences.getValueKey(AppPreferences.CONTACTS));
			syncContact();                                          
		}

		public override void Xmpp_OnMessage(object sender, Messaging.MessageEventArgs e)
		{
			base.Xmpp_OnMessage(sender, e);
			msgFriend(sender, e);
			//RunOnUiThread(() => updateIndexUserRecentMensages(e)
			//);
			updateListCardUi();
		}

		void msgFriend(object sender, Messaging.MessageEventArgs e)
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

		//void saveRecentUser(string friend, AppCore.Message message)
		//{

		//	recentConversationsUserName = userService.getRecentUsers(appPreferences);
		//	if (recentConversationsUserName.TryGetValue(friend, out message))
		//	{
		//		recentConversationsUserName.Remove(friend);
		//	}

		//	Dictionary<string, AppCore.Message> reTm = new Dictionary<string, AppCore.Message>();
		//	foreach (var item in recentConversationsUserName)
		//	{
		//		reTm.Add(item.Key, item.Value);
		//	}
		//	reTm.Add(friend, msgs[msgs.Count - 1]);
		//	string recentUserConv = LegionUtils.parceToGson(reTm);
		//	appPreferences.saveKey(AppPreferences.CONVERSA_BY_JID, recentUserConv);
		//}

		public void ShowContacts(View v)
		{
			Log.Info("Show contacts button pressed. Checking permissions.");

			// Verify that all required contact permissions have been granted.
			if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) != (int)Permission.Granted
				|| ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteContacts) != (int)Permission.Granted
				|| ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != (int)Permission.Granted
				|| ActivityCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != (int)Permission.Granted)
			{
				// Contacts permissions have not been granted.
				Log.Info("Contact permissions has NOT been granted. Requesting permissions.");
				RequestContactsPermissions();
			}
			else {
				// Contact permissions have been granted. Show the contacts fragment.
				Log.Info("Contact permissions have already been granted. Displaying contact details.");

			}
		}

		void showProgress() {
			RunOnUiThread(() =>
               VisibleProgress(Resource.Id.pbHeaderProgress, VISIBLE)
            );
		}

		void RequestContactsPermissions()
		{
			if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadContacts)
				|| ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteContacts)
				|| ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera)
				|| ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.RecordAudio))
			{

				// Provide an additional rationale to the user if the permission was not granted
				// and the user would benefit from additional context for the use of the permission.
				// For example, if the request has been denied previously.
				Log.Info("Displaying contacts permission rationale to provide additional context.");

				// Display a SnackBar with an explanation and a button to trigger the request.
				Snackbar.Make(layout, Resource.String.permission_contacts_rationale,
					Snackbar.LengthIndefinite).SetAction(Resource.String.ok, new Action<View>(delegate (View obj)
					{
						ActivityCompat.RequestPermissions(this, PERMISSIONS_CONTACT, REQUEST_CONTACTS);

					})).Show();
			}
			else {
				// Contact permissions have not been granted yet. Request them directly.
				ActivityCompat.RequestPermissions(this, PERMISSIONS_CONTACT, REQUEST_CONTACTS);
			}
		}       
    }
}

