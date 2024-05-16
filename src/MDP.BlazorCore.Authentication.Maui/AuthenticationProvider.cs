using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Maui
{
    public interface IAuthenticationProvider : IDisposable
    {
        // Methods
        Task LoginAsync(string returnUrl = null);

        Task LogoutAsync(string returnUrl = null);
    }
}
