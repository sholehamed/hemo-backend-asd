# ST-08 | حساب‌ها و Seed یکپارچه
این مرحله حساب جدید نمی‌سازد؛ چهارده حساب قبلی با مجوزهای دقیق ST02–07 استفاده می‌شوند. فقط در محیط Development و دیتابیس مجزای آزمایشی شرکت فعال کنید. اطلاعات، گواهی‌ها و تأییدهای ساختگی هیچ ارزش بالینی ندارند.

| کلید تنظیم رمز | نام کاربری | مسئولیت |
|---|---|---|
| Admission | hemo.admission | ثبت پذیرش و بررسی هویت/سوابق/آمادگی |
| Supervisor | hemo.supervisor | پذیرش و اصلاح هویت با ثبت دلیل |
| Viewer | hemo.viewer | مشاهده پذیرش |
| Nurse | hemo.nurse | درخواست، تحویل، پایش و گزارش واکنش |
| Doctor | hemo.doctor | درخواست، تصمیم، بررسی پزشکی و توقف مستند |
| BloodBank | hemo.bloodbank | دریافت واحد، آزمایش، رزرو، پایش دما و مرجوعی |
| BankSupervisor | hemo.bank.supervisor | بازرسی، صدور، امحا و بازبینی QC |
| WitnessNurse | hemo.nurse.witness | شاهد مستقل تطبیق |
| HvOfficer | hemo.hv.officer | بررسی، RCA، CAPA، گزارش |
| HvReviewer | hemo.hv.reviewer | تأیید مستقل و بستن پرونده واکنش |
| EquipmentTech | hemo.equipment.tech | شناسنامه، سرویس و حادثه |
| EquipmentReviewer | hemo.equipment.reviewer | آزادسازی فنی مستقل |
| QualityOfficer | hemo.quality.officer | QC، کمیته، ممیزی، آموزش، اطلاعات مرکز |
| QualityReviewer | hemo.quality.reviewer | بازبینی مستقل کیفیت |

## اجرای اولیه (PowerShell 7)
ابتدا Connection String و سایر تنظیمات موجود شرکت را به محیط مجزای توسعه متصل کنید؛ مطمئن شوید هیچ سرویس/دیتابیس Production مقصد نیست. کلیدهای زیر فقط برای HemoDemoSeed هستند. رمزهای متفاوت ۱۶ تا ۲۵۶ کاراکتری برای حساب‌های ایجادنشده لازم است؛ رمز کاربر موجود بازنشانی نمی‌شود.
```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:HemoSeed__Enabled = 'true'
$env:HemoSeed__IncludeDeliverySamples = 'true'
$keys = @('Admission','Supervisor','Viewer','Nurse','Doctor','BloodBank','BankSupervisor','WitnessNurse','HvOfficer','HvReviewer','EquipmentTech','EquipmentReviewer','QualityOfficer','QualityReviewer')
try {
  foreach ($key in $keys) {
    $secret = Read-Host "Password for $key (unique, 16+ characters; blank only if account already exists)" -MaskInput
    if ($secret) { [Environment]::SetEnvironmentVariable("HemoSeed__Passwords__$key", $secret, 'Process') }
  }
  dotnet run --project web/web.csproj --no-launch-profile
} finally {
  foreach ($key in $keys) { [Environment]::SetEnvironmentVariable("HemoSeed__Passwords__$key", $null, 'Process') }
  Remove-Item Env:HemoSeed__Enabled,Env:HemoSeed__IncludeDeliverySamples -ErrorAction SilentlyContinue
  Remove-Variable secret -ErrorAction SilentlyContinue
}
```
رمزها را در Git، مستندات، خروجی CI یا فایل تنظیمات نسخه‌بندی‌شده نگذارید. Seed پیش‌فرض خاموش است؛ فعال‌سازی در محیط غیر Development خطا می‌دهد. داده‌های نمونه جدا از داده عملیاتی هستند. اجرای مجدد پرونده‌های تمرین‌شده، رمز و تاریخ انقضا را بازنویسی نمی‌کند. برای تکرار تمرین از دیتابیس آزمایشگاهی تازه یا داده جدید استفاده کنید؛ دیتابیس مشترک حذف نشود.

| شناسه تمرین | وضعیت اولیه |
|---|---|
| DEMO-ST05-01 / DEMO-UNIT-01 | آماده صدور و تکمیل تزریق |
| DEMO-ST05-02 / DEMO-UNIT-02 | آماده صدور و تمرین مرجوعی |
| DEMO-ST05-03 / DEMO-UNIT-03 | آماده صدور و تمرین توقف/نگرانی |
| DEMO-ST05-04 / DEMO-UNIT-04 | فرآورده آماده پس از ذوب |
| DEMO-ST06-01 | حواله متوقف، آماده گزارش اولیه |
| DEMO-ST06-02 / DEMO-HV-02 | گزارش ثبت شده، بررسی پزشک |
| DEMO-ST06-03 / DEMO-HV-03 | بررسی بانک شده، تصمیم RCA |
| DEMO-ST06-04 / DEMO-HV-04 | پیگیری CAPA |
| DEMO-ST07-FRIDGE / DEMO-ST07-U1 | تجهیز و واحد نمونه |
| DEMO-ST07-CENTRIFUGE / DEMO-EQI-01 | حادثه باز و سرویس |
| DEMO-ST07-FREEZER / DEMO-ST07-WATERBATH | توقف یا سررسید |
| DEMO-ST07-QC-01 / BTC-01 / AUD-01 / TRN-01 | QC، کمیته، ممیزی، آموزش |

جزئیات هر نمونه در اسناد مرحله‌های قبلی: [تزریق](../transfusion/DEMO_ACCOUNTS_FA.md)، [کیفیت](../quality/DEMO_ACCOUNTS_FA.md). فلگ جدید همان سه Sample Seed موجود را در یک تراکنش اجرا می‌کند؛ منطق Seed زیرساخت شرکت تغییر نکرده است.
