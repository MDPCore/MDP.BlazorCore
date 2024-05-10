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
            this.TokenEndpoint = OAuthSSODefaults.TokenEndpoint;
            this.UserInformationEndpoint = OAuthSSODefaults.UserInformationEndpoint;
            this.CallbackEndpoint = OAuthSSODefaults.CallbackEndpoint;
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
                this.CallbackEndpoint = string.Format(OAuthSSODefaults.CallbackEndpoint, value);
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
                this.TokenEndpoint = string.Format(OAuthSSODefaults.TokenEndpoint, value);
                this.UserInformationEndpoint = string.Format(OAuthSSODefaults.UserInformationEndpoint, value);
            }
        }

        public string AuthorizationEndpoint { get; private set; }

        public string TokenEndpoint { get; private set; }

        public string UserInformationEndpoint { get; private set; }

        public string CallbackEndpoint { get; private set; }
    }
}
