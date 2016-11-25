
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
using Messaging;
using Android.Views.InputMethods;

namespace Legion.Client
{
	[Activity (Label = "BaseActivity")]			
	public class BaseActivity : Activity
	{
		public AppPreferences appPreferences { get; private set;}
		public String Username { get; set;}
		public String Password { get; set;}
		public String Email { get; set; }
		public ProgressBar progress;
		public static bool VISIBLE = true;
		public static bool INVISIBLE = false;
        InputMethodManager inputManager; 

        protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			appPreferences = new AppPreferences (Android.App.Application.Context);
            Username = appPreferences.getValueKey(AppPreferences.USERNAME);
            Password = appPreferences.getValueKey(AppPreferences.PASSWORD);
            inputManager = (InputMethodManager)this.GetSystemService(Context.InputMethodService);

        }

        public Xmpp createXmpp()
        {
            return new Xmpp(Username, WServiceUrl.XMPP_URL_LEGION, Password, GetString(Resource.String.matrix_linc));
        }

        public void VisibleProgress(int id, bool isVisible) {
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

		protected override void OnDestroy()
		{
			base.OnDestroy();
		}
			
		public bool isExistUser()
		{
			string userJson = appPreferences.getValueKey(AppPreferences.USER);
            Username = appPreferences.getValueKey(AppPreferences.USERNAME);
            Password = appPreferences.getValueKey(AppPreferences.PASSWORD);

            if (Username == null || Password == null)
			{
				return false;
			} else {

                if (userJson != null)
                {
                    var userObj = Newtonsoft.Json.Linq.JObject.Parse(userJson);
                    Username = (string)userObj["username"];
                    Email = (string)userObj["email"];
                    Password = (string)userObj["password"];
                }
               
				return true;
			}

		}

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

        public void hidenKeyBoard() {
            inputManager.HideSoftInputFromWindow(this.CurrentFocus.WindowToken, HideSoftInputFlags.NotAlways);
        }

	}
}

