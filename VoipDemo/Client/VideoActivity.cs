using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Messaging;
using Legion.Client.utils;

using FM.IceLink.WebRTC;

namespace Legion.Client
{
    [Activity(Label = "IceLink Conference - WebRTC", Icon = "@drawable/icon")]
    public class VideoActivity : BaseAppCompatActivity

    {

		ActivityService ActivityService;
        private static bool LocalMediaStarted;
        private static bool ConferenceStarted;

        private RelativeLayout Layout;
		private RelativeLayout Bgvideoaction;
		private ImageView Bgconnection;
		private ImageView avatar;
		private TextView TxUserName;
		private TextView TxConnection;
        private ImageButton LeaveButton;
        private static RelativeLayout Container;
        private GestureDetector GestureDetector;
		private string callType;
        
        protected override void OnCreate(Bundle bundle)
        {
			
            base.OnCreate(bundle);
			this.callType = this.Intent.GetStringExtra ("callType");
			RequestWindowFeature(WindowFeatures.NoTitle);
			Window.AddFlags(WindowManagerFlags.KeepScreenOn);
			SetContentView(Resource.Layout.Video);
			Layout = (RelativeLayout)FindViewById(Resource.Id.layout);
			ActivityService = ActivityService.GetInstance;
			if (friend == null)
			{
				if (this.callType.Equals("incoming"))
				{
					friend = new AppCore.User();
					friend.Jid = this.Intent.GetStringExtra("userChat");
					friend.username = LegionUtils.GetUserNameClear(friend.Jid);
				}
				else { 
					friend = LegionUtils.getUserson(Intent.GetStringExtra("userChat"));
				}

			}

			//ActivityService.GetInstance.App.Xmpp.OnReceiveSessionAccept += (sender, e) => {
			//	Log.Info("Success connection");

			//	//RunOnUiThread(() =>
			//	//		showConnectionVideo(true)
			//	//		 );
				
			//};
            

            
			//Bgconnection = (ImageView)FindViewById(Resource.Id.
			//                                       bgconnection);
			//avatar = (ImageView)FindViewById(Resource.Id.avatarFriend);
			if (friend.Photo != null)
			{
				//Bitmap bitmap = xmppFace.Base64ToBitmap(friend.Photo);
				//var d = new CircleDrawable(bitmap);
				//avatar.SetBackgroundDrawable(d);
			}
			//TxUserName = (TextView)FindViewById(Resource.Id.userNameFriend);
			//TxUserName.Text = friend.username;
			//TxConnection = (TextView)FindViewById(Resource.Id.connect);

            LeaveButton = (ImageButton)FindViewById(Resource.Id.leaveButton);
            
            // Load native libraries.
            if (!Build.CpuAbi.ToLower().Contains("x86") && !Build.CpuAbi.ToLower().Contains("arm64"))
            {
                Java.Lang.JavaSystem.LoadLibrary("audioprocessing");
                Java.Lang.JavaSystem.LoadLibrary("audioprocessingJNI");
            }
            Java.Lang.JavaSystem.LoadLibrary("opus");
            Java.Lang.JavaSystem.LoadLibrary("opusJNI");
            Java.Lang.JavaSystem.LoadLibrary("vpx");
            Java.Lang.JavaSystem.LoadLibrary("vpxJNI");

            LeaveButton.Click += new EventHandler(LeaveButton_Click);

            Messaging.Log.Debug("VideoActivity adding event handler Xmpp_OnReceiveSessionTerminate");
            //oadXmppConnect(this);
            ActivityService.GetInstance.App.Xmpp.OnReceiveSessionTerminate += Xmpp_OnReceiveSessionTerminate;
            //ActivityService.GetInstance.App.Xmpp.OnReceiveSessionInitiate += Xmpp_OnReceiveSessionInitiate;

            // For demonstration purposes, use the double-tap gesture
            // to switch between the front and rear camera.
            GestureDetector = new GestureDetector(this, new OnGestureListener(ActivityService.GetInstance.App));

            // Preserve a static container across
            // activity destruction/recreation.
            var c = (RelativeLayout)FindViewById(Resource.Id.container);
            if (Container == null)
            {
                Container = c;

                Toast.MakeText(this, "Double tap to switch camera.", ToastLength.Short).Show();
            }
			Layout.RemoveView(c);

            if (!LocalMediaStarted)
            {
                StartLocalMedia();
            }

           
        }

        private void StartLocalMedia()
		{
            Messaging.Log.Debug("VideoActivity.StartLocalMedia");
            // Android's video providers need a context
            // in order to create surface views for video
            // rendering, so we need to supply one before
            // we start up the local media.
            DefaultProviders.AndroidContext = this;

            LocalMediaStarted = true;
			LocalMedia.Audio = true;
			LocalMedia.Video = true;
            ActivityService.GetInstance.App.StartLocalMedia(Container, (error) =>
            {
                if (error != null)
                {
                    Alert(error);
                }
                else
                {
                    // Start conference now that the local media is available.
                    if (!ConferenceStarted)
                    {
                        StartConference();
                    }
                }
            });
		}

		private void StartConference()
		{
            Messaging.Log.Debug("VideoActivity.StartConference");
            ConferenceStarted = true;

			ActivityService.GetInstance.App.StartConference(this.callType, (error) =>
			{
				if (error != null)
				{
					Alert(error);
				}
			});
		}

        /// <summary>
        /// Fired when recipient sends hangup
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Xmpp_OnReceiveSessionTerminate(object sender, Messaging.Jingle.JingleEventArgs e)
        {
            Messaging.Log.Info("VideoActivity Xmpp_OnReceiveSessionTerminate");
            ConferenceStarted = false;
            ActivityService.GetInstance.App.StopConference((error) =>
            {
                Messaging.Log.Debug("VideoActivity.Xmpp_OnReceiveSessionTerminate StopLocalMedia callback");
                StopLocalMedia();
            });
        }

        private void StopLocalMedia()
        {
            Messaging.Log.Debug("VideoActivity.StopLocalMedia");
            if (LocalMediaStarted)
			{
				LocalMediaStarted = false;

                ActivityService.GetInstance.App.Xmpp.OnReceiveSessionTerminate -= Xmpp_OnReceiveSessionTerminate;
                Messaging.Log.Debug("VideoActivity.StopLocalMedia LocalMediaStarted");
                ActivityService.GetInstance.App.StopLocalMedia((error) =>
				{
					Container = null;

                    Messaging.Log.Debug("VideoActivity.StopLocalMedia LocalMediaStarted Finish Callback");
                    Finish();
	            });
        	}
			else
			{
				Container = null;

                Messaging.Log.Debug("VideoActivity.StopLocalMedia !LocalMediaStarted Finish");
                Finish();
			}
		}

        /// <summary>
        /// Disconnect video session
        /// </summary>
        private void StopConference()
        {
            Messaging.Log.Info("VideoActivity.StopConference");
            if (ConferenceStarted)
            {
                try
                {
                    // send recipient hangup notice
                    ActivityService.GetInstance.App.Xmpp.SendSessionTerminate(Signalling.To);
                }
                catch (Exception ex)
                {
                    Messaging.Log.Error(ex.ToString());
                }
				ConferenceStarted = false;

				ActivityService.GetInstance.App.StopConference((error) =>
				{
                    StopLocalMedia();
				});
			}
			else
			{
                Messaging.Log.Debug("VideoActivity.StopConference StopLocalMedia !ConferenceStarted");
                StopLocalMedia();
			}
        }

        void LeaveButton_Click(object sender, EventArgs e)
        {
            Messaging.Log.Debug("VideoActivity LeaveButton_Click");
            StopConference();
        }

		public override void OnBackPressed()
		{
            Messaging.Log.Debug("VideoActivity LeaveButton_Click");
            StopConference();
		}

		private void showConnectionVideo(bool isShowConnect) {

			Bgconnection.Visibility = ViewStates.Gone;
			avatar.Visibility = ViewStates.Gone;
			TxUserName.Visibility = ViewStates.Gone;
			TxConnection.Visibility = ViewStates.Gone;

			Bgconnection.RefreshDrawableState();
			avatar.RefreshDrawableState();
			TxUserName.RefreshDrawableState();
			TxConnection.RefreshDrawableState();

		}

        protected override void OnPause()
        {
            // Android requires us to pause the local
            // video feed when pausing the activity.
            // Not doing this can cause unexpected side
            // effects and crashes.
            if (LocalMediaStarted)
            {
                ActivityService.GetInstance.App.PauseLocalVideo();
            }
            
            // Remove the static container from the current layout.
            if (Container != null)
            {
				Layout.RemoveView(Container);
            }

            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // Add the static container to the current layout.
            if (Container != null)
            {
                Layout.AddView(Container);
            }

            // Resume the local video feed.
            if (LocalMediaStarted)
            {
                ActivityService.GetInstance.App.ResumeLocalVideo();
            }
        }
    
        public override bool OnTouchEvent(MotionEvent evt)
        {
            // Handle the double-tap event.
            if (GestureDetector == null || !GestureDetector.OnTouchEvent(evt))
            {
                return base.OnTouchEvent(evt);
            }
            return true;
        }

		class OnGestureListener : GestureDetector.SimpleOnGestureListener
        {
            private App App;

            public OnGestureListener(App app)
            {
                App = app;
            }

            public override bool OnDoubleTap(MotionEvent e)
            {
                App.UseNextVideoDevice();
                return true;
            }
        }
    }
}

