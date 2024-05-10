using MDP.BlazorCore.Authentication.Lab;
using MDP.BlazorCore.Authentication.Lab.Layout;

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
