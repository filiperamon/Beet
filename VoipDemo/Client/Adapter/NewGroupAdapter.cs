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
using Android.Support.V7.Widget;
using AppCore;
using Android.Graphics;
using Legion.Client.utils;
using Matrix1 = Android.Graphics.Matrix;

namespace Legion.Client.Adapter 
{
    public class NewGroupAdapter : RecyclerView.Adapter
    {

        public event EventHandler<int> ItemClick;
		List<AppCore.User> userGroups;
        GroupAddActivity activity;		

        public NewGroupAdapter(List<AppCore.User> list, GroupAddActivity activity)
        {
            this.userGroups = list;
            this.activity = activity;

        }

        public override int ItemCount
        {
            get
            {
                return userGroups.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            NewGroupViewHolder vh = holder as NewGroupViewHolder;

            if (userGroups[position].Photo != null)
            {
                byte[] imageAsBytes = Android.Util.Base64.Decode(userGroups[position].Photo, Android.Util.Base64Flags.Default);
                activity.RunOnUiThread(() =>
                {
                    Bitmap bMap = BitmapFactory.DecodeByteArray(imageAsBytes, 0, imageAsBytes.Length);
                    var d = new CircleDrawable(bMap);
                    vh.Image.SetBackgroundDrawable(d);

                });

            }
            else
            {
                vh.Image.SetBackgroundResource(Resource.Drawable.avatar_upload);
                vh.Image.SetImageResource(Resource.Drawable.circle);
            }

            if (vh.Phone != null)
            {
                vh.Phone.Text = userGroups[position].Phone;
            }

			//is Group@
			if (userGroups[position].username.Contains('@'))
			{
				vh.Name.Text = userGroups[position].username.Split('@')[0];
			}
			else {
				vh.Name.Text = userGroups[position].username;
			}

            if (userGroups[position].isCheck)
                vh.Check.Checked = true;
            else
                vh.Check.Checked = false;

            vh.Image.RefreshDrawableState();
			vh.Check.RefreshDrawableState();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            View itemView = LayoutInflater.From(parent.Context).
                                          Inflate(Resource.Layout.CardAddNewGroup, parent, false);

            NewGroupViewHolder vh = new NewGroupViewHolder(itemView, OnClick);
            return vh;

        }

		//Raise an event when the item-click takes place
		void OnClick(int position)
		{
			
			if (userGroups[position].isCheck == false) {
				userGroups[position].isCheck = true;
			} 

			else
			{
				userGroups[position].isCheck = false;
			}
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

