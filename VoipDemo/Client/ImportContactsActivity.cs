
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Xamarin.Contacts;
using AppCore;
using Messaging;
using Android;
using Java.Interop;
using Android.Support.V4.App;
using Android.Content.PM;
using Android.Support.Design.Widget;

namespace Legion.Client
{
	[Activity(Label = "ImportContactsActivity")]
	public class ImportContactsActivity : BaseAppCompatActivity, ActivityCompat.IOnRequestPermissionsResultCallback
	{

		ActivityService ActivityService;
		private Button btnImport;
        private ProgressBar Load;
		private static readonly AddressBook addressBook = new AddressBook(Application.Context) { PreferContactAggregation = true };
        private int totalContactReady = 0;
        private List<User> contactJIdList = new List<User>();
		private View layout;
		private List<User> users;
		private UserService userService;

		static readonly int REQUEST_CONTACTS = 1;
		static string[] PERMISSIONS_CONTACT = {
			Manifest.Permission.ReadContacts,
			Manifest.Permission.WriteContacts, Manifest.Permission.Camera,
			        Manifest.Permission.RecordAudio,
			        Manifest.Permission.WriteExternalStorage,
                    Manifest.Permission.ReadExternalStorage            
        };


        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Contacts);
			layout = FindViewById(Resource.Id.sample_main_layout_2);
			btnImport = (Button)FindViewById(Resource.Id.btnImport);
            Load      = (ProgressBar)FindViewById(Resource.Id.pbHeaderProgress);
			userService = new UserService(appPreferences);

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

            btnImport.Click += new EventHandler(OnButtonClicked);            
        }

        void OnButtonClicked(object sender, EventArgs args)
        { 
            ThreadPool.QueueUserWorkItem(o => importContacts());

            Load.Visibility = ViewStates.Visible;
            btnImport.Enabled = false;
            btnImport.Text = GetString(Resource.String.wait);
            btnImport.RefreshDrawableState();            
        }


    void importContacts()
		{

            try
			{
				this.users = LegionUtils.readContacts(addressBook);
                RunOnUiThread(() => processContacts(users) );
			}
			catch (Exception ex) {
				Toast.MakeText(ApplicationContext, "some problem in importing " + ex.Message,
							   ToastLength.Short).Show();
			}


		}
        
		async void processContacts(List<User> contatcs) {

			// search JId by email exist
			if (contatcs.Count > 0)
			{
				await Task.WhenAll(contatcs.Select(i => searchContactByEmail(i.email)));
				validaSaveContactNext();

			}
			else {
				
				loadNewActivity();
			}

		}


		async Task searchContactByEmail(string email)
		{
			if (email == null || email.Equals("")) {
				totalContactReady += 1;
				return; 
			}
            await Task.Run(() =>

			               ActivityService.GetInstance.App.Xmpp.Search(email, delegate (object s, Matrix.Xmpp.Client.IqEventArgs eIq)
							{
								var iq = eIq.Iq;
                                totalContactReady += 1;
								if (iq.Type == Matrix.Xmpp.IqType.Result)
								{
									var searchQuery = iq.Query as Matrix.Xmpp.Search.Search;
									foreach (var sItem in searchQuery.GetItems())
									{
										Log.Info("search result: " + sItem.Jid + "\n");
                                        User user = new User();
                                        user.Jid = sItem.Jid;
										user.Nick = sItem.Nick;
										user.First = sItem.First;
										user.Last = sItem.Last;
										//is same user name
										if (user.Jid != Username + "@" + GetString(Resource.String.jid))
										{
											ActivityService.GetInstance.App.Xmpp.Add(user.Jid);
                                            contactJIdList.Add(user);
										}
									
                                       
                                    }

									validaSaveContactNext();
                                }
                                
                            })

						  );

		}

		//public void loadXmppConnect()
		//{
  //          if (this.App.Xmpp == null) {
  //             this.App.Xmpp = createXmpp();
  //             this.App.Xmpp.Connect();
  //          }
            
  //      }

        private void loadNewActivity() {
            StartActivity(new Intent(ApplicationContext, typeof(SessionActivity)));
            OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
        }

		void validaSaveContactNext()
		{ 
			if (totalContactReady == this.users.Count)
				{
					Log.Info("Load Contacts Complete");
					if (contactJIdList.Count > 0)
					{                    
                        LegionUtils.saveFoundContacts(contactJIdList, appPreferences, AppPreferences.CONTACTS);
                        loadNewActivity();
					}
					else {
						loadNewActivity();
					}

				}
		}


		public void ShowContacts(View v)
		{
			Log.Info("Show contacts button pressed. Checking permissions.");

			// Verify that all required contact permissions have been granted.
			if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts) != (int)Permission.Granted
				|| ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteContacts) != (int)Permission.Granted
			    || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Camera) != (int)Permission.Granted
			    || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.RecordAudio) != (int)Permission.Granted
				|| ActivityCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage) != (int)Permission.Granted
                || ActivityCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != (int)Permission.Granted)
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

		void RequestContactsPermissions()
		{
			if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadContacts)
				|| ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteContacts)
			    || ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera)
			    || ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.RecordAudio)
			    || ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteExternalStorage)
                || ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage)                
               )
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

