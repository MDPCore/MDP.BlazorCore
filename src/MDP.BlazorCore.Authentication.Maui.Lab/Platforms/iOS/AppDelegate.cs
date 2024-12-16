using Foundation;
using MDP.BlazorCore.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using UIKit;

namespace MDP.BlazorCore.Authentication.Maui.Lab
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiApplicationDelegate
    {
        // Methods
        protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
