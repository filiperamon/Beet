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
using FM.IceLink.WebRTC;
using Legion.Client.utils;
using System.Timers;
using System.Diagnostics;

namespace Legion.Client
{
	[Activity(Label = "VoiceCallActivity")]
	public class VoiceCallActivity : BaseAppCompatActivity
	{
		private App App;

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
		private string callType;
		private XmppFacade xmppFace;
		private Timer timer = new Timer();
		Stopwatch stopwatch = new Stopwatch();

		protected override void OnCreate(Bundle bundle)
		{

			base.OnCreate(bundle);
			SetContentView(Resource.Layout.VoiceCall);
			this.callType = this.Intent.GetStringExtra("callType");
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
			App = App.Instance;
			xmppFace = new XmppFacade();
			App.Xmpp.OnReceiveSessionAccept += (sender, e) =>
			{
				Log.Info("Success connection");
				timer.Interval = 1000;
				timer.Elapsed += OnTimedEvent;
				timer.Enabled = true;

			};

			Layout = (RelativeLayout)FindViewById(Resource.Id.layout);
			Bgconnection = (ImageView)FindViewById(Resource.Id.
												   bgconnection);
			avatar = (ImageView)FindViewById(Resource.Id.avatarFriend);
			if (friend.Photo != null)
			{
				Bitmap bitmap = xmppFace.Base64ToBitmap(friend.Photo);
				var d = new CircleDrawable(bitmap);
				avatar.SetBackgroundDrawable(d);
			}
			TxUserName = (TextView)FindViewById(Resource.Id.userNameFriend);
			TxUserName.Text = friend.username;
			TxConnection = (TextView)FindViewById(Resource.Id.time);

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

			Messaging.Log.Debug("VoiceCallActivity adding event handler Xmpp_OnReceiveSessionTerminate");
			//loadXmppConnect(this);
			//App.Instance.Xmpp.OnReceiveSessionTerminate += Xmpp_OnReceiveSessionTerminate;
			//App.Instance.Xmpp.OnReceiveSessionInitiate += Xmpp_OnReceiveSessionInitiate;


			if (!SignallingStarted)
			{
				StartSignalling();
			}

			if (!LocalMediaStarted)
			{
				StartLocalMedia();
			}


		}

		private void OnTimedEvent(object sender, System.Timers.ElapsedEventArgs e)
		{
			stopwatch.Start();
			RunOnUiThread(() => countTimer());
		}

		private void countTimer() {
			TxConnection.Text = stopwatch.Elapsed.ToString("c");
		}

		private void StartLocalMedia()
		{
			Messaging.Log.Debug("VoiceCallActivity.StartLocalMedia");
			// Android's video providers need a context
			// in order to create surface views for video
			// rendering, so we need to supply one before
			// we start up the local media.
			DefaultProviders.AndroidContext = this;

			LocalMediaStarted = true;
			LocalMedia.Audio = true;
			LocalMedia.Video = false;
			App.StartLocalMedia(Container, (error) =>
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
			Messaging.Log.Debug("VoiceCallActivity.StartConference");
			ConferenceStarted = true;

			App.StartConference(this.callType, (error) =>
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
			Messaging.Log.Info("VoiceCallActivity Xmpp_OnReceiveSessionTerminate");
			ConferenceStarted = false;
			App.StopConference((error) =>
			{
				Messaging.Log.Debug("VoiceCallActivity.Xmpp_OnReceiveSessionTerminate StopLocalMedia callback");
				StopLocalMedia();
			});
		}

		private void StopLocalMedia()
		{
			Messaging.Log.Debug("VoiceCallActivity.StopLocalMedia");
			if (LocalMediaStarted)
			{
				LocalMediaStarted = false;
				App.Instance.Xmpp.OnReceiveSessionTerminate -= Xmpp_OnReceiveSessionTerminate;
				Messaging.Log.Debug("VoiceCallActivity.StopLocalMedia LocalMediaStarted");
				App.StopLocalMedia((error) =>
				{
					Container = null;

					//Messaging.Log.Debug("VoiceCallActivity.StopLocalMedia LocalMediaStarted Finish Callback");
					Finish();
				});
			}
			else
			{
				Container = null;

				Messaging.Log.Debug("VoiceCallActivity.StopLocalMedia !LocalMediaStarted Finish");
				Finish();
			}
		}

		/// <summary>
		/// Disconnect video session
		/// </summary>
		private void StopConference()
		{
			Messaging.Log.Info("VoiceCallActivity.StopConference");
			if (ConferenceStarted)
			{
				try
				{
					// send recipient hangup notice
					App.Xmpp.SendSessionTerminate(Signalling.To);
				}
				catch (Exception ex)
				{
					Messaging.Log.Error(ex.ToString());
				}
				ConferenceStarted = false;

				App.StopConference((error) =>
				{
					StopLocalMedia();
				});
			}
			else
			{
				Messaging.Log.Debug("VoiceCallActivity.StopConference StopLocalMedia !ConferenceStarted");
				StopLocalMedia();
			}
		}

		void LeaveButton_Click(object sender, EventArgs e)
		{
			Messaging.Log.Debug("VoiceCallActivity LeaveButton_Click");
			StopConference();
		}

		public override void OnBackPressed()
		{
			Messaging.Log.Debug("VoiceCallActivity LeaveButton_Click");
			StopConference();
		}

		protected override void OnPause()
		{
			// Android requires us to pause the local
			// video feed when pausing the activity.
			// Not doing this can cause unexpected side
			// effects and crashes.
			if (LocalMediaStarted)
			{
				App.PauseLocalVideo();
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
				App.ResumeLocalVideo();
			}
		}

	}
}

