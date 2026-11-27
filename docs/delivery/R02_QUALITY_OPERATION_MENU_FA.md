# R02 — منو و دسترسی عملیات کیفیت
مبنا: main d9dcf8b3؛ همراه Frontend PR34.
## فعال‌سازی Development
HemoSeed__Enabled=true، HemoSeed__IncludeQuality=true، HemoSeed__IncludeQualityOperationPages=true. گزینه آخر منوهای خواندنی کیفیت و عملیات تجهیزات را نیز فعال می‌کند. گزینه‌های مراحل قبلی حفظ شوند؛ IncludeQualitySamples داده ساختگی آموزشی موجود را فعال می‌کند.
- منوهای qc، committee، audit، training: نگاشت Create فقط برای نقش‌های دارای همان مجوز قبلی.
- مسیرهای جزئیات نوع‌دار /hemo/quality/{qc|committee|audit|training}/:guid: خصوصی و مخفی؛ نگاشت دقیق Detail، Edit، Line، Submit، Approve و در صورت نوع مربوطه Owners، Progress، Verify و Close. Reject و Return در سرور با مجوزهای Approve و Verify کنترل می‌شوند.
- /hemo/quality/facility: مشاهده مشخصات مرکز؛ FacilitySave فقط برای دارنده مجوز پیشین.
- QualityOfficer (hemo.quality.officer): ایجاد/اصلاح/ارسال پرونده و ذخیره مشخصات مرکز؛ QualityReviewer (hemo.quality.reviewer): بازبینی مستقل، تأیید مصوبات، بستن کمیته و مشاهده مشخصات مرکز. مسئول مصوبه و بانک خون مطابق مجوزهای موجود. حساب تازه ایجاد نشده است.
## آزمون و محدودیت
یک تست سیاست نقش جدید و توسعه تست SQL برای ارتقا، نگاشت دقیق، حفظ زوج‌های نقش/API و تکرارپذیری. هیچ Migration و تغییر در قواعد یا قراردادهای سرور. CI و UAT واقعی هنوز تأیید نشده‌اند.
