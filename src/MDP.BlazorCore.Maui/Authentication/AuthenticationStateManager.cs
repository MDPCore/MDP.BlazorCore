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
        // Methods
        public async Task SetAsync(ClaimsPrincipal claimsPrincipal)
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

            // SetAsync
            await SecureStorage.SetAsync(this.GetType().FullName, claimListString);

            // Raise
            this.OnPrincipalChanged(claimsPrincipal);
        }

        public async Task<ClaimsPrincipal> GetAsync()
        {
            // GetAsync
            var claimListString = await SecureStorage.GetAsync(this.GetType().FullName);
            if (string.IsNullOrEmpty(claimListString) == true) return new ClaimsPrincipal(new ClaimsIdentity());

            // ClaimList
            var claimList = JsonSerializer.Deserialize<List<Claim>>(claimListString, new JsonSerializerOptions
            {
                Converters = { new ClaimConverter() }
            });
            if (claimList == null) throw new InvalidOperationException($"{nameof(claimList)}=null");

            // AuthenticationType
            var authenticationType = claimList.FirstOrDefault(o => o.Type == AuthenticationClaimTypes.AuthenticationType)?.Value;
            if (string.IsNullOrEmpty(authenticationType) == true) throw new InvalidOperationException($"{nameof(authenticationType)}=null");

            // ClaimList.Filter
            claimList.RemoveAll(o => o.Type == AuthenticationClaimTypes.AuthenticationType);

            // Return
            return new ClaimsPrincipal(new ClaimsIdentity(claimList, authenticationType));
        }        

        public Task ClearAsync()
        {
            // ClearAsync
            SecureStorage.Remove(this.GetType().FullName);

            // Raise
            this.OnPrincipalChanged(new ClaimsPrincipal(new ClaimsIdentity()));

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
