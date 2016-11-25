using System;
using Android.Support.V4.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace com.vasundharareddy.emojicon
{
    public delegate void OnEmojiClicked(Emojicon e);
    public class EmojiconGridFragment : Fragment
    {
        private IEmojiconRecents mRecents;
        private Emojicon[] mData;
        private GridView gridView;
        public event OnEmojiClicked EmojiClicked;
        public static EmojiconGridFragment NewInstance(Emojicon[] emojicons, IEmojiconRecents recents)
        {
            EmojiconGridFragment emojiGridFragment = new EmojiconGridFragment();
            emojiGridFragment.mData = emojicons;
			emojiGridFragment.mRecents = recents;
            return emojiGridFragment;
        }
        public IEmojiconRecents Recents
        {
            set { mRecents = value; }
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.emojicon_grid, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            gridView = (GridView)view.FindViewById(Resource.Id.Emoji_GridView);
            if (mData == null)
            {
                mData = People.DATA;
            }           
            gridView.Adapter = new EmojiAdapter(view.Context, mData);
            gridView.ItemClick += OnItemClick;
        }
        public void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (EmojiClicked != null)
            {
                EmojiClicked((Emojicon)e.Parent.GetItemAtPosition(e.Position));
            }
        }
    }
 }

