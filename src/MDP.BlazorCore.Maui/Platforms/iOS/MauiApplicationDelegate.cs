using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIKit;

namespace MDP.BlazorCore.Maui
{
    public abstract class MauiApplicationDelegate : Microsoft.Maui.MauiUIApplicationDelegate
    {
        // Methods
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            // ActivationManager
            var activationManager = IPlatformApplication.Current.Services.GetService<ActivationManager>();
            if (activationManager == null) return false;

            // HandleUrl
            var result = activationManager.HandleUrl(url.AbsoluteString);
            if (result == true) return true;

            // Base
            return base.OpenUrl(app, url, options);
        }
    }
}
