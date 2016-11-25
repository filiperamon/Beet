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
using Android.Text.Style;
using Android.Graphics.Drawables;

namespace com.vasundharareddy.emojicon
{
    public class EmojiconSpan : DynamicDrawableSpan
    {
        private readonly Context mContext;
        private readonly int mResourceId;
        private readonly int mSize;
        private Drawable mDrawable;
        public EmojiconSpan(Context context, int resourceId, int size):base()
        {
            mContext = context;
            mResourceId = resourceId;
            mSize = size;
        }

        public override Drawable Drawable
        {
            get{
                if (mDrawable == null)
                {
                    try
                    {
                        mDrawable = mContext.Resources.GetDrawable(mResourceId);
                        int size = mSize;
                        mDrawable.SetBounds(0, 0, size, size);
                    }
                    catch (Exception)
                    {
                        // swallow
                    }
                }
                return mDrawable;
            }
            
            
        }
    }
}