using MDP.Network.Http;
using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    [Service<MDP.Network.Http.HttpClientHandler>(singleton: false, autoRegister: true)]
    public class MauiCredentialHandler : MDP.Network.Http.HttpClientHandler 
    {
        // Fields
        private readonly AuthenticationManager _authenticationManager = null;

        private readonly AuthenticationTokenManager _authenticationTokenManager = null;


        // Constructors
        public MauiCredentialHandler(AuthenticationManager authenticationManager, AuthenticationTokenManager authenticationTokenManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authenticationManager);
            ArgumentNullException.ThrowIfNull(authenticationTokenManager);

            #endregion

            // Default
            _authenticationManager = authenticationManager;
            _authenticationTokenManager = authenticationTokenManager;
        }


        // Methods
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            #region Contracts

            if (request == null) throw new ArgumentException($"{nameof(request)}=null");

            #endregion

            // Send
            HttpResponseMessage response = null;
            {
                // AuthenticationToken
                var authenticationToken = await _authenticationTokenManager.GetAsync();
                if (authenticationToken == null) return await base.SendAsync(request, cancellationToken);

                // SendAsync
                {
                    // Headers
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationToken.AccessToken);
                }
                response = await base.SendAsync(request, cancellationToken);
                if (response == null) throw new InvalidOperationException($"{nameof(response)}=null");
                if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized) return response;
            }

            // Re-Send
            {
                // Refresh
                await _authenticationManager.RefreshAsync();

                // AuthenticationToken
                var authenticationToken = await _authenticationTokenManager.GetAsync();
                if (authenticationToken == null) return response;

                // SendAsync
                var newRequest = await request.CloneAsync();
                if (authenticationToken != null)
                {
                    // Headers
                    newRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticationToken.AccessToken);
                }
                return await base.SendAsync(newRequest, cancellationToken);
            }
        }
    }
}
