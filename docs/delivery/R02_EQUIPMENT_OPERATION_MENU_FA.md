# R02 — منو و دسترسی عملیات تجهیزات
مبنا: main 6c355e54؛ همراه Frontend PR33.
## فعال‌سازی Development
HemoSeed__Enabled=true، HemoSeed__IncludeQuality=true، HemoSeed__IncludeEquipmentOperationPages=true. گزینه آخر صفحات خواندنی کیفیت را نیز فعال می‌کند. گزینه‌های سایر ماژول‌ها حفظ شوند؛ IncludeQualitySamples برای داده آموزشی موجود اختیاری است.
- /hemo/quality/equipment: نگاشت ثبت شناسنامه فقط به دارندگان مجوز قبلی.
- /hemo/quality/equipment/:guid: منوی خصوصی مخفی؛ نیازمند quality-equipment-detail؛ نگاشت دقیق دسترسی ویرایش، سرویس، توقف، آزادسازی، آرشیو، دما، حادثه و رفع نقص فقط مطابق مجوزهای موجود.
- EquipmentTech (hemo.equipment.tech): ثبت و سرویس، توقف، دما و رفع نقص؛ بدون آزادسازی. EquipmentReviewer (hemo.equipment.reviewer): بازبینی فنی، آزادسازی و آرشیو؛ بدون ثبت سرویس. حساب‌های بانک خون طبق مجوز قبلی. حساب تازه ایجاد نشده است.
## آزمون و محدودیت
یک تست سیاست نقش جدید و توسعه تست SQL برای ارتقا، نگاشت دقیق و تکرارپذیری. قواعد فنی و قرنطینه بانک خون موجود حفظ شده‌اند؛ هیچ Migration. CI و تست واقعی سرتاسری هنوز تأیید نشده‌اند.
