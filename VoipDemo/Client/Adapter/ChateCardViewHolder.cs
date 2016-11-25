
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

namespace Legion.Client
{
	public class ChateCardViewHolder : RecyclerView.ViewHolder
	{
		public CardView cardMessage       { get; private set; }
		public CardView cardFreandMessage { get; private set; }

        public TextView txvMyMessage         { get; private set; }
		public TextView txvFriendMessage     { get; private set; }
		public TextView txvMyMessagehour     { get; private set; }
		public TextView txvFriendMessageHour { get; private set; }
		public ImageView imgMyPhoto { get; private set;}
		public ImageView imgFPhoto { get; private set;}

		public ImageView arrowLeft { get; set;}
		public ImageView arrowRight { get; set;}


		public TextView nameFriend { get; private set; }
		public TextView myName { get; private set; }
            
        public ChateCardViewHolder(View itemView) : base(itemView)
		{
			cardMessage          = itemView.FindViewById<CardView>(Resource.Id.cv);
			cardFreandMessage    = itemView.FindViewById<CardView>(Resource.Id.cvMymessage);
			txvMyMessage         = itemView.FindViewById<TextView>(Resource.Id.txtMyMessage);
			txvFriendMessage     = itemView.FindViewById<TextView>(Resource.Id.txtFriendMessage);
			txvMyMessagehour     = itemView.FindViewById<TextView>(Resource.Id.timeMyMessage);
			txvFriendMessageHour = itemView.FindViewById<TextView>(Resource.Id.timeFreandMessage);
			imgMyPhoto = itemView.FindViewById<ImageView>(Resource.Id.im_photo_me);
			imgFPhoto = itemView.FindViewById<ImageView>(Resource.Id.im_photo_f);
			nameFriend			 = itemView.FindViewById<TextView>(Resource.Id.nameFriend);
			nameFriend.Visibility = ViewStates.Gone;
			myName				 = itemView.FindViewById<TextView>(Resource.Id.MyName);
			myName.Visibility = ViewStates.Gone;

            
        }
    }
}

