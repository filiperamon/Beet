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
using Android.Util;
using Android.Content.Res;
using Android.Text;

namespace com.vasundharareddy.emojicon
{
    public class EmojiconTextView : TextView
    {
        private int mEmojiconSize;
        private int mTextStart = 0;
        private int mTextLength = -1;

        public EmojiconTextView(Context context):base(context) {
            init(null);
        }

        public EmojiconTextView(Context context, IAttributeSet attrs):base(context,attrs) {
            init(attrs);
        }

        public EmojiconTextView(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            init(attrs);
        }
        private void init(IAttributeSet attrs)
        {
            if (attrs == null)
            {
                mEmojiconSize = (int)TextSize;
            }
            else
            {
                TypedArray a = Context.ObtainStyledAttributes(attrs,Resource.Styleable.Emojicon);
                mEmojiconSize = (int)a.GetDimension(Resource.Styleable.Emojicon_emojiconSize, TextSize);
                mTextStart = a.GetInteger(Resource.Styleable.Emojicon_emojiconTextStart, 0);
                mTextLength = a.GetInteger(Resource.Styleable.Emojicon_emojiconTextLength, -1);
                a.Recycle();
            }
            Text = base.Text;
        }
		public new string Text
		{
			set
			{
				SpannableStringBuilder builder = new SpannableStringBuilder(value);
				EmojiconHandler.AddEmojis(Context, builder, mEmojiconSize, mTextStart, mTextLength);
				base.TextFormatted = builder;
			}
			get{
				return base.Text;
			}
		}
        public int EmojiconSize
        {
            set { mEmojiconSize = value; }
        }

    }
}