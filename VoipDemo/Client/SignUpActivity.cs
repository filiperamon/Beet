
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using RestSharp;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AppCore;
using System.Threading;

namespace Legion.Client
{
	[Activity(Label = "SignUpActivity")]
	public class SignUpActivity : BaseAppCompatActivity
	{

		private EditText userName;
		private EditText email;
		private EditText password;
		private Button btnUpdateProfile;
		private JsonBuilderService jbService;
		private string jsonUser;


		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);
			SetContentView(Resource.Layout.SignUpActivity);

			loadComponents();
		}

		private void loadComponents() {

			jbService = new JsonBuilderService();
			userName = (EditText)FindViewById(Resource.Id.editFirstName);
			email = (EditText)FindViewById(Resource.Id.editTextEmail);
			password = (EditText)FindViewById(Resource.Id.editTextPassword);

			btnUpdateProfile = (Button)FindViewById(Resource.Id.btnUpdateProfile);
			btnUpdateProfile.Click += async(sender, e) => {

				if (isValidLogin())
				{
					try
					{
						hidenKeyBoard();
						string url = WServiceUrl.CREATE_USER;

						var msg = await CreateUserAsync(url);
						PostClick(msg);

					}
					catch (Exception ex) {
						VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
						Alert("Please verify your Internet Connection.");
					}
				}

			};

			loadUser(appPreferences);

		}

		public User createUserByScreen() {
			User user = new User();
			user.username = userName.Text;
			user.email = email.Text;
			user.password = password.Text;
			return user;
		}

		private void PostClick(string msg) { 
			var obj = Newtonsoft.Json.Linq.JObject.Parse(msg);
			string message = (string)obj["message"];
			if (message.Equals(WServiceUrl.SUCCESS))
			{
				appPreferences.saveKey(AppPreferences.USER, jsonUser);
				startApp();


			} else { 
				Toast.MakeText(ApplicationContext, 
				               WServiceUrl.getMessagingDesc(WServiceUrl.getMessagegingServer(message)), ToastLength.Short).Show();
			}

			VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE);
		}

		private void loadUser(AppPreferences appPref)
		{
			string userJson = appPref.getValueKey(AppPreferences.USER);

			if (isExistUser())
			{
				var userObj = Newtonsoft.Json.Linq.JObject.Parse(userJson);

				userName.Text = (string)userObj["username"];
				email.Text = (string)userObj["email"];
				password.Text = (string)userObj["password"];
			}

		}

		private bool isValidLogin() {
			if (userName.Text.Length == 0 || userName.Text.Length < 6) {
				userName.SetError(GetString(Resource.String.edtLength), null);
				userName.RefreshDrawableState();
				return false;
			}
			if (email.Text.Length == 0) {
				email.SetError(GetString(Resource.String.emailErro), null);
				email.RefreshDrawableState();
				return false;
			}
			if (LegionUtils.isValidEmail(email.Text) == false) {
				email.SetError(GetString(Resource.String.emailErro), null);
				email.RefreshDrawableState();
				return false;
			}
				
			if (password.Text.Length == 0 || password.Text.Length < 6)
			{
				password.SetError(GetString(Resource.String.edtLength), null);
				password.RefreshDrawableState();
				return false;
			}

			return true;
		}

		public override void OnBackPressed()
		{
			base.OnBackPressed();
			OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
		}

		private async Task<string> CreateUserAsync(string url)
		{


			VisibleProgress(Resource.Id.pbHeaderProgress, VISIBLE);

			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(new Uri(url));
			request.ContentType = "application/json";
			request.Method = "POST";

			using (var streamWriter = new StreamWriter(request.GetRequestStream()))
			{
				jsonUser = jbService.jsonCreateUser(createUserByScreen());
				streamWriter.Write(jsonUser);
				streamWriter.Flush();
				streamWriter.Close();
			}

				using (WebResponse response = await request.GetResponseAsync())
				{
					using (Stream stream = response.GetResponseStream())
					{
						var serializer = new JsonSerializer();
						var sr = new StreamReader(stream);
						var jsonTextReader = new JsonTextReader(sr);
						var json = serializer.Deserialize(jsonTextReader);

						string msgObj = JsonConvert.SerializeObject(json);
						return msgObj;
					}
				}
				
			}


		void startApp() {
			Finish();
			loadAppUserNamePass(userName.Text, password.Text);
			appPreferences.saveKey(AppPreferences.USERNAME, userName.Text);
			appPreferences.saveKey(AppPreferences.PASSWORD, password.Text);

			StartActivity(new Intent(ApplicationContext, typeof(ImportContactsActivity)));
			OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
		}

	}
}


