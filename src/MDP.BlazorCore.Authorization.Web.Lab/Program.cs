using MDP.BlazorCore.Authorization.Components;
using MDP.BlazorCore.Authorization.Components.Layout;

namespace MDP.BlazorCore.Authorization.Web.Lab
{
    public class Program
    {
        // Methods
        public static void Main()
        {
            // Host
            MDP.BlazorCore.Web.Host.Run<App>(typeof(MainLayout));
        }
    }
}
