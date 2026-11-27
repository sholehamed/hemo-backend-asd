# R02 — منوی صفحات خواندنی پذیرش
فرانت fe080401 (PR15، CI پس از merge: 88 تست + 11 تست snapshot) سه مسیر stats/list/his را با Guard شرکت فعال کرده است.
- در دیتابیس آزمایشی Development، HemoSeed:Enabled=true و HemoSeed:IncludeAdmissionReadPages=true تنظیم شود. این گزینه شامل پایلوت گزارش و seed مسیرهای پذیرش نیز هست. برای نسخه قدیمی فرانت همچنان IncludeCompanyMenuPilot گزارش را به تنهایی فعال می‌کند.
- تنها دو leaf متعلق به Seed، بدون فرزند، با حفظ GUID و مجوزهای RoleAccess/MenuApi به ریشه خصوصی /Hemo منتقل می‌شوند. منوهای دیگر شرکت و مسیرهای clinical دست‌نخورده‌اند. خاموش کردن گزینه rollback نیست.
- تست SQL پایلوت موجود گسترش یافت: ارتقا از report-only، حفظ شناسه‌ها و grantها، بازنمایی دسترسی پرستار به دو صفحه بدون stats، idempotence و سازگاری flag قدیمی. CI جدید هنوز تایید نشده است.
- حساب یا migration جدید وجود ندارد. HIS ارتباط خارجی واقعی ندارد. Package/SSO و UAT واقعی همچنان باقی است.
