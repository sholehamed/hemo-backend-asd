# ST-08 | راهنمای توسعه‌دهنده، استقرار و کنترل تحویل
> سند تاریخی ST08؛ برای نسخه جاری Angular 19 و دستورهای معتبر، Seed ۱۵ حساب، CI مرورگر واقعی و استقرار آزمایشی به [R03_FINAL_HANDOVER_FA.md](R03_FINAL_HANDOVER_FA.md) مراجعه کنید. دستور npm run test:e2e تاریخی در package.json جاری موجود نیست. قبل از اجرای web، اتصال ایزوله باید توسط مسئول شرکت تأیید شود.
## معماری و محدوده
Backend یکپارچه .NET 9 و یک ApplicationDbContext؛ Domain (Entity)، Application (CQRS/Services)، Infrastructure (EF/SQL/Seed)، web (EndpointGroup). Frontend Angular 22 standalone، Angular Material، RTL و HTTP service. ریپوهای قدیمی و hemo-raw فقط مرجع. هیچ تغییر عمومی Identity، CORS، Connection String، Middleware، Pipeline و Seed پایه مجاز نیست؛ تصمیم مالک در [Scope](../HEMO_ASD_SCOPE_OVERRIDE_FA.md).
## ساخت و تست
از ریشه backend:
```bash
dotnet build web/web.csproj -c Release
dotnet test tests/Hemo.Admission.Tests -c Release --filter "Category!=SqlServer"
# HEMO_TEST_SQL must point to an isolated test SQL Server with permission to create/delete test DBs.
dotnet test tests/Hemo.Admission.Tests -c Release --filter "Category=SqlServer"
```
CI موجود SQL Server 2022 موقت با رمز تصادفی می‌سازد؛ آزمون‌ها migration و handlerهای واقعی روی DB جدا اجرا می‌شوند. هیچ Connection String تولید در HEMO_TEST_SQL نگذارید.
از ریشه frontend، Node مطابق .node-version:
```bash
npm install --no-audit --no-fund
npm test
npm run build
npx playwright install --with-deps chromium
npm run test:e2e
```
Playwright فعلی با HTTP contract fixtures اجرا می‌شود، نه backend واقعی. SQL workflow tests از synthetic actor و handler واقعی استفاده می‌کنند، نه مسیر کامل HTTP login. آزمون مرورگر زنده به محیط توسعه مشترک/تنظیمات مجاز نیاز دارد.
## استقرار
1. نسخه commit هر دو repo و نتایج CI را ثبت کنید؛ تا سبز شدن یا استثنای مصوب، merge/استقرار انجام نشود.
2. از SQL Server آزمایشی backup قابل بازیابی بگیرید و Migrationهای ST02–07 را ابتدا روی نسخه کپی‌شده بررسی کنید. Startup فعلی migration اجرا می‌کند؛ ابتدا تنها یک instance برای migration راه‌اندازی شود. ST08 تغییر schema ندارد.
3. Backend را با پیکربندی مصوب شرکت اجرا کنید؛ secretها بیرون Git. Seed آموزشی در Production خاموش بماند. [حساب‌ها](DEMO_ACCOUNTS_FA.md).
4. ثبت Api/MenuApi/RoleAccess طبق [Provision](../admission/PROVISION_FA.md) و manifestهای هر حوزه؛ grant مستقیم ترجیح دارد. هیچ نقش بالینی را superadmin نکنید. کاربر فقط در مرکز نقش فعال عمل کند.
5. Frontend build production را پشت reverse proxy هم‌مبدأ موجود با مسیرهای /Identity و /captchaendpoint قرار دهید؛ در توسعه proxy.conf.json با میزبان واقعی تنظیم شود. Backend/Frontend سازگار deploy شوند.
6. Smoke: Login/Captcha/OTP موجود، انتخاب نقش و مرکز، باز کردن داشبورد، دریافت capabilities، خطاهای 401/403، درخواست GET/POST آزمون مصنوعی، خروج و ورود مجدد. هیچ داده بیمار واقعی در محیط آزمایش.
7. Rollback: توقف deploy جدید، بازگرداندن زوج نسخه‌های سازگار؛ بازگردانی دیتابیس فقط طبق برنامه مصوب و پس از بررسی تغییرات/backup، نه downgrade یا حذف داده خودکار.
## تغییرات ST08
- HemoSeed:IncludeDeliverySamples opt-in؛ شامل ST05/06/07 Sample Seed و تمام نقش‌های قبلی. هیچ حساب جدید یا migration جدید.
- ReviewAdmission: برای Clearance/Ready، در تراکنش Serializable علاوه بر هویت/سوابق، درخواست پیش‌نویس/برگشتی/در انتظار پزشک، واحد مرتبط در گردش بانک، حواله ناتمام و کمبود تعداد Completed برای AwaitingBank بررسی می‌شوند. درخواست خون جدید نیز با خواندن Admission در Serializable ساخته می‌شود. پیام خطا فارسی؛ هیچ داده/وضعیت بالینی خودکار تغییر نمی‌کند.
- درخواست AwaitingBank پس از تزریق کامل همچنان همین status را دارد؛ شمارش Completed با ProductCode استفاده شده است. این یک gate اداری محافظه‌کارانه است، نه مجوز ترخیص پزشکی. مسیر تعیین تکلیف کسری درخواست پس از مصرف/مرجوعی بخشی از اقلام هنوز تعریف نشده؛ ممکن است آمادگی اداری مسدود بماند. نیازمند سیاست مالک و مرکز برای close/cancel remainder. هیچ override بدون مجوز ایجاد نشده. پرونده واکنش/CAPA می‌تواند پس از بستری باز باشد؛ الزام اداری/بالینی نهایی نیازمند مصوبه، فعلاً بررسی مستند به عهده نقش مجاز است.
## شروط پذیرش
- Seed تکرارپذیر و چهارده نقش بدون privilege اضافی؛ نمونه‌های تمام مراحل با شناسه مصنوعی.
- SQL regression از پذیرش تا تزریق، سناریوهای واکنش، تجهیزات، مجوز و scope. تست ST08 جدید و مسیر مثبت پس از تکمیل تزریق.
- مرورگر: همه fixtureهای قبلی و نمایش رد آمادگی ترخیص؛ آزمون live هنوز لازم است.
- بررسی manual: نقش‌های مستقل، چاپ مچ‌بند واقعی، قرارداد HIS/IBTO/ثبت احوال، سیاست بالینی/فنی مصوب، امنیت زیرساخت توسط متولی شرکت. تا تأیید این موارد آماده استفاده بالینی تلقی نشود.
- وضعیت واقعی هر check در Worklog و PR ثبت شود؛ نتیجه مشاهده‌نشده PASS نیست.
