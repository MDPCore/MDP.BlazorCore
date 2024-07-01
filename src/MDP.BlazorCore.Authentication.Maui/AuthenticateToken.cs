using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class AuthenticateToken
    {
        // Constructors
        public AuthenticateToken(string tokenType, string accessToken, string refreshToken)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNullOrEmpty(tokenType);
            ArgumentNullException.ThrowIfNullOrEmpty(accessToken);
            ArgumentNullException.ThrowIfNullOrEmpty(refreshToken);

            #endregion

            // Default
            this.TokenType = tokenType;
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
        }


        // Properties
        public string TokenType { get; private set; } = null;

        public string AccessToken { get; private set; } = null;

        public string RefreshToken { get; private set; } = null;
    }
}
