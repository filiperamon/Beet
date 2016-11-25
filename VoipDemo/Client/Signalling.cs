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

using FM;
using FM.IceLink;
using FM.IceLink.WebSync;
using FM.WebSync;
using FM.WebSync.Subscribers;

using Messaging.Jingle;

namespace Legion.Client
{
	// Peers have to exchange information when setting up P2P links,
	// like descriptions of the streams (called the offer or answer)
	// and information about network addresses (called candidates).
	// IceLink generates this information for you automatically.
	// Your responsibility is to pass messages back and forth between
	// peers as quickly as possible. This is called "signalling".
	class Signalling
	{
		public static string To;
		public static bool IS_CALL_VIDEO;
		ActivityService ActivityService;
        public static bool isAlredAddMethods = false;
	// Legion signalling will be done through xmpp class
	private Messaging.Xmpp xmpp;
		// IceLink component
        private FM.IceLink.Conference Conference = null;

		public Signalling(Messaging.Xmpp xmpp)
        {
			this.xmpp = xmpp;
        }


        #region Both
        public void Start(Action<string> callback)
        {
            if (this.xmpp == null)
                return;

            if(isAlredAddMethods == false) { 
                this.xmpp.OnReceiveSessionAccept += Xmpp_OnReceiveSessionAccept;
                this.xmpp.OnReceiveCandidate += Xmpp_OnReceiveCandidate;
                isAlredAddMethods = true;
            }
        }

        #endregion

        #region Jingle
        /// <summary>
        /// Begin outgoing call
        /// </summary>
        /// <param name="conference"></param>
        /// <param name="to"></param>
        /// <param name="callback"></param>
        public void Call(FM.IceLink.Conference conference, string to, Action<string> callback)
		{
            To = to;
            Messaging.Log.Info(">Call " + to);
			Conference = conference;

			// when IceLink generates an Offer or a Candidate, raise event
			Conference.OnLinkOfferAnswer += SendSessionInitiate;
			Conference.OnLinkCandidate += SendCandidate;
			Conference.OnUnhandledException += Conference_OnUnhandledException;

            // Icelink generate candidates
			Conference.Link(to);
		}

        /// <summary>
        /// Answer incoming call
        /// </summary>
        /// <param name="conference"></param>
        /// <param name="to"></param>
        /// <param name="sdp"></param>
        /// <param name="callback"></param>
		public void Answer(FM.IceLink.Conference conference, string to, string sdp, Action<string> callback)
		{
            To = to;
			// Console.WriteLine ("Answer callback");
			Conference = conference;

			Conference.OnLinkOfferAnswer += SendSessionAccept;
			Conference.OnLinkCandidate += SendCandidate;
			Conference.OnUnhandledException += Conference_OnUnhandledException;
			Conference.OnLinkRemoteOfferAnswer += LinkRemoteOfferAnswer;

			OfferAnswer oa = new OfferAnswer();
			oa.SdpMessage = sdp;
			oa.IsOffer = true;

            Messaging.Log.Info ("Peer ID: " + to);
            Messaging.Log.Info ("SI SDP in:\n" + sdp);

            Messaging.Log.Debug("Answer > Conference.ReceiveOfferAnswer");
            // send to Icelink to generate candidates
			Conference.ReceiveOfferAnswer(oa, to);
		}
			
		void Xmpp_OnReceiveCandidate (object sender, Messaging.Jingle.JingleEventArgs e)
        {
            Messaging.Log.Debug("signalling got xmpp candidate");

			//TODO: implement receivecandidate
			///Conference.ReceiveCandidate(e.Sdp, e.From);
        }

		void LinkRemoteOfferAnswer(LinkOfferAnswerArgs e) { 
			var sdp = SDPMessage.Parse(e.OfferAnswer.SdpMessage);
			foreach (var md in sdp.MediaDescriptions)
			{
				if (md.Media.MediaType == SDPMediaType.Video)
				{
					foreach (var ma in md.MediaAttributes)
					{
						if (ma is SDPReceiveOnlyAttribute || ma is SDPInactiveAttribute)
						{

							IS_CALL_VIDEO = true;
							// Remote participant is NOT sending video.
							// ReceiveOnly: not sending video, but willing to receive video
							// Inactive: not sending video, and doesn't want to receive video either
						}
						else if (ma is SDPSendOnlyAttribute || ma is SDPSendReceiveAttribute)
						{
							//is call audio
							IS_CALL_VIDEO = true;
							// Remote participant IS sending video.
							// SendOnly: sending video, but doesn't want to receive video
							// SendReceive: sending video, and willing to receive video
						}
					}
				}
			}
		}


		void Conference_OnUnhandledException (FM.IceLink.UnhandledExceptionArgs p)
		{
            Messaging.Log.Debug(p.ToString ());
		}

        /// <summary>
        /// Recipient accepted call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		void Xmpp_OnReceiveSessionAccept (object sender, Messaging.Jingle.JingleSdpEventArgs e)
        {
            Messaging.Log.Debug("OnReceiveSessionAccept > Conference.ReceiveOfferAnswer " + e.From);

            OfferAnswer oa = new OfferAnswer();
			oa.SdpMessage = e.Sdp;
			oa.IsOffer = false;

            Messaging.Log.Info ("Peer ID: " + e.From);
            Messaging.Log.Info ("SA SDP in:  " + e.Sdp);


			if (Conference == null) {
				Conference = ActivityService.GetInstance.App.GetConference();
			}
            // send candidates to Icelink to negotiate connection
			Conference.ReceiveOfferAnswer(oa, e.From);
        }

        /*
        private void Xmpp_OnReceiveSessionTerminate(object sender, JingleEventArgs e)
        {
            try {
                Messaging.Log.Debug("OnReceiveSessionTerminate > Conference.UnlinkAll");
                if (Conference != null)
                    Conference.UnlinkAll();
                    
            }
            catch (Exception ex)
            {
                Console.WriteLine("more exception " + ex.ToString());
            }
        }
*/

        /// <summary>
        /// Send call request over xmpp to recipient
        /// </summary>
        /// <param name="e"></param>
        private void SendSessionInitiate(LinkOfferAnswerArgs e)
		{
            Messaging.Log.Debug("Signalling.SendSessionInitiate");

			  Console.WriteLine ("Peer ID: " + e.PeerId);
			  Console.WriteLine ("SI SDP out: " + e.OfferAnswer.SdpMessage);
			  App.Instance.Xmpp.SendSessionInitiate (e.PeerId, e.OfferAnswer.SdpMessage);
		}

        /// <summary>
        /// Send answer call accept over xmpp to recipient
        /// </summary>
        /// <param name="e"></param>
		private void SendSessionAccept (LinkOfferAnswerArgs e)
		{
            Messaging.Log.Debug("Signalling.SendSessionAccept");

			//Console.WriteLine ("Peer ID: " + e.PeerId);
			//Console.WriteLine ("SA SDP out:\n" + e.OfferAnswer.SdpMessage);
			App.Instance.Xmpp.SendSessionAccept(e.PeerId, e.OfferAnswer.SdpMessage);
		}

        /// <summary>
        /// send candidates when renegotiating connection
        /// </summary>
        /// <param name="e"></param>
		private void SendCandidate(LinkCandidateArgs e)
		{
            Messaging.Log.Debug("SendOfferCandidate");
            
            //WebSyncClient.Notify(new NotifyArgs(new Guid(e.PeerId), e.Candidate.ToJson(), "candidate:" + SessionId));
            //App.Instance.Xmpp.SendSessionInitiate(e.PeerId, e.Candidate);
            
        }
		#endregion

		#region cleanup
        /// <summary>
        /// hangup
        /// </summary>
        /// <param name="callback"></param>
        public void Detach(Action<string> callback)
        {
            Messaging.Log.Debug("Signalling.Detach");
            //this.xmpp.OnReceiveSessionAccept -= Xmpp_OnReceiveSessionAccept;
            //this.xmpp.OnReceiveSessionInitiate -= Xmpp_OnReceiveSessionInitiate;
            //this.xmpp.OnReceiveCandidate -= Xmpp_OnReceiveCandidate;
            if (Conference != null)
            {            
			    Conference.OnLinkOfferAnswer -= SendSessionInitiate;
			    Conference.OnLinkCandidate -= SendCandidate;
			    Conference.OnUnhandledException -= Conference_OnUnhandledException;

			    Conference = null;
            }
            
            if (callback != null)
            {
                Messaging.Log.Debug("Signallin.gDetach callback");
                callback(null);
            }
        }
		#endregion
    }
		
}