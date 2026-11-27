# کاربران و داده تمرینی ST06
| نام کاربری | دسترسی |
|---|---|
| hemo.nurse | گزارش اولیه، مشاهده تکمیلی، اجرا/پیشرفت CAPA تخصیص‌یافته |
| hemo.doctor | بررسی پزشک، مشاهده تکمیلی، اجرای CAPA تخصیص‌یافته |
| hemo.bloodbank / hemo.bank.supervisor | بررسی بانک، مشاهده تکمیلی، اجرای CAPA تخصیص‌یافته |
| hemo.hv.officer | جدید: بررسی مسئول، تصمیم RCA، تعیین مسئول و موعد CAPA، انتشار RCA، گزارش و آرشیو، ثبت وضعیت گزارش بیرونی |
| hemo.hv.reviewer | جدید: بازبین مستقل CAPA و بستن پرونده، مشاهده گزارش و آرشیو |
Seed همان مکانیزم Development opt-in است و مراحل قبلی را برای نقش‌ها فعال می‌کند. حساب شاهد و حساب پذیرش تغییر نقش ندارند. رمزهای ۱۶+ کاراکتری مستقل از پیکربندی برای حساب‌های ایجادنشده، بدون رمز پیش‌فرض.
```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:HemoSeed__Enabled = 'true'
$env:HemoSeed__IncludeHemovigilance = 'true'
$env:HemoSeed__IncludeHemovigilanceSamples = 'true'
$env:HemoSeed__Passwords__HvOfficer = Read-Host 'Password for hemo.hv.officer'
$env:HemoSeed__Passwords__HvReviewer = Read-Host 'Password for hemo.hv.reviewer'
dotnet run --project web/web.csproj --no-launch-profile
```
Migration ابتدا روی DB توسعه اعمال شود. IncludeHemovigilanceSamples نقش‌ها را نیز فعال می‌کند، ولی نمونه ST05 را فقط با فلگ مستقل خودش ایجاد می‌کند. داده‌های نمونه ST06 از ST05 مستقل و با پیش‌نیازهای مصنوعی بیمار/پذیرش/درخواست/آزمایش/واحد/حواله و توقف ایجاد می‌شوند:
| پرونده | حواله | واحد | وضعیت |
|---|---|---|---|
| DEMO-ST06-01 | DEMO-TX-ST06-01 | DEMO-HV-UNIT-01 | توقف ثبت‌شده؛ بدون پرونده واکنش، آماده گزارش |
| DEMO-ST06-02 | DEMO-TX-ST06-02 | DEMO-HV-UNIT-02 | DEMO-HV-02 گزارش‌شده، بررسی پزشک |
| DEMO-ST06-03 | DEMO-TX-ST06-03 | DEMO-HV-UNIT-03 | DEMO-HV-03 بررسی بانک شده، تصمیم RCA |
| DEMO-ST06-04 | DEMO-TX-ST06-04 | DEMO-HV-UNIT-04 | DEMO-HV-04 RCA منتشر، CAPA باز تخصیص‌یافته به پرستار |
همه اطلاعات ساختگی، بدون هویت واقعی یا نتیجه تشخیصی بالینی؛ فقط برای تمرین. Seed تکراری پرونده یا اقدام تمرین‌شده را بازنویسی نمی‌کند؛ نمونه‌ها را در دیتابیس تازه ایجاد کنید. زمان‌های فرضی برای تمرین نرم‌افزار هستند. Production: تخصیص نقش و مجوز مصوب بر اساس ACCESS_MANIFEST.json، بدون اجرای seed آزمایشی.
