# نقش‌ها و داده آموزشی ST07
| نام کاربری | نقش |
|---|---|
| hemo.equipment.tech | کارشناس تجهیزات (جدید) |
| hemo.equipment.reviewer | بازبین فنی تجهیزات (جدید) |
| hemo.quality.officer | مسئول کیفیت (جدید) |
| hemo.quality.reviewer | بازبین مستقل کیفیت (جدید) |
| hemo.bloodbank | ثبت QC، مشاهده تجهیزات/ثبت دما و حادثه، اجرای مصوبه تخصیص‌یافته |
| hemo.bank.supervisor | بازبینی QC، آمار، اجرای مصوبه |
| hemo.nurse / hemo.doctor | مشاهده و اجرای مصوبه تخصیص‌یافته |
| hemo.hv.officer / hemo.hv.reviewer | مشاهده پرونده‌های مدیریتی و گزارش تجمیعی |
فهرست دقیق دسترسی‌های افزوده در ACCESS_MANIFEST.json. رمزهای مستقل حداقل ۱۶ کاراکتر برای حساب‌های ایجادنشده. Seed فقط Development و opt-in.
```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:HemoSeed__Enabled = 'true'
$env:HemoSeed__IncludeQuality = 'true'
$env:HemoSeed__IncludeQualitySamples = 'true'
$env:HemoSeed__Passwords__EquipmentTech = Read-Host 'hemo.equipment.tech password'
$env:HemoSeed__Passwords__EquipmentReviewer = Read-Host 'hemo.equipment.reviewer password'
$env:HemoSeed__Passwords__QualityOfficer = Read-Host 'hemo.quality.officer password'
$env:HemoSeed__Passwords__QualityReviewer = Read-Host 'hemo.quality.reviewer password'
dotnet run --project web/web.csproj --no-launch-profile
```
حساب‌های قبلی در صورت نبودن رمز مستقل لازم دارند. فلگ ST07 نقش‌های قبلی را فعال می‌کند اما نمونه‌های ST05/ST06 فقط با فلگ‌های خودشان ایجاد می‌شوند.
| شناسه | سناریو |
|---|---|
| DEMO-ST07-FRIDGE | تجهیز عملیاتی مصنوعی با ثبت دمای آموزشی |
| DEMO-ST07-CENTRIFUGE / DEMO-EQI-01 | حادثه باز، رفع نقص و سرویس/بازبینی مستقل |
| DEMO-ST07-FREEZER | سررسید کالیبراسیون گذشته |
| DEMO-ST07-WATERBATH | توقف فنی، آماده تمرین سرویس |
| DEMO-ST07-QC-01 | QC ارسال‌شده، آماده بررسی مستقل |
| DEMO-ST07-BTC-01 | مصوبه باز برای hemo.nurse |
| DEMO-ST07-AUD-01 | ممیزی در پیش‌نویس، ردیف نمونه |
| DEMO-ST07-TRN-01 | دوره، شرکت‌کننده ثبت‌نام‌شده آماده حضور |
| اطلاعات مرکز | بیمارستان آزمایشی و پانویس آموزشی |
همه داده‌ها مصنوعی و غیرقابل استفاده بالینی/فنی؛ seed تکراری نباید داده تمرین‌شده را بازنویسی کند. شناسنامه کدهای تاریخی ST05/06 به صورت خودکار ساخته نمی‌شود؛ در گزارش کدهای بدون شناسنامه قابل مشاهده‌اند تا مسئول با مستندات واقعی ثبت کند.
