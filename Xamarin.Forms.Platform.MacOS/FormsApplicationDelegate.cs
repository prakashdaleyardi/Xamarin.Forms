﻿using System;
using System.ComponentModel;
using AppKit;
using CoreGraphics;

namespace Xamarin.Forms.Platform.MacOS
{
	public abstract class FormsApplicationDelegate : NSApplicationDelegate
	{

		Application _application;
		IApplicationController _applicationController => _application as IApplicationController;
		bool _isSuspended;

		public abstract NSWindow MainWindow { get; }

		protected FormsApplicationDelegate()
		{
		}
		protected override void Dispose(bool disposing)
		{
			if (disposing && _application != null)
				_application.PropertyChanged -= ApplicationOnPropertyChanged;

			base.Dispose(disposing);
		}


		protected void LoadApplication(Application application)
		{
			if (application == null)
				throw new ArgumentNullException("application");

			Application.Current = application;
			_application = application;

			application.PropertyChanged += ApplicationOnPropertyChanged;
		}

		public override void DidFinishLaunching(Foundation.NSNotification notification)
		{
			if (MainWindow == null)
				throw new InvalidOperationException("Please provide a main window in your app");

			MainWindow.Display();
			MainWindow.MakeKeyAndOrderFront(NSApplication.SharedApplication);
			if (_application == null)
				throw new InvalidOperationException("You MUST invoke LoadApplication () before calling base.FinishedLaunching ()");

			SetMainPage();
			_applicationController?.SendStart();
		}

		public override void DidBecomeActive(Foundation.NSNotification notification)
		{
			// applicationDidBecomeActive
			// execute any OpenGL ES drawing calls
			if (_application != null && _isSuspended)
			{
				_isSuspended = false;
				_applicationController?.SendResume();
			}

		}

		public override async void DidResignActive(Foundation.NSNotification notification)
		{
			// applicationWillResignActive
			if (_application != null)
			{
				_isSuspended = true;
				await _applicationController?.SendSleepAsync();
			}

		}

		void ApplicationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Application.MainPage))
				UpdateMainPage();
		}

		void SetMainPage()
		{
			UpdateMainPage();
		}

		void UpdateMainPage()
		{
			if (_application.MainPage == null)
				return;

			var platformRenderer = (PlatformRenderer)MainWindow.ContentViewController;
			MainWindow.ContentViewController = _application.MainPage.CreateViewController();
			if (platformRenderer != null)
				((IDisposable)platformRenderer.Platform).Dispose();
		}
	}
}
