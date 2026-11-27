# R02 — منوی پایلوت شرکت
- فعال‌سازی فقط Development و HemoSeed:Enabled=true و HemoSeed:IncludeCompanyMenuPilot=true. گزینه جدید IncludeAdmissionRouteMenus را نیز فعال می‌کند.
- /Hemo ریشه خصوصی و اختصاصی Seed است و تنها مسیر فعال فعلی /admission/stats به عنوان فرزند آن نمایش داده می‌شود. شناسه Menu و RoleAccess/MenuApi صفحه ثابت می‌مانند. مسیرهای هنوز متصل‌نشده زیر /Hemo قرار نمی‌گیرند.
- در دیتابیس موجود، فقط leaf متعلق به R02 و بدون فرزند قابل انتقال است. در صورت وجود /Hemo متعلق به شرکت، seed به جای دستکاری یا تصاحب آن متوقف می‌شود. محیط واقعی از مدیریت منوی موجود استفاده می‌کند.
- هیچ ApiGUID، RoleAccess یا کاربر جدیدی برای ریشه تعریف نمی‌شود. منطق موجود MenuBindUserMenu برای نمایش ancestor کافی است. تست SQL predicate همان منطق را بررسی می‌کند؛ اجرای handler با mapper و HTTP واقعی هنوز انجام نشده.
- Seed تکرارپذیر است؛ غیرفعال کردن گزینه rollback تغییرات قبلی نیست. ساخت Package و OrgLocationPackage و مسیر واقعی صدور/refresh توکن هنوز نیازمند تعیین قرارداد شرکت است. ایجاد پکیج فرضی یا اعطای نقش عمومی مجاز نیست.
- نتیجه CI در زمان ثبت: تایید نشده. هیچ تغییر در زیرساخت عمومی Identity، MenuService، environment یا frontend اعمال نشده است.
