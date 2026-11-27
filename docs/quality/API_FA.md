# API ST07 — /Identity/HemoQuality
Bearer + UserRole و grant صریح؛ همه Query/Command در مرکز جاری.
- GET Capabilities: permissions + userGuid؛ POST EquipmentList / EquipmentStats / Incidents؛ GET EquipmentDetail/{guid}.
- POST EquipmentCreate/Edit/Service/Hold/Release/Archive، IncidentReport/Resolve، EquipmentTemperature. نسخه رکورد الزامی برای تغییر. کد تجهیز ثابت و یکتا در مرکز، Hold اولیه. ثبت سرویس یا رفع نقص مجوز بازگشت خودکار نیست؛ release فقط با آخرین گواهی پذیرفته‌شده پس از آخرین توقف/رفع نقص، بازبین مستقل و سررسید معتبر. آرشیو فقط در Hold بدون واحد ذخیره‌شده/حادثه باز.
- دما: BankTemperatureLog واحد؛ جزئیات شیفت/min/max/چک‌ها در رویداد تجهیز؛ assessment دستی. انحراف ثبت‌شده موجب Hold فنی و قرنطینه اداری واحدهای هنوز در بانک می‌شود. واحد صادرشده تغییر نمی‌کند. کدهای قدیمی بدون شناسنامه در EquipmentStats گزارش می‌شوند و همچنان تابع ST04 هستند. برای تجهیز ثبت‌شده Hold/Archived/سررسید گذشته عملیات جدید دریافت/آماده‌سازی/صدور مسدود است.
- QC / Committee / Audit / Training: POST {Kind}List/Create/Edit/Line/Submit/Approve/Reject، GET {Kind}Detail/{guid}؛ ردیف‌های نوع‌دار و فیلدهای خاص در JSON، نه payload آزاد. تنها سازنده در Draft اصلاح/ارسال می‌کند؛ بازبین مستقل ثبت یا رد می‌کند. Reject همان مجوز Approve. CommitteeOwners/TrainingOwners برای حساب‌های فعال؛ CommitteeProgress/Verify/Return/Close. پیشرفت ۱۰۰ = Submitted؛ تأیید مستقل لازم است. Training ظرفیت، عدم تکرار، حضور و نمره ۰..۲۰. Audit امتیاز ۰..۲؛ نتیجه رسمی خودکار ندارد. QC نتیجه ثبت‌شده، لات، انقضا، قرائت و انتظار؛ موارد Pending مانع ارسال، عدم انطباق نیازمند شرح اقدام؛ تایید QC معادل مجوز فرآورده نیست.
- GET Facility / POST FacilitySave: فقط متادیتای مرکز و پانویس، بدون تنظیمات زیرساخت، کلید یا پیامک.
- POST Stats: From/To/Ward؛ شمارش واقعی پرونده‌ها، اقدامات باز، حضور، حوادث/واکنش و مصرف از ST04/05؛ دریافت و امحا مرکز-محور هستند حتی با فیلتر بخش، شروع/پایان برحسب بخش. نرخ C/T و MSBOS بدون تعاریف/داده معتبر محاسبه نمی‌شود.
- کلیه فرم‌ها داخلی و بدون ادعای گواهی قانونی/اعتبار بالینی. قواعد بالینی/فنی مصوب، فایل گواهی/اتصال خارجی و آزمون عملیاتی در مراحل پذیرش نهایی.

- Bank UnitRow اکنون EquipmentBlocked را نشان می‌دهد؛ واحدهای متوقف فنی در موجودی آزاد قابل استفاده و صف صدور شمارش نمی‌شوند (وضعیت گردش اصلی حفظ می‌شود).

- GET Facility همچنین حداکثر ۱۰۰ رویداد اخیر تغییر مشخصات مرکز را با کاربر، زمان، دلیل و snapshot قبل/بعد بازمی‌گرداند.
