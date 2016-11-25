using System;
using Android.Content;
using Android.InputMethodServices;
using Android.Util;
using Android.Views;
using Android.Views.Animations;

namespace Legion.Client
{
	public class CustomKeyboardView : KeyboardView
	{
		public CustomKeyboardView(Context context, IAttributeSet attrs) : base(context, attrs)
		{
		}

		public void ShowWithAnimation(Animation animation)
		{
			animation.AnimationEnd += (sender, e) =>
			{
				Console.WriteLine("Set visibility!");
				Visibility = ViewStates.Visible;
			};

			Animation = animation;
		}


	}
}


