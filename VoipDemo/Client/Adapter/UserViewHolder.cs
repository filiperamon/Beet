
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
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using AppCore;
using Android.Telephony;

namespace Legion.Client
{
	
	public class UserViewHolder : RecyclerView.ViewHolder
	{
		public ImageView Image { get; private set; }
		public TextView Name { get; private set; }
        public TextView Phone { get; private set; }
		public ImageView IconPhone { get; set;}
		public ImageView IconVideo { get; set; }
        public ImageView IconStatusOn { get; set; }
        public FrameLayout fragmentMsg { get; set; }
        public TextView countMsg { get; private set; }
        

        public UserViewHolder(View itemView, Action<int> listener, Action<string, int> calltype) : base(itemView) { 

			Image = itemView.FindViewById<ImageView>(Resource.Id.contact_photo);
			Name  = itemView.FindViewById<TextView>(Resource.Id.contact_name);
            Phone = itemView.FindViewById<TextView>(Resource.Id.contact_msg);
            Phone.AddTextChangedListener(new PhoneNumberFormattingTextWatcher());
			IconPhone = itemView.FindViewById<ImageView>(Resource.Id.imageButton3);
			IconVideo = itemView.FindViewById<ImageView>(Resource.Id.imageView2);
            IconStatusOn = itemView.FindViewById<ImageView>(Resource.Id.status_msg);
            fragmentMsg = itemView.FindViewById<FrameLayout>(Resource.Id.fragmentMsg);
            countMsg = itemView.FindViewById<TextView>(Resource.Id.countMsg);

            IconPhone.Click += (sender, e) => calltype("callPhone", base.Position);
			IconVideo.Click += (sender, e) => calltype("callVideo", base.Position);

			itemView.Click += (sender, e) => listener(base.Position);            

        }

	}
}

