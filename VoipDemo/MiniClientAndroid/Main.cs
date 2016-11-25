using System;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Matrix.Xmpp.Client;

namespace MiniClient
{
	[Activity (Label = "MiniClient Demo", MainLauncher = true)]
	public class Main : Activity, ActionBar.ITabListener
	{
        private XmppClient xmppClient = new XmppClient();

		Fragment fragTab1;
		Fragment fragTab2;
		Fragment fragTab3;
		Fragment fragTabContacts;

		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);           

			SetContentView (Resource.Layout.Main);

			// request demo licnese from here:
			//http://www.ag-software.net/matrix-xmpp-sdk/request-demo-license/
			string lic = @"Put your evaluation key here";

            Matrix.License.LicenseManager.SetLicense(lic);
			            
		
			this.ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			ActionBar.NavigationMode = ActionBarNavigationMode.Tabs;

			
			ActionBar.AddTab(ActionBar.NewTab().SetText("Login").SetTabListener(this));
			ActionBar.AddTab(ActionBar.NewTab().SetText("Contacts").SetTabListener(this));
			ActionBar.AddTab(ActionBar.NewTab().SetText("XML Log").SetTabListener(this));
			ActionBar.AddTab(ActionBar.NewTab().SetText("Events").SetTabListener(this));

			xmppClient.OnMessage +=	xmppClient_OnMessage;

			xmppClient.OnError += (object sender, Matrix.ExceptionEventArgs e) => {
				System.Diagnostics.Debug.WriteLine(e.Exception.StackTrace);
			};

		}
		                    

		void xmppClient_OnMessage(object sender, MessageEventArgs e)
		{
			if (e.Message.Body == null) return;

			RunOnUiThread(delegate {			
				var text = "Message from: " + e.Message.From + "\r\n";
				text += e.Message.Body;
				Toast.MakeText(this, text, ToastLength.Long).Show();
			});
		}
        
		#region << ActionBar.ITabListener >>
		public void OnTabReselected (ActionBar.Tab tab, FragmentTransaction ft)
		{
			// nothing
		}
		
		public void OnTabSelected (ActionBar.Tab tab, FragmentTransaction ft)
		{			
			switch(tab.Text)
			{
				case "Login":
					if (fragTab1 == null)
					{
						fragTab1 = new LoginTabFragment(xmppClient);
						ft.Add(Resource.Id.fragmentContainer, fragTab1);
					}
					else
					ft.Show(fragTab1);
					
					break;
				case "Contacts":
					if (fragTabContacts == null)
					{
						fragTabContacts = new ContactsTabFragment(xmppClient);
						ft.Add(Resource.Id.fragmentContainer, fragTabContacts);
					}
					else
						ft.Show(fragTabContacts);
					
					break;
				case "XML Log":
					if (fragTab2 == null)
					{
						fragTab2 = new LogTabFragment(xmppClient);
						ft.Add(Resource.Id.fragmentContainer, fragTab2);
					}
					else
						ft.Show(fragTab2);
					
					break;
				case "Events":
					if (fragTab3 == null)
					{
						fragTab3 = new EventsTabFragment(xmppClient);
						ft.Add(Resource.Id.fragmentContainer, fragTab3);
					}
					else
					ft.Show(fragTab3);
					
					break;
			}
		}
		
		public void OnTabUnselected (ActionBar.Tab tab, FragmentTransaction ft)
		{
			
			switch(tab.Text)
			{
				case "Login":
					ft.Hide(fragTab1);						
					break;
				case "Contacts":
					ft.Hide(fragTabContacts);
					break;
				case "XML Log":
					ft.Hide(fragTab2);						
					break;
				case "Events":
					ft.Hide(fragTab3);						
					break;
			}
		}
		#endregion << ActionBar.ITabListener >
	}
}