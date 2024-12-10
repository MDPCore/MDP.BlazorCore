using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public interface IAuthenticationProvider
    {
        // Properties
        string AuthenticationScheme { get; }


        // Methods
        Task<AuthenticationToken> LoginAsync();

        Task<AuthenticationToken> RefreshAsync(string refreshToken);

        Task<ClaimsIdentity> GetUserInformationAsync(string accessToken);

        Task LogoutAsync();

        Task CancelAsync();
    }
}
