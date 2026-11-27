using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace web.Services;

public class GoogleTokenValidator
{
    private const string GoogleCertsUrl = "https://www.googleapis.com/oauth2/v3/certs";

    public static async Task<bool> ValidateGoogleTokenAsync(string idToken, string clientId)
    {
        try
        {
            // دریافت JSON کلیدهای عمومی گوگل
            using var httpClient = new HttpClient();
            string json = await httpClient.GetStringAsync(GoogleCertsUrl);

            // استخراج کلید مناسب از JSON
            var rsaKey = GetRsaKeyFromJson(json);
            if (rsaKey == null)
                return false;

            // تنظیمات اعتبارسنجی
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://accounts.google.com",

                ValidateAudience = true,
                ValidAudience = clientId,

                ValidateLifetime = true,

                IssuerSigningKey = new RsaSecurityKey(rsaKey)
            };

            // بررسی توکن
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(idToken, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static RSA GetRsaKeyFromJson(string json)
    {
        var document = JsonDocument.Parse(json);
        var keys = document.RootElement.GetProperty("keys").EnumerateArray();
        var key = keys.FirstOrDefault();

        if (key.ValueKind == JsonValueKind.Undefined)
            return null;

        string n = key.GetProperty("n").GetString();
        string e = key.GetProperty("e").GetString();

        return ConvertToRsa(n, e);
    }

    private static RSA ConvertToRsa(string n, string e)
    {
        var rsa = RSA.Create();
        rsa.ImportParameters(new RSAParameters
        {
            Modulus = Base64UrlDecode(n),
            Exponent = Base64UrlDecode(e)
        });
        return rsa;
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string base64 = input.Replace('-', '+').Replace('_', '/');
        while (base64.Length % 4 != 0) base64 += "=";
        return Convert.FromBase64String(base64);
    }
}