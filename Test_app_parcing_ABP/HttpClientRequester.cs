using System.Net.Http;

namespace Test_app_parcing_ABP
{
    internal class HttpClientRequester
    {
        private HttpClient client;

        public HttpClientRequester(HttpClient client)
        {
            this.client = client;
        }
    }
}