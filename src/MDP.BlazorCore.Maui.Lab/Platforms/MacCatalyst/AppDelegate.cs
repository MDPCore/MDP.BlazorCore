using Foundation;
using Microsoft.Maui;

namespace MDP.BlazorCore.Maui.Lab
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiApplicationDelegate
    {
        protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
