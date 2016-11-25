
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

namespace Legion.Client.Adapter
{
    public class NewGroupViewHolder : RecyclerView.ViewHolder
    {
        public ImageView Image { get; private set; }
        public TextView Name { get; private set; }
        public TextView Phone { get; private set; }
        public CheckBox Check { get; private set; }

        public NewGroupViewHolder(View itemView, Action<int> listener) : base(itemView) {

            Image = itemView.FindViewById<ImageView>(Resource.Id.contact_photo);
            Name = itemView.FindViewById<TextView>(Resource.Id.contact_name);
            Phone = itemView.FindViewById<TextView>(Resource.Id.contact_msg);
            Check = itemView.FindViewById<CheckBox>(Resource.Id.check_contact);
            Phone.AddTextChangedListener(new PhoneNumberFormattingTextWatcher());

			Check.Click += (sender, e) => listener(base.Position);
        }
    }
} 