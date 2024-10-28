using MDP.BlazorCore.Components;
using MDP.BlazorCore.Components.Layout;

namespace MDP.BlazorCore.Web.Lab
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
