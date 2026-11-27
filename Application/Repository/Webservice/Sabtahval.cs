using System.Net.Http.Headers;
using System.Text;
using Application.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Application.Repository.Webservice;

public class Sabtahval
{
    public async Task<PersonInfo> GetUserInfo(string Username,DateTime Bdate)
    {
        using (var client = new HttpClient())
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://gateway.ivo.ir/Get2");
            
            request.Headers.Add("accept", "application/json, text/plain, */*");
            request.Headers.Add("accept-language", "en-US,en;q=0.9");
            request.Headers.Add("origin", "https://erp.ivo.ir");
            request.Headers.Add("priority", "u=1, i");
            request.Headers.Add("referer", "https://erp.ivo.ir/");
            request.Headers.Add("sec-ch-ua",
                "\"Chromium\";v=\"140\", \"Not=A?Brand\";v=\"24\", \"Google Chrome\";v=\"140\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("sec-fetch-dest", "empty");
            request.Headers.Add("sec-fetch-mode", "cors");
            request.Headers.Add("sec-fetch-site", "same-site");
            request.Headers.Add("user-agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/140.0.0.0 Safari/537.36");

            request.Headers.Add("systemguid", "32E187EB-346B-4599-A852-2E2F944E880F");
            request.Headers.Add("url", "/WebService/WebService");
            request.Headers.Add("userrole", "1");
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJzY29wZSI6WyIzMiIsIjQiLCIxIl0sIlVzZXJSb2xlcyI6WyIzIiwiNSIsIjEiXSwiVXNlclJvbGVPcmdsb2NhdGlvbiI6WyIzLzVhM2FlMjAzLTYzMjYtNDcwYi1iODQ1LWFlOWI5NTgyMjE1YyIsIjUvNWEzYWUyMDMtNjMyNi00NzBiLWI4NDUtYWU5Yjk1ODIyMTVjIiwiMS81YTNhZTIwMy02MzI2LTQ3MGItYjg0NS1hZTliOTU4MjIxNWMiXSwiVG9rZW5LZXkiOiIwNmQ1YmY3Yy1mZjVmLTQ5YjUtYjM3NS02YjgyNzE1ZGI2NGIiLCJJZCI6IjNlOTExZDU1LTE2ZWUtNGVmNi1hMDJlLTk2MDIyYzMzYjQ2YiIsIkV4cGlyZURhdGUiOiI0LzQvMjAyNiA0OjA2OjEyIFBNIiwiVXNlckFnZW50IjoiTW96aWxsYS81LjAgKFdpbmRvd3MgTlQgMTAuMDsgV2luNjQ7IHg2NCkgQXBwbGVXZWJLaXQvNTM3LjM2IChLSFRNTCwgbGlrZSBHZWNrbykgQ2hyb21lLzE0MC4wLjAuMCBTYWZhcmkvNTM3LjM2IiwiSXBBZGRyZXNzIjoiMTkyLjE2OC43Mi4xMzIiLCJVc2VybmFtZSI6ImFkbWluIiwiVXNlckZ1bGxuYW1lIjoi2YXYr9uM2LEg2KfYsdi02K8g2LPYp9mF2KfZhtmHINmH2KciLCJFbWFpbCI6InZhZG9vZC52YWhpZDIxQGdtYWlsLmNvbSIsIklzUmVhbCI6IiIsIkJpcnRoRGF5IjoiIiwic3ViIjoiYWRtaW4iLCJlbWFpbCI6InZhZG9vZC52YWhpZDIxQGdtYWlsLmNvbSIsImp0aSI6IjMzN2JmN2RjLTg5YjUtNGIxOC1iMTA0LTk3MTJjZDI0NjQ4ZiIsIm5iZiI6MTc3NTMwNTU3MiwiZXhwIjoxNzc1MzA2MTcyLCJpYXQiOjE3NzUzMDU1NzIsImlzcyI6Imh0dHA6Ly9hc2QuaXIiLCJhdWQiOiJodHRwOi8vYXNkLmlyIn0.zY6SnXbUV4tGHvlAGxHlbVP5FJgaSGzbCuV35kDQGP5eTWpUzZqH4Ex-xmXZAMhZJgMIQ6GzjmocgX632lsBfQ");
            
            request.Headers.Add("Cookie", "cookiesession1=678A8C38F1BD6925A682D5085E5775A6");

            JObject SendData = new JObject();
            JObject UserData = new JObject();
            UserData["birthDate"] = Bdate.ToShamsi().Replace("/","");
            UserData["nin"] = Username;
            SendData["WebserviceGkey"] = 21;
            SendData["json"] = JsonConvert.SerializeObject(UserData);
            var jsonBody = JsonConvert.SerializeObject(SendData);

            request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            
            try
            {
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode(); 

                string result = await response.Content.ReadAsStringAsync();
                return ParsePerson(result);
            }
            catch (Exception ex)
            {
                throw new ServiceExeption("کد ملی یا تاریخ تولد صحیح نمی باشد.");
            }
        }
    }

    private PersonInfo ParsePerson(string json)
    {
        var api = JsonSerializer.Deserialize<ApiResponse>(json);

        var r = api.result.data.getEstelam3Response.@return;

        return new PersonInfo
        {
            Name = DecodeBase64(r.name),
            Family = DecodeBase64(r.family),
            FatherName = DecodeBase64(r.fatherName),
            IsMan = r.gender=="1"
        };
    }

    private string DecodeBase64(string base64)
    {
        if (string.IsNullOrEmpty(base64))
            throw new Exception();;

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}

public class ApiResponse
{
    public Result result { get; set; }
}

public class Result
{
    public Data data { get; set; }
}

public class Data
{
    public GetEstelam3Response getEstelam3Response { get; set; }
}

public class GetEstelam3Response
{
    public ReturnData @return { get; set; }
}

public class ReturnData
{
    public string family { get; set; }
    public string fatherName { get; set; }
    public string gender { get; set; }
    public string name { get; set; }
}

public class PersonInfo
{
    public string Name { get; set; }
    public string Family { get; set; }
    public string FatherName { get; set; }
    public bool IsMan { get; set; }
}