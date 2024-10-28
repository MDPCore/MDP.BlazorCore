using MDP.BlazorCore.Authentication.Components;
using MDP.BlazorCore.Authentication.Components.Layout;

namespace MDP.BlazorCore.Authentication.Web.Lab
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
