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
	public class EventsTabFragment: Fragment
	{	
		XmppClient xmppClient;
		LinearLayout layout;
		ScrollView scrollView;
		
		public EventsTabFragment(XmppClient xmppClient)
		{
			this.xmppClient = xmppClient;
		}
		
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);
			
			var view = inflater.Inflate (Resource.Layout.Log, container, false);					
			layout = view.FindViewById<LinearLayout>(Resource.Id.logLinearLayout);
			scrollView = view.FindViewById<ScrollView>(Resource.Id.logScrollView);
			
			xmppClient.OnRosterStart += (sender, e) => { AddEvent(inflater, "OnRosterStart"); };
			xmppClient.OnRosterEnd += (sender, e) => {AddEvent(inflater, "OnRosterEnd"); };
			xmppClient.OnRosterItem += (sender, e) => { AddEvent(inflater, "OnRosterItem => jid:" + e.RosterItem.Jid.Bare);};
			
			xmppClient.OnMessage += (sender, e) => { AddEvent(inflater, "OnMessage => from:" + e.Message.From);};
			xmppClient.OnIq += (sender, e) => { AddEvent(inflater, "OnIq");};
			xmppClient.OnPresence += (sender, e) => { AddEvent(inflater, "OnPresence => from:" + e.Presence.From + " | " + e.Presence.Show + " | " + e.Presence.Status);};
			
			xmppClient.OnLogin += (sender, e) => { AddEvent(inflater, "OnLogin");};
			xmppClient.OnClose += (sender, e) => { AddEvent(inflater, "OnClose");};
			xmppClient.OnBind += (sender, e) => { AddEvent(inflater, "OnBind");};
			xmppClient.OnTls += (sender, e) => { AddEvent(inflater, "OnTls");};
			xmppClient.OnError += (sender, e) => { AddEvent(inflater, "OnError");};
			xmppClient.OnAuthError += (sender, e) => { AddEvent(inflater, "OnAuthError");};
			xmppClient.OnBeforeSasl  += (sender, e) => { AddEvent(inflater, "OnBeforeSasl");};
			
			return view;
		}
		
		public void AddEvent(LayoutInflater inflater, string evt)
		{
			Activity.RunOnUiThread(delegate {
				var text = new TextView(inflater.Context){Text = evt};
				RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(WindowManagerLayoutParams.FillParent, WindowManagerLayoutParams.WrapContent);
				text.LayoutParameters = layoutParams;
				layout.AddView(text);
				
				scrollView.ScrollTo(0, scrollView.Bottom);
			});
		}
	}
}