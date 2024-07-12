namespace MDP.BlazorCore.Authentication.Maui
{
    public class OAuthSSOOptions
    {
        // Fields
        private string _clientUrl = null;

        private string _serverUrl = null;


        // Constructors
        public OAuthSSOOptions()
        {
            // Options
            this.AuthorizationEndpoint = OAuthSSODefaults.AuthorizationEndpoint;
            this.LogoutEndpoint = OAuthSSODefaults.LogoutEndpoint;
            this.TokenEndpoint = OAuthSSODefaults.TokenEndpoint;
            this.UserInformationEndpoint = OAuthSSODefaults.UserInformationEndpoint;
            this.LoginCallbackEndpoint = OAuthSSODefaults.LoginCallbackEndpoint;
            this.LogoutCallbackEndpoint = OAuthSSODefaults.LogoutCallbackEndpoint;
        }


        // Properties
        public string ClientId { get; set; }

        public string ClientUrl
        {
            get
            {
                // Get
                return _clientUrl;
            }
            set
            {
                // Set
                _clientUrl = value;
                this.LoginCallbackEndpoint = string.Format(OAuthSSODefaults.LoginCallbackEndpoint, value);
                this.LogoutCallbackEndpoint = string.Format(OAuthSSODefaults.LogoutCallbackEndpoint, value);
            }
        }

        public string ServerUrl
        {
            get
            {
                // Get
                return _serverUrl;
            }
            set
            {
                // Set
                _serverUrl = value;
                this.AuthorizationEndpoint = string.Format(OAuthSSODefaults.AuthorizationEndpoint, value);
                this.LogoutEndpoint = string.Format(OAuthSSODefaults.LogoutEndpoint, value);
                this.TokenEndpoint = string.Format(OAuthSSODefaults.TokenEndpoint, value);
                this.UserInformationEndpoint = string.Format(OAuthSSODefaults.UserInformationEndpoint, value);
            }
        }

        public string AuthorizationEndpoint { get; private set; }

        public string LogoutEndpoint { get; private set; }

        public string TokenEndpoint { get; private set; }

        public string UserInformationEndpoint { get; private set; }

        public string LoginCallbackEndpoint { get; private set; }

        public string LogoutCallbackEndpoint { get; private set; }

        public bool UseCookies { get; set; } = false;

        public bool IgnoreServerCertificate { get; set; } = false;
    }
}
