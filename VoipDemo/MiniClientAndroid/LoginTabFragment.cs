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
	public class LoginTabFragment: Fragment
	{
		XmppClient xmppClient;
		
		public LoginTabFragment(XmppClient xmppClient)
		{
			this.xmppClient = xmppClient;
		}
		
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);
			
			var view = inflater.Inflate (Resource.Layout.Login, container, false);
			
			view.FindViewById<Button> (Resource.Id.cmdConnect).Click += (object sender, System.EventArgs e) => {
				var username 	= view.FindViewById<EditText> (Resource.Id.editUsername).Text;
				var password 	= view.FindViewById<EditText> (Resource.Id.editPassword).Text;
				var xmppDomain 	= view.FindViewById<EditText> (Resource.Id.editXmppDomain).Text;
				var hostname 	= view.FindViewById<EditText> (Resource.Id.editHostname).Text;
				
				xmppClient.XmppDomain = xmppDomain;
				
				if (!String.IsNullOrEmpty(hostname))
				{
					xmppClient.ResolveSrvRecords = false;
					xmppClient.Hostname = hostname;
				}				
				
				xmppClient.Username = username;
				xmppClient.Password = password;
				
				xmppClient.Open();
			};
			
			view.FindViewById<Button> (Resource.Id.cmdDisconnect).Click += (object sender, System.EventArgs e) => {
				xmppClient.Close();
			};
			
			return view;
		}
	}
}