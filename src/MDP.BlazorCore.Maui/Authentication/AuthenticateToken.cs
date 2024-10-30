using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public class AuthenticateToken
    {
        // Constructors
        public AuthenticateToken
        (
            string tokenType, 
            string accessToken, 
            DateTime accessTokenExpireTime,
            string refreshToken,
            DateTime refreshTokenExpireTime
        )
        {
            #region Contracts

            ArgumentNullException.ThrowIfNullOrEmpty(tokenType);
            ArgumentNullException.ThrowIfNullOrEmpty(accessToken);
            ArgumentNullException.ThrowIfNullOrEmpty(refreshToken);

            #endregion

            // Default
            this.TokenType = tokenType;
            this.AccessToken = accessToken;
            this.AccessTokenExpireTime = accessTokenExpireTime;
            this.RefreshToken = refreshToken;
            this.RefreshTokenExpireTime = refreshTokenExpireTime;
        }


        // Properties
        public string TokenType { get; private set; } = null;

        public string AccessToken { get; private set; } = null;

        public DateTime AccessTokenExpireTime { get; private set; } = DateTime.MinValue;

        public string RefreshToken { get; private set; } = null;

        public DateTime RefreshTokenExpireTime { get; private set; } = DateTime.MinValue;
    }
}
