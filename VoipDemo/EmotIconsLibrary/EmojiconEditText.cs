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
    public class EmojiconEditText : EditText
    {
        private int mEmojiconSize;
  
        public EmojiconEditText(Context context):base(context)
		{
			mEmojiconSize =(Int32)TextSize;
			TextChanged += OnTextChanged;
        }

        void OnTextChanged (object sender, TextChangedEventArgs e)
        {

			EmojiconHandler.AddEmojis(Context, new SpannableStringBuilder(TextFormatted), mEmojiconSize);
        }

        public EmojiconEditText(Context context, IAttributeSet attrs):base(context,attrs) 
		{
            init(attrs);
        }

        public EmojiconEditText(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {
            init(attrs);
        }
        private void init(IAttributeSet attrs)
        {
			TextChanged += OnTextChanged;
            if (attrs == null)
            {
                mEmojiconSize = (int)TextSize;
            }
            else
            {
                TypedArray a = Context.ObtainStyledAttributes(attrs,Resource.Styleable.Emojicon);
                mEmojiconSize = (int)a.GetDimension(Resource.Styleable.Emojicon_emojiconSize, TextSize);
                a.Recycle();
            }
			Text = base.Text;
        }
		public new string Text
		{
			set
			{
				SpannableStringBuilder builder = new SpannableStringBuilder(value);
				EmojiconHandler.AddEmojis(Context, builder, mEmojiconSize);
				base.TextFormatted = builder;
			}
			get
			{
				return base.Text;
			}
		}
		protected override void Dispose (bool disposing)
		{

			TextChanged -= OnTextChanged;
			base.Dispose (disposing);
		}

        public int EmojiconSize
        {
            set 
			{
				mEmojiconSize = value;
			}
        }

    }
}