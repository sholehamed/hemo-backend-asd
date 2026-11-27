# R02 — راهنمای تحویل یکپارچه و دروازه پذیرش
مبنا: Backend main f2f1e3d0 / Frontend Integration a4d10b4b.
## نصب آموزشی در Development
فقط روی دیتابیس آزمایشی جداگانه. تنظیم اتصال فعلی Infrastructure/DependencyInjection.cs وابسته به شرکت و ثابت است؛ قبل از اجرای web، متولی شرکت باید مقصد را بررسی و ایزوله کند. هیچ رمز یا رشته اتصال در این سند درج نشده است.
```json
{
  "HemoSeed": {
    "Enabled": true,
    "IncludeR02Pages": true,
    "IncludeDashboardSamples": true
  }
}
```
IncludeR02Pages تمام ۶۲ مسیر فعال، مجوزهای متناظر و ۱۵ حساب را فعال می‌کند؛ IncludeDashboardSamples اختیاری و فقط نمونه‌های مصنوعی قبلی است. فلگ‌های قبلی نیز سازگار باقی می‌مانند. رمز یکتای ۱۶ تا ۲۵۶ کاراکتر در secret/environment برای هر حساب ایجادنشده لازم است.
| کلید رمز (HemoSeed__Passwords__...) | نام کاربری |
|---|---|
| Admission | hemo.admission |
| Supervisor | hemo.supervisor |
| Viewer | hemo.viewer |
| Nurse | hemo.nurse |
| Doctor | hemo.doctor |
| BloodBank | hemo.bloodbank |
| BankSupervisor | hemo.bank.supervisor |
| WitnessNurse | hemo.nurse.witness |
| HvOfficer | hemo.hv.officer |
| HvReviewer | hemo.hv.reviewer |
| EquipmentTech | hemo.equipment.tech |
| EquipmentReviewer | hemo.equipment.reviewer |
| QualityOfficer | hemo.quality.officer |
| QualityReviewer | hemo.quality.reviewer |
| DashboardManager | hemo.dashboard.manager |
حساب‌های جدیدی تعریف نشده‌اند. برای اجرای مجدد، رمز حساب موجود بازنویسی نمی‌شود. Seed در Production ممنوع است.
## قرارداد و بررسی یکپارچگی
- r02-active-route-contract.json حاوی ۶۲ مسیر دقیق Angular و Seed متناظر است؛ تطبیق استاتیک دو مخزن در مبنای فوق بدون اختلاف انجام شد. مسیر /hemo/dashboard جایگزین Home قدیمی HEMO، و /hemo/quality/facility مسیر فعال مشخصات مرکز است. صفحه / شرکت حفظ شده.
- DeliverySqlWorkflowTests با IncludeR02Pages و SQL واقعی، ایجاد پذیرش تا تکمیل تزریق/واکنش/آمادگی ترخیص را اجرا می‌کند؛ پیش از گردش، تطبیق ۶۲ منو، ۱۵ نقش، دسترسی Handler واقعی CheckPagePermission و عدم گسترش زوج‌های Role/API را بررسی می‌کند.
- QualitySqlWorkflowTests نصب کامل همراه نمونه‌ها و تکرار بدون افزایش منو/نگاشت/Grant، هفت API مدیر آمار و منع درخواست فهرست/جزئیات را بررسی می‌کند. منوهای اصلی قدیمی ممکن است به‌علت دسترسی آماری اجازه ورود صفحه بدهند؛ مثلاً /hemo/requests. این به معنای مجوز فهرست نیست؛ Capability و Handler مستقل آن را رد می‌کنند.
- CI بک‌اند SQL Server موقت می‌سازد؛ HEMO_TEST_SQL فقط رشته اتصال آزمایشی با مجوز ایجاد/حذف DB اختصاصی. CI فرانت Build، تست مدل و Karma HTTP fixture دارد. این‌ها Login/HTTP واقعی یا E2E مرورگر با backend واقعی نیستند.
## پذیرش نهایی و موانع
پیش از merge نهایی فرانت به main، PRهای مرحله نهایی باید سبز و بررسی شوند؛ PR ادغام اصلی باید جداگانه CI مقصد main را بگذراند. استقرار واقعی، Login/Captcha/OTP/انتخاب نقش، چاپ واقعی مچ‌بند، اجرای مرورگر با API و DB آزمایشی واقعی، بررسی سیاست فنی/بالینی، تنظیم امن اتصال و قرارداد خدمات بیرونی توسط متولی شرکت لازم است. تنظیمات مشترک زیرساخت یا Identity در این تغییر دست نخورده‌اند. ریسک شناخته‌شده: تعیین تکلیف کسری درخواست پس از مرجوعی/مصرف ناقص هنوز سیاست مصوب ندارد؛ خطای شبکه پیش از پاسخ قطعی ایجاد پیش‌نویس مستلزم بررسی پیش از تکرار است. نگهداری سابقه موجودی و تجمیع چندمرکزی خارج از قرارداد جاری هستند. هیچ تست نرم‌افزاری جایگزین پذیرش بالینی نیست.
