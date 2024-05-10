using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MDP.BlazorCore.Authentication
{
    [Service<AuthorizationManager>(singleton: true, autoRegister: true)]
    public class AuthorizationManager
    {
        // Fields
        private readonly IList<IAuthorizationProviderBuilder> _authorizationProviderBuilderList = null;


        // Constructors
        public AuthorizationManager(IList<IAuthorizationProviderBuilder> authorizationProviderBuilderList)
        {
            #region Contracts

            if (authorizationProviderBuilderList == null) throw new ArgumentException($"{nameof(authorizationProviderBuilderList)}=null");

            #endregion

            // Default
            _authorizationProviderBuilderList = authorizationProviderBuilderList;
        }


        // Methods
        public Task LoginAsync(IServiceProvider serviceProvider, string scheme = null, string returnUrl = null)
        {
            #region Contracts

            if (serviceProvider == null) throw new ArgumentException($"{nameof(serviceProvider)}=null");

            #endregion

            // AuthorizationProviderBuilder
            IAuthorizationProviderBuilder authorizationProviderBuilder = null;
            if (string.IsNullOrEmpty(scheme) == true)
            {
                authorizationProviderBuilder = _authorizationProviderBuilderList.FirstOrDefault();
            }
            else
            {
                authorizationProviderBuilder = _authorizationProviderBuilderList.FirstOrDefault(e => e.Name == scheme);
            }
            if (authorizationProviderBuilder == null) throw new InvalidOperationException($"{nameof(authorizationProviderBuilder)}=null");

            // AuthorizationProvider
            using (var authorizationProvider = authorizationProviderBuilder.BuildProvider(serviceProvider))
            {
                // Require
                if (authorizationProvider == null) throw new InvalidOperationException($"{nameof(authorizationProvider)}=null");

                // LoginAsync
                return authorizationProvider.LoginAsync(returnUrl);
            }           
        }
    }
}