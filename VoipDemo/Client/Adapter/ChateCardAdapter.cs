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
using Android.Graphics.Drawables;
using Android.Graphics;
using Legion.Client.utils;
using System.IO;

namespace Legion.Client
{
    public class ChateCardAdapter : RecyclerView.Adapter
    {
        public User user;
        Activity activity;
        AppCore.Message message;
        List<AppCore.Message> msgs;
        public AppPreferences appPreferences = new AppPreferences(Application.Context);

        public ChateCardAdapter(User user, Activity activity, AppCore.Message message, List<AppCore.Message> msgs)
        {
            this.user = user;
            this.activity = activity;
            this.message = message;
            this.msgs = msgs;
        }

        public override int ItemCount
        {
            get
            {
                return msgs.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            ChateCardViewHolder vh = holder as ChateCardViewHolder;
            String imgName = "";

            vh.imgFPhoto.Click += (s, e) => ImgPhoto_Click(imgName);
            vh.imgMyPhoto.Click += (s, e) => ImgPhoto_Click(imgName);

            if (msgs[position].isMe)
            {
                if (msgs[position].isImage)
                {
                    imgName = msgs[position].imgName;
                    Bitmap bit = LegionUtils.BitmapByName(msgs[position].imgName);
                    Drawable dr = new BitmapDrawable(bit);
                    vh.imgMyPhoto.SetBackgroundDrawable(dr);
                    vh.imgMyPhoto.Visibility = ViewStates.Visible;
                    vh.txvMyMessage.Visibility = ViewStates.Gone;
                }
                else
                {
                    vh.txvMyMessage.Text = msgs[position].Msg;
                    vh.txvMyMessage.Visibility = ViewStates.Visible;
                }

                vh.cardMessage.Visibility = ViewStates.Gone;
                vh.cardFreandMessage.Visibility = ViewStates.Visible;
                vh.txvFriendMessageHour.Visibility = ViewStates.Gone;
                vh.txvMyMessagehour.Text = msgs[position].Msgdate.ToString("t");
                vh.txvMyMessagehour.Visibility = ViewStates.Visible;
                vh.imgFPhoto.Visibility = ViewStates.Gone;


            }
            else
            {

                if (msgs[position].isImage)
                {
                    imgName = msgs[position].imgName;
                    Bitmap bit = LegionUtils.BitmapByName(msgs[position].imgName);
                    Drawable dr = new BitmapDrawable(bit);
                    vh.imgFPhoto.SetBackgroundDrawable(dr);
                    vh.imgFPhoto.Visibility = ViewStates.Visible;
                    vh.txvFriendMessage.Visibility = ViewStates.Gone;
                }
                else
                {
                    vh.txvFriendMessage.Text = msgs[position].Msg;
                    vh.txvFriendMessage.Visibility = ViewStates.Visible;
                }

                vh.cardMessage.Visibility = ViewStates.Visible;
                vh.cardFreandMessage.Visibility = ViewStates.Gone;
                vh.txvMyMessagehour.Visibility = ViewStates.Gone;
                vh.txvFriendMessageHour.Text = msgs[position].Msgdate.ToString("t");
                vh.txvFriendMessageHour.Visibility = ViewStates.Visible;
                vh.imgMyPhoto.Visibility = ViewStates.Gone;
                vh.txvMyMessage.Visibility = ViewStates.Gone;


            }

            vh.txvFriendMessage.RefreshDrawableState();
            vh.cardFreandMessage.RefreshDrawableState();
            vh.txvMyMessage.RefreshDrawableState();
            vh.cardMessage.RefreshDrawableState();
            vh.txvFriendMessageHour.RefreshDrawableState();
            vh.txvMyMessagehour.RefreshDrawableState();
            vh.imgMyPhoto.RefreshDrawableState();
            vh.imgFPhoto.RefreshDrawableState();

        }

        private void ImgPhoto_Click(String img)
        {
            Bundle b = new Bundle();
            Intent intent = new Intent(activity, typeof(ImgFullScreemActivity));
            b.PutString("IMG", img);
            intent.PutExtras(b);
            activity.StartActivity(intent);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).
                                          Inflate(Resource.Layout.ChatCardView, parent, false);
            ChateCardViewHolder vh = new ChateCardViewHolder(itemView);
            return vh;
        }

    }

    public class ChatCardViewHolder : RecyclerView.ViewHolder
    {
        public CardView cardMessage { get; private set; }

        public ChatCardViewHolder(View itemView) : base(itemView)
        {
            cardMessage = itemView.FindViewById<CardView>(Resource.Id.cv);
        }
    }
}

