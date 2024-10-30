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
    public class AuthenticateTokenManager
    {
        // Fields
        private bool _isCached = false;

        private DateTime _expireTime { get; set; } = DateTime.MinValue;

        private AuthenticateToken _authenticateToken = null;


        // Methods
        public async Task<AuthenticateToken> GetAsync()
        {
            // Require
            if (_isCached == true)
            {
                // Cache
                if (_expireTime <= DateTime.Now)
                {
                    _isCached = true;
                    _expireTime = DateTime.MaxValue;
                    _authenticateToken = null;
                }

                // Return
                return _authenticateToken;
            }

            // AuthenticateTokenString   
            var authenticateTokenString = await SecureStorage.GetAsync(this.GetType().FullName);
            if (string.IsNullOrEmpty(authenticateTokenString) == true)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _authenticateToken = null;

                // Return
                return _authenticateToken;
            }

            // AuthenticateToken
            var authenticateToken = JsonSerializer.Deserialize<AuthenticateToken>(authenticateTokenString);
            if (authenticateToken == null)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _authenticateToken = null;

                // Return
                return _authenticateToken;
            }

            // ExpireTime
            var expireTime = authenticateToken.RefreshTokenExpireTime;
            if (expireTime <= DateTime.Now)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _authenticateToken = null;

                // Return
                return _authenticateToken;
            }

            // Cache
            _isCached = true;
            _expireTime = expireTime;
            _authenticateToken = authenticateToken;

            // Return
            return _authenticateToken;
        }

        public async Task SetAsync(AuthenticateToken authenticateToken)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authenticateToken);

            #endregion

            // AuthenticateTokenString
            var authenticateTokenString = JsonSerializer.Serialize(authenticateToken);
            if (string.IsNullOrEmpty(authenticateTokenString) == true) throw new InvalidOperationException($"{nameof(authenticateTokenString)}=null");

            // SetAsync
            await SecureStorage.SetAsync(this.GetType().FullName, authenticateTokenString);

            // Cache
            _isCached = true;
            _expireTime = authenticateToken.RefreshTokenExpireTime;
            _authenticateToken = authenticateToken;

            // Raise
            this.OnTokenChanged(authenticateToken);

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
            _authenticateToken = null;

            // Raise
            this.OnTokenChanged(_authenticateToken);

            // Return
            return Task.CompletedTask;
        }


        // Events
        public event Action<AuthenticateToken> TokenChanged;
        protected void OnTokenChanged(AuthenticateToken authenticateToken = null)
        {
            // Raise
            var handler = this.TokenChanged;
            if (handler != null)
            {
                handler(authenticateToken);
            }
        }
    }
}
