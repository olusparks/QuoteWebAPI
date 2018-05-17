using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace RelianceQuoteWebAPI.WebAPIHelperCode
{
    public class ConnectToCRM
    {
        private HttpClient httpClient = null;
        private static Version webAPIVersion = new Version(8, 0);
        private static string getVersionedWebAPIPath()
        {
            return string.Format("v{0}", webAPIVersion.ToString(2));
        }

        public async Task getWebAPIVersion()
        {

            HttpRequestMessage RetrieveVersionRequest = new HttpRequestMessage(HttpMethod.Get, getVersionedWebAPIPath() + "RetrieveVersion");
            HttpResponseMessage RetrieveVersionResponse = await httpClient.SendAsync(RetrieveVersionRequest);

            if (RetrieveVersionResponse.StatusCode == HttpStatusCode.OK)  //200
            {
                JObject RetrievedVersion = JsonConvert.DeserializeObject<JObject>(await RetrieveVersionResponse.Content.ReadAsStringAsync());
                //Capture the actual version available in this organization
                webAPIVersion = Version.Parse((string)RetrievedVersion.GetValue("Version"));
            }
            else
            {
                //Console.WriteLine("Failed to retrieve the version for reason: {0}", RetrieveVersionResponse.ReasonPhrase);
                throw new CrmHttpException(RetrieveVersionResponse.Content);
            }

        }

        public static HttpClient HttpConnect(string apiVersion)
        {
            Configuration config = new Configuration("CrmOnline");
            Authentication auth = new Authentication(config);

            string url = config.ServiceUrl;

            //string url = new Configuration("CrmOnline").ServiceUrl.ToString();

            string accessToken = Authentication.AcquireToken().AccessToken;

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url + apiVersion);
            httpClient.Timeout = new TimeSpan(0, 2, 0);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }


        //Overloaded Method
        public static HttpClient HttpConnect()
        {
            Configuration config = new Configuration("CrmOnline");
            Authentication auth = new Authentication(config);

            string url = config.ServiceUrl;

            //string url = new Configuration("CrmOnline").ServiceUrl.ToString();

            string accessToken = Authentication.AcquireToken().AccessToken;

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url + "api/data" + getVersionedWebAPIPath());
            httpClient.Timeout = new TimeSpan(0, 2, 0);
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}