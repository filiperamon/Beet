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
using Android.Graphics.Drawables;
using Android.Graphics;

namespace Legion.Client
{
    [Activity(Label = "ImgFullScreemActivity")]
    public class ImgFullScreemActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.imgFullScreem);

            ImageView imgFullScreem = (ImageView)FindViewById(Resource.Id.imageFullScreem);

            if (this.Intent.Extras != null)
            {
                String resultData = this.Intent.Extras.GetString("IMG");

                if (resultData != null)
                { 
                    Bitmap bit = LegionUtils.BitmapByName(resultData);
                    Drawable dr = new BitmapDrawable(bit);
                    imgFullScreem.SetBackgroundDrawable(dr);
					imgFullScreem.LayoutParameters.Height = bit.Height;
					imgFullScreem.LayoutParameters.Width = bit.Width;
                }
            }
        
        }

		public override void OnBackPressed()
		{
			base.OnBackPressed();
			Finish();
		}
    }
}