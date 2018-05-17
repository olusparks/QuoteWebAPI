using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace RelianceQuoteWebAPI.WebAPIHelperCode
{
    public class Authentication
    {
        private static Configuration _config = null;
        private static HttpMessageHandler _clientHandler = null;
        private static AuthenticationContext _context = null;
        private static string _authority = null;

        public HttpMessageHandler ClientHandler
        {
            get { return _clientHandler; }
            set { _clientHandler = value; }
        }

        public AuthenticationContext Context
        {
            get { return _context; }
            set { _context = value; }
        }

        public string Authority
        {
            get
            {
                if (_authority != null)
                {
                    _authority = DiscoverAuthority(_config.ServiceUrl);
                }
                return _authority;
            }
            set { _authority = value; }
        }

        #region Constructor

        public Authentication() { }

        public Authentication(Configuration config) : base()
        {
            _config = config;
            SetClientHandler();
        }

        #endregion


        public static string DiscoverAuthority(string serviceUrl)
        {
            try
            {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                AuthenticationParameters ap = AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(serviceUrl + "api/data/")).Result;
                return ap.Authority;
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An HTTP request exception occurred during authority discovery.", e);
            }
        }

        public static async Task<string> DiscoverAuthorityAsync(string serviceUrl)
        {
            try
            {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                AuthenticationParameters ap = await AuthenticationParameters.CreateFromResourceUrlAsync(new Uri(serviceUrl + "api/data/"));
                return ap.Authority;
            }
            catch (HttpRequestException e)
            {

                throw new Exception("An HTTP request exception occurred during authority discovery.", e);
            }
        }

        public static async Task<AuthenticationResult> AcquireTokenAsync()
        {
            try
            {
                if (_config != null && (_config.Username != null && _config.Password != null))
                {
                    UserPasswordCredential userCred = new UserPasswordCredential(_config.Username, _config.Password);
                    _context = new AuthenticationContext(DiscoverAuthorityAsync(_config.ServiceUrl).Result);
                    return await _context.AcquireTokenAsync(_config.ServiceUrl, _config.ClientId, userCred);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed. Verify the configuration values are correct.", e);
            }
            return null;
        }

        public static AuthenticationResult AcquireToken()
        {
            try
            {
                if (_config != null && (_config.Username != null && _config.Password != null))
                {
                    UserPasswordCredential userCred = new UserPasswordCredential(_config.Username, _config.Password);
                    _context = new AuthenticationContext(DiscoverAuthority(_config.ServiceUrl));
                    //_context.AcquireToken(_config.ServiceUrl, _config.ClientId, userCred);
                    return _context.AcquireTokenAsync(_config.ServiceUrl, _config.ClientId, userCred).Result;
                }
            }
            catch (Exception e)
            {
                throw new Exception("Authentication failed. Verify the configuration values are correct.", e);
            }
            return null;
        }

        /// <summary>  
        /// Sets the client message handler as appropriate for the type of authentication  
        /// in use on the web service endpoint.  
        /// </summary>  
        private void SetClientHandler()
        {
            // Check the Authority to determine if OAuth authentication is used.  
            if (String.IsNullOrEmpty(Authority))
            {
                if (_config.Username != String.Empty)
                {
                    _clientHandler = new HttpClientHandler()
                    { Credentials = new NetworkCredential(_config.Username, _config.Password, _config.Domain) };
                }
                else
                // No username is provided, so try to use the default domain credentials.  
                {
                    _clientHandler = new HttpClientHandler()
                    { UseDefaultCredentials = true };
                }
            }
            else
            {
                _clientHandler = new OAuthMessageHandler(this, new HttpClientHandler());
                _context = new AuthenticationContext(Authority, false);
            }
        }

        /// <summary>  
        /// Custom HTTP client handler that adds the Authorization header to message requests. This  
        /// is required for IFD and Online deployments.  
        /// </summary>  

    }

    class OAuthMessageHandler : DelegatingHandler
    {
        //Authentication _auth = null;
        private Authentication authenticationn;
        private HttpClientHandler httpClientHandler;

        public OAuthMessageHandler(Authentication authenticationn, HttpClientHandler httpClientHandler)
        {
            this.authenticationn = authenticationn;
            this.httpClientHandler = httpClientHandler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            // It is a best practice to refresh the access token before every message request is sent. Doing so  
            // avoids having to check the expiration date/time of the token. This operation is quick.  
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Authentication.AcquireToken().AccessToken);

            return base.SendAsync(request, cancellationToken);
        }
    }
}