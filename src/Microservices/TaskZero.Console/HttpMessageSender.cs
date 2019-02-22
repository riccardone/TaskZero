using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TaskZero.Console
{
    public class HttpMessageSender : IMessageSender
    {
        private readonly Uri _uri;

        public HttpMessageSender(Uri uri)
        {
            _uri = uri;
        }

        public async Task<string> Post<T>(string path, T data)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = new TimeSpan(0, 0, 5);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(data);

                var responseMessage = await client.PostAsync(_uri + path, new StringContent(json, Encoding.UTF8, "application/json"));

                var response = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.IsSuccessStatusCode)
                {
                    return response;
                }

                throw new Exception("Request failed");
            }
        }
    }
}
