# R02 — تکمیل منوی پذیرش
- HemoSeed:Enabled=true و HemoSeed:IncludeAdmissionCompletePages=true فقط روی دیتابیس Development آزمایشی و فرانت دارای PR19. گزینه کامل شامل گزینه‌های گزارش و خواندنی است.
- هشت route seed-owned پذیرش با حفظ GUID و RoleAccess/MenuApi تحت /Hemo قرار می‌گیرند؛ جزئیات پارامتری مخفی است. منوی اصلی /admission چون marker مالکیت ندارد جابه‌جا نمی‌شود. مسیر اختصاصی /admission/dashboard برای منوی داشبورد اضافه شده است.
- فقط نقش دارای stats یا list از مجوزهای صریح موجود روی /admission، مجوز داشبورد می‌گیرد. نگاشت stats/list/detail/create/find/reviews/history با تقاطع مجوزهای قبلی همان نقش؛ هیچ Role/API grant جدیدی به لحاظ زوج role/api ایجاد نمی‌شود. whole-menu grant وجود ندارد. لینک‌های مقصد با Guard مستقل کنترل می‌شوند.
- تست SQL موجود ارتقا از read-only، حفظ شناسه و grantها، مخفی بودن جزئیات، عدم توسعه دسترسی پرستار به آمار، حفظ منوی اصلی، و تکرارپذیری flag قدیمی را بررسی می‌کند. CI هنوز تایید نشده است.
- حساب یا migration جدید وجود ندارد. خاموش کردن flag rollback نیست. فقط پس از استقرار نسخه متناظر فرانت flag را فعال کنید. UAT واقعی و اسکنر/چاپگر و ماژول‌های دیگر باقی است.
