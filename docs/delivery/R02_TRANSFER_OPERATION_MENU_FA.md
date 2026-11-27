# R02 — دسترسی عملیات تحویل و تزریق
مبنا: main 2c9d0941؛ PR21 ادغام شده. همراه Frontend PR27.
## فعال‌سازی Development
HemoSeed__Enabled=true، HemoSeed__IncludeTransfusion=true، HemoSeed__IncludeTransferOperationPages=true. گزینه آخر صفحات خواندنی را نیز فعال می‌کند. تنظیمات قبلی پذیرش، درخواست و بانک خون حفظ شوند.
- /hemo/transfusion/dispatch: منوی قابل مشاهده؛ tx-candidates، tx-dispatch و tx-detail لازم هستند؛ فقط دو API صدور و کاندیدا نگاشت می‌شوند.
- /hemo/transfusion/detail/:guid: منوی خصوصی مخفی؛ tx-detail لازم است. عملیات موجود فقط برای نقش دارای همان مجوز قبلی نگاشت می‌شوند.
- پرستار اول، شاهد مستقل، پزشک، کارشناس و مسئول بانک طبق Seed قبلی؛ مدیر آمار مجوز جزئیات ندارد. هیچ زوج نقش/API جدید، مجوز عمومی یا حساب جدید تعریف نشده است.
## آزمون
یک تست سیاست نقش جدید و توسعه تست SQL برای ارتقا، نگاشت دقیق و تکرارپذیری. قواعد بالینی، Migration و قرارداد API تغییر نکرده‌اند. CI و UAT واقعی هنوز تأیید نشده‌اند.
