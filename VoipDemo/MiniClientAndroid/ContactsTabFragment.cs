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

using Matrix.Xmpp.Client;

namespace MiniClient
{
	public class ContactsTabFragment: Fragment
	{	
		XmppClient xmppClient;
		ContactListAdapter listAdapter;
		
		public ContactsTabFragment(XmppClient xmppClient)
		{
			this.xmppClient = xmppClient;
		}
		
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);
			
			var view = inflater.Inflate (Resource.Layout.Contacts, container, false);					
			
			listAdapter = new ContactListAdapter(Activity, inflater, xmppClient);
			//Find the listview reference
			var listView = view.FindViewById<ListView>(Resource.Id.listContacts);
			listView.Adapter = listAdapter;
			
			return view;
		}
	}
}