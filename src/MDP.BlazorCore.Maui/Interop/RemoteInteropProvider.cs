using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MDP.BlazorCore.Maui
{
    public class RemoteInteropProvider : InteropProvider
    {
        // Constants
        private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };


        // Fields
        private readonly IHttpClientFactory _httpClientFactory = null;


        // Constructors
        public RemoteInteropProvider(IHttpClientFactory httpClientFactory)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(httpClientFactory);

            #endregion

            // Default
            _httpClientFactory = httpClientFactory;
        }


        // Methods
        public async Task<InteropResponse> InvokeAsync(ClaimsPrincipal principal, InteropRequest interopRequest, InteropResource interopResource)
        {
            #region Contracts

            if (principal == null) throw new ArgumentNullException(nameof(principal));
            if (interopRequest == null) throw new ArgumentNullException(nameof(interopRequest));
            if (interopResource == null) throw new ArgumentNullException(nameof(interopResource));

            #endregion

            // Execute
            try
            {
                // HttpClient
                var httpClient = _httpClientFactory.CreateClient("DefaultService");
                if (httpClient == null) throw new InvalidOperationException($"{nameof(httpClient)}=null");

                // PostAsync
                var interopResponse = await httpClient.PostAsync<InteropResponse, InteropResponse>("/.blazor/interop/invokeAsync", content: new
                {
                    controllerUri = $"{httpClient.BaseAddress.Scheme}://{httpClient.BaseAddress.Host}{interopRequest.ControllerUri.PathAndQuery}",
                    actionName = interopRequest.ActionName,
                    actionParameters = interopRequest.ActionParameters
                });
                if (interopResponse == null) throw new InvalidOperationException($"{nameof(interopResponse)}=null");

                // MethodInfo
                var methodInfo = interopResource.ServiceType.GetMethod(interopRequest.ActionName);
                if (methodInfo == null)
                {
                    // NotFound
                    return new InteropResponse()
                    {
                        StatusCode = InteropStatusCode.NotFound,
                        Result = null,
                        ErrorMessage = $"Not found for resource: {interopRequest.RoutePath}/{interopRequest.ActionName}"
                    };
                }

                // ResultType
                var resultType = methodInfo.ReturnType;
                {
                    // Task
                    if (resultType != null && resultType == typeof(Task))
                    {
                        resultType = null;
                    }

                    // Task<>
                    if (resultType != null && resultType.IsGenericType && resultType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        resultType = resultType.GetGenericArguments().FirstOrDefault();
                    }
                }
                if (resultType == null) return interopResponse;

                // Result
                var resultString = string.Empty;
                if (string.IsNullOrEmpty(resultString) == true && interopResponse.Result is JsonElement) resultString = this.CreateString((JsonElement)interopResponse.Result);
                if (string.IsNullOrEmpty(resultString) == true && interopResponse.Result != null) resultString = interopResponse.Result.ToString();
                interopResponse.Result = this.CreateResult(resultType, resultString);

                // Return
                return interopResponse;
            }
            catch (HttpException<InteropResponse> exception)
            {
                // InteropResponse
                var interopResponse = exception.ErrorModel as InteropResponse;
                if (interopResponse == null) 
                {
                    interopResponse = new InteropResponse()
                    {
                        StatusCode = InteropStatusCode.InternalServerError,
                        Result = null,
                        ErrorMessage = exception.Message
                    };
                }

                // Return
                return interopResponse;
            }
            catch (Exception exception)
            {
                // Require
                while (exception.InnerException != null) exception = exception.InnerException;

                // InteropResponse
                var interopResponse = new InteropResponse()
                {
                    StatusCode = InteropStatusCode.InternalServerError,
                    Result = null,
                    ErrorMessage = exception.Message
                };

                // Return
                return interopResponse;
            }
        }

        private object CreateResult(Type resultType, string resultString = null)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(resultType);

            #endregion

            // String
            if (resultType == typeof(string))
            {
                // ResultString
                if (string.IsNullOrEmpty(resultString) == true) resultString = string.Empty;

                // Result
                var result = resultString;

                // Return
                return result;
            }

            // Convertible
            if (typeof(IConvertible).IsAssignableFrom(resultType) == true)
            {
                // ResultString
                if (string.IsNullOrEmpty(resultString) == true) resultString = string.Empty;

                // Result
                var result = Convert.ChangeType(resultString, resultType);

                // Return
                return result;
            }

            // Deserialize
            {
                // ResultString
                if (string.IsNullOrEmpty(resultString) == true) resultString = "{}";

                // Result
                var result = System.Text.Json.JsonSerializer.Deserialize(resultString, resultType, _serializerOptions);

                // Return
                return result;
            }
        }

        private string CreateString(JsonElement jsonElement)
        {
            #region Contracts

            ArgumentNullException.ThrowIfNull(jsonElement);

            #endregion

            switch (jsonElement.ValueKind)
            {
                // Null 
                case JsonValueKind.Null:
                    return null;

                // Number
                case JsonValueKind.Number:
                    return jsonElement.GetRawText();

                // String
                case JsonValueKind.String:
                    return jsonElement.GetString();
                
                // Bool
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return jsonElement.GetBoolean().ToString();
               
                // Object
                case JsonValueKind.Object:
                case JsonValueKind.Array:
                    return jsonElement.GetRawText();

                // Unknown
                default:
                    throw new InvalidOperationException($"{nameof(jsonElement.ValueKind)}={jsonElement.ValueKind}");
            }
        }
    }
}
