using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public class UrlSchemeActivity : Activity
    {
        // Methods
        protected override void OnCreate(Bundle savedInstanceState)
        {
            // Base
            base.OnCreate(savedInstanceState);

            // Handle
            if (this.Intent?.Data != null)
            {
                this.HandleIntent(Intent);
            }

            // Finish
            this.Finish();
        }

        protected override void OnNewIntent(Intent intent)
        {
            // Base
            base.OnNewIntent(intent);

            // Handle
            if (intent?.Data != null)
            {
                this.HandleIntent(Intent);
            }

            // Finish
            this.Finish();
        }

        private void HandleIntent(Intent intent)
        {
            // Url
            var url = intent?.DataString;
            if (string.IsNullOrEmpty(url) == true) return;

            // ActivationManager
            var activationManager = IPlatformApplication.Current.Services.GetService<ActivationManager>();
            if (activationManager == null) return;

            // HandleUrl
            var result = activationManager.HandleUrl(url);
            if (result == true) return;           
        }
    }
}
