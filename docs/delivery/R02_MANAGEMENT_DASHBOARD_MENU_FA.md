# R02 — داشبورد مدیریت بیمارستان
مبنا: main 9dbfcd16؛ همراه Frontend PR35.
## تنظیم Development
HemoSeed__Enabled=true، HemoSeed__IncludeDashboardPages=true. گزینه آخر ماژول‌های API کیفیت و مراحل وابسته و حساب موجود در تعریف Seed به نام hemo.dashboard.manager را فعال می‌کند، بدون ایجاد داده آموزشی اجباری. HemoSeed__Passwords__DashboardManager رمز یکتای حداقل ۱۶ کاراکتر برای نصب اولیه؛ تنظیمات رمز سایر حساب‌های نصب اولیه نیز لازم است. برای نمونه‌های آموزشی و پذیرش تاریخی IncludeDashboardSamples=true اختیاری است؛ تمام داده‌های Demo صرفاً ساختگی‌اند.
- منوی خصوصی /hemo/dashboard زیر /Hemo و نگاشت دقیق هفت API آمار. صفحه پیش‌فرض شرکت تغییر نمی‌کند.
- نقش‌های دارای دسترسی آماری قبلی به همان APIهای آماری نگاشت می‌شوند؛ هیچ زوج نقش/API اضافه برای نقش‌های موجود و هیچ دسترسی کل منو ایجاد نمی‌شود.
- مدیر آمار فقط stats، request-stats، bank-stats، tx-stats، hv-stats، quality-equipment-stats و quality-stats را دریافت می‌کند. هیچ مجوز List، Detail یا عملیات نوشتنی ندارد.
- برای فعال‌سازی مسیرهای عملیاتی و لینک‌های مرتبط، گزینه‌های IncludeAdmissionCompletePages، IncludeRequestFormPages، IncludeRequestDetailPages، IncludeBankUnitPages، IncludeBankCompletePages، IncludeTransferOperationPages، IncludeHvReportPages و IncludeQualityOperationPages مطابق مراحل قبلی فعال بمانند. نقش‌های عملیاتی طبق مجوز واقعی صفحات لینک دریافت می‌کنند.
## آزمون و محدودیت
دو تست سیاست نقش و SQL Server برای حفظ زوج‌ها، نقش صرفاً آماری و تکرارپذیری. Migration و endpoint جدید ندارد؛ CI و UAT واقعی هنوز تأیید نشده‌اند. تجمیع چندمرکزی و تاریخچه موجودی در قرارداد موجود ارائه نشده است.
