using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Matrix.Xmpp;
using Matrix.Xmpp.Client;
using Matrix.Xmpp.Roster;

namespace MiniClient
{
	public class ContactListAdapter : BaseAdapter
	{
		LayoutInflater inflater;
		public List<Contact> items = new List<Contact>();
		public XmppClient xmppClient;

		public ContactListAdapter(Activity activity, LayoutInflater inflater, XmppClient xmppClient)
			: base()
		{
			this.inflater = inflater;
			this.xmppClient = xmppClient;

			this.xmppClient.OnRosterItem += delegate(object sender, RosterEventArgs e) {
				if (e.RosterItem.Subscription != Subscription.Remove)
				{
					var contact = items.FirstOrDefault(i => i.Jid.Bare.Equals(e.RosterItem.Jid.Bare));

					if (contact == null)
						contact = new Contact{Jid = e.RosterItem.Jid};

					contact.Name = e.RosterItem.Name;

					items.Add(contact);
					activity.RunOnUiThread(delegate {
						NotifyDataSetChanged();
					});
				}
			};

			this.xmppClient.OnPresence += delegate(object sender, PresenceEventArgs e) {
				if (items.Any(i => i.Jid.Bare.Equals(e.Presence.From.Bare)))
				{
					var contact = items.First(i => i.Jid.Bare.Equals(e.Presence.From.Bare));
					contact.Status = e.Presence.Status;
					activity.RunOnUiThread(delegate {
						NotifyDataSetChanged();
					});
				}
			};
		}

		public override int Count
		{
			get { return items.Count; }
		}

		public override Java.Lang.Object GetItem(int position)
		{
			return position;
		}

		public override long GetItemId(int position)
		{
			return position;
		}

		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			//Get our object for this position
			var item = items[position];         

			var view = (convertView ?? 
			            inflater.Inflate(Resource.Layout.RosterListItem, parent, false)) as LinearLayout;

			var txtName 	= view.FindViewById(Resource.Id.txtName) as TextView;
			var txtStatus 	= view.FindViewById(Resource.Id.txtStatus) as TextView;

			txtName.Text = item.Name + " (" + item.Jid + ")";
			txtStatus.Text = item.Status;

			return view;
		}
		public Contact GetItemAtPosition(int position)
		{
			return items[position];
		}
	}
}

