using MDP.BlazorCore.Authentication.Components;
using MDP.BlazorCore.Authentication.Components.Layout;

namespace MDP.BlazorCore.Authentication.Maui.Lab
{
    public class MauiProgram
    {
        // Methods
        public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
        {
            // Host
            return MDP.BlazorCore.Maui.Host.CreateMauiApp<MauiProgram>(typeof(MainLayout));
        }
    }
}
