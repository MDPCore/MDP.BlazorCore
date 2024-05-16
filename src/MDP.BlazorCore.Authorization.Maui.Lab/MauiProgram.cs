using MDP.BlazorCore.Authorization.Lab;
using MDP.BlazorCore.Authorization.Lab.Layout;

namespace MDP.BlazorCore.Authorization.Maui.Lab
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
