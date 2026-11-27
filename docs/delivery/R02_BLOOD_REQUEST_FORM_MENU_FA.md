# R02 — دسترسی فرم‌های درخواست خون
مبنا: backend main 750a5ca3؛ PRهای 15 و 16 با CI موفق ادغام شده‌اند.
## فعال‌سازی آزمایشی
Development: HemoSeed__Enabled=true، HemoSeed__IncludeBloodRequests=true، HemoSeed__IncludeRequestFormPages=true. این گزینه مسیرهای خواندنی و جزئیات را نیز فعال می‌کند. حساب و رمز جدید، Migration و اجرای Production ندارد.
## مسیرها
- /hemo/requests/new: request-nurse + request-detail + admission list/detail؛ Submit فقط اگر قبلاً مجاز باشد.
- /hemo/requests/orders: request-doctor + request-detail + admission list/detail؛ Submit فقط اگر قبلاً مجاز باشد.
- /hemo/requests/edit/:guid: مخفی، request-edit + request-detail + admission detail؛ Submit فقط با مجوز قبلی.
- از مجوزهای موجود در منوی اصلی پذیرش و درخواست خون استفاده می‌شود؛ هیچ زوج نقش/API تازه و هیچ مجوز تصمیم پزشک روی فرم ثبت یا ویرایش ایجاد نمی‌شود. سرویس موجود مالکیت، وضعیت، نسخه و مرکز را کنترل می‌کند.
## آزمون و پذیرش
یک تست policy جدید و توسعه تست SQL موجود برای ارتقا، دقیق بودن زوج‌ها و تکرارپذیری. CI جدید و تست HTTP واقعی هنوز تأیید نشده‌اند.
