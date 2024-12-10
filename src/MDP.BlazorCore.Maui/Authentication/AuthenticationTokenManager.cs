using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public class AuthenticationTokenManager
    {
        // Fields
        private bool _isCached = false;

        private DateTime _expireTime { get; set; } = DateTime.MinValue;

        private AuthenticationToken _authenticationToken = null;


        // Methods
        public async Task<AuthenticationToken> GetAsync()
        {
            // Require
            if (_isCached == true)
            {
                // Cache
                if (_expireTime <= DateTime.Now)
                {
                    _isCached = true;
                    _expireTime = DateTime.MaxValue;
                    _authenticationToken = null;
                }

                // Return
                return _authenticationToken;
            }

            // AuthenticationTokenString   
            var authenticationTokenString = await SecureStorage.GetAsync(this.GetType().FullName);
            if (string.IsNullOrEmpty(authenticationTokenString) == true)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _authenticationToken = null;

                // Return
                return _authenticationToken;
            }

            // AuthenticationToken
            var authenticationToken = JsonSerializer.Deserialize<AuthenticationToken>(authenticationTokenString);
            if (authenticationToken == null)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _authenticationToken = null;

                // Return
                return _authenticationToken;
            }

            // ExpireTime
            var expireTime = authenticationToken.RefreshTokenExpireTime;
            if (expireTime <= DateTime.Now)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _authenticationToken = null;

                // Return
                return _authenticationToken;
            }

            // Cache
            _isCached = true;
            _expireTime = expireTime;
            _authenticationToken = authenticationToken;

            // Return
            return _authenticationToken;
        }

        public async Task SetAsync(AuthenticationToken authenticationToken)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authenticationToken);

            #endregion

            // AuthenticationTokenString
            var authenticationTokenString = JsonSerializer.Serialize(authenticationToken);
            if (string.IsNullOrEmpty(authenticationTokenString) == true) throw new InvalidOperationException($"{nameof(authenticationTokenString)}=null");

            // SetAsync
            await SecureStorage.SetAsync(this.GetType().FullName, authenticationTokenString);

            // Cache
            _isCached = true;
            _expireTime = authenticationToken.RefreshTokenExpireTime;
            _authenticationToken = authenticationToken;

            // Raise
            this.OnTokenChanged(authenticationToken);

            // Return
            return;
        }

        public Task RemoveAsync()
        {
            // RemoveAsync
            SecureStorage.Remove(this.GetType().FullName);

            // Cache
            _isCached = true;
            _expireTime = DateTime.MaxValue;
            _authenticationToken = null;

            // Raise
            this.OnTokenChanged(_authenticationToken);

            // Return
            return Task.CompletedTask;
        }


        // Events
        public event Action<AuthenticationToken> TokenChanged;
        protected void OnTokenChanged(AuthenticationToken authenticationToken = null)
        {
            // Raise
            var handler = this.TokenChanged;
            if (handler != null)
            {
                handler(authenticationToken);
            }
        }
    }
}
