using MDP.Registration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace MDP.BlazorCore.Maui
{
    public class AuthenticationStateManager
    {
        // Fields
        private bool _isCached = false;

        private DateTime _expireTime { get; set; } = DateTime.MinValue;

        private ClaimsPrincipal _claimsPrincipal = null;


        // Methods
        public async Task<ClaimsPrincipal> GetAsync()
        {
            await Task.Delay(5000);
            // Require
            if (_isCached == true)
            {
                // Cache
                if (_expireTime <= DateTime.Now)
                {
                    _isCached = true;
                    _expireTime = DateTime.MaxValue;
                    _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
                }

                // Return
                return _claimsPrincipal;
            }

            // AuthenticatePrincipalString   
            var authenticatePrincipalString = await SecureStorage.GetAsync(this.GetType().FullName);
            if (string.IsNullOrEmpty(authenticatePrincipalString) == true)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

                // Return
                return _claimsPrincipal;
            }

            // AuthenticatePrincipal
            var authenticatePrincipal = JsonSerializer.Deserialize<AuthenticatePrincipal>(authenticatePrincipalString, new JsonSerializerOptions { Converters = { new ClaimConverter() } });
            if (authenticatePrincipal == null)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

                // Return
                return _claimsPrincipal;
            }

            // ClaimsPrincipal
            var claimsPrincipal = authenticatePrincipal.CreateClaimsPrincipal();
            if (claimsPrincipal == null)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

                // Return
                return _claimsPrincipal;
            }

            // ExpireTime
            var expireTime = authenticatePrincipal.ExpireTime;
            if (expireTime <= DateTime.Now)
            {
                // Cache
                _isCached = true;
                _expireTime = DateTime.MaxValue;
                _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

                // Return
                return _claimsPrincipal;
            }

            // Cache
            _isCached = true;
            _expireTime = expireTime;
            _claimsPrincipal = claimsPrincipal;

            // Return
            return _claimsPrincipal;
        }

        public async Task SetAsync(ClaimsPrincipal claimsPrincipal, DateTime expireTime)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(claimsPrincipal);

            #endregion

            // AuthenticatePrincipal
            var authenticatePrincipal = new AuthenticatePrincipal(claimsPrincipal, expireTime);

            // AuthenticatePrincipalString
            var authenticatePrincipalString = JsonSerializer.Serialize(authenticatePrincipal, new JsonSerializerOptions
            {
                Converters = { new ClaimConverter() }
            });
            if (string.IsNullOrEmpty(authenticatePrincipalString) == true) throw new InvalidOperationException($"{nameof(authenticatePrincipalString)}=null");

            // SetAsync
            await SecureStorage.SetAsync(this.GetType().FullName, authenticatePrincipalString);

            // Cache
            _isCached = true;
            _expireTime = expireTime;
            _claimsPrincipal = claimsPrincipal;

            // Raise
            this.OnPrincipalChanged(_claimsPrincipal);

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
            _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            // Raise
            this.OnPrincipalChanged(_claimsPrincipal);

            // Return
            return Task.CompletedTask;
        }


        // Events
        public event Action<ClaimsPrincipal> PrincipalChanged;
        protected void OnPrincipalChanged(ClaimsPrincipal principal = null)
        {
            // Raise
            var handler = this.PrincipalChanged;
            if (handler != null)
            {
                handler(principal);
            }
        }


        // Class
        private class ClaimConverter : JsonConverter<Claim>
        {
            // Methods
            public override Claim Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                #region Contracts

                ArgumentNullException.ThrowIfNull(typeToConvert);
                ArgumentNullException.ThrowIfNull(options);

                #endregion

                // Require
                if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();

                // Red
                var type = string.Empty;
                var value = string.Empty;
                while (reader.Read())
                {
                    // End
                    if (reader.TokenType == JsonTokenType.EndObject)
                    {
                        // Require
                        if (string.IsNullOrEmpty(type) == true) throw new JsonException();
                        if (string.IsNullOrEmpty(value) == true) throw new JsonException();

                        // Create
                        var claim = new Claim(type, value);
                        type = string.Empty;
                        value = string.Empty;

                        // Return
                        return claim;
                    }

                    // Property
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        // PropertyName
                        string propertyName = reader.GetString();
                        reader.Read();

                        // PropertyValue
                        switch (propertyName)
                        {
                            case "Type":
                                type = reader.GetString();
                                break;
                            case "Value":
                                value = reader.GetString();
                                break;
                        }
                    }
                }

                // Throw
                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, Claim claim, JsonSerializerOptions options)
            {
                #region Contracts

                ArgumentNullException.ThrowIfNull(writer);
                ArgumentNullException.ThrowIfNull(claim);
                ArgumentNullException.ThrowIfNull(options);

                #endregion

                writer.WriteStartObject();
                writer.WriteString("Type", claim.Type);
                writer.WriteString("Value", claim.Value);
                writer.WriteEndObject();
            }
        }
    }
}
