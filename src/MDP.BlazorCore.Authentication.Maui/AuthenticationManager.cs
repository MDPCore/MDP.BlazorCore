using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MDP.BlazorCore.Authentication.Maui
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
        public Task LoginAsync(string scheme = null)
        {
            // AuthenticationProvider
            IAuthenticationProvider authenticationProvider = null;
            if (string.IsNullOrEmpty(scheme) == true)
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault();
            }
            else
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault(e => e.Name == scheme);
            }
            if (authenticationProvider == null) throw new InvalidOperationException($"{nameof(authenticationProvider)}=null");

            // LoginAsync
            return authenticationProvider.LoginAsync();
        }

        public Task LogoutAsync(string scheme = null)
        {
            // AuthenticationProvider
            IAuthenticationProvider authenticationProvider = null;
            if (string.IsNullOrEmpty(scheme) == true)
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault();
            }
            else
            {
                authenticationProvider = _authenticationProviderList.FirstOrDefault(e => e.Name == scheme);
            }
            if (authenticationProvider == null) throw new InvalidOperationException($"{nameof(authenticationProvider)}=null");

            // LogoutAsync
            return authenticationProvider.LogoutAsync();
        }
    }
}