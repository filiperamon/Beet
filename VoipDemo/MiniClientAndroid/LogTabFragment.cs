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
	public class LogTabFragment: Fragment
	{	
		XmppClient xmppClient;
		ScrollView scrollView;
		
		public LogTabFragment(XmppClient xmppClient)
		{
			this.xmppClient = xmppClient;
		}
		
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			base.OnCreateView (inflater, container, savedInstanceState);
			
			var view = inflater.Inflate (Resource.Layout.Log, container, false);					
			var layout = view.FindViewById<LinearLayout>(Resource.Id.logLinearLayout);
			scrollView = view.FindViewById<ScrollView>(Resource.Id.logScrollView);
			
			RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(WindowManagerLayoutParams.FillParent, WindowManagerLayoutParams.WrapContent);
			
			this.xmppClient.OnReceiveXml += (sender, e) => 
			{
				Activity.RunOnUiThread(delegate {			
					var text = new TextView(inflater.Context){Text = "RECV: " + e.Text};
					text.LayoutParameters = layoutParams;
					layout.AddView(text);
					scrollView.ScrollTo(0, scrollView.Bottom);
				});
				
			};
			
			this.xmppClient.OnSendXml += (sender, e) => 
			{
				Activity.RunOnUiThread(delegate {
					var text = new TextView(inflater.Context){Text = "SEND: " + e.Text};
					text.LayoutParameters = layoutParams;
					layout.AddView(text);
					scrollView.ScrollTo(0, scrollView.Bottom);
				});
			};
			
			return view;
		}
	}
}