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
using Android.Media;
using System.Threading.Tasks;
using Android.Locations;
using AppCore;
using System.Timers;
using Java.Lang;
using Legion.Client.utils;
using Android.Content.PM;

namespace Legion.Client
{

    [Activity(MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, NoHistory = true)]		
	public class SplashAuthVideo : BaseAppCompatActivity, MediaPlayer.IOnPreparedListener, ISurfaceHolderCallback
	{

        
        private MediaPlayer mediaPlayer;
		private SurfaceView videoView;
		private int countText;
		private User user;
		private Timer timer = new Timer();
		private string assetUri = "";
		private static int _videoW = 1280;
		private static int _videoH = 720;
		public bool isUser = false;
        
        protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState); 			
            if (Username == null)
            {
                SetContentView(Resource.Layout.SplashAuthVideo);
            }
            //else {
			//	isUser = true;
               // StartActivity(new Intent(ApplicationContext, typeof(SessionActivity)));
                //SetContentView(Resource.Layout.Splash);
            //}

            //StartService(new Intent(this, typeof(ConnectionService)));
            var lang = Resources.Configuration.Locale;
            appPreferences.saveKey(AppPreferences.LOCALE_USER, lang.Country);

            System.Threading.ThreadPool.QueueUserWorkItem(o =>
            {
                if (appPreferences.getValueKey(AppPreferences.USERNAME) == null)
                {
					playVideoByCountry();
					showMenssgesInit();
				}
               
            });

        }
 

        private void SwitchToSession(bool isUser)
		{
			
			if (isUser)
			{				
				Finish();
				if (appPreferences.getValueKey(AppPreferences.CONTACTS) == null)
				{
					StartActivity(new Intent(ApplicationContext, typeof(ImportContactsActivity)));
				}
				else {
					StartActivity(new Intent(ApplicationContext, typeof(SessionActivity)));
				}

				OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
				timer.Enabled = false;
			}
			else {
                Finish();
                StartActivity(new Intent(ApplicationContext, typeof(LoginActivity)));
				OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
				
			}

		}

		public void playVideoByCountry() {
			videoView = this.FindViewById<SurfaceView> (Resource.Id.splashVideo);

			ISurfaceHolder holder = videoView.Holder;
			holder.SetType (SurfaceType.PushBuffers);
			holder.AddCallback(this);

			if (appPreferences.getValueKey(AppPreferences.LOCALE_USER) == "US")
			{
				assetUri = "video/splash_en_US.mp4";
			} 
			else if (appPreferences.getValueKey(AppPreferences.LOCALE_USER) == "BR")
			{
				assetUri = "video/splash_pt_BR.mp4";
			} else { 
				assetUri = "video/splash_en_US.mp4";
			}

		}

		public void showMenssgesInit() {
			timer.Interval = 3000;
			timer.Elapsed += OnTimedEvent;
			timer.Enabled = true;
		}

		public void textsScreen()
		{

			TextView tv = FindViewById<TextView>(Resource.Id.titlePri);
			tv.SetTextColor(Android.Graphics.Color.White);
			TextView st = FindViewById<TextView>(Resource.Id.titleS);
			st.SetTextColor(Android.Graphics.Color.White);

			if (countText == 0)
			{
				tv.Text = GetString(Resource.String.tvTitle1);
				st.Text = GetString(Resource.String.tvTxt1);
			}
			else if (countText == 1)
			{
				tv.Text = GetString(Resource.String.tvTitle2);
				st.Text = GetString(Resource.String.tvTxt2);
			}
			else if (countText == 2)
			{
				tv.Text = GetString(Resource.String.tvTitle3);
				st.Text = GetString(Resource.String.tvTxt3);
			}
			else if (countText == 3)
			{
				tv.Text = GetString(Resource.String.tvTitle4);
				st.Text = GetString(Resource.String.tvTxt4);
			}

			countText += 1;
			if (countText > 3)
			{
				countText = 0;

				timer.Enabled = true;
				timer.Stop();


				SwitchToSession(isUser);

			}


		}

        private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
		{
			RunOnUiThread (() => textsScreen());
		}

		public void SurfaceCreated(ISurfaceHolder holder) {
			mediaPlayer = new MediaPlayer();
			mediaPlayer.SetDisplay(holder);

			var uri = Assets.OpenFd(assetUri);

			mediaPlayer.SetDataSource(uri.FileDescriptor, uri.StartOffset, uri.Length);

			mediaPlayer.Prepare();
			mediaPlayer.Looping = true;

				/*
		       *  Handle aspect ratio
		       */
			int surfaceView_Width = videoView.Width;
			int surfaceView_Height = videoView.Height;


			int videoWidth = mediaPlayer.VideoWidth;
			int videoHeight = mediaPlayer.VideoHeight;
			float videoProportion = (float)videoWidth / (float)videoHeight;

			int screenWidth = WindowManager.DefaultDisplay.Width;
			int screenHeight = WindowManager.DefaultDisplay.Height;
			float screenProportion = (float)screenWidth / (float)screenHeight;

			float ratio_width;
			float ratio_height;
			float aspectratio;
			if (videoWidth == 0)
			{
				ratio_width = screenWidth / _videoW;
				ratio_height = screenHeight / _videoH;
				aspectratio = _videoW / _videoH;
			}
			else { 
			    ratio_width = surfaceView_Width / videoWidth;
				ratio_height = surfaceView_Height / videoHeight;
				aspectratio = videoWidth / videoHeight;
			}

			mediaPlayer.Start();
		}

		public void SurfaceDestroyed(ISurfaceHolder holder)
		{

		}
		public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int w, int h)
		{

		}
		public void OnPrepared(MediaPlayer player)
		{

		}


		protected override void OnDestroy() {
			base.OnDestroy();			
			if (mediaPlayer != null) {
				mediaPlayer.Pause();
				mediaPlayer.Stop();
				mediaPlayer = null;
			}
		}


        public void Xmpp_OnConnected(object sender, EventArgs e)
        {
                       
            if (appPreferences.getValueKey(AppPreferences.CONTACTS) == null)
            {
                StartActivity(new Intent(ApplicationContext, typeof(ImportContactsActivity)));
            }
            else
            {
                StartActivity(new Intent(ApplicationContext, typeof(SessionActivity)));
            }
            OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);            

        }


     }
}

