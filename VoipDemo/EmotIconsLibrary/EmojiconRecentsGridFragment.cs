using System;
using Android.Widget;

namespace com.vasundharareddy.emojicon
{
	public class EmojiconRecentsGridFragment : EmojiconGridFragment,IEmojiconRecents
	{
		private EmojiAdapter mAdapter;
		public static EmojiconRecentsGridFragment NewInstance()
		{
			return new EmojiconRecentsGridFragment();
		}
		public override void OnViewCreated (Android.Views.View view, Android.OS.Bundle savedInstanceState)
		{
			EmojiconRecentsManager.Context =view.Context;
			mAdapter = new EmojiAdapter(view.Context, EmojiconRecentsManager.Recents);
			GridView gridView = (GridView) view.FindViewById(Resource.Id.Emoji_GridView);
			gridView.Adapter = mAdapter;
			gridView.ItemClick += OnItemClick;
		}
		public override void OnDestroyView ()
		{
			base.OnDestroyView ();
			mAdapter = null;
		}
		public override void OnResume ()
		{
			//mAdapter.NotifyDataSetChanged();
			base.OnResume ();
		}
		#region IEmojiconRecents implementation

		public void OnAddRecentEmoji (Emojicon emojicon)
		{
			EmojiconRecentsManager.Push(emojicon);

			// notify dataset changed
			//if (mAdapter != null)
			//	mAdapter.NotifyDataSetChanged();
		}

		#endregion
	}
}

