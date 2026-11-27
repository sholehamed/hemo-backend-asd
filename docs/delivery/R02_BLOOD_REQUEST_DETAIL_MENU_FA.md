# R02 — دسترسی جزئیات درخواست خون
وابسته به Backend PR #15 و همراه Frontend PR #21. با HemoSeed__Enabled=true، HemoSeed__IncludeBloodRequests=true و HemoSeed__IncludeRequestDetailPages=true در Development فعال می‌شود. این گزینه صفحات خواندنی را نیز فعال می‌کند.
- /hemo/requests/detail/:guid: detail و فقط عملیات Submit/Cancel/Decide که قبلاً در نقش مجاز بوده‌اند.
- /hemo/requests/bank/:guid: فقط bankDetail.
- هر دو منوی مخفی، غیرعمومی و با والد /Hemo هستند. هیچ مجوز تازه در سطح زوج نقش/API یا نقش سراسری ایجاد نمی‌شود.
- مالکیت، مرکز، وضعیت، نسخه و تصمیم بالینی همچنان در سرویس فعلی کنترل می‌شوند. داده واقعی/production اجرا نشده است.
- آزمون SQL موجود با سناریوی ارتقا و تکرار Seed و کنترل زوج‌های دقیق نقش/عملیات تکمیل شده؛ CI در زمان نگارش هنوز تأیید نشده. فرم ایجاد و ویرایش در slice بعدی.
