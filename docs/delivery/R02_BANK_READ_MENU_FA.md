# R02 — دسترسی صفحات خواندنی بانک خون
مبنا: main f5ac38dd، PR17 ادغام شده. همراه Frontend PR23.
## فعال‌سازی
Development: HemoSeed__Enabled=true، HemoSeed__IncludeBloodBank=true، HemoSeed__IncludeBankReadPages=true. گزینه‌های قبلی درخواست خون و پذیرش را مطابق استقرار قبلی حفظ کنید.
- /hemo/bank: منوی سابق با GUID و مجوزهای موجود، انتقال برگ آزمایشی دقیق به /Hemo.
- /hemo/bank/inventory: فقط bank-inventory.
- /hemo/bank/reports: bank-stats الزامی، inventory و temperature-history فقط اگر قبلاً در نقش مجاز بوده باشند.
- نقش‌های hemo.bloodbank و hemo.bank.supervisor از Seed قبلی؛ hemo.dashboard.manager در صورت فعال بودن Seed مربوطه فقط آمار. حساب یا رمز جدید نیست.
- مجوز مستقیم root، عمومی، کل منو یا زوج جدید نقش/API ساخته نمی‌شود. منوی خارجی ناسازگار باعث خطا و rollback می‌شود.
## تست
دو تست policy و SQL برای نقش‌ها، عدم گسترش مجوزها، GUID و سلسله‌مراتب و تکرارپذیری. CI و UAT واقعی هنوز تأیید نشده‌اند. Migration یا اجرای Production ندارد. عملیات بانک و جزئیات واحد در مرحله بعد.
