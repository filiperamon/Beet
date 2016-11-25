
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
using AppCore;

using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Legion.Client.Adapter;
using Android.Graphics;
using Android.Provider;
using System.IO;
using Legion.Client.utils;
using Matrix1 = Android.Graphics.Matrix;
using System.Threading.Tasks;

namespace Legion.Client
{
	[Activity(Label = "GroupAddActivity")]
	public class GroupAddActivity : BaseAppCompatActivity
	{
		private ImageView imvAvatar;
		private TextView txvNameGroup;
		private RecyclerView mRecyclerView;
		private LinearLayoutManager mLayoutManager;
		public List<AppCore.User> userGroups;
		public AppCore.User group;
		public List<AppCore.User> listGroups;

        protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.AddNewGroup);
			userGroups = listContacts();
			group = new AppCore.User();
			listGroups = LegionUtils.getGroupsTempToList(AppPreferences.GROUPS_TEMP, appPreferences);

			var toolbar = loadToolBar(Resource.String.newGroup);
			toolbar.SetNavigationIcon(Resource.Drawable.back);

			txvNameGroup = (TextView)FindViewById(Resource.Id.groupName);
			imvAvatar = (ImageView)FindViewById(Resource.Id.avatar);
			imvAvatar.SetBackgroundResource(Resource.Drawable.avatar_upload);

            mRecyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerView);
			mLayoutManager = new LinearLayoutManager(this);
			mRecyclerView.SetLayoutManager(mLayoutManager);
			NewGroupAdapter userAdapter = new NewGroupAdapter(userGroups, this);
			mRecyclerView.SetAdapter(userAdapter);

            imvAvatar.Click += delegate
            {
                var imageIntent = new Intent();
                imageIntent.SetType("image/*");
                imageIntent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(
                    Intent.CreateChooser(imageIntent, "Select photo"), 0);
            };


        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.actionbar_main, menu);
            return base.OnCreateOptionsMenu(menu);
        }

		public List<User> filterUserFromGroup(List<User> user) {
			List<User> returnList = new List<User>();
			foreach (User u in user) {
				if (u.isGroup == false) {
					returnList.Add(u);
				}
			}

            return returnList;
		}

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    startBack();
                    return true;
				case Resource.Id.action_add:

					if (LegionUtils.ContainsSpecialChars(txvNameGroup.Text)) {
						Toast.MakeText(Application.Context, "Group should not contain any special characters", ToastLength.Long).Show();
						return false;
					}
					if (txvNameGroup.Text != "" && userGroups.Count > 0)
					{
						group.username = txvNameGroup.Text.Replace(" ", "_");
						addNewGroup(group);
					}
					else 
					{
                        if (txvNameGroup.Text == "")
                        {
                            txvNameGroup.SetError(GetString(Resource.String.invalidField), null);
                            txvNameGroup.RefreshDrawableState();
                        }
                        else
                        {                            
                            Toast.MakeText(this, GetString(Resource.String.errorNoContact), ToastLength.Long).Show();
                        }
                    }

					return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        void startBack()
        {
            Finish();
            StartActivity(new Intent(ApplicationContext, typeof(ContacPhoneActivity)));
            OverridePendingTransition(Resource.Animator.slide_in_left, Resource.Animator.stable);
        }

		public override void OnBackPressed()
		{
			startBack();
		}

		protected void addNewGroup(AppCore.User group) {

			group.Jid = AddNewGroup();
			group.isGroup = true;
			listGroups.Add(group);
			saveSeachGroups(listGroups);

			ChangeGroupSubject(group.Jid, group.username);
			invictFriend(usersIsCheck(userGroups), group.Jid);

			RunOnUiThread(() =>
			   VisibleProgress(Resource.Id.pbHeaderProgress, VISIBLE)
			);
			Task.Delay(TimeSpan.FromSeconds(5)).Wait();

			string groupJson =  LegionUtils.parceToGson(group);
			appPreferences.saveKey(AppPreferences.GROUP_JSON, groupJson);

			var activityChat = new Intent(this, typeof(ChatActivity));
			activityChat.PutExtra("chatGroup", true);
			activityChat.PutExtra("groupJson", "");
			activityChat.PutExtra("isFromGroup", true);

			this.StartActivity(activityChat);
			OverridePendingTransition(Resource.Animator.slide_out_right, Resource.Animator.stable);
			RunOnUiThread(() =>
			              VisibleProgress(Resource.Id.pbHeaderProgress, INVISIBLE)
			);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			Finish();
		}

		private List<AppCore.User> usersIsCheck(List<AppCore.User> list) {
			var us = list.FindAll(x =>  x.isCheck == true);  
			return us;
		}

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                Bitmap bitmap = LegionUtils.getCircleDrawable(data, this);
                CircleDrawable d = new CircleDrawable(bitmap);

                imvAvatar.SetBackgroundDrawable(d);

                var ms = new MemoryStream();
                bitmap.Compress(Bitmap.CompressFormat.Png, 0, ms);
                var imageByteArray = ms.ToArray();

				group.Photo = Convert.ToBase64String(imageByteArray);
            }
        }
    }
}

