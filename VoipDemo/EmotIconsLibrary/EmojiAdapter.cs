using System;
using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace com.vasundharareddy.emojicon
{

	public class EmojiAdapter :  ArrayAdapter<Emojicon>
	{
		public EmojiAdapter (Context context, List<Emojicon> data) :base(context,Resource.Layout.emojicon_item,data)
		{

		}
		public EmojiAdapter (Context context, Emojicon[] data) :base(context,Resource.Layout.emojicon_item,data)
		{

		}
		public override View GetView(int position, View convertView, ViewGroup parent)
		{
			View v = convertView;
			ViewHolder holder;
			if (v == null) {
				v = View.Inflate(Context, Resource.Layout.emojicon_item, null);
				holder = new ViewHolder();
				holder.icon = v.FindViewById<EmojiconTextView>(Resource.Id.emojicon_icon);
				v.Tag = holder;
			}
			Emojicon emoji = GetItem(position);
			holder = (ViewHolder) v.Tag;
			holder.icon.Text = emoji.Emoji;
			return v;
		}
		class ViewHolder : Java.Lang.Object {
            public EmojiconTextView icon;		  
		}
	}

}

