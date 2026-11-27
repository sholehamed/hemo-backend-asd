# R02 — دسترسی صفحات خواندنی تحویل و تزریق
مبنا: main 3063ffaf؛ PR20 ادغام شده. همراه Frontend PR26.
## فعال‌سازی Development
HemoSeed__Enabled=true، HemoSeed__IncludeTransfusion=true، HemoSeed__IncludeTransferReadPages=true. تنظیمات قبلی پذیرش، درخواست و بانک خون حفظ شوند.
- /hemo/transfusion: منوی اصلی موجود با GUID و مجوزهای قبلی، انتقال به ریشه /Hemo.
- /hemo/transfusion/tasks و /hemo/transfusion/records: tx-list.
- /hemo/transfusion/vitals: tx-list و tx-vital.
- /hemo/transfusion/monitoring: tx-list و tx-doctor-note.
- /hemo/transfusion/returns: tx-list و حداقل یکی از tx-return، tx-accept-return، tx-dispose.
صفحات خواندنی فقط API فهرست را نگاشت می‌کنند. مجوزهای عملیات توسعه نمی‌یابند. نقش‌های قبلی پرستار، پزشک، شاهد، کارشناس و مسئول بانک خون طبق Seed موجود؛ مدیر آمار فقط آمار. هنگام فعال‌سازی اولیه IncludeTransfusion حساب hemo.nurse.witness طبق Seed قدیمی با رمز تنظیم‌شده توسط مدیر ایجاد می‌شود؛ حساب تازه‌ای در این PR تعریف نشده است.
## آزمون
یک تست سیاست نقش و یک تست SQL حفظ زوج نقش/API، GUID، مسیر، دسترسی خصوصی و تکرارپذیری. CI و UAT واقعی هنوز تأیید نشده‌اند. بدون Migration یا تغییر قواعد بالینی.
