using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace ETHAN.WebSvc
{
    public class WebSvcHelper
    {
        private readonly HttpClient _httpClient;

        public WebSvcHelper()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetWebServiceDataAsync(string url)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw an exception if the response status code is not successful

                string responseData = await response.Content.ReadAsStringAsync();
                return responseData;
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the web service call
                Console.WriteLine($"An error occurred: {ex.Message}");
                return null;
            }
        }
    }
}
