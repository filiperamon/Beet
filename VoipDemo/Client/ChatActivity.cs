
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.InputMethodServices;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Views.Animations;
using Android.Views.InputMethods;
using Android.Widget;
using AppCore;
using Java.IO;
using Java.Lang;
using Legion.Client.utils;
using Messaging;
using com.vasundharareddy.emojicon;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Support.Design.Widget;

namespace Legion.Client
{
	[Activity(Label = "ChatActivity")]
	public class ChatActivity : BaseAppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
	{

        ActivityService ActivityService;
        public ImageView btnSendMsg;
		//public EditText edtMsg;
		public EmojiconEditText edtMsg;

		private XmppFacade xmppFace;
		private UserService userService = new UserService();
		private string jsonUser;
		private bool isGroupChat = false;
		private string GroupName;
		string groupJson;
		//public CustomKeyboardView mKeyboardView;
		public Keyboard mKeyboard;
		public ImageView smileyKeyBoard;
		public RelativeLayout relative2;

		private int iHeightDefault;
		private RelativeLayout rootView;
		public Rect r;
		public View view;

		public bool tecladoShow = false;
		public static readonly int PickImageId = 1000;
		View layout;
		List<Group> grupos;
		private User groupFromGson;
		private List<AppCore.User> usersRecents;
		private bool isSaveRecentUse = false;
		private bool isFromGroup = false;
		private string lastJidEMessage = "";

		protected override void OnCreate(Bundle savedInstanceState)
		{
			
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.ChatActivity);
			layout = FindViewById(Resource.Id.rootRelative);
			mKeyboard = new Keyboard(this, Resource.Layout.keyboard2);
			smileyKeyBoard = (ImageView)FindViewById(Resource.Id.btn_key_board);

			smileyKeyBoard.Touch += (sender, e) =>
			{
				ShowKeyboardWithAnimation();
			};

			Rect r = new Rect();
			View view = Window.DecorView;
			view.GetWindowVisibleDisplayFrame(r);
			iHeightDefault = r.Bottom;

			rootView = (RelativeLayout)FindViewById(Resource.Id.rootRelative);
			relative2 = (RelativeLayout)FindViewById(Resource.Id.relative2);
			usersRecents = getGroupsTempToList(AppPreferences.GROUPS_TEMP);

			rootView.ViewTreeObserver.GlobalLayout += (sender, e) =>
			{
				r = new Rect();
				view = Window.DecorView;
				view.GetWindowVisibleDisplayFrame(r);

				if (iHeightDefault > r.Bottom)
				{
					// Keyboard is Show
					if (relative2.Visibility == ViewStates.Visible)
					{
						relative2.Visibility = ViewStates.Gone;
					}
				}
			};

			var toolbar = loadToolBar(Resource.Id.toolbar);
			toolbar.SetNavigationIcon(Resource.Drawable.back);

			TextView tvToo = (TextView)toolbar.FindViewById(Resource.Id.toolbar_title);
			mLayoutManager = new LinearLayoutManager(this);

			rvList = (RecyclerView)FindViewById(Resource.Id.rvLista);
			btnSendMsg = (ImageView)FindViewById(Resource.Id.btnEnviar);
			edtMsg = (EmojiconEditText)FindViewById(Resource.Id.edtMessage);

			EmojiconsFragment.EmojiClicked += (e) =>
			{
				EmojiconsFragment.Input(edtMsg, e);
			};
			EmojiconsFragment.EmojiconBackspaceClicked += (v) =>
			{
				EmojiconsFragment.Backspace(edtMsg);
			};

		
			this.isGroupChat = Intent.GetBooleanExtra("chatGroup", false);
			jsonUser = Intent.GetStringExtra("userChat");
			if (jsonUser != null)
			{
				friend = LegionUtils.getUserson(jsonUser);
				appPreferences.saveKeyMSGCount(LegionUtils.GetUserNameClear(friend.Jid), 0);
				inChatJid = LegionUtils.GetUserNameClear(friend.Jid);
			}
			if (this.isGroupChat)
			{
				string gson = Intent.GetStringExtra("groupJson");
				isFromGroup = Intent.GetBooleanExtra("isFromGroup", false);

				if (gson == null || gson.Equals("")) {
					gson = appPreferences.getValueKey(AppPreferences.GROUP_JSON);
				}
				groupFromGson = LegionUtils.getUserson(gson);
				GroupName = groupFromGson.username;
				inChatJid = GroupName;

				tvToo.Text = (GroupName.Replace("_", " "));
				tvToo.Text = tvToo.Text.Split('@')[0];
				msgs = getMsgs(groupFromGson.Jid);
				appPreferences.saveKeyMSGCount(LegionUtils.GetUserNameClear(groupFromGson.Jid), 0);
			}
			else {
				friend.Jid = friend.username + "@" + GetString(Resource.String.jid);
				tvToo.Text = (friend.username);
				msgs = getMsgs(friend.username);
			}


			btnSendMsg.Click += BtnSendMsg_Click;


			RefrashRecycleView();

            //loadXmppConnect(this);
           // ActivityService = new ActivityService(this);


            if (!SignallingStarted)
			{
				StartSignalling();
			}

			var isDirect = Intent.GetBooleanExtra("isDirectCall", false);
			var typeCall = Intent.GetStringExtra("callType");
			if (isDirect)
			{
				if (typeCall.Equals("callPhone"))
				{
					startCall(PHONE_CALL);
				}
				else {
					startCall(VIDEO_CALL);
				}
			}

			recentConversationsUserName = userService.getRecentUsers(appPreferences);
			clearUnreadMessage();

		}

		public override void Xmpp_OnMessage(object sender, MessageEventArgs e)
		{
			base.Xmpp_OnMessage(sender, e);
			msgFriend(sender, e);
			lastJidEMessage = e.From;
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
				if (friend.Jid.Split('@')[0].Equals(e.From))
				{
					RefrashRecycleView(friend);
				}
				else {
					CountMessage(sender, e);
				}
				
			}

		}
        
        private void RefrashRecycleView()
		{
			if (msgs == null || msgs.Count <= 0) { return; }
			rvList.SetLayoutManager(mLayoutManager);
			ChateCardAdapter adapter = new ChateCardAdapter(friend, this, message, msgs);
			rvList.SetAdapter(adapter);

			//Seta o tamanho da lista para o scroll ir para o final da lista
			rvList.SmoothScrollToPosition(msgs.Count - 1);
            adapter.NotifyDataSetChanged();

        }

		private void BtnSendMsg_Click(object sender, EventArgs e)
		{
			if (edtMsg.Text.Equals(""))
			{
				return;
			}

			if (isGroupChat)
			{
				setUserRecentChat(groupFromGson.Jid, edtMsg.Text);
				messageToGroup();
			}
			else { 
				setUserRecentChat(friend.Jid, edtMsg.Text);
				messageToFriend();
			}


		}

		void messageToFriend() { 
			AppCore.Message m = new AppCore.Message(friend.username, edtMsg.Text, DateTime.Now, true);
			this.message = m;
			this.msgs.Add(m);

			sendMessage(edtMsg.Text);
			edtMsg.Text = "";
			saveMsg();
			hidenKeyBoard();
			rvList.SmoothScrollToPosition(msgs.Count - 1);
			RefrashRecycleView();
		}

		void messageToFriend(string image64, string filename)
		{
			AppCore.Message m = new AppCore.Message(friend.username, image64, DateTime.Now, true, filename, true);
			this.message = m;
			this.msgs.Add(m);

			sendMessage(image64);
			//edtMsg.Text = "";
			saveMsg();
			//hidenKeyBoard();
			rvList.SmoothScrollToPosition(msgs.Count - 1);
			RefrashRecycleView();
		}

		void messageToGroup()
		{
			AppCore.Message m = new AppCore.Message(groupFromGson.Jid, edtMsg.Text, DateTime.Now, true, true);
			this.message = m;
			this.msgs.Add(m);

			sendMessageGroup(m.Msg);
			edtMsg.Text = "";
			saveMsg(m);
			hidenKeyBoard();
			rvList.SmoothScrollToPosition(msgs.Count - 1);
			RefrashRecycleView();
		}

		void sendMessage(string msg)
		{
			XmppSend(friend.Jid, msg);
		}

		void sendMessageGroup(string msg)
		{

			sendMsgGroup(groupFromGson.Jid.Split('@')[0], msg);
			//sendMsgGroup(groupFromGson.Jid, msg);
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Android.Resource.Id.Home:
					startBack();
					return true;
				case Resource.Id.action_call:
					startCall(PHONE_CALL);
					return true;
				case Resource.Id.action_video:
					startCall(VIDEO_CALL);
				    return true;
				case Resource.Id.optionschat:
					callGallery();
					return true;
                case Resource.Id.optionsdeletgroup:

                    //Menu de contexto confirmacao
                    Android.Support.V7.App.AlertDialog.Builder alert = new Android.Support.V7.App.AlertDialog.Builder(this);
                    alert.SetTitle(GetString(Resource.String.confirmTitle));
                    alert.SetMessage(GetString(Resource.String.confirmDeletGroup));
                    alert.SetPositiveButton(GetString(Resource.String.confirmOk), (senderAlert, args) =>
                    {
                        this.removeGroup(groupFromGson);
                        startBack();
                    });

                    alert.SetNegativeButton(GetString(Resource.String.confirmNOk), (senderAlert, args) =>
                    {
                    });

                    Dialog dialog = alert.Create();
                    dialog.Show();

                    return true;
                default:
					return base.OnOptionsItemSelected(item);
			}

		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			
			if (this.isGroupChat)
			{
				this.MenuInflater.Inflate(Resource.Menu.actionbar_chat_group, menu);
			}
			else { 
				this.MenuInflater.Inflate(Resource.Menu.actionbar_chat, menu);
			}

			return base.OnCreateOptionsMenu(menu);
		}

		void clearOnBack() { 
			if (friend != null)
			{
				if (friend.Jid.Split('@')[0].Equals(lastJidEMessage.Split('@')[0]))
				{
					clearUnreadMessage();
				}

			}
		}

		void startBack()
		{

			clearOnBack();
			//Finish();
			closeXmpp();
			if (isFromGroup)
			{
				StartActivity(new Intent(this, typeof(ContacPhoneActivity)));
			}
			else { 
				StartActivity(new Intent(this, typeof(SessionActivity)));
			}


		}


		[Obsolete]
		public void saveMsg()
		{
			string tmpMsgs = LegionUtils.parceToGson(msgs);
			appPreferences.saveKey(friend.username, tmpMsgs);
			saveRecentUser(friend.username);
		}

		public void saveMsg(AppCore.Message msg) { 
			string tmpMsgs = LegionUtils.parceToGson(msgs);
			string recentKey = null;
			if (msg.isGroup)
			{
				recentKey = msg.GroupId;
				appPreferences.saveKey(msg.GroupId, tmpMsgs);
			}
			else {
				recentKey = friend.username;
				appPreferences.saveKey(friend.username, tmpMsgs);
			}
			saveRecentUser(recentKey);
		}

		void saveRecentUser(string recentKey)
		{

			if (recentConversationsUserName.TryGetValue(recentKey, out message))
			{
				recentConversationsUserName.Remove(recentKey);
			}

			Dictionary<string, AppCore.Message> reTm = new Dictionary<string, AppCore.Message>();
			foreach (var item in recentConversationsUserName)
			{
				reTm.Add(item.Key, item.Value);
			}
			reTm.Add(recentKey, msgs[msgs.Count - 1]);
			string recentUserConv = LegionUtils.parceToGson(reTm);
			appPreferences.saveKey(AppPreferences.CONVERSA_BY_JID, recentUserConv);
		}

		void startCall(string typeOfCall)
		{
			
			SetSessionID(LegionUtils.GenerateSessionId());
			foreach (string jidWithHash in ActivityService.LIST_USERS_ONLINE) {
				if (jidWithHash.Contains(friend.Jid)) {
					friend.Jid = jidWithHash;
				}
			}

			if (friend.Jid == null || friend.Jid == "")
			{
				Toast.MakeText(Application.Context, "User offiline", ToastLength.Long).Show();
				return;
			}

			SetJID(friend.Jid);

			// Switch to video call with App.Jid
			Intent intent = null;
			if (typeOfCall.Equals(VIDEO_CALL))
			{
				intent = new Intent(ApplicationContext, typeof(VideoActivity));
			} else { 
				intent = new Intent(ApplicationContext, typeof(VoiceCallActivity));
			}
            
			intent.PutExtra("callType", "outgoing");
			intent.PutExtra("userChat", jsonUser);
			//Finish();
			StartActivity(intent);
		}


		void callGallery() {

            if ((int)Build.VERSION.SdkInt >= 23)
            {
                if (ActivityCompat.CheckSelfPermission(this,
                    Android.Manifest.Permission.ReadContacts) != (int)Permission.Granted)
                {
                    RequestGalleryPermissions();
                }
                else {
                    showGallery();
                    
                }
            }
            else { showGalleryMinor(); }
            
        }

        static readonly int REQUEST_CONTACTS = 1;
        static string[] PERMISSIONS_CONTACT = {            
                    Android.Manifest.Permission.ReadExternalStorage
        };
        void RequestGalleryPermissions()
        {
            if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Android.Manifest.Permission.ReadExternalStorage)) {
                Snackbar.Make(layout, Resource.String.permission_contacts_rationale,
                Snackbar.LengthIndefinite).SetAction(Resource.String.ok, new Action<View>(delegate (View obj)
                {
                    ActivityCompat.RequestPermissions(this, PERMISSIONS_CONTACT, REQUEST_CONTACTS);

                })).Show();
            }
        }

        void showGallery() {
            var imageIntent = new Intent(Intent.ActionPick);
            imageIntent.SetType("image/*");
            StartActivityForResult(
            Intent.CreateChooser(imageIntent, "Select Image"), 0);
        }

        void showGalleryMinor() {
            var imageIntent = new Intent();
            imageIntent.SetType("image/*");
            imageIntent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(
            Intent.CreateChooser(imageIntent, "Select photo"), 0);
        }

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data) {
			base.OnActivityResult(requestCode, resultCode, data);

			if (resultCode == Result.Ok)
			{
                compressBitmap();
                Bitmap bitmap;
				Stream stream = ContentResolver.OpenInputStream(data.Data);
				bitmap = BitmapFactory.DecodeStream(stream);

				var ms = new MemoryStream();
				bitmap = LegionUtils.getResizedBitmap(bitmap, 600, 600);
				bitmap.Compress(Bitmap.CompressFormat.Png, 50, ms);

				string filename = LegionUtils.SaveBitmapGalerry(bitmap);

				var imageByteArray = ms.ToArray();

				string stringPhoto = Convert.ToBase64String(imageByteArray);

				messageToFriend(stringPhoto, filename);

			}

		}

        private void compressBitmap()
        {            
            VisibleProgress(Resource.Id.pbHeaderProgress, VISIBLE);

            //show imageWill send

            new System.Threading.Thread(new ThreadStart(() =>
            {
               
                //compress and send

                RunOnUiThread(() => onSuccessfulBitmap());
            })).Start();
        }

        private void onSuccessfulBitmap()
        {
            VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
        }

        public void ShowKeyboardWithAnimation()
		{			
			InputMethodManager imm = (InputMethodManager)GetSystemService(Context.InputMethodService);
			imm.HideSoftInputFromWindow( relative2.WindowToken, 0 );

			if (relative2.Visibility == ViewStates.Gone)
			{
                relative2.Visibility = ViewStates.Visible;
            }
		}

		protected override void RefrashRecycleView(AppCore.User friend) { 
			if (msgs == null || msgs.Count <= 0) { return; }
			RunOnUiThread(() =>
			{
				rvList.SetLayoutManager(mLayoutManager);
				ChateCardAdapter adapter = new ChateCardAdapter(friend, this, message, msgs);
				rvList.SetAdapter(adapter);

				//Seta o tamanho da lista para o scroll ir para o final da lista
				rvList.SmoothScrollToPosition(msgs.Count - 1);
				adapter.NotifyDataSetChanged();
			});
		}

        public override void OnBackPressed()
        {

			clearOnBack();
            if (relative2.Visibility == ViewStates.Visible)
            {
                relative2.Visibility = ViewStates.Gone;
            }
            else
            {
                //Finish();
				closeXmpp();
                base.OnBackPressed();
            }
        }

		void clearUnreadMessage() {
			string jidUser = "";
			inChatJid = null;
			if (isGroupChat)
			{
				jidUser = groupFromGson.Jid;
			}
			else {
				jidUser = friend.Jid;
			}
			appPreferences.saveKeyMSGCount(LegionUtils.GetUserNameClear(jidUser.Split('@')[0]), 0);
		}
    }
}

