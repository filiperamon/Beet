using System;
using Android.Support.V4.App;
using System.Collections.Generic;

namespace com.vasundharareddy.emojicon
{
	public class EmojisPagerAdapter : FragmentStatePagerAdapter
	{
		private List<EmojiconGridFragment> fragments;
		public EmojisPagerAdapter (FragmentManager fm, List<EmojiconGridFragment> fragments):base(fm)
		{
			this.fragments = fragments;
		}
		public override Fragment GetItem (int position)
		{
			return fragments [position];
		}
		public override int Count
		{
			get
			{
				return fragments.Count;
			}
		}
	}
}

