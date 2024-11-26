using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;

#if ANDROID
using Android.OS;
using Android.Views;
using Android.Content.Res;
using Android.Content;
using Android.Net;

#elif IOS
using UIKit;
using Foundation;
#endif

namespace MDP.BlazorCore.Maui
{
    public partial class AppPage : ContentPage
    {
        // Fields
        private string _safeAreaColor = "#FFFFFF";


        // Constructors
        public AppPage()
        {
            // Initialize
            InitializeComponent();

            // Loaded
            this.Loaded += this.AppPage_Loaded;
        }


        // Methods
        protected override void OnAppearing()
        {
            // Base
            base.OnAppearing();            

            // UrlLoading
            this.blazorWebView.UrlLoading += this.BlazorWebView_UrlLoading;
        }

        protected override void OnDisappearing()
        {
            // Base
            base.OnDisappearing();

            // UrlLoading
            this.blazorWebView.UrlLoading -= this.BlazorWebView_UrlLoading;
        }

        private void SetSafeArea(string color)
        {
#if ANDROID
            // Android
            try
            {
                // Window
                var window = (this.Handler?.MauiContext?.Context as Android.App.Activity)?.Window;
                if (window == null) return;

                // Resources
                var resources = (this.Handler?.MauiContext?.Context as Android.App.Activity)?.Resources;
                if (resources == null) return;

                // SystemUI
                if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
                {
                    window.DecorView.SystemUiVisibility = (StatusBarVisibility)
                    (
                        SystemUiFlags.LayoutStable | 
                        SystemUiFlags.LayoutHideNavigation
                    );
                }

                // Area 
                var statusBarHeight = 0;
                var statusBarHeightId = resources.GetIdentifier("status_bar_height", "dimen", "android");
                if (statusBarHeightId > 0 && resources.DisplayMetrics != null)
                {
                    statusBarHeight = (int)(resources.GetDimensionPixelSize(statusBarHeightId) / resources.DisplayMetrics.Density);
                }
                this.Padding = new Thickness(0, statusBarHeight, 0, 0);                

                // Color
                window.SetStatusBarColor(Android.Graphics.Color.ParseColor(color));
                window.SetNavigationBarColor(Android.Graphics.Color.Transparent);
                this.BackgroundColor = Microsoft.Maui.Graphics.Color.Parse(color);
                this.blazorWebView.BackgroundColor = Microsoft.Maui.Graphics.Color.Parse("#FFFFFF");
            }
            catch { }
#elif IOS
            // iOS
            try
            {
                // Window
                var window = UIApplication.SharedApplication.Delegate.GetWindow();
                if (window == null) return;

                // Area
                this.Padding = new Thickness(0, window.SafeAreaInsets.Top, 0, 0);

                // Color
                this.BackgroundColor = Microsoft.Maui.Graphics.Color.Parse(color);
                this.blazorWebView.BackgroundColor = Microsoft.Maui.Graphics.Color.Parse("#FFFFFF");
            }
            catch { }
#endif
        }


        // Handlers
        private void AppPage_Loaded(object sender, EventArgs e)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(e);

            #endregion

            // Loaded
            this.Loaded -= this.AppPage_Loaded;

            // SafeArea
            this.SetSafeArea(_safeAreaColor);
        }

        private void BlazorWebView_UrlLoading(object sender, UrlLoadingEventArgs e)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(e);

            #endregion
           
            // Require
            if (e.Url.Host.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase) == true) { e.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView; return; }
            if (e.Url.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) == true) { e.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView; return; }

            // Launcher
            {
#if ANDROID
                var intent = new Intent(Intent.ActionView);
                {
                    intent.SetData(Android.Net.Uri.Parse(e.Url.ToString()));
                }
                (this.Handler?.MauiContext?.Context as Android.App.Activity)?.StartActivity(intent);
#elif IOS
                UIApplication.SharedApplication.OpenUrl(e.Url, new UIApplicationOpenUrlOptions(), (success) => { });
#endif
            }
            e.UrlLoadingStrategy = UrlLoadingStrategy.CancelLoad;
        }
    }
}
