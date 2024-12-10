using Foundation;
using MDP.BlazorCore.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using UIKit;

namespace MDP.BlazorCore.Authentication.Maui.Lab
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        // Methods
        protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

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
