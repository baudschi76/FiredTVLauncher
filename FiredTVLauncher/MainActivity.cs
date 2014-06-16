﻿using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using Android.Content.PM;
using Android.Content.Res;
using System.Threading;

namespace FiredTVLauncher
{
	[IntentFilter (new [] { Android.Content.Intent.ActionMain }, Categories=new [] { Android.Content.Intent.CategoryDefault, Android.Content.Intent.CategoryHome })]
	[Activity (Label = "FiredTV")]
	public class MainActivity : Activity
	{
		Timer timerUpdate;
		AppsAdapter adapter;
		GridView gridView;
		TextView textDate;
		TextView textTime;
		ImageView imageLogo;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			Settings.Load ();


			RequestWindowFeature (WindowFeatures.NoTitle);
			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			textDate = FindViewById<TextView> (Resource.Id.textViewDate);
			textTime = FindViewById<TextView> (Resource.Id.textViewTime);
			imageLogo = FindViewById<ImageView> (Resource.Id.imageViewLogo);

			imageLogo.SetImageResource (Resource.Drawable.firedtv);

			gridView = FindViewById<GridView> (Resource.Id.gridView);
		

			StartService (new Intent (this, typeof(ExcuseMeService)));

			adapter = new AppsAdapter () { Context = this };

			gridView.ItemClick += (sender, e) => {
				var app = adapter[e.Position];

				// If we're launching home, tell the service that checks
				// for intercepting this that it's ok
				if (app.PackageName == Settings.HOME_PACKAGE_NAME)
					ExcuseMeService.AllowFireTVHome = true;

				StartActivity (app.LaunchIntent);
			};
				
			gridView.Adapter = adapter;

			timerUpdate = new Timer (state => Setup (), null, Timeout.Infinite, Timeout.Infinite);
		}

		protected override void OnResume ()
		{
			base.OnResume ();

			Setup ();

			// Tell the service to continue checking again now
			// that we've resumed our launcher
			ExcuseMeService.AllowFireTVHome = false;

			adapter.Reload ();

			textDate.Visibility = Settings.Instance.HideDate ? ViewStates.Gone : ViewStates.Visible;
			textTime.Visibility = Settings.Instance.HideTime ? ViewStates.Gone : ViewStates.Visible;
			imageLogo.Visibility = Settings.Instance.HideFiredTVLogo ? ViewStates.Gone : ViewStates.Visible;

			timerUpdate.Change (TimeSpan.FromSeconds (10), TimeSpan.FromSeconds (10));
		}

		protected override void OnPause ()
		{
			base.OnPause ();

			timerUpdate.Change (Timeout.Infinite, Timeout.Infinite);
		}


		public override bool OnPrepareOptionsMenu (IMenu menu)
		{
			return false;
		}

		public override bool OnKeyDown (Keycode keyCode, KeyEvent e)
		{
			if (keyCode == Keycode.Menu) {
				StartActivity (typeof(SettingsActivity));
				return true;
			}
			return base.OnKeyDown (keyCode, e);
		}
			
		void Setup()
		{
			RunOnUiThread (() => {
				textTime.Text = DateTime.Now.ToString ("H:mm tt");
				textDate.Text = DateTime.Now.ToString ("dddd MMMM d");
			});
		}

	}
}

