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
using Android.Graphics;
using Messaging;
using System.Threading;
using System.Threading.Tasks;

using Matrix.Xmpp;
using Matrix.Xml;
using Matrix.Xmpp.Client;

using FM.IceLink.WebRTC;
using Android.Text;

using AppCore;
using Messaging;
using com.vasundharareddy.emojicon;

namespace Legion.Client
{
	[Activity(Label = "Legion VOIP Demo", Icon = "@drawable/icon", NoHistory = true)]
	public class LoginActivity : BaseAppCompatActivity
    {
        
		private EditText UsernameEdit;
		private EditText PasswordEdit;
		private Button LoginButton;
		private Button SignUpBtn;
		private UserService userService;

        protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.Login);
			myActivity = this;
            LoginButton = (Button)FindViewById(Resource.Id.buttonLogin);
			LoginButton.Click += new EventHandler(LoginButton_Click);
			SignUpBtn = (Button)FindViewById(Resource.Id.fogotpBtn);
			SignUpBtn.Click += new EventHandler(GotoSignUp);
			loadComponents();
            //FinishActivity(Resource.Layout.SplashAuthVideo);            
        }

       
        void LoginButton_Click(object sender, EventArgs e)
		{
			//LoginButton.Clickable = false;
            ThreadPool.QueueUserWorkItem(o => {
                hidenKeyBoard();
                if (isValidLogin())
                {
                    Username = UsernameEdit.Text;
                    Password = PasswordEdit.Text;
                    loadAppUserNamePass(Username, Password);

                    try
                    {
						loadConnectionXmpp();
                    }
                    catch (Exception ex)
                    {
                        var a = ex.Message;

                    }
                };
            });


            VisibleProgress(Resource.Id.pbHeaderProgress, VISIBLE);			
		}
        
        private bool isValidLogin() {
			if (UsernameEdit.Text.Length == 0) {
				UsernameEdit.SetError(GetString(Resource.String.invalidField), null);
				UsernameEdit.RefreshDrawableState();
				return false;
			}

			if (PasswordEdit.Text.Length == 0) {
				PasswordEdit.SetError(GetString(Resource.String.invalidField), null);
				PasswordEdit.RefreshDrawableState();
				return false;
			}

			return true;
		}

		private void loadComponents() { 			
			
		    UsernameEdit = (EditText)FindViewById(Resource.Id.editTextUsername);
			PasswordEdit = (EditText)FindViewById(Resource.Id.editTextPassword);

            loadUser(appPreferences);
			userService = new UserService(appPreferences);
		}

		private void loadUser(AppPreferences appPref) {
			if (isExistUser()) {
				UsernameEdit.Text = Username;
				PasswordEdit.Text = Password;
               
			}

		}

		void GotoSignUp(object sender, EventArgs e) {
			StartActivity(new Intent(ApplicationContext, typeof(SignUpActivity)));
			OverridePendingTransition(Resource.Animator.slide_out_right, Resource.Animator.stable);
		}

        public void Xmpp_OnConnected(object sender, EventArgs e)
        {
			//base.Xmpp_OnConnected(sender, e);
			doLoginOnAPP();
        }

		protected override void Xmpp_OnListGroup(object sender, string[] rooms)
		{

			if (rooms.Length == 0) {
				doLoginOnAPP();
			}

			List<AppCore.User> groups = new List<AppCore.User>();
			foreach (string room in rooms)
			{
				AppCore.User gUser = userService.turnGroupToUserToList(room, room);
				groups.Add(gUser);

				if (room.Equals(rooms[rooms.Length - 1]))
				{
					saveSeachGroups(groups);
					doLoginOnAPP();
				}
			}


        }

		void doLoginOnAPP() {
			appPreferences.saveKey(AppPreferences.USERNAME, UsernameEdit.Text);
			appPreferences.saveKey(AppPreferences.PASSWORD, PasswordEdit.Text);

            if (appPreferences.getValueKey(AppPreferences.CONTACTS) == null)
            {
				Finish();
                StartActivity(new Intent(ApplicationContext, typeof(ImportContactsActivity)));
            }
            else
            {                
                StartActivity(new Intent(ApplicationContext, typeof(SessionActivity)));
            }
            OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);            
            
        }

		public void XmppClient_OnAuthError(object sender, Matrix.Xmpp.Sasl.FailureCondition e)
		{ 
			Log.Debug("Failure");
			RunOnUiThread(() =>
			   showMsg("Ops! Error in your Username or Password.")
			);
			SingOutApp();

		}


    }

}

