using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Application.Common.Utility.HTTP;

public static class HTTP
{
    public static async Task<string> PostFormUrlEncoded(string BaseAddress, string Path,
        IEnumerable<KeyValuePair<string, string>> postData)
    {
        using (var httpClientHandler = new HttpClientHandler())
        {
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;


            var response = new HttpResponseMessage();
            using (var httpClient = new HttpClient(httpClientHandler))
            {
                httpClient.BaseAddress = new Uri(BaseAddress);
                var content = new FormUrlEncodedContent(postData);

                response = await httpClient.PostAsync(Path, content);
            }

            return await response.Content.ReadAsStringAsync();
        }
    }

    public static async Task<string> Post(string Url, string Data, IConfiguration configuration,
        List<HTTPHeader> Header = null)
    {
        using (var httpClientHandler = new HttpClientHandler())
        {
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;


            using (var client = new HttpClient(httpClientHandler))
            {
                var GatewayUrl = configuration.GetSection("GatewayUrl").Value;
                var content = new StringContent(Data, Encoding.UTF8, "application/json");
                AddHeader(client, Header);
                var PostResult = await client.PostAsync(GatewayUrl + Url, content);
                var JsonResult = PostResult.Content.ReadAsStringAsync().Result;
                return JsonResult;
            }
        }
    }

    public static async Task<string> Post(string Url, object Data = null, string jwtToken = null,
        List<HTTPHeader> Header = null)
    {
        using (var httpClientHandler = new HttpClientHandler())
        {
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            using (var client = new HttpClient(httpClientHandler))
            {
                var Json = "{}";
                if (Data != null) Json = JsonConvert.SerializeObject(Data);
                var content = new StringContent(Json, Encoding.UTF8, "application/json");

                // اضافه کردن هدرها
                AddHeader(client, Header);

                // اضافه کردن هدر Authorization با JWT
                if (!string.IsNullOrEmpty(jwtToken))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

                var PostResult = await client.PostAsync(Url, content);
                var JsonResult = await PostResult.Content.ReadAsStringAsync();
                return JsonResult;
            }
        }
    }


    public static async Task<string> Get(string Url, List<HTTPHeader> Header = null,int Timeout=0)
    {
        using (var httpClientHandler = new HttpClientHandler())
        {
            httpClientHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

            
            using (var client = new HttpClient(httpClientHandler))
            {
                if (Timeout>0)
                {
                    client.Timeout=TimeSpan.FromSeconds(99999);
                }
                
                AddHeader(client, Header);
                var PostResult = await client.GetAsync(Url);
                var JsonResult = PostResult.Content.ReadAsStringAsync().Result;
                return JsonResult;
            }
        }
    }

    private static void AddHeader(HttpClient HttpClient, List<HTTPHeader> Headers = null)
    {
        if (Headers != null)
            foreach (var Header in Headers)
                HttpClient.DefaultRequestHeaders.Add(Header.Key, Header.Value);
    }
}

public class HTTPHeader
{
    public string Key { get; set; }
    public string Value { get; set; }
}