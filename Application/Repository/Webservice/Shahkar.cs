using System.Text;
using Application.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Application.Repository.Webservice;

public class Shahkar
{
    public async Task<bool> CheckMobile(string MelliCode, string Mobile)
    {
        
        try
        {
            using (var client = new HttpClient())
            {
                if (!Mobile.StartsWith("0"))
                {
                    Mobile = "0" + Mobile;
                }
        
        
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", Token);
                Random random = new Random();
        
                JObject jobject = new JObject();
                jobject["serviceType"] = "2";
                jobject["identificationType"] = "0";
                jobject["identificationNo"] = MelliCode;
                jobject["requestId"] = "0519" + DateTime.Now.ToString("yyyyMMddHHmmss") +
                                       random.Next(000000, 999999).ToString();
                jobject["serviceNumber"] = Mobile;
        
        
                Newtonsoft.Json.Linq.JObject webserviceInput = new Newtonsoft.Json.Linq.JObject();
                webserviceInput["WebserviceGkey"] = 130;
                webserviceInput["json"] = JsonConvert.SerializeObject(jobject);
        
        
                string json = JsonConvert.SerializeObject(webserviceInput);
        
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                var PostResult = client.PostAsync("https://amalkardgw.aro.gov.ir/Get", content).Result;
                string JsonResult = PostResult.Content.ReadAsStringAsync().Result;
        
                JObject Res = JObject.Parse(JsonResult);
        
                return "200" == Res["result"]?["data"]?["response"]?.ToString();
            }
        }
        catch (Exception e)
        {
            if (e?.InnerException?.Message.Contains("No such host is known.") == true)
            {
                return await CheckMobile(MelliCode, Mobile);
            }
        
            else
            {
                throw new ServiceExeption(
                    "وب سرویس استعلام شماره موبایل در دسترس نمی باشد لطفا بعدا مراجعه فرمایید");
            }
        }
    }
    
    
}