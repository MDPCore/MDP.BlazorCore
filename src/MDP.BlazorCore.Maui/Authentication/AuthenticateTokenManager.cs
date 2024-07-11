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
        private readonly object _syncRoot = new object();

        private bool _isCached = false;

        private AuthenticateToken _authenticateToken = null;


        // Methods
        public Task<AuthenticateToken> GetAsync()
        {
            // Sync
            lock (_syncRoot)
            {
                // Require
                if (_isCached == true) return Task.FromResult(_authenticateToken);

                // GetAsync   
                var authenticateTokenString = SecureStorage.GetAsync(this.GetType().FullName).GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(authenticateTokenString) == true)
                {
                    // Cache
                    _isCached = true;
                    _authenticateToken = null;

                    // Return
                    return Task.FromResult(_authenticateToken);
                }

                // AuthenticateToken
                var authenticateToken = JsonSerializer.Deserialize<AuthenticateToken>(authenticateTokenString);
                if (authenticateToken == null)
                {
                    // Cache
                    _isCached = true;
                    _authenticateToken = null;

                    // Return
                    return Task.FromResult(_authenticateToken);
                }

                // Cache
                _isCached = true;
                _authenticateToken = authenticateToken;

                // Return
                return Task.FromResult(_authenticateToken);
            }
        }

        public Task SetAsync(AuthenticateToken authenticateToken)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authenticateToken);

            #endregion

            // AuthenticateTokenString
            var authenticateTokenString = JsonSerializer.Serialize(authenticateToken);
            if (string.IsNullOrEmpty(authenticateTokenString) == true) throw new InvalidOperationException($"{nameof(authenticateTokenString)}=null");

            // Sync
            lock (_syncRoot)
            {
                // SetAsync
                SecureStorage.SetAsync(this.GetType().FullName, authenticateTokenString).GetAwaiter().GetResult();

                // Cache
                _isCached = true;
                _authenticateToken = authenticateToken;
            }

            // Return
            return Task.CompletedTask;
        }

        public Task RemoveAsync()
        {
            // AuthenticateToken
            AuthenticateToken authenticateToken = null;

            // Sync
            lock (_syncRoot)
            {
                // RemoveAsync
                SecureStorage.Remove(this.GetType().FullName);

                // Cache
                _isCached = true;
                _authenticateToken = authenticateToken;
            }

            // Return
            return Task.CompletedTask;
        }
    }
}
