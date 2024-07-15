using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MDP.BlazorCore.Maui
{
    public class AuthenticationManager
    {
        // Fields
        private readonly IList<IAuthenticationProvider> _authenticationProviderList = null;


        // Constructors
        public AuthenticationManager(IList<IAuthenticationProvider> authenticationProviderList)
        {
            #region Contracts

            if (authenticationProviderList == null) throw new ArgumentException($"{nameof(authenticationProviderList)}=null");

            #endregion

            // Default
            _authenticationProviderList = authenticationProviderList;
        }


        // Methods
        public Task LoginAsync(string authenticationScheme = null)
        {
            // AuthenticationProvider
            IAuthenticationProvider authenticationProvider = null;
            if (string.IsNullOrEmpty(authenticationScheme) == true)
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault();
            }
            else
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault(e => e.AuthenticationScheme == authenticationScheme);
            }
            if (authenticationProvider == null) throw new InvalidOperationException($"{nameof(authenticationProvider)}=null");

            // LoginAsync
            return authenticationProvider.LoginAsync();
        }

        public Task LogoutAsync(string authenticationScheme = null)
        {
            // AuthenticationProvider
            IAuthenticationProvider authenticationProvider = null;
            if (string.IsNullOrEmpty(authenticationScheme) == true)
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault();
            }
            else
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault(e => e.AuthenticationScheme == authenticationScheme);
            }
            if (authenticationProvider == null) throw new InvalidOperationException($"{nameof(authenticationProvider)}=null");

            // LogoutAsync
            return authenticationProvider.LogoutAsync();
        }

        public Task RefreshAsync(string authenticationScheme = null)
        {
            // AuthenticationProvider
            IAuthenticationProvider authenticationProvider = null;
            if (string.IsNullOrEmpty(authenticationScheme) == true)
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault();
            }
            else
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault(e => e.AuthenticationScheme == authenticationScheme);
            }
            if (authenticationProvider == null) return Task.CompletedTask;

            // RefreshAsync
            return authenticationProvider.RefreshAsync();
        }
    }
}