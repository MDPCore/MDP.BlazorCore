using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Maui
{
    public abstract class AuthenticateTokenManager
    {
        // Methods
        public abstract Task<AuthenticateToken> GetAsync();

        public abstract Task SetAsync(AuthenticateToken authenticateToken);

        public abstract Task RemoveAsync();
    }
}
