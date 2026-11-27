# R02 — دسترسی صفحات خواندنی هموویژیلانس
مبنا: main 7897735b؛ همراه Frontend PR29.
## فعال‌سازی Development
HemoSeed__Enabled=true، HemoSeed__IncludeHemovigilance=true، HemoSeed__IncludeHvReadPages=true. گزینه‌های منوهای مراحل قبل حفظ شوند.
- /hemo/hv: منوی اصلی موجود با GUID و مجوزهای قبلی، زیر /Hemo.
- /hemo/hv/cases: hv-list.
- /hemo/hv/doctor: hv-list و hv-doctor.
- /hemo/hv/bank: hv-list و hv-bank.
- /hemo/hv/rca: hv-list و یکی از hv-investigate یا hv-rca.
- /hemo/hv/capa: hv-list و یکی از hv-assign، hv-progress، hv-verify یا hv-close.
صفحات فوق فقط hv-list را نگاشت می‌کنند و دسترسی عملیات توسعه نمی‌یابد. حساب‌های hemo.hv.officer و hemo.hv.reviewer از Seed موجود؛ در صورت فعال‌سازی اولیه، رمز هر حساب از تنظیمات HemoSeed:Passwords:HvOfficer و HemoSeed:Passwords:HvReviewer توسط مدیر تعیین شود. پرستار، پزشک و بانک خون همان حساب‌های قبلی هستند. مدیر آمار صرفاً آمار طبق مجوز قبلی.
## آزمون و محدودیت
یک تست سیاست نقش و یک تست SQL Server برای حفظ GUID، زوج نقش/API، خصوصی بودن منو و تکرارپذیری. CI و UAT واقعی هنوز تأیید نشده‌اند. هیچ Migration، حساب تازه یا تغییر قواعد بالینی در این PR وجود ندارد. داده آموزشی موجود با HemoSeed:IncludeHemovigilanceSamples و تنظیمات نمونه وابسته طبق Seed قدیمی فعال می‌شود؛ برای استفاده بالینی نیست.
