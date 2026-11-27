# حساب‌ها و نقش‌های آزمایشی HEMO — ST-02

مرکز مشترک: بیمارستان آزمایشی HEMO. ورود از صفحه HEMO و Captcha واقعی شرکت.

| نام کاربری | نقش | مجوز |
|---|---|---|
| hemo.admission | کارشناس پذیرش | مشاهده، ثبت و ویرایش بستری، ثبت بررسی هویت/سوابق/ترخیص |
| hemo.supervisor | مسئول پذیرش | همه موارد کارشناس + اصلاح مشخصات هویتی و تعیین هویت موقت |
| hemo.viewer | مشاهده پذیرش | داشبورد، آمار، جستجو، جزئیات، سوابق و تاریخچه؛ بدون تغییر |

رمز ثابت در Git وجود ندارد. پیش از اولین راه‌اندازی، سه رمز متفاوت حداقل ۱۶ کاراکتری تنظیم کنید. از PowerShell در محیط توسعه، پیش از اجرای پروژه web:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:HemoSeed__Enabled = 'true'
$env:HemoSeed__Passwords__Admission = Read-Host 'Password for hemo.admission'
$env:HemoSeed__Passwords__Supervisor = Read-Host 'Password for hemo.supervisor'
$env:HemoSeed__Passwords__Viewer = Read-Host 'Password for hemo.viewer'
$env:HemoSeed__ApiBaseUrl = 'http://127.0.0.1:1002'
dotnet run --project web/web.csproj --no-launch-profile
```

تنظیمات اصلی اتصال DB و JWT شرکت نیز باید قبلاً موجود باشند. Seed فقط در Development و با فعال‌سازی صریح اجرا می‌شود؛ در اجرای دوباره رمزها یا حساب‌های موجود بازنویسی نمی‌شوند. اعتبار تخصیص نقش یک سال است؛ تمدید توسط مدیر. Seed در دیتابیس تولید اجرا نشود. هیچ حساب سوپرادمینی ایجاد نمی‌شود. در DB جدید، صرفاً نقش ساختاری ریشه و metadata زیرسیستم Identity در صورت خالی بودن ساخته می‌شود. سیاست OTP موجود دست‌نخورده است؛ اگر در محیط آزمایش فعال باشد، حساب‌ها باید شماره موبایل آزمایشی مجاز داشته باشند. هیچ بیمار واقعی seed نمی‌شود.
