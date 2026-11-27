# R02 — منوی خواندنی درخواست خون
## پیش‌نیاز
Frontend Integration: PR #20، merge e0b4f4be، 116/116 CI. Backend main e098925a، 120/120 CI.
فقط Development با Seed فعال: HemoSeed__Enabled=true، HemoSeed__IncludeBloodRequests=true، HemoSeed__IncludeRequestReadPages=true. رمزهای حساب‌های تازه از تنظیمات خصوصی و مطابق راهنمای قبلی؛ هیچ رمز پیش‌فرض یا حساب تازه‌ای در این تغییر نیست.
## دسترسی
- /hemo/requests: همان منوی قبلی و همان مجوزها؛ انتقال برگ آزمایشی به /Hemo با حفظ GUID. منوی خارجی یا عمومی با شکل متفاوت به‌جای بازنویسی باعث خطا و rollback می‌شود.
- /hemo/requests/bank: فقط نقش دارای request-bank.
- /hemo/requests/approval و alias پنهان /hemo/requests/doctor: فقط نقش دارای هر دو request-list و request-decide. فقط BindPaging به صفحه نگاشت می‌شود؛ عملیات تصمیم در این صفحه فعال نیست.
- hemo.nurse: فهرست؛ hemo.doctor: فهرست و کارتابل؛ hemo.bloodbank و hemo.bank.supervisor (در صورت فعال بودن بانک): صف بانک؛ hemo.dashboard.manager (در صورت Seed قبلی): آمار بدون لیست بیمار.
- /Hemo مجوز مستقیم یا عمومی نمی‌گیرد؛ منو با مجوز برگ‌ها ظاهر می‌شود.
## آزمون
دو آزمون جدید policy و SQL برای حفظ زوج نقش/API، تکرارپذیری، GUID اصلی، نبود مجوز عمومی یا مستقیم root و مجوز دقیق هر alias. CI در زمان نگارش هنوز اجرا/تأیید نشده؛ پذیرش HTTP واقعی و فرم‌ها/جزئیات/ویرایش در slice بعدی. Migration، حساب جدید، تغییر زیرساخت شرکت و اجرای production نداریم.
