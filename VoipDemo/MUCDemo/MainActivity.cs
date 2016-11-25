using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using System.Collections;


namespace MUCDemo
{
    [Activity(Label = "MUCDemo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        int count = 1;
        // standard xmpp
        Button buttonConnect;
        Button buttonDisconnect;

        // MUC (multi user chat) specific
        Button buttonJoin;
        Button buttonLeave;
        Button buttonMessage;
        Button buttonInvite;
        Button buttonSubject;

        string lastGroup;

        Hashtable muc;

        EditText editText1;
        Messaging.Xmpp xmpp;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            buttonConnect = FindViewById<Button>(Resource.Id.buttonConnect);
            buttonDisconnect = FindViewById<Button>(Resource.Id.buttonDisconnect);
            buttonJoin = FindViewById<Button>(Resource.Id.buttonJoin);
            buttonLeave = FindViewById<Button>(Resource.Id.buttonLeave);
            buttonMessage = FindViewById<Button>(Resource.Id.buttonMessage);
            buttonInvite = FindViewById<Button>(Resource.Id.buttonInvite);
            buttonSubject = FindViewById<Button>(Resource.Id.buttonSubject);
            editText1 = FindViewById<EditText>(Resource.Id.editText1);

            this.muc = new Hashtable();

            buttonConnect.Click += ButtonConnect_Click;
            buttonDisconnect.Click += ButtonDisconnect_Click;

            buttonJoin.Click += ButtonJoin_Click;
            buttonLeave.Click += ButtonLeave_Click;
            buttonMessage.Click += ButtonMessage_Click;
            buttonInvite.Click += ButtonInvite_Click;
            buttonSubject.Click += ButtonSubject_Click;  
            
        }

        private void ButtonSubject_Click(object sender, EventArgs e)
        {
            this.xmpp.ChangeGroupSubject(lastGroup, this.editText1.Text);
        }

        private void Xmpp_OnJoinGroup(object sender, string roomJid)
        {
            Messaging.Log.Info("joined a room " + roomJid);
            this.lastGroup = roomJid;
        }

        private void ButtonInvite_Click(object sender, EventArgs e)
        {
            this.xmpp.InviteGroup(lastGroup, this.editText1.Text + "@messaging.beetmessenger.com");
        }

        private void ButtonMessage_Click(object sender, EventArgs e)
        {
            this.xmpp.SendGroup(lastGroup, this.editText1.Text);
        }

        private void ButtonLeave_Click(object sender, EventArgs e)
        {
            this.xmpp.LeaveGroup(lastGroup);
        }

        /// <summary>
        /// This will create a new chat room or join and existing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonJoin_Click(object sender, EventArgs e)
        {
            this.xmpp.JoinGroup(this.editText1.Text);

            this.lastGroup = this.xmpp.CreateGroup();

            //this.lastGroup = this.editText1.Text;
        }

        private void ButtonDisconnect_Click(object sender, EventArgs e)
        {
            this.xmpp.Close();
            this.xmpp = null;
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            this.xmpp = new Messaging.Xmpp(editText1.Text, "messaging.beetmessenger.com", "P@ssw0rd", "eJxkkd1SwjAQRl+l4y2jSSml4oSMpShIW0VtBb0LTSjF/NGmWHh68Q8uvPv2nN3ZnVkUFRmTFbMawWXVPyP5eaWW5oOU7Ir/qDOMpqWidWbuKI6VVMNSFRSBE0SPNZGmMDtsI3DMKKgrowQrMbongmFfEEkZ01aqNeEIfEMUKKGJPEz2Ol7nElo+X7DSECsy9MKyEPjz6EaQgmPOudrR6/yruMiUQOCHHxqP21JNiWE3jS5KNjwk3Ia2Bz3oIvBPoecil8TUJcPBq3gZGN1bGe75ai7mq1okL2AxUk8TvztditwO4O0odHJ7kOYfUaeAs3HXTux5+ZBsg0H8HsfR3mkXm81uuXZk1IQ0eWzchePBmUvnPdjolrjssKcsBaHUvSAbes5CbVXXdzWZvMYVexsSPp1BcLvZUinabtham7GnR2EyHexjL+3GJjSj9z4Cp7sR+P0h/hRA");
            this.xmpp.OnConnected += Xmpp_OnConnected;
            this.xmpp.OnJoinGroup += Xmpp_OnJoinGroup;
            this.xmpp.OnListGroup += Xmpp_OnListGroup;
            this.xmpp.OnSubjectGroup += Xmpp_OnSubjectGroup;
            this.xmpp.Connect();

        }

        private void Xmpp_OnSubjectGroup(object sender, Messaging.MessageEventArgs e)
        {
            Messaging.Log.Info("Room " + e.From + " " + e.Body);
        }

        private void Xmpp_OnListGroup(object sender, string[] rooms)
        {
            foreach (string room in rooms)
            {
                Console.WriteLine("hey its " + room);
            }
        }

        private void Xmpp_OnConnected(object sender, EventArgs e)
        {
            this.xmpp.ListGroup();   
        }
    }
}

