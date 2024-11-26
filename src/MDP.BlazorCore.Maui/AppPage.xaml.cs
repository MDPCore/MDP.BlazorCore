using Microsoft.AspNetCore.Components.WebView;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
#if ANDROID
using Android.OS;
using Android.Views;
using Android.Content.Res;
#elif IOS
using UIKit;
#endif

namespace MDP.BlazorCore.Maui
{
    public partial class AppPage : ContentPage
    {
        // Fields
        private string _safeAreaColor = "#000000";


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

        private async void BlazorWebView_UrlLoading(object sender, UrlLoadingEventArgs e)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(sender);
            ArgumentNullException.ThrowIfNull(e);

            #endregion
           
            // Require
            if (e.Url.Host.Equals("0.0.0.0", StringComparison.OrdinalIgnoreCase) == true) { e.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView; return; }
            if (e.Url.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) == true) { e.UrlLoadingStrategy = UrlLoadingStrategy.OpenInWebView; return; }

            // External
            {
                // Cancel
                e.UrlLoadingStrategy = UrlLoadingStrategy.CancelLoad;

                // Launcher
                await Launcher.OpenAsync(e.Url);
            }
        }
    }
}
