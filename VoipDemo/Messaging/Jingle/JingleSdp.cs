using System;
using System.Collections.Generic;

using Matrix;
using Matrix.Xmpp;
using Matrix.Xml;
using Matrix.Xmpp.Client;

using Console = System.Console;
namespace Messaging.Jingle
{
	sealed class JingleSdp : Matrix.Xmpp.Jingle.Jingle
	{

		public Matrix.Xml.XmppXElement Session {
			get { return Element <XmppXElement> (); }
			set { Replace (value); }
		}

		public new IEnumerable<ContentSdp> Content {
			get { return Elements <ContentSdp> (); }
		} 

		public JingleSdp() : base() { // : base("urn:xmpp:jingle:1", "jingle") {
			//
		}

		public static JingleSdp FromSdp(string sdp) 
		{
			JingleSdp jingle = new JingleSdp ();

			string[] s = sdp.Split(new string[] { "\nm=" }, StringSplitOptions.None);
			string sdpSession = s [0];

			jingle.Session =  new Matrix.Xml.XmppXElement ("urn:xmpp:jingle:apps:sdp", "session");
			jingle.Session.Text (sdpSession);

			System.Text.RegularExpressions.Regex regexUfrag = new System.Text.RegularExpressions.Regex ("^a=ice-ufrag:(.+)");
			System.Text.RegularExpressions.Regex regexPwd = new System.Text.RegularExpressions.Regex ("^a=ice-pwd:(.+)");
			System.Text.RegularExpressions.Regex regexFingerprint = new System.Text.RegularExpressions.Regex ("^a=fingerprint:(.+)");
			System.Text.RegularExpressions.Regex regexMedia = new System.Text.RegularExpressions.Regex("^(\\w+)\\b");
			System.Text.RegularExpressions.MatchCollection matches;

			for (int i = 1; i < s.Length; i++) {
				string[] sdpLines  = s [i].Split (new string[] { "\r\n" }, StringSplitOptions.None);
				string sdpContent  = "";
				string iceUfrag    = "";
				string icePwd      = "";
				string fingerprint = "";
				string mediaName   = "";
				matches = regexMedia.Matches (sdpLines [0]);
				sdpLines [0] = "m=" + sdpLines [0];
				mediaName = matches [0].Groups [1].Value;


				foreach (string sdpLine in sdpLines) {					
					matches = regexUfrag.Matches (sdpLine);
					if (matches.Count > 0) {
						iceUfrag = matches [0].Groups[1].Value;
						continue;
					}
					matches = regexPwd.Matches (sdpLine);
					if (matches.Count > 0) {
						icePwd = matches [0].Groups[1].Value;
						continue;
					}
					matches = regexFingerprint.Matches (sdpLine);
					if (matches.Count > 0) {
						fingerprint = matches[0].Groups[1].Value;
						continue;
					}

					sdpContent += sdpLine + "\n";
				}

				ContentSdp c = new ContentSdp ();
				c.Creator = "initiator";
				c.Name = mediaName;

				c.Description = new Matrix.Xml.XmppXElement ("urn:xmpp:jingle:apps:sdp", "description");
				c.Description.Text(sdpContent);

				Matrix.Xmpp.Jingle.Transports.TransportIceUdp transport;
				transport = new Matrix.Xmpp.Jingle.Transports.TransportIceUdp ();
				transport.Pwd = icePwd;
				transport.Ufrag = iceUfrag;

				Matrix.Xml.XmppXElement xmlFingerprint = new Matrix.Xml.XmppXElement("urn:xmpp:tmp:jingle:apps:dtls:0", "fingerprint", new System.Xml.Linq.XAttribute ("hash", "sha-256"));
				xmlFingerprint.Text(fingerprint);

				transport.Add (xmlFingerprint);
				c.TransportIceUdp = transport;

				// set ufrag
				// set pwd
				// add fingerprint

				//jingle.Add(c);
				jingle.Add (c);
			}

			Log.Verbose (jingle.ToString ());
			return jingle;			
		}


		// Convert from Jingle back into Sdp for Icelink
		public string ToSdp()
		{
			string sdp = "";
			sdp = this.Session.Value + "\n";

			foreach (ContentSdp content in this.Content)
			{
				string cont = content.Description.Value;
				string iceUfrag    = "a=ice-ufrag:" + content.TransportIceUdp.Ufrag + "\n";
				string icePwd      = "a=ice-pwd:"   + content.TransportIceUdp.Pwd   + "\n";

				//JingleSdp jingle2 = jIq.Element<JingleSdp>();
				XmppXElement f = content.TransportIceUdp.FirstXmppXElement;

				/*
				Matrix.Xmpp.Jingle.Candidates.CandidateIceUdp candidate = new Matrix.Xmpp.Jingle.Candidates.CandidateIceUdp ();
				candidate.Component = 1;
				candidate.Foundation = 1;
				candidate.Generation = 0;
				candidate.Id="m3110wc4nd";
				candidate.IPAddress = System.Net.IPAddress.Parse("127.0.0.1");
				candidate.Network = 0;
				candidate.Port = 9001;
				candidate.Priority = (int) 2114978047;
				candidate.Protocol = Matrix.Xmpp.Jingle.Protocol.Udp;
				candidate.Type = Matrix.Xmpp.Jingle.CandidateType.Host;
				*/

				string fingerprint = "a=fingerprint:" + f.Value + "\n"; // content.TransportIceUdp.

				cont = cont.Replace ("a=sendrecv\n", iceUfrag + icePwd + "a=sendrecv\n");
				cont = cont.Replace ("a=setup:actpass\n", fingerprint + "a=setup:actpass\n");
				cont = cont.Replace ("a=setup:active\n", fingerprint + "a=setup:active\n");
				sdp += cont;
			}

			return sdp;
		}
	}
}

