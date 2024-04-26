using MDP.BlazorCore.Authentication.Lab;
using MDP.BlazorCore.Authentication.Lab.Layout;

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
