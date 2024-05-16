using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Authentication.Maui
{
    public class OAuthSSOHandler : IDisposable
    {
        // Fields
        public readonly object _syncRoot = new object();

        public readonly OAuthSSOOptions _authOptions = null;

        public readonly IHostEnvironment _hostEnvironment = null;

        public HttpClient _httpClient = null;


        // Constructors
        public OAuthSSOHandler(OAuthSSOOptions authOptions, IHostEnvironment hostEnvironment)
        {
            #region Contracts

            if (authOptions == null) throw new ArgumentException($"{nameof(authOptions)}=null");
            if (hostEnvironment == null) throw new ArgumentException($"{nameof(hostEnvironment)}=null");

            #endregion

            // Default
            _authOptions = authOptions;
            _hostEnvironment = hostEnvironment;
        }

        public void Dispose()
        {
            // HttpClient
            HttpClient httpClient = null;
            lock (_syncRoot)
            {
                // Remove
                httpClient = _httpClient;
                _httpClient = null;
            }
            if (httpClient != null) httpClient.Dispose();
        }


        // Properties
        private HttpClient Backchannel
        {
            get { 

                // Sync
                lock (_syncRoot)
                {
                    // Create
                    if (_httpClient == null)
                    {
                        if (_hostEnvironment.IsDevelopment() == true)
                        {
                            _httpClient = new HttpClient(new HttpClientHandler()
                            {
                                ServerCertificateCustomValidationCallback = (HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors) => true
                            });
                        }
                        else
                        {
                            _httpClient = new HttpClient();
                        }
                    }

                    // Return
                    return _httpClient;
                }
            }
        }


        // Methods
        public async Task<string> AuthenticateAsync()
        {
            // CodeVerifier
            var codeVerifier = CreateCodeVerifier();
            if (string.IsNullOrEmpty(codeVerifier) == true) throw new InvalidOperationException($"{nameof(codeVerifier)}=null");

            // CodeChallenge
            var codeChallenge = CreateCodeChallenge(codeVerifier);
            if (string.IsNullOrEmpty(codeChallenge) == true) throw new InvalidOperationException($"{nameof(codeChallenge)}=null");

            // State
            var state = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(state) == true) throw new InvalidOperationException($"{nameof(state)}=null");

            // RedirectUri
            var redirectUri = $"{_authOptions.CallbackEndpoint}";
            if (string.IsNullOrEmpty(redirectUri) == true) throw new InvalidOperationException($"{nameof(redirectUri)}=null");

            // ChallengeUrl
            var challengeUrl = $"{_authOptions.AuthorizationEndpoint}?client_id={_authOptions.ClientId}&response_type=code&scope=openid profile email&redirect_uri={redirectUri}&code_challenge={codeChallenge}&code_challenge_method=S256&state={state}";
            if (string.IsNullOrEmpty(challengeUrl) == true) throw new InvalidOperationException($"{nameof(challengeUrl)}=null");

            // Authenticate
            var authenticateResult = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(challengeUrl),
                new Uri(redirectUri)
            );
            if (authenticateResult == null) throw new InvalidOperationException($"{nameof(authenticateResult)}=null");

            // CallbackState
            string callbackState = null;
            if (authenticateResult.Properties.ContainsKey("state") == true)
            {
                callbackState = authenticateResult.Properties["state"];
            }
            if (string.IsNullOrEmpty(callbackState) == true) throw new InvalidOperationException($"{nameof(callbackState)}=null");
            if (callbackState != state) throw new InvalidOperationException($"{nameof(callbackState)}!={nameof(state)}");

            // AuthenticateCode
            string authenticateCode = null;
            if (authenticateResult.Properties.ContainsKey("code") == true)
            {
                authenticateCode = authenticateResult.Properties["code"];
            }
            if (string.IsNullOrEmpty(authenticateCode) == true) throw new InvalidOperationException($"{nameof(authenticateCode)}=null");

            // AuthenticateToken
            var authenticateToken = await this.ExchangeCodeAsync(authenticateCode, codeVerifier);
            if (string.IsNullOrEmpty(authenticateToken) == true) throw new InvalidOperationException($"{nameof(authenticateToken)}=null");

            // Return
            return authenticateToken;
        }

        private string CreateCodeVerifier()
        {
            // CodeVerifierBytes
            var codeVerifierBytes = new byte[32];
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(codeVerifierBytes);
            }

            // CodeVerifier
            var codeVerifier = Convert.ToBase64String(codeVerifierBytes)
                                      .TrimEnd('=')
                                      .Replace('+', '-')
                                      .Replace('/', '_');
            if (string.IsNullOrEmpty(codeVerifier) == true) throw new InvalidOperationException($"{nameof(codeVerifier)}=null");

            // Return
            return codeVerifier;
        }

        private string CreateCodeChallenge(string codeVerifier)
        {
            #region Contracts

            if (string.IsNullOrEmpty(codeVerifier) == true) throw new ArgumentException($"{nameof(codeVerifier)}=null");

            #endregion

            // CodeChallengeBytes
            byte[] codeChallengeBytes = null;
            using (var sha256 = SHA256.Create())
            {
                codeChallengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            }
            if (codeChallengeBytes == null) throw new InvalidOperationException($"{nameof(codeChallengeBytes)}=null");

            // CodeChallenge
            var codeChallenge = Convert.ToBase64String(codeChallengeBytes)
                                       .TrimEnd('=')
                                       .Replace('+', '-')
                                       .Replace('/', '_');
            if (string.IsNullOrEmpty(codeChallenge) == true) throw new InvalidOperationException($"{nameof(codeChallenge)}=null");

            // Return
            return codeChallenge;
        }

        private async Task<string> ExchangeCodeAsync(string authenticateCode, string codeVerifier)
        {
            #region Contracts

            if (string.IsNullOrEmpty(authenticateCode) == true) throw new ArgumentException($"{nameof(authenticateCode)}=null");
            if (string.IsNullOrEmpty(codeVerifier) == true) throw new ArgumentException($"{nameof(codeVerifier)}=null");

            #endregion

            // Request
            var request = new HttpRequestMessage(HttpMethod.Post, _authOptions.TokenEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"grant_type", "authorization_code"},
                {"client_id", _authOptions.ClientId},
                {"redirect_uri", _authOptions.CallbackEndpoint},
                {"code", authenticateCode},
                {"code_verifier", codeVerifier}
            });

            // Response
            var response = await this.Backchannel.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content) == false) throw new HttpRequestException(content);
                if (string.IsNullOrEmpty(content) == true) throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            // Payload
            using (var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
            {
                // TokenType
                var tokenType = payload.RootElement.GetProperty("token_type").GetString();
                if (string.IsNullOrEmpty(tokenType) == true) throw new InvalidOperationException($"{nameof(tokenType)}=null");
                if (tokenType.Equals("Bearer", StringComparison.OrdinalIgnoreCase) == false) throw new InvalidOperationException($"{nameof(tokenType)}={tokenType}");

                // AccessToken
                var accessToken = payload.RootElement.GetProperty("access_token").GetString();
                if (string.IsNullOrEmpty(accessToken) == true) throw new InvalidOperationException($"{nameof(accessToken)}=null");

                // Return
                return accessToken;
            }
        }


        public async Task<string> GetAccessTokenAsync(string authenticateToken)
        {
            #region Contracts

            if (string.IsNullOrEmpty(authenticateToken) == true) throw new ArgumentException($"{nameof(authenticateToken)}=null");

            #endregion

            // Request
            var request = new HttpRequestMessage(HttpMethod.Post, _authOptions.TokenEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"grant_type", "urn:ietf:params:oauth:grant-type:token-exchange"},
                {"subject_token", authenticateToken},
                {"subject_token_type", "urn:ietf:params:oauth:token-type:access_token"},
                {"requested_token_type", "urn:ietf:params:oauth:token-type:jwt"}
            });

            // Response
            var response = await this.Backchannel.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content) == false) throw new HttpRequestException(content);
                if (string.IsNullOrEmpty(content) == true) throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            // Payload
            using (var payload = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
            {
                // TokenType
                var tokenType = payload.RootElement.GetProperty("token_type").GetString();
                if (string.IsNullOrEmpty(tokenType) == true) throw new InvalidOperationException($"{nameof(tokenType)}=null");
                if (tokenType.Equals("Bearer", StringComparison.OrdinalIgnoreCase) == false) throw new InvalidOperationException($"{nameof(tokenType)}={tokenType}");

                // IssuedTokenType
                var issuedTokenType = payload.RootElement.GetProperty("issued_token_type").GetString();
                if (string.IsNullOrEmpty(issuedTokenType) == true) throw new InvalidOperationException($"{nameof(issuedTokenType)}=null");
                if (issuedTokenType.Equals("urn:ietf:params:oauth:token-type:jwt", StringComparison.OrdinalIgnoreCase) == false) throw new InvalidOperationException($"{nameof(issuedTokenType)}={issuedTokenType}");

                // AccessToken
                var accessToken = payload.RootElement.GetProperty("access_token").GetString();
                if (string.IsNullOrEmpty(accessToken) == true) throw new InvalidOperationException($"{nameof(accessToken)}=null");

                // Return
                return accessToken;
            }
        }

        public async Task<ClaimsIdentity> GetUserInformationAsync(string authenticateToken)
        {
            #region Contracts

            if (string.IsNullOrEmpty(authenticateToken) == true) throw new ArgumentException($"{nameof(authenticateToken)}=null");

            #endregion

            // Request
            var request = new HttpRequestMessage(HttpMethod.Post, _authOptions.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticateToken);

            // Response
            var response = await this.Backchannel.SendAsync(request);
            if (response.IsSuccessStatusCode == false)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(content) == false) throw new HttpRequestException(content);
                if (string.IsNullOrEmpty(content) == true) throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct.");
            }

            // UserInfoData
            UserInfoData userInfoData = null;
            try
            {
                userInfoData = JsonSerializer.Deserialize<UserInfoData>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions
                {
                    Converters = { new ClaimsConverter() }
                });
            }
            catch (Exception exception)
            {
                throw new HttpRequestException(exception.Message);
            }
            if (userInfoData == null) throw new InvalidOperationException($"{nameof(userInfoData)}=null");

            // ClaimsIdentity
            var claimsIdentity = userInfoData.GetClaimsIdentity();
            if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");

            // Return
            return claimsIdentity;
        }


        // Class
        private class UserInfoData : Dictionary<string, object>
        {
            // Methods
            public ClaimsIdentity GetClaimsIdentity()
            {
                // ClaimList
                var claimList = new List<Claim>();
                foreach (var claim in this)
                {
                    // Require
                    if (string.IsNullOrEmpty(claim.Key) == true) continue;
                    if (claim.Value == null) continue;
                    if (claim.Key == AuthenticationClaimTypes.AuthenticationType) continue;

                    // Add Claim
                    if (claim.Value is string)
                    {
                        var claimValue = claim.Value as string;
                        {
                            claimList.Add(new Claim(claim.Key, claim.Value as string));
                        }
                    }

                    // Add ClaimArray
                    if (claim.Value is string[])
                    {
                        foreach (var claimValue in claim.Value as string[])
                        {
                            claimList.Add(new Claim(claim.Key, claimValue));
                        }
                    }
                }

                // AuthenticationType
                var authenticationType = this.FirstOrDefault(o => o.Key == AuthenticationClaimTypes.AuthenticationType).Value as string;
                if (string.IsNullOrEmpty(authenticationType) == true) throw new InvalidOperationException($"{nameof(authenticationType)}=null");

                // Return
                return new ClaimsIdentity(claimList, authenticationType);
            }
        }

        private class ClaimsConverter : JsonConverter<object>
        {
            // Methods
            public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                #region Contracts

                if (typeToConvert == null) throw new ArgumentException($"{nameof(typeToConvert)}=null");
                if (options == null) throw new ArgumentException($"{nameof(options)}=null");

                #endregion

                // String
                if (reader.TokenType == JsonTokenType.String) return reader.GetString();

                // StringArray
                if (reader.TokenType == JsonTokenType.StartArray) return JsonSerializer.Deserialize<string[]>(ref reader, options);

                // Object
                return JsonSerializer.Deserialize<object>(ref reader, options);
            }

            public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
            {
                #region Contracts

                if (writer == null) throw new ArgumentException($"{nameof(writer)}=null");
                if (options == null) throw new ArgumentException($"{nameof(options)}=null");

                #endregion

                // Write
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }
    }
}
