
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
using AppCore;
using Android.Graphics;
using Android.Util;
using Android.Provider;
using Android.Graphics.Drawables;
using System.IO;
using System.Drawing;
using Legion.Client;
using System.Threading.Tasks;

using Matrix1 = Android.Graphics.Matrix;
using Legion.Client.utils;
using Android.Telephony;
using static System.Net.Mime.MediaTypeNames;

namespace Legion.Client
{
    [Activity(Label = "ConfigurationActivity")]
    public class ConfigurationActivity : BaseAppCompatActivity
    {
        
        private TextView userName;
        private EditText lastName;
        private EditText password;
        private EditText firstName;
        private Switch enable;
        private Button btnUpdate;
        private EditText phone;
        private ImageView imgAvatar;
        private string avatarBase64String;
        private int changeAvatar = 0;
        private string enableIsChecked = "true";
        private XmppFacade xmppFace;
        private string jsonUser;
        private JsonBuilderService jbService;
        private App App;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Configuration);
			VisibleProgress(Resource.Id.pbHeaderProgress, VISIBLE);

			this.App = App.Instance;
            //loadXmppConnect(this);
            xmppFace = new XmppFacade();

            var toolbar = loadToolBar(Resource.String.configurations);
            toolbar.SetNavigationIcon(Resource.Drawable.back);

            jbService = new JsonBuilderService();
            
            loadComponents();
            loadUser(appPreferences);
        }

   //     public void loadXmppConnect()
   //     {
			//ActivityService aa = ActivityService.
			//VisibleProgress(Resource.Id.pbHeaderProgress, VISIBLE);
   //         // this.App.Xmpp = createXmpp();
			//if (!isExistUser())
			//{
			//	this.App.Xmpp.OnConnected += Xmpp_OnConnected;
			//}
			//else {
			//	VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
			//}
   //         this.App.Xmpp.Connect();
   //     }

        public void loadComponents()
        {
            userName = (TextView)FindViewById(Resource.Id.userName);
            lastName = (EditText)FindViewById(Resource.Id.lastName);
            password = (EditText)FindViewById(Resource.Id.changePassw);
            firstName = (EditText)FindViewById(Resource.Id.firstName);
            imgAvatar = (ImageView)FindViewById(Resource.Id.avatar);
            btnUpdate = (Button)FindViewById(Resource.Id.btnSignOut);
            enable = (Switch)FindViewById(Resource.Id.btnEnable);
            phone = (EditText)FindViewById(Resource.Id.phone);

            phone.AddTextChangedListener(new PhoneNumberFormattingTextWatcher());

            imgAvatar.Click += delegate
            {
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(
                    Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };

            btnUpdate.Click += BtnUpdate_Click;

            enable.CheckedChange += delegate (object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                enableIsChecked = e.IsChecked ? "true" : "false";
            };
        }

        public User createUserByScreen()
        {
            User user = new User();
            user.username = userName.Text;            
            user.First = firstName.Text;
            user.Last = lastName.Text;
            user.password = password.Text;
            user.Phone = phone.Text;
            user.Photo = avatarBase64String;
            
            return user;
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            User user = createUserByScreen();
            jsonUser = jbService.jsonCreateUser(user);

            appPreferences.saveKey(AppPreferences.USER, jsonUser);

            appPreferences.saveKey(AppPreferences.AVATAR, avatarBase64String);

            var vCard = xmppFace.saveVCard(user, appPreferences, avatarBase64String);

            Task.Run(() => this.App.Xmpp.SetMyVCard(vCard));

            Toast.MakeText(ApplicationContext, GetString(Resource.String.success), ToastLength.Short).Show();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    startBack();
                    return true;
				case Resource.Id.action_invisible:

					//Menu de contexto confirmacao
					AlertDialog.Builder alert = new AlertDialog.Builder(this);
					alert.SetTitle(GetString(Resource.String.confirmTitle));
					alert.SetMessage(GetString(Resource.String.confirmMSG));
					alert.SetPositiveButton(GetString(Resource.String.confirmOk), (senderAlert, args) =>
					{
						appPreferences.clearPreferences();
                        SingOutApp();
                        startBackLogin();
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
			this.MenuInflater.Inflate(Resource.Menu.actionbar_invisible, menu);
            return base.OnCreateOptionsMenu(menu);
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

		void startBackLogin()
		{
			//Finish();
            //CloseSession();
			StartActivity(new Intent(ApplicationContext, typeof(LoginActivity)));
			OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
		}

        private void loadUser(AppPreferences appPref)
        {
            string userJson = appPref.getValueKey(AppPreferences.USER);

            if (userJson != null)
            {
                var userObj = Newtonsoft.Json.Linq.JObject.Parse(userJson);

				userName.Text = (string)userObj["username"] == "" ? appPref.getValueKey(AppPreferences.USERNAME) : (string)userObj["username"];
                password.Text = (string)userObj["password"];
                firstName.Text = (string)userObj["firstname"];
                lastName.Text = (string)userObj["lastname"];
                phone.Text = (string)userObj["phone"];               

                if (appPref.getValueKey(AppPreferences.ENABLE) != null)
                {
                    if (appPref.getValueKey(AppPreferences.ENABLE).Equals("false"))
                        enable.Checked = false;
                    else
                        enable.Checked = true;
                }

                if ((string)userObj["avatar"] != "")
                {
                    Bitmap bitmap = Base64ToBitmap((string)userObj["avatar"]);
                    var d = new CircleDrawable(bitmap);
                    imgAvatar.SetBackgroundDrawable(d);
					avatarBase64String = (string)userObj["avatar"];
                }
                else
                {
                    imgAvatar.SetBackgroundResource(Resource.Drawable.avatar_upload);
                }

				VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
            }
			else
			{
				imgAvatar.SetBackgroundResource(Resource.Drawable.avatar_upload);
				loadMyVCard();
			}

        }

		public override void Xmpp_OnMessage(object sender, Messaging.MessageEventArgs e)
		{
			base.Xmpp_OnMessage(sender, e);
			msgFriend(sender, e);

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

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                changeAvatar = 1;
                Bitmap bitmap = LegionUtils.getCircleDrawable(data, this);
                CircleDrawable d = new CircleDrawable(bitmap);

                imgAvatar.SetBackgroundDrawable(d);

                var ms = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, ms);
                var imageByteArray = ms.ToArray(); 
                avatarBase64String = Convert.ToBase64String(imageByteArray);
            }
        }


        public Bitmap Base64ToBitmap(String base64String)
        {
            byte[] imageAsBytes = Android.Util.Base64.Decode(base64String, Android.Util.Base64Flags.Default);
            return BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
        }
 
        private void loadMyVCard()
        {           

            App.Instance.Xmpp.GetVCard(Username + "@" + GetString(Resource.String.jid), delegate (object s, Matrix.Xmpp.Client.IqEventArgs eIq)
            {
                RunOnUiThread(() =>
                {
                    var vc = eIq.Iq.Query as Matrix.Xmpp.Vcard.Vcard;
					User u = new User();

					if (vc.Nickname != null)
					{
						userName.Text = vc.Nickname;
						u.username = vc.Nickname;
					}
					else { 
						userName.Text = App.Username;
					}
                    
					string[] firstLast = null;

					if (vc.Fullname != null) {
						firstLast = vc.Fullname.Split(new char[0]);

						if (firstLast[0] != null)
						{
							firstName.Text = firstLast[0];
							u.First = firstLast[0];
						}

						if (firstLast[1] != null)
						{
							lastName.Text = firstLast[1];
							u.Last = firstLast[1];
						}
					}

                    if(vc.Photo != null) { 
                        byte[] ph = vc.Photo.Binval;
						avatarBase64String = Convert.ToBase64String(ph);
						u.Photo = avatarBase64String;
                        setImageViewWithByteArray(imgAvatar, ph);
						appPreferences.saveKey(AppPreferences.AVATAR, avatarBase64String);
                    }

					jsonUser = jbService.jsonCreateUser(u);
					appPreferences.saveKey(AppPreferences.USER, jsonUser);
					VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
                    
                });
            });
        }

        public void setImageViewWithByteArray(ImageView view, byte[] data)
        {
            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);            
            var c = new CircleDrawable(bitmap);
            view.SetBackgroundDrawable(c);
            view.RefreshDrawableState();
        }


        public void setImageViewBase64(ImageView view, string base64) {
            Bitmap bitmap = Base64ToBitmap(base64);            
            var c = new CircleDrawable(bitmap);
            view.SetBackgroundDrawable(c);
            view.RefreshDrawableState();
        }

    }
}

