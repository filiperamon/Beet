
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
using Matrix1 = Android.Graphics.Matrix;
using AppCore;
using Legion.Client.utils;
using Android.Graphics;
using Drawables = Android.Graphics.Drawables;

namespace Legion.Client
{
	public class UserAdapter : RecyclerView.Adapter
	{

		public event EventHandler<int> ItemClick;
		public User user;
		Activity activity;
        private UserViewHolder vh;
        public AppPreferences appPreferences;


        public UserAdapter(User user, Activity activity) {
			this.user = user;
			this.activity = activity;
            appPreferences = new AppPreferences(activity.Application.ApplicationContext);
        }

		public override int ItemCount
		{
			get
			{
				return user.users.Count; 
			}
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			vh = holder as UserViewHolder;
			if (user.users[position].Photo != null)
			{
				byte[] imageAsBytes = Android.Util.Base64.Decode(user.users[position].Photo, Android.Util.Base64Flags.Default);
				activity.RunOnUiThread(() =>
				{
					Bitmap bMap = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
					var d = new CircleDrawable(bMap);
					vh.Image.SetBackgroundDrawable(d);

				});

			}
			else {
				vh.Image.SetBackgroundResource(Resource.Drawable.avatar_upload);
				vh.Image.SetImageResource(Resource.Drawable.circle);
			}

			if (vh.Phone != null)
			{
				vh.Phone.Text = user.users[position].Phone;
			}

			//teste for online user
			if (user.users[position].isGroup)
			{
				vh.IconVideo.Visibility = ViewStates.Gone;
				vh.IconPhone.Visibility = ViewStates.Gone;
				//vh.IconStatusOn.Visibility = ViewStates.Gone;
			}
			else
			{
				vh.IconVideo.Visibility = ViewStates.Visible;
				vh.IconPhone.Visibility = ViewStates.Visible;
			}

			//teste for online user
			if (user.users[position].username != null)
			{
				vh.Name.Text = user.users[position].username.Replace("_", " ");
				if (vh.Name.Text.Contains('@'))
				{
					vh.Name.Text = vh.Name.Text.Split('@')[0];
				}
			}
			else if (user.users[position].Nick != null) {
				vh.Name.Text = user.users[position].Nick.Replace("_", " ");
				if (vh.Name.Text.Contains('@'))
				{
					vh.Name.Text = vh.Name.Text.Split('@')[0];
				}
			}

            //is msg no ready
            if(user.users[position].username != null || user.users[position].Nick != null) { 
                int msgCount = appPreferences.getValueKeyMSGCount(LegionUtils.GetUserNameClear(user.users[position].username));
                if (msgCount > 0)
                {
                    vh.fragmentMsg.Visibility = ViewStates.Visible;
                    vh.countMsg.Text = msgCount.ToString();
                }
                else {
                    vh.fragmentMsg.Visibility = ViewStates.Gone;
                }
                vh.fragmentMsg.RefreshDrawableState();
            }

			vh.IconVideo.RefreshDrawableState();
			vh.IconPhone.RefreshDrawableState();
			vh.Name.RefreshDrawableState();
			vh.fragmentMsg.RefreshDrawableState();
            vh.Image.RefreshDrawableState();
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{

			View itemView = LayoutInflater.From(parent.Context).
			                              Inflate(Resource.Layout.ContactsCardView, parent, false);

			UserViewHolder vh = new UserViewHolder(itemView, OnClick, OnClick);
			return vh;

		}

		//Raise an event when the item-click takes place
		void OnClick(int position)
		{ 
            User userChat = user.users.ElementAt(position);
			string jsonTmp = LegionUtils.parceToGson(userChat);
			var activityChat = new Intent(activity, typeof(ChatActivity));

            if (userChat.isGroup)
			{
                activityChat.PutExtra("chatGroup", true);
				activityChat.PutExtra("groupJson", jsonTmp);
			}
			else { 
				activityChat.PutExtra("userChat", jsonTmp);
				activityChat.PutExtra("isDirectCall", false);
			}


			activity.StartActivity(activityChat);
		}

		void OnClick(string calltype, int position)
		{
			User userChat = user.users.ElementAt(position);
			string jsonTmp = LegionUtils.parceToGson(userChat);
			var activityChat = new Intent(activity, typeof(ChatActivity));
			activityChat.PutExtra("userChat", jsonTmp);
			activityChat.PutExtra("isDirectCall", true);
			activityChat.PutExtra("callType", calltype);
			activity.StartActivity(activityChat);
		}

		public Bitmap Base64ToBitmap(String base64String)
		{
			byte[] imageAsBytes = Android.Util.Base64.Decode(base64String, Android.Util.Base64Flags.Default);
			return BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
		}

		public void setImageViewWithByteArray(ImageView view, byte[] data)
		{

            Bitmap bitmap = BitmapFactory.DecodeByteArray(data, 0, data.Length);
            bitmap = getResizedBitmap(bitmap, 170, 170);
            var c = new CircleDrawable(bitmap);
            view.SetBackgroundDrawable(c);

        }

		public Bitmap getResizedBitmap(Bitmap bm, int newWidth, int newHeight)
		{
            int width = bm.Width;
            int height = bm.Height;
            newHeight = CalculateProportionalHeight(bm.Width, bm.Height, newWidth);

            float scaleWidth = ((float)newWidth) / width;
            float scaleHeight = ((float)newHeight) / height;
            var matrix = new Matrix1();
            matrix.PostScale(scaleWidth, scaleHeight);

            Bitmap resizedBitmap = Bitmap.CreateBitmap(
                bm, 0, 0, width, height, matrix, false);
            bm.Recycle();
            return resizedBitmap;
        }

        private static int CalculateProportionalHeight(int oldWidth, int oldHeight, int newWidth)
        {
            if (oldWidth <= 0 || oldHeight <= 0 || newWidth <= 0)
                // For safety.
                return oldHeight;
            double widthFactor = (double)newWidth / (double)oldWidth;
            int newHeight = (int)Math.Round(widthFactor * (double)oldHeight);
            if (newHeight < 1)
                newHeight = 1; // just in case.
            return newHeight;
        }


        private static Bitmap GetRoundedCornerBitmap(Bitmap bitmap, int pixels)
		{
			Bitmap output = null;

			try
			{
				output = Bitmap.CreateBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.Argb8888);
				Canvas canvas = new Canvas(output);

				Color color = new Color(66, 66, 66);
				Paint paint = new Paint();
				Rect rect = new Rect(0, 0, bitmap.Width, bitmap.Height);
				RectF rectF = new RectF(rect);
				float roundPx = pixels;

				paint.AntiAlias = true;
				canvas.DrawARGB(0, 0, 0, 0);
				paint.Color = color;
				canvas.DrawRoundRect(rectF, roundPx, roundPx, paint);

				paint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.SrcIn));
				canvas.DrawBitmap(bitmap, rect, rect, paint);
			}
			catch (Exception err)
			{
				System.Console.WriteLine("GetRoundedCornerBitmap Error - " + err.Message);
			}

			return output;
		}


	}
}

