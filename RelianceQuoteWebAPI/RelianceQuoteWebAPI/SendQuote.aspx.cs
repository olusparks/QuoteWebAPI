using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RelianceQuoteWebAPI.WebAPIHelperCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RelianceQuoteWebAPI
{
    public partial class SendQuote : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            GetData();
        }

        public void GetData()
        {
            string[] contactProperties = { "fullname", "jobtitle", "annualincome" };
            string[] quote = { "quoteid", "name", "quotenumber" };
            string[] quoteDetails = { "baseamount", "description", "extendedamount", "lineitemnumber", "quantity" };
            string[] accountContact = { };
            //Use WebAPI: 
            /*TODO: Connect to quote entity, retrieve fields,
             * get reference to quotedetails entity
             * get reference to user entity 
             * get reference to account entity
             * */
            string quoteNumber = "QUO-01000-F0J5W7";
            string quoteId = "a2d033b5-3bfc-e711-a94f-000d3ab29582";
            string apiVersion = "api/data/v9.0";
            
            HttpClient httpClient = ConnectToCRM.HttpConnect(apiVersion);


            string expand = "&$expand=quote_details($select=" + String.Join(",", quoteDetails) + ")";
            string filter = string.Format("&$filter=_quoteid_value eq {0}", quoteId);
            string queryOperation = "?$select=" + String.Join(",", quote) + expand;
            string quoteDetailsQuery = "?$select=" + String.Join(",", quoteDetails) + filter ;

            HttpResponseMessage quoteResponseMessage =  httpClient.GetAsync(httpClient.BaseAddress.ToString() + "/quotes" + queryOperation).Result;
            string url = $"{httpClient.BaseAddress.ToString()}{"/quotes"}{"("}{quoteId}{")"}{"/quote_details?$select=baseamount,description,extendedamount,lineitemnumber,quantity"}";
            HttpResponseMessage quoteDetailsResponseMessage = httpClient.GetAsync(url).Result;
            if (quoteResponseMessage.StatusCode  == HttpStatusCode.OK )
            {
                dynamic quoteRetrievedResult = JsonConvert.DeserializeObject(quoteResponseMessage.Content.ReadAsStringAsync().Result);
                dynamic quoteDetailsRetrievedResult = JsonConvert.DeserializeObject(quoteDetailsResponseMessage.Content.ReadAsStringAsync().Result);

                //Loop for Quote
                foreach (var item in quoteRetrievedResult.value)
                {
                    string fullname = item.name.Value;
                }

                //Loop for QuoteDeatils
                foreach (var item in quoteDetailsRetrievedResult.value)
                {
                    string baseamount = item.baseamount.Value;
                }
                JObject parsedResult = JObject.Parse(quoteRetrievedResult.ToString());
                
                //Quote quote = new Quote
                //{
                //    QuoteNumber = parsedResult["quotenumber"].ToString(),
                //    QuoteId = (Guid)parsedResult["quoteid"],
                //    QuoteName = parsedResult["name"].ToString()
                //};

                //for (int i = 0; i < parsedResult.SelectToken("quote_details").ToArray().Length; i++)
                //{
                //    quote.QuoteDetails.Add(new QuoteDetails
                //    {
                //        ProductName = parsedResult["quote_details"][i]["productname"].ToString(),
                //        Quantity = parsedResult["quote_details"][i]["quantity"].ToString(),
                //        ProductDescription = parsedResult["quote_details"][i]["description"].ToString(),
                //        Priceperunitbase = parsedResult["quote_details"][i]["productname"].ToString(),
                //        ExtendedAmountbase = parsedResult["quote_details"][i]["productname"].ToString(),
                //    });
                //}
            }
        }
    }
}