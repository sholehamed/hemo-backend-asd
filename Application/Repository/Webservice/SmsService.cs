using System.Text;
using Application.Common.Exceptions;
using Newtonsoft.Json;

namespace Application.Repository.Webservice;

public class SmsService
{
    // public async Task<bool> SendSms(string Message, string mobile)
    // {
    //     try
    //     {
    //         if (Message.Length > 900)
    //         {
    //             throw new ServiceExeption(" طول کل متن پیامک بیش از حد مجاز می باشد");
    //         }
    //     
    //         if (!mobile.StartsWith("0"))
    //         {
    //             mobile = "0" + mobile;
    //         }
    //     
    //         using (var client = new HttpClient())
    //         {
    //             Newtonsoft.Json.Linq.JObject Data1 = new Newtonsoft.Json.Linq.JObject();
    //             Data1["receiver_number"] = mobile;
    //             Data1["sender_number"] = "40401102";
    //             Data1["note_arr"] = Message;
    //     
    //     
    //             string jsonInput = JsonConvert.SerializeObject(Data1);
    //     
    //     
    //             Newtonsoft.Json.Linq.JObject webserviceInput = new Newtonsoft.Json.Linq.JObject();
    //             webserviceInput["WebserviceGkey"] = 124;
    //             webserviceInput["json"] = jsonInput;
    //     
    //             string json = JsonConvert.SerializeObject(webserviceInput);
    //     
    //             StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
    //             var PostResult = client.PostAsync("https://amalkardgw.aro.gov.ir/Get", content).Result;
    //             string JsonResult = PostResult.Content.ReadAsStringAsync().Result;
    //     
    //             return true;
    //         }
    //     }
    //     
    //     catch (Exception e)
    //     {
    //         if (e?.InnerException?.Message == "No such host is known.")
    //         {
    //             return await SendSms(Message, mobile);
    //         }
    //         return false;
    //     }
    // }
    //
    
    public async Task<string> SendOtpMessageAsync(long phoneNumber, long otp)
    {
        var url = "https://safir.bale.ai/api/v3/send_message";

        var json = $@"
        {{
            ""bot_id"": 651861,
            ""phone_number"": ""98{phoneNumber}"",
            ""message_data"": {{
                ""otp_message"": {{
                    ""otp"": ""{otp}""
                }}
            }}
        }}";

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Add("api-access-key", "E9s3033wekDg9Lv6");
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        using (HttpClient _httpClient = new HttpClient())
        {
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}