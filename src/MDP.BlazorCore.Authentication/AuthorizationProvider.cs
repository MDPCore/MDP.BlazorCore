using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication
{
    public interface IAuthorizationProvider : IDisposable
    {
        // Methods
        Task LoginAsync(string returnUrl = null);
    }
}
