using System;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Text;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace com.vasundharareddy.emojicon
{
	public delegate void OnEmojiconBackspaceClicked(View v);
	public delegate void OnAddRecentEmoji(Emojicon emojicon);
	public class EmojiconsFragment : Fragment, IEmojiconRecents
	{
		private int mEmojiTabLastSelectedIndex = -1;
		private View[] mEmojiTabs;
		private PagerAdapter mEmojisAdapter;
		public static event OnEmojiconBackspaceClicked EmojiconBackspaceClicked;
		public static event OnEmojiClicked EmojiClicked;
		public static event OnAddRecentEmoji AddRecentEmoji;
		public override View OnCreateView (LayoutInflater inflater, ViewGroup container, Android.OS.Bundle savedInstanceState)
		{
			View view = inflater.Inflate(Resource.Layout.emojicons, container, false);
			ViewPager emojisPager = view.FindViewById<ViewPager>(Resource.Id.emojis_pager);
			emojisPager.PageSelected += OnPageSelected;
			AddRecentEmoji += OnAddRecentEmoji;
			// we handle recents
			IEmojiconRecents recents = this;
			List<EmojiconGridFragment> emojiFragments = new List<EmojiconGridFragment> () {
				EmojiconRecentsGridFragment.NewInstance (),
				EmojiconGridFragment.NewInstance (People.DATA, recents),
				EmojiconGridFragment.NewInstance (Nature.DATA, recents),
				EmojiconGridFragment.NewInstance (Objects.DATA, recents),
				EmojiconGridFragment.NewInstance (Places.DATA, recents),
				EmojiconGridFragment.NewInstance (Symbols.DATA, recents)
			};
			foreach (var fragment in emojiFragments) 
			{
				fragment.EmojiClicked += (Emojicon e) => 
				{
					if(EmojiClicked != null)
					{
						EmojiClicked(e);
					}
				};
			}
			mEmojisAdapter = new EmojisPagerAdapter(FragmentManager, emojiFragments);
			emojisPager.Adapter = mEmojisAdapter;
			//ToDo: Solve Recents Issue
			mEmojiTabs = new View[6];
			mEmojiTabs[0] = view.FindViewById(Resource.Id.emojis_tab_0_recents);
			mEmojiTabs[1] = view.FindViewById(Resource.Id.emojis_tab_1_people);
			mEmojiTabs[2] = view.FindViewById(Resource.Id.emojis_tab_2_nature);
			mEmojiTabs[3] = view.FindViewById(Resource.Id.emojis_tab_3_objects);
			mEmojiTabs[4] = view.FindViewById(Resource.Id.emojis_tab_4_cars);
			mEmojiTabs[5] = view.FindViewById(Resource.Id.emojis_tab_5_punctuation);
//			mEmojiTabs = new View[5];
//			mEmojiTabs[0] = view.FindViewById(Resource.Id.emojis_tab_1_people);
//			mEmojiTabs[1] = view.FindViewById(Resource.Id.emojis_tab_2_nature);
//			mEmojiTabs[2] = view.FindViewById(Resource.Id.emojis_tab_3_objects);
//			mEmojiTabs[3] = view.FindViewById(Resource.Id.emojis_tab_4_cars);
//			mEmojiTabs[4] = view.FindViewById(Resource.Id.emojis_tab_5_punctuation);
			for (int i = 0; i < mEmojiTabs.Length; i++)
			{
				int position = i;
				mEmojiTabs[i].Click += (object sender, EventArgs e)
				=>{
					emojisPager.CurrentItem = position;
				};
			}
			view.FindViewById(Resource.Id.emojis_backspace).Click += (object sender, EventArgs e) => {
				if(EmojiconBackspaceClicked != null)
				{
					EmojiconBackspaceClicked((View)sender);
				}
			};

			// get last selected page
			EmojiconRecentsManager.Context = view.Context;
            int page = EmojiconRecentsManager.RecentPage;
            // last page was recents, check if there are recents to use
            // if none was found, go to page 1
            if (page == 0 && EmojiconRecentsManager.Count == 0) {
                page = 1;
            }
            if (page == 0) {
				OnPageSelected(null,new ViewPager.PageSelectedEventArgs(page));
            }
            else {
                emojisPager.SetCurrentItem(page, false);
            }
            return view;
		}

		void OnPageSelected (object sender, ViewPager.PageSelectedEventArgs e)
		{
			if (mEmojiTabLastSelectedIndex == e.Position) {
				return;
			}
			switch ( e.Position) {
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
				if (mEmojiTabLastSelectedIndex >= 0 && mEmojiTabLastSelectedIndex < mEmojiTabs.Length)
				{
					mEmojiTabs [mEmojiTabLastSelectedIndex].Selected = false;
				}
				mEmojiTabs [e.Position].Selected = true;
				mEmojiTabLastSelectedIndex = e.Position;
				EmojiconRecentsManager.RecentPage = e.Position;
				break;
			}
		}
		public override void OnAttach (Android.App.Activity activity)
		{
			base.OnAttach (activity);
		}
		public override void OnDetach ()
		{
			EmojiconBackspaceClicked = null;
			EmojiClicked = null;
			base.OnDetach ();
		}
		public static void Input(EmojiconEditText editText, Emojicon emojicon)
		{
			AddRecentEmoji (emojicon);
			if (editText == null || emojicon == null)
			{
				return;
			}
	    	int start = Math.Max(editText.SelectionStart,0);        
			int end =Math.Max(editText.SelectionEnd,0);        
			if (start < 0) 
			{
				editText.Text += emojicon.Emoji;
				editText.SetSelection (editText.Text.Length);
			}
			else
			{                
				editText.Text = editText.Text.Substring (0, Math.Min (start, end)) + emojicon.Emoji + editText.Text.Substring (Math.Max (start, end));			               
				editText.SetSelection (start+emojicon.Emoji.Length);
			}
		}

		#region IEmojiconRecents implementation
		public void OnAddRecentEmoji (Emojicon emojicon)
		{
			ViewPager emojisPager = View.FindViewById<ViewPager>(Resource.Id.emojis_pager);
			EmojiconRecentsGridFragment fragment = (EmojiconRecentsGridFragment) mEmojisAdapter.InstantiateItem(emojisPager, 0);
			fragment.OnAddRecentEmoji(emojicon);
		}
		#endregion
		public static void Backspace(EditText editText)
		{
			KeyEvent evnt = new KeyEvent(0, 0, 0,Keycode.Del, 0, 0, 0, 0,KeyEventFlags.Canceled);
			editText.DispatchKeyEvent(evnt);
		}
		public override void OnDestroy ()
		{
			base.OnDestroy ();
			AddRecentEmoji -= OnAddRecentEmoji;
		}
	}
}

