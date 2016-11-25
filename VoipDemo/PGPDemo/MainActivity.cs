using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

namespace PGPDemo
{
    [Activity(Label = "PGPDemo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        Messaging.Xmpp xmpp;
        EditText editText1;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            
            Button buttonConnect    = FindViewById<Button>(Resource.Id.buttonConnect);
            Button buttonRequest    = FindViewById<Button>(Resource.Id.buttonRequest);
            Button buttonPublish    = FindViewById<Button>(Resource.Id.buttonPublish);
            Button buttonSubscribe  = FindViewById<Button>(Resource.Id.buttonSubscribe);
            Button buttonRemove     = FindViewById<Button>(Resource.Id.buttonRemove);
            Button buttonMessage    = FindViewById<Button>(Resource.Id.buttonMessage);
            Button buttonDisconnect = FindViewById<Button>(Resource.Id.buttonDisconnect);
            
            editText1 = FindViewById<EditText>(Resource.Id.editText1);

            buttonDisconnect.Click += delegate
            {
                this.xmpp.Close();
            };

            buttonMessage.Click += delegate
            {
                this.xmpp.SendPgp(editText1.Text + "@messaging.legiontech.org", "the quick brown fox jumped over the lazy dog " + count);
                count += 1;
            };

            buttonConnect.Click += delegate 
            {
                this.xmpp = new Messaging.Xmpp(editText1.Text, "messaging.legiontech.org", "P@ssw0rd", "eJxkkd1SwjAQRl+l4y2jSSml4oSMpShIW0VtBb0LTSjF/NGmWHh68Q8uvPv2nN3ZnVkUFRmTFbMawWXVPyP5eaWW5oOU7Ir/qDOMpqWidWbuKI6VVMNSFRSBE0SPNZGmMDtsI3DMKKgrowQrMbongmFfEEkZ01aqNeEIfEMUKKGJPEz2Ol7nElo+X7DSECsy9MKyEPjz6EaQgmPOudrR6/yruMiUQOCHHxqP21JNiWE3jS5KNjwk3Ia2Bz3oIvBPoecil8TUJcPBq3gZGN1bGe75ai7mq1okL2AxUk8TvztditwO4O0odHJ7kOYfUaeAs3HXTux5+ZBsg0H8HsfR3mkXm81uuXZk1IQ0eWzchePBmUvnPdjolrjssKcsBaHUvSAbes5CbVXXdzWZvMYVexsSPp1BcLvZUinabtham7GnR2EyHexjL+3GJjSj9z4Cp7sR+P0h/hRA");
                this.xmpp.Connect();

                xmpp.OnSubscribeRequest += delegate (object sender, Messaging.SubscribeRequestEventArgs e)
                {
                    this.xmpp.Approve(e.From);

                    Messaging.Log.Info("approved!");
                };
            };

            buttonRequest.Click += delegate
            {
                throw new NotImplementedException("Not yet implemented");
            };

            buttonPublish.Click += delegate 
            {
                throw new NotImplementedException("Obsoleted");
                //this.xmpp.openPGP.Generate();
                //this.xmpp.openPGP.Publish();
            };

            buttonSubscribe.Click += delegate
            {
                // this.xmpp.pubsubManager.Subscribe("", "urn:xmpp:openpgp:0", editText1.Text + "@messaging.legiontech.org");
                this.xmpp.Add(editText1.Text + "@messaging.legiontech.org");
            };

            buttonRemove.Click += delegate
            {
                this.xmpp.Remove(editText1.Text + "@messaging.legiontech.org");
            };
           
        }
    }
}

