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
        private readonly object _syncRoot = new object();

        private bool _isCached = false;

        private ClaimsPrincipal _claimsPrincipal = null;


        // Methods
        public Task<ClaimsPrincipal> GetAsync()
        {
            // Sync
            lock (_syncRoot)
            {
                // Require
                if (_isCached == true) return Task.FromResult(_claimsPrincipal);

                // GetAsync   
                var claimListString = SecureStorage.GetAsync(this.GetType().FullName).GetAwaiter().GetResult();
                if (string.IsNullOrEmpty(claimListString) == true)
                {
                    // Cache
                    _isCached = true;
                    _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

                    // Return
                    return Task.FromResult(_claimsPrincipal);
                }

                // ClaimList
                var claimList = JsonSerializer.Deserialize<List<Claim>>(claimListString, new JsonSerializerOptions { Converters = { new ClaimConverter() } });
                if (claimList == null)
                {
                    // Cache
                    _isCached = true;
                    _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

                    // Return
                    return Task.FromResult(_claimsPrincipal);
                }

                // AuthenticationType
                var authenticationType = claimList.FirstOrDefault(o => o.Type == AuthenticationClaimTypes.AuthenticationType)?.Value;
                if (string.IsNullOrEmpty(authenticationType) == true)
                {
                    // Cache
                    _isCached = true;
                    _claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

                    // Return
                    return Task.FromResult(_claimsPrincipal);
                }

                // ClaimsPrincipal
                {
                    // ClaimList.Filter
                    claimList.RemoveAll(o => o.Type == AuthenticationClaimTypes.AuthenticationType);
                }
                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claimList, authenticationType));

                // Cache
                _isCached = true;
                _claimsPrincipal = claimsPrincipal;

                // Return
                return Task.FromResult(_claimsPrincipal);
            }
        }

        public Task SetAsync(ClaimsPrincipal claimsPrincipal)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(claimsPrincipal);

            #endregion

            // ClaimsIdentity
            var claimsIdentity = claimsPrincipal.Identity as ClaimsIdentity;
            if (claimsIdentity == null) throw new InvalidOperationException($"{nameof(claimsIdentity)}=null");

            // ClaimList
            var claimList = claimsIdentity.Claims.ToList();
            claimList.RemoveAll(o => o.Type == AuthenticationClaimTypes.AuthenticationType);
            claimList.Add(new Claim(AuthenticationClaimTypes.AuthenticationType, claimsIdentity.AuthenticationType));

            // ClaimListString
            var claimListString = JsonSerializer.Serialize(claimList, new JsonSerializerOptions
            {
                Converters = { new ClaimConverter() }
            });
            if (string.IsNullOrEmpty(claimListString) == true) throw new InvalidOperationException($"{nameof(claimListString)}=null");

            // Sync
            lock (_syncRoot)
            {
                // SetAsync
                SecureStorage.SetAsync(this.GetType().FullName, claimListString).GetAwaiter().GetResult();

                // Cache
                _isCached = true;
                _claimsPrincipal = claimsPrincipal;
            }

            // Raise
            this.OnPrincipalChanged(claimsPrincipal);

            // Return
            return Task.CompletedTask;
        }               

        public Task RemoveAsync()
        {
            // ClaimsPrincipal
            ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());

            // Sync
            lock (_syncRoot)
            {
                // RemoveAsync
                SecureStorage.Remove(this.GetType().FullName);

                // Cache
                _isCached = true;
                _claimsPrincipal = claimsPrincipal;
            }

            // Raise
            this.OnPrincipalChanged(claimsPrincipal);

            // Return
            return Task.CompletedTask;
        }


        // Events
        public event Action<ClaimsPrincipal> PrincipalChanged;
        protected void OnPrincipalChanged(ClaimsPrincipal principal)
        {
            #region Contracts

            if (principal == null) throw new ArgumentException($"{nameof(principal)}=null");

            #endregion

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
