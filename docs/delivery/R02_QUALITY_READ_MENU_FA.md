# R02 — دسترسی صفحات خواندنی تجهیزات و کیفیت
مبنا: main 79996811؛ همراه Frontend PR32.
## فعال‌سازی Development
HemoSeed__Enabled=true، HemoSeed__IncludeQuality=true، HemoSeed__IncludeQualityReadPages=true. گزینه‌های منوهای قبلی حفظ شوند؛ IncludeQualitySamples برای داده ساختگی آموزشی اختیاری است.
- /hemo/quality منوی اصلی موجود با GUID و دسترسی‌های قبلی حفظ می‌شود.
- equipment-dashboard: آمار تجهیزات؛ equipment: فهرست تجهیزات؛ maintenance: فهرست و مجوز سرویس؛ temperature: فهرست و مجوز ثبت دما؛ incidents: مشاهده حوادث.
- qc، committee، audit، training: مجوز فهرست مستقل هر نوع. فقط API خواندنی لازم برای هر مسیر جدید نگاشت می‌شود. هیچ زوج نقش/API تازه تعریف نمی‌شود.
## حساب‌های موجود
hemo.equipment.tech (EquipmentTech): کارشناس تجهیزات؛ hemo.equipment.reviewer (EquipmentReviewer): بازبین فنی؛ hemo.quality.officer (QualityOfficer): مسئول کیفیت؛ hemo.quality.reviewer (QualityReviewer): بازبین مستقل کیفیت. رمز حساب‌های جدید در نصب اولیه توسط مدیر از HemoSeed:Passwords:<Key> تعیین شود. این PR تعریف حساب تازه ندارد. حساب‌های قبلی بانک خون و مدیر آمار طبق مجوز موجود استفاده می‌شوند.
## آزمون و محدودیت
یک تست سیاست نقش و یک تست SQL Server برای حفظ GUID، دسترسی‌ها، منوهای خصوصی و تکرارپذیری. هیچ Migration یا تغییر قواعد بالینی/فنی. CI و UAT واقعی هنوز تأیید نشده‌اند؛ فرم‌های نوشتنی و جزئیات تجهیزات در مرحله بعد.
