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
        private readonly IList<IAuthenticationProviderBuilder> _authenticationProviderBuilderList = null;


        // Constructors
        public AuthenticationManager(IList<IAuthenticationProviderBuilder> authenticationProviderBuilderList)
        {
            #region Contracts

            if (authenticationProviderBuilderList == null) throw new ArgumentException($"{nameof(authenticationProviderBuilderList)}=null");

            #endregion

            // Default
            _authenticationProviderBuilderList = authenticationProviderBuilderList;
        }


        // Methods
        public Task LoginAsync(string scheme = null, string returnUrl = null)
        {
            // AuthenticationProviderBuilder
            IAuthenticationProviderBuilder authenticationProviderBuilder = null;
            if (string.IsNullOrEmpty(scheme) == true)
            {
                authenticationProviderBuilder = _authenticationProviderBuilderList.FirstOrDefault();
            }
            else
            {
                authenticationProviderBuilder = _authenticationProviderBuilderList.FirstOrDefault(e => e.Name == scheme);
            }
            if (authenticationProviderBuilder == null) throw new InvalidOperationException($"{nameof(authenticationProviderBuilder)}=null");

            // AuthenticationProvider
            using (var authenticationProvider = authenticationProviderBuilder.BuildProvider())
            {
                // Require
                if (authenticationProvider == null) throw new InvalidOperationException($"{nameof(authenticationProvider)}=null");

                // LoginAsync
                return authenticationProvider.LoginAsync(returnUrl);
            }           
        }

        public Task LogoutAsync(string scheme = null, string returnUrl = null)
        {
            // AuthenticationProviderBuilder
            IAuthenticationProviderBuilder authenticationProviderBuilder = null;
            if (string.IsNullOrEmpty(scheme) == true)
            {
                authenticationProviderBuilder = _authenticationProviderBuilderList.FirstOrDefault();
            }
            else
            {
                authenticationProviderBuilder = _authenticationProviderBuilderList.FirstOrDefault(e => e.Name == scheme);
            }
            if (authenticationProviderBuilder == null) throw new InvalidOperationException($"{nameof(authenticationProviderBuilder)}=null");

            // AuthenticationProvider
            using (var authenticationProvider = authenticationProviderBuilder.BuildProvider())
            {
                // Require
                if (authenticationProvider == null) throw new InvalidOperationException($"{nameof(authenticationProvider)}=null");

                // LogoutAsync
                return authenticationProvider.LogoutAsync(returnUrl);
            }
        }
    }
}