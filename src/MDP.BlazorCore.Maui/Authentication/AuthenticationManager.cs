using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public class AuthenticationManager
    {
        // Fields
        private readonly IList<IAuthenticationProvider> _authenticationProviderList = null;

        private readonly AuthenticationTokenManager _authenticationTokenManager = null;

        private readonly AuthenticationStateManager _authenticationStateManager = null;


        // Constructors
        public AuthenticationManager(IList<IAuthenticationProvider> authenticationProviderList, AuthenticationTokenManager authenticationTokenManager, AuthenticationStateManager authenticationStateManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authenticationProviderList);
            ArgumentNullException.ThrowIfNull(authenticationTokenManager);
            ArgumentNullException.ThrowIfNull(authenticationStateManager);

            #endregion

            // Default
            _authenticationProviderList = authenticationProviderList;
            _authenticationTokenManager = authenticationTokenManager;
            _authenticationStateManager = authenticationStateManager;
        }


        // Methods
        public async Task LoginAsync(string authenticationScheme = null)
        {
            // Clear
            await _authenticationTokenManager.RemoveAsync();
            await _authenticationStateManager.RemoveAsync();

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
            var authenticationToken = await authenticationProvider.LoginAsync();
            if (authenticationToken == null) return;

            // GetUserInformationAsync
            var claimsIdentity = await authenticationProvider.GetUserInformationAsync(authenticationToken.AccessToken);
            if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");

            // Save
            await _authenticationTokenManager.SetAsync(authenticationToken);
            await _authenticationStateManager.SetAsync(new ClaimsPrincipal(claimsIdentity), authenticationToken.RefreshTokenExpireTime);
        }

        public async Task LogoutAsync(string authenticationScheme = null)
        {
            // Clear
            await _authenticationTokenManager.RemoveAsync();
            await _authenticationStateManager.RemoveAsync();

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
            await authenticationProvider.LogoutAsync();
        }

        public async Task RefreshAsync(string authenticationScheme = null)
        {
            // Execute
            try
            {
                // Require
                var authenticationToken = await _authenticationTokenManager.GetAsync();
                if (authenticationToken == null) throw new InvalidOperationException($"{nameof(authenticationToken)}=null");

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

                // RefreshAsync
                authenticationToken = await authenticationProvider.RefreshAsync(authenticationToken.RefreshToken);
                if (authenticationToken == null) throw new InvalidOperationException($"{nameof(authenticationToken)}=null");

                // GetUserInformationAsync
                var claimsIdentity = await authenticationProvider.GetUserInformationAsync(authenticationToken.AccessToken);
                if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");

                // Save
                await _authenticationTokenManager.SetAsync(authenticationToken);
                await _authenticationStateManager.SetAsync(new ClaimsPrincipal(claimsIdentity), authenticationToken.RefreshTokenExpireTime);
            }
            catch (Exception)
            {
                // Clear
                await _authenticationTokenManager.RemoveAsync();
                await _authenticationStateManager.RemoveAsync();
            }
        }

        public async Task CancelAsync(string authenticationScheme = null)
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

            // CancelAsync
            await authenticationProvider.CancelAsync();
        }
    }
}