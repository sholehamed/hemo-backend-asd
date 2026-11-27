// using Google.Apis.Auth;
// using Microsoft.AspNetCore.Mvc;
//
// var builder = WebApplication.CreateBuilder(args);
//
// var app = builder.Build();
//
// app.UseRouting();
//
// app.MapPost("/test", async (HttpContext context) =>
//     {
//         // 1. بررسی نوع محتوای درخواست
//         if (!context.Request.HasFormContentType)
//         {
//             return Results.BadRequest(new
//             {
//                 Error = "Invalid content type",
//                 Message = "Expected 'application/x-www-form-urlencoded' or 'multipart/form-data'"
//             });
//         }
//
//         // 2. خواندن داده‌های فرم
//         var form = await context.Request.ReadFormAsync();
//
//         // 3. بررسی وجود فیلدهای ضروری
//         if (!form.ContainsKey("credential") || !form.ContainsKey("g_csrf_token"))
//         {
//             return Results.BadRequest(new
//             {
//                 Error = "Missing required fields",
//                 RequiredFields = new[] { "credential", "g_csrf_token" }
//             });
//         }
//
//         // 4. دریافت توکن‌ها
//         var credential = form["credential"].ToString(); // JWT از گوگل
//         var csrfToken = form["g_csrf_token"].ToString();
//
//         // 5. اعتبارسنجی CSRF Token
//         var cookieToken = context.Request.Cookies["g_csrf_token"];
//
//         if (string.IsNullOrEmpty(cookieToken) || csrfToken != cookieToken)
//         {
//             return Results.BadRequest(new
//             {
//                 Error = "Invalid CSRF token",
//                 Message = "CSRF token validation failed"
//             });
//         }
//
//         try
//         {
//             // 6. اعتبارسنجی JWT گوگل
//             var payload = await GoogleJsonWebSignature.ValidateAsync(
//                 credential,
//                 new GoogleJsonWebSignature.ValidationSettings()
//             );
//
//             // 7. پردازش اطلاعات کاربر
//             return Results.Ok(new
//             {
//                 Success = true,
//                 UserId = payload.Subject,
//                 Email = payload.Email,
//                 FullName = payload.Name,
//                 FirstName = payload.GivenName,
//                 LastName = payload.FamilyName,
//                 ProfilePicture = payload.Picture,
//                 EmailVerified = payload.EmailVerified
//             });
//         }
//         catch (Exception ex)
//         {
//             return Results.BadRequest(new
//             {
//                 Error = "Invalid Google credential",
//                 Message = ex.Message
//             });
//         }
//     })
//     .WithName("GoogleLogin");
//
//
// app.Run();
//
//
//
//
//
