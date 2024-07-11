using MDP.Network.Http;
using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace MDP.BlazorCore.Maui
{
    [Service<HttpDelegatingHandler>(singleton: true, autoRegister: true)]
    public class MauiCredentialHandler : HttpDelegatingHandler
    {
        // Fields
        private readonly AuthenticationManager _authenticationManager = null;

        private readonly AuthenticateTokenManager _authenticateTokenManager = null;


        // Constructors
        public MauiCredentialHandler(AuthenticationManager authenticationManager, AuthenticateTokenManager authenticateTokenManager)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(authenticationManager);
            ArgumentNullException.ThrowIfNull(authenticateTokenManager);

            #endregion

            // Default
            _authenticationManager = authenticationManager;
            _authenticateTokenManager = authenticateTokenManager;
        }


        // Methods
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            #region Contracts

            if (request == null) throw new ArgumentException($"{nameof(request)}=null");

            #endregion

            // AuthenticateToken
            var authenticateToken = await _authenticateTokenManager.GetAsync();
            if (authenticateToken != null)
            {
                // Headers
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authenticateToken.AccessToken);
            }

            // Return
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
