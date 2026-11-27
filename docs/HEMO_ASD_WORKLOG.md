# HEMO-ASD — دفتر ثبت توسعه و تصمیم‌ها (Worklog)

**قانون:** این فایل با هر Slice به‌روزرسانی می‌شود. `DONE` فقط برای کاری است که واقعاً در repository انجام شده؛ `PASS` فقط پس از اجرای تست و مشاهده نتیجه ثبت می‌شود. نتیجه‌های `NOT RUN` پنهان یا سبز گزارش نمی‌شوند.

## Snapshot مبنا — 2026-11-27

| مورد | نتیجه |
|---|---|
| Demo | `sholehamed/hemo-raw@5ebfc2e1101491d1162853a449ce947c7cc01941` — 62 HTML |
| Backend مقصد | `sholehamed/hemo-backend-asd@382b763c90332bb79b52113dd21ff601691d0244` — .NET 9، چهار لایه اصلی، 847 entry |
| Frontend مقصد | `sholehamed/hemo-frontend-asd@7a168d786920749c699d6b72cc270b506547657f` — تنها `.gitignore` |
| تحلیل قدیمی | `sholehamed/hemo-backend` و `sholehamed/hemo-frontend` — فقط مرجع، نه مقصد |
| Branch مستندات | `docs/st-00-hemo-asd-plan-20261127` در هر دو repo |

## گزارش مراحل

| شناسه | عملیات | وضعیت | شواهد/نکته |
|---|---|---|---|
| ST00-001 | شناسایی و snapshot پنج ریپو | DONE | GitHub repository metadata و tree روی `main` |
| ST00-002 | بررسی ۶۲ صفحه، هفت حوزه و لینک‌های منوی Raw | DONE (inventory) | ۱۲ لینک ناموجود، ۳ فایل جایگزین/نسخه دومِ آشکار |
| ST00-003 | بررسی معماری Backend ASD | DONE (static review) | `Domain/Application/Infrastructure/web`، .NET 9، EF، CQRS، Endpoint Groups |
| ST00-004 | بررسی معماری Frontend ASD | DONE | در لحظه تحلیل فقط `.gitignore`، بدون Angular code |
| ST00-005 | بررسی نمونه‌های Frontend/Backend قبلی | DONE (reference review) | Angular 22/Material/Standalone و سند جامع فرآیندهای قبل |
| ST00-006 | انتقال سند ۶۲ صفحه‌ای به مقصد | DONE | `docs/reference/HEMO_WORKFLOWS_AND_DATA_MODEL_FA.md` |
| ST00-007 | تعریف سند توسعه، الزامات، مراحل، کیفیت، امنیت و Sliceها | DONE | `docs/HEMO_ASD_DEVELOPMENT_GUIDE_FA.md` |
| ST00-008 | ایجاد سند/ثبت تغییرات Frontend | DONE | Frontend [#1](https://github.com/sholehamed/hemo-frontend-asd/pull/1) |
| ST00-009 | PR مرور مستندات برای هر دو ریپو | DONE (OPEN PRs) | Backend [#1](https://github.com/sholehamed/hemo-backend-asd/pull/1) / Frontend [#1](https://github.com/sholehamed/hemo-frontend-asd/pull/1)؛ merge نشده |
| ST01-001 | تثبیت اتصال و Bootstrap یکپارچه | TODO | کدنویسی نشده |
| ST01-002 | اصلاح secrets، auth و seed | TODO | کدنویسی نشده |
| ST01-003 | CI و تست‌ها | TODO / NOT RUN | SDK .NET در محیط تحلیل حاضر نبود؛ هیچ Build جدیدی اجرا نشده |
| ST02-001 | HEMO-ADM-001 پذیرش واقعی | TODO | بعد از Gate زیرساخت |

## Findingها (ریسک‌های اولویت‌دار)

| ID | اولویت | شرح | اقدام لازم |
|---|---|---|---|
| SEC-001 | Critical | رمز/Connection string ثابت در `Infrastructure/DependencyInjection.cs` | انتقال به secrets/env؛ rotate credential؛ عدم بازنشر مقدار |
| SEC-002 | Critical | نمونه endpointها با `RequireAuthorization` کامنت؛ pipeline Authorize غیرفعال | اثبات server-side enforcement با تست 401/403 و scope |
| SEC-003 | High | CORS باز در `web/Program.cs` | CORS مناسب Originهای واقعی؛ تست credential |
| DB-001 | High | `TrySeedAsync()` در Initialiser فراخوانی نمی‌شود | Seed idempotent نقش/منو/OrgLocation و reference data |
| DB-002 | High | auto-migration در startup | کنترل استقرار همزمان و failure |
| FE-001 | High | مقصد Angular ندارد | ایجاد workspace مطابق قرارداد API و الگوهای قبلی |
| RAW-001 | Medium | ۱۲ مسیر منوی Raw فایل مقصد ندارند | بررسی و نگاشت به صفحه حقیقی، عدم ایجاد صفحات ساختگی |
| QA-001 | High | در snapshot مقصد test/CI مشاهده نشد | pipeline مستقل build/unit/integration/e2e |

## مغایرت‌های ثبت‌شده در منوی Raw

`nurse-monitoring.html`, `nurse-workitems.html`, `supervisor-special.html`, `supervisor-quarterly.html`, `supervisor-quality.html`, `supervisor-staff.html`, `hv-manager-dashboard.html`, `hv-analysis.html`, `hv-committee.html`, `hv-training.html`, `hv-audit.html`, `hv-reports.html`.

این‌ها **لینک در menu.js** هستند، نه شواهد وجود صفحه. نسخه‌های جایگزین باید با صفحات موجود مطابقت داده شوند.

**Tracker اصلی:** [HEMO-ASD Master Roadmap #2](https://github.com/sholehamed/hemo-backend-asd/issues/2).

## نحوه افزودن رکورد جدید

هر اجرای بعدی باید جدول زیر را به انتهای فایل اضافه کند:

| فیلد | مقدار |
|---|---|
| تاریخ / Stage / Slice | YYYY-MM-DD / ST-XX / HEMO-XXX-NNN |
| هدف و Flow ID | ... |
| تصمیم‌ها و دلایل | ... |
| فایل‌های Backend/Frontend تغییر یافته | ... |
| Entity/Schema/Migration/Seed | ... |
| Endpoint/route/permissions | ... |
| Unit / Integration / UI / E2E | PASS/FAIL/NOT RUN همراه با command و شواهد |
| Commit/PR | URL/SHA |
| مشکلات باز / کار بعدی | ... |

## تصمیم‌های قطعی معماری

- ADR-001: Backend یکپارچه؛ Identity و Hemo در `hemo-backend-asd` و یک `ApplicationDbContext`.
- ADR-002: الگوهای کد موجود در Backend ASD بر طراحی تازه اولویت دارند؛ بدون بازنویسی معماری.
- ADR-003: `hemo-frontend-asd` خالی است؛ الگوی Angular قبلی تنها مرجع پیشنهادی است و نیاز به انطباق دارد.
- ADR-004: Raw UI و تحلیل قدیمی برای ردیابی قابلیت‌ها هستند؛ قرارداد حقیقی DB/API باید در ASD تعریف و تست شود.
- ADR-005: هر عملیات مستقل صفحه به یک Slice قابل تست و پیگیری تبدیل می‌شود؛ شرط «اتمام» شامل DB، API، UI، مجوز، تست و مستندات است.

## وضعیت تست در ثبت اولیه

- Backend build: **NOT RUN**
- Frontend build: **NOT RUN** (workspace در مقصد وجود ندارد)
- Backend unit/integration tests: **NOT RUN**
- Frontend unit/E2E tests: **NOT RUN**
- Database migration on target: **NOT RUN**
- CI: **NOT CONFIGURED** در snapshot فعلی

> این مرحله فقط تحلیل ایستای GitHub و ایجاد مستندات است؛ هیچ فرآیند بالینی یا endpoint جدیدی هنوز عملیاتی نشده است.

## 2026-11-27 — ST-02 | Data model

- دستور مالک: هیچ اصلاح عمومی در زیرساخت انجام نشود؛ [تصمیم محدوده](HEMO_ASD_SCOPE_OVERRIDE_FA.md).
- افزودن Patient، PatientAdmission و PatientAdmissionReview با AuditedEntity و GUID، سه سرویس BaseRepository، EF configurations، DbSet و SQL migration. شناسه بیمار در مرکز جاری یکتا است؛ HISNumber نیز در همان مرکز یکتا است؛ rowversion پذیرش جهت کنترل تغییر همزمان. هیچ رفتار موجود بازنویسی نشد.
- Migration و snapshot دستی تهیه شده‌اند، نیازمند validation واقعی EF؛ Build/Migration/Tests: NOT RUN.

### ST-02 | Backend API

- افزودن Command ثبت پذیرش و Validator، Query صفحه‌بندی/جستجو/سوابق/آمار، Command ویرایش اطلاعات بستری، بررسی هویت/سوابق/آمادگی ترخیص و تاریخچه بررسی، concurrency، EndpointGroup در مسیر `/Identity/PatientAdmission`.
- استفاده از UserRoleService/AuthService و RoleAccess موجود برای scope مرکز؛ برای نقش غیر ادمین منوی `/admission` لازم است. محدودیت فقط به endpointهای جدید مربوط است.
- HIS/IBTO و تزریق واقعی خارج از قرارداد موجود است؛ تأیید سوابق دستی و آمادگی ترخیص جایگزین ترخیص واقعی نیست.
- Test/Build/Migration: NOT RUN؛ کد هنوز نیازمند CI و آزمون DB است.

### ST-02 | تکمیل اصلاح هویت و مجوز عملیات

- افزودن اصلاح هویت بیمار موقت/اصلاح نام و مشخصات موجود؛ عدم تغییر شناسه قطعی یا merge خودکار؛ ابطال بررسی‌های مرتبط و ثبت تاریخچه با reason. مجوز endpoint به endpoint بر اساس الگوی RoleAccess مستقیم یا MenuApi+RoleAccess(ApiGUID=null) موجود؛ بدون تغییر هیچ فایل Identity. Manifest داده اولیه دسترسی و راهنمای provisioning (اعطای مجوز فقط با تصمیم مدیر). بازبینی جدید هویت/سوابق آمادگی قبلی ترخیص را نامعتبر می‌کند.
- عدم دسترسی سرویس خارجی و نبود آزمون runtime همچنان باز است.


### ST-02 verification update
- Run 46013810433 on 320f2393: Release build PASS; 22 rule/model tests PASS; SQL Server clean migration test PASS (1/1), no live DB accessed.
- Added a SQL integration fixture covering creation, duplicate HIS rejection, temporary identity resolution, linked readmissions, stale version rejection, identity/history/clearance transitions, audit reviews, statistics, facility isolation and API-specific role grant. It uses real SQL and actual handlers, with test-only identity claims/audit stamping rather than company login/Mongo logging. Execution pending next CI run.
- Backend PR #3 / Frontend PR #2 remain drafts. External service integration and live role provisioning still pending.

## 2026-11-27 — ST-02 acceptance / seed
- Prior CI both latest PR/push green. Found ordinary login only emits role Gkey 1/2 absent package; added HEMO-specific endpoint using existing login handler/Captcha/OTP and token issuance, selecting authorized HEMO assignments. Existing login behavior retained. Added local admission endpoint expiry/session validation and blocked/force-change role checks. Development-only opt-in seed for three accounts, exact API grants, facility and menu; no fixed secrets or privileged users, no changes to existing seed. Requires CI and actual organizational environment verification.

## ST-03 | آغاز درخواست خون
- ST-01/ST-02 در main ادغام شدند؛ PR Backend #3 و Frontend #2. آخرین CI Backend 25/25، Frontend 12/12 PASS.
- شاخه جداگانه ST-03؛ چهار موجودیت درخواست، اقلام، اطلاعات بالینی و تاریخچه تصمیم؛ EF configs، migration، repository services و قواعد گردش‌کار با تست. API، permission و UI هنوز در این اسلایس تکمیل نشده‌اند. CI این commit باید بررسی شود. سند اسلایس‌ها: docs/blood-requests/ST03_PLAN_FA.md.

### ST03-02/03 | API درخواست و تصمیم پزشک
- DTO و validator، پیش‌نویس پرستار/پزشک، ویرایش مالک، ثبت اقلام و داده فرم‌های اختصاصی، ارسال، برگشت، رد، لغو، تأیید پزشک، snapshot تاریخچه و صف مستقل بانک خون. مجوز صریح برای اقدام بالینی، scope مرکز، rowversion و کنترل تغییر پذیرش. Migration ST03 هنوز merge نشده و دو فیلد JSON برای اطلاعات تکمیلی و snapshot به آن اضافه شد. CI این تغییرات هنوز اجرا نشده است.

### ST03-05 | تست یکپارچه و فرم‌های مجزا
- تست واقعی SQL برای چرخه پیش‌نویس، اصلاح اقلام، idempotency، ارسال، بازگشت، تغییر پذیرش، رد مجوز پرستار، تأیید پزشک، صف بانک و ایزوله بودن مرکز اضافه شد؛ seed هر شش نقش در SQL بررسی می‌شود. محدودیت نوع فرم براساس منبع و تطبیق اقلام فرم اختصاصی اضافه شد. Build پیشین تا 682f5add سبز؛ نتیجه کامیت جدید هنوز بررسی نشده.

### ST-03 | final acceptance review
- Previous head c779e7c5: Release build and all 48 tests green (44 non-SQL, 4 SQL). Final review found a gap between reading admission rowversion and committing a request transition. New request transitions use a serializable SQL transaction and detail reads use a single statement for row/version/history consistency. No existing infrastructure changed. Revalidate with the full existing SQL lifecycle test suite. Later ST-04 must revalidate current admission/decision before any blood allocation or release.

## ST-04 | مدل داده و طرح توسعه
- چهار موجودیت BankBloodUnit, BankCompatibilityTest, BankUnitEvent, BankTemperatureLog با FK، index، rowversion، migration و تست قواعد وضعیت. مرجع هفت صفحه Raw. هیچ صدور/تحویل یا تصمیم بالینی خودکار. CI هنوز بررسی نشده.

### ST04-02/03 | API و گردش واحد
- CQRS دریافت تک‌واحد، بازرسی، نتیجه آزمایش مستند، رزرو دقیق فرآورده/تعداد، ذوب با ثبت انقضای مستند، آماده‌سازی، توقف، لغو رزرو، امحای ثبت‌شده، دما و برگشت درخواست؛ تراکنش Serializable، کنترل نسخه و مجوز صریح. نگرانی دمای ثبت‌شده باعث توقف محافظه‌کارانه واحدهای تجهیز می‌شود. بدون صدور فیزیکی یا تعیین خودکار سازگاری. CI pending.

### ST04-04/05 | Seed و آزمون SQL
- نقش موجود hemo.bloodbank با grants دقیق توسعه یافت؛ hemo.bank.supervisor جدا با بازرسی/امحا/برگشت. فلگ مستقل Development. تست چرخه واقعی SQL شامل نقش غیرمجاز، دریافت تکراری، آزمایش مردود، رزرو بیش از مقدار، rowversion، ذوب، کاهش انقضا، قرنطینه دمایی، لغو رزرو، برگشت درخواست و scope مرکز. مستند API/حساب‌ها ثبت شد. CI pending.

### ST04 | تکمیل گزارش و کارتابل ذوب
- CI قبلی 57 غیر SQL + 5 SQL سبز. برای نزدیک‌تر شدن به Raw، ماتریس موجودی گروه/فرآورده با aggregate SQL، گزارش آزمایش‌ها با فیلتر بازه و نتیجه و صف واقعی رزروهای نیازمند ذوب اضافه شد؛ تست SQL گسترش یافت. CI جدید pending.

### ST04 | بازبینی نهایی همزمانی
- CI add33073: 62/62 PASS. آزمون SQL به دو DbContext مستقل و رزرو همزمان دو واحد برای یک قلم یک‌واحدی گسترش یافت؛ دقیقاً یک رزرو باید ثبت شود. خطای deadlock/lock timeout در عملیات واحد با پیام بازیابی اطلاعات در ماژول جدید کنترل می‌شود. آزمون تغییر نسخه پذیرش پس از تأیید نیز اضافه شد. CI این کامیت pending.

- Final build failed because Application has no direct SqlClient reference. Used provider-neutral DbException in new bank command only; no dependency/package changes. Re-run full suite.

- Concurrent integration fixture uses independent IHttpContextAccessor instances to avoid shared AsyncLocal state between synthetic actors. No production change.

- Concurrent SQL test exposed actual SQL Server deadlock, wrapped by EF in InvalidOperationException → DbUpdateException → provider exception. Handle the wrapped database failure only within bank unit operation; fail safely, rollback, return refresh/review error instead of blindly retrying. Keep concurrency test and re-run.

## ST-05 | ST05-01
- بررسی ۹ صفحه Raw و قرارداد ST04؛ سه موجودیت حواله، رویداد و پایش، FK، unique unit، rowversion و migration؛ قواعد وضعیت و تست‌ها. در انتظار CI.

### ST05-02/03/04 | CQRS و API
- صدور واحد آماده با اعتبارسنجی مجدد ST04، حواله یکتا، شناسه‌های واقعی مشاهده‌شده و حمل مستند؛ دریافت یا مغایرت، تطبیق دو حساب مستقل، T0 و شروع، پایش، پایان/توقف، مرجوعی و دریافت بانک/امحا، یادداشت پزشک. تراکنش و rowversion، scope مرکز و مجوزهای صریح. محدودیت لازم ST04: نگرانی دمایی فقط واحدهای هنوز در بانک را تغییر می‌دهد. CI pending.

### ST05-05 | نقش‌ها و داده تمرینی
- افزوده شدن grants دقیق به کاربران موجود و حساب شاهد مستقل hemo.nurse.witness؛ فلگ Development و نمونه‌های مصنوعی مجزا با سابقه ST02–04 و چهار واحد آماده. اسناد API، حساب‌ها، شناسه‌های نمونه. CI در انتظار.

### ST05 | آزمون SQL و Seed
- سناریوی SQL از داده‌های واقعی seed‌شده در دیتابیس موقت استفاده می‌کند: تکرارپذیری، چهار پرونده، صدور تکراری، شناسه نامعتبر، دسترسی نقش، دو حساب، T0، پایش و پایان، مغایرت دریافت و مرجوعی، امحا، توقف/نگرانی پزشک، عدم تغییر وضعیت پس از گزارش دمای بانک، تغییر نسخه پذیرش، مرکز دیگر. نتیجه CI pending.

### ST05 | تکمیل فرم Raw
- CI 69cb5358: 78 غیر SQL + 6 SQL سبز. فیلد حجم مشاهده‌شده، سرعت ثبت‌شده، نوبت‌های تکمیلی و زمان واقعی اختیاری مستقل از زمان ثبت به ماژول اضافه شد. گزارش انقضای بانک واحدهای خارج‌شده را حذف می‌کند. تست SQL برای مقادیر ثبت‌شده گسترش یافت. CI جدید pending.

## ST-06 | ST06-01
- بررسی Raw و ST05، چهار موجودیت پرونده واکنش، تاریخچه، CAPA و آرشیو گزارش؛ FK/rowversion و migration، قواعد گردش و تست‌های پایه. کمیته، ممیزی، آموزش و تنظیمات در ST07. CI pending.

### ST06-02/03/04
- API پرونده از حواله شروع‌شده، گزارش idempotent و منع پرونده باز تکراری، نگرانی ST05 بدون توقف خودکار؛ بررسی پزشک و بانک، تصمیم مسئول، RCA و CAPA با مسئول دارای مجوز مرکز و بازبین مستقل، ثبت ارسال دستی و آرشیو آماری بدون استنباط نرخ، کنترل دسترسی و تراکنش. CI pending.

### ST06-05 | نقش و Seed
- دو نقش جدید مسئول و بازبین مستقل، دسترسی‌های محدود کاربران موجود، manifest و چهار پرونده مصنوعی مستقل از ST05 با مراحل مختلف. اسناد کاربران و API. آزمون CI در انتظار.

### ST06 | SQL regression
- آزمون SQL جدید: seed تکراری، منع نقش/مرکز دیگر، گزارش idempotent و منع پرونده باز دوم، نسخه قدیمی، بازگشت پزشک، RCA و CAPA با مالک و بازبین مستقل، رد/ارسال مجدد، الزام مرجع بیرونی، بستن پرونده و snapshot تغییرناپذیر. ST05 نمونه ST06 را نیز آموزشی نشان می‌دهد. CI pending.

- CI 46029143184: build/90 unit/6 regression SQL green; new SQL workflow exposed EF translation of record constructor + Distinct/OrderBy in eligible CAPA owners. SQL projection to anonymous columns, then DTO mapping. Re-run without weakening authorization or deleting test.

- CI 46029839717: 90 غیر SQL و 7 SQL سبز. در بازبینی نهایی RcaQueue، فیلتر CAPA مسئول اجرا و استقلال بستن نسبت به بررسی‌کننده، رویداد ST05 نمونه‌ها و آزمون دو ثبت همزمان با DbContext مستقل و مسیر عدم نیاز RCA اضافه شد. CI مجدد.

## ST-07 | ST07-01 مدل داده
- تحلیل ۱۲ صفحه Raw تجهیزات/مدیریت کیفیت، طرح تقسیم‌بندی، ۷ موجودیت شناسنامه، رویداد، حادثه، پرونده/ردیف/رویداد کیفیت و اطلاعات مرکز، Migration و DbSet/Repository. گردش‌ها و رابط هنوز در حال پیاده‌سازی هستند.

### ST07-02/03 | تجهیزات
- CQRS ثبت شناسنامه ثابت، اصلاح در Hold، سرویس با گواهی، بازبینی مستقل، حادثه و رفع نقص، آرشیو بدون واحد ذخیره‌شده؛ ثبت دما در همان BankTemperatureLog و قرنطینه اداری مطابق ST04. تجهیز ثبت‌شده در Hold/Archived/سررسید گذشته مانع عملیات جدید بانک/صدور است، کدهای تاریخی فاقد شناسنامه رفتار قبلی دارند. تست پایه؛ API mapping و CI pending.

### ST07-04..09 | مدیریت کیفیت
- قراردادهای نوع‌دار QC، کمیته، ممیزی، آموزش، ردیف و رویداد، گردش پیش‌نویس/ارسال/تأیید مستقل و مصوبه تا اجرا/تأیید، ظرفیت و حضور، نمرات و امتیاز ممیزی. اطلاعات پایه مرکز و گزارش شمارش عملیاتی. Endpointهای مجزا و grant صریح. CI pending.

### ST07-10 | Seed/permissions
- ۴ حساب جدید، grant نوع‌محور، login menu additive، manifest، ۴ تجهیز و حادثه و ۴ پرونده آموزشی و پروفایل مرکز. ثبت مغایرت دما از ST04 نیز Hold تجهیز ثبت‌شده را لحاظ می‌کند؛ release نیازمند گواهی پس از رفع نقص و تأیید مستقل. CI pending.

- CI ST07-01: migration و 97 تست قبلی سبز. ST07-02/03: 101 غیر SQL + 7 SQL سبز. خطای navigation گزارش BankUnitEvent در ST07-04..09 اصلاح شد. قرارداد capabilities با شناسه کاربر؛ آزمون SQL جدید seed، حادثه/سرویس، توقف دمایی، QC/ممیزی/آموزش/مصوبه، پروفایل و جداسازی مرکز. CI pending.

- CI 46032935110: 101 غیر SQL و 8 SQL سبز. بازبینی نهایی: واحد ذخیره‌شده آموزشی مرتبط با یخچال، تفکیک TemperatureHold و نمایش/فیلتر واحدهای مسدود فنی در ST04/05. آزمون SQL interlock و ۵ آزمون اعتبارسنجی افزوده شد. CI مجدد.

- تکمیل ثبت مستقل تاریخچه مشخصات مرکز (قبل/بعد و دلیل تغییر)، Migration و assertion SQL.

- بازبینی نهایی: زمان واقعی انجام سرویس باید پس از آخرین توقف/رفع نقص باشد، نه فقط زمان ثبت آن؛ آزمون SQL سرویس قدیمی ثبت‌شده بعد از رفع نقص. هشدار EquipmentBlocked فقط روی وضعیت‌های نگهداری بانک اعمال می‌شود، نه تاریخچه صدور/تزریق.

## ST-08 | Delivery / hardening (in progress)
- Baseline ST07 merged in both repos. Branch feature/st-08-delivery. Owner scope preserved.
- ST08-01/02: audited admission vs request/bank/transfer. Added administrative clearance guard and serialized admission review and request creation. Existing AwaitingBank status persists after completed transfusion: quantity comparison, not status alone. No schema changes; no clinical discharge or override. Partial order disposition remains open; clinical policy and reaction/CAPA follow-up requirements must be confirmed.
- ST08-04: opt-in HemoSeed:IncludeDeliverySamples, 14 existing roles, independent sample sets ST05/06/07 together. No additional password defaults or account grants; no expiry refresh. Account/setup document docs/delivery/DEMO_ACCOUNTS_FA.md.
- ST08-05/06: SQL tests for aggregate seed repeatability and negative clearance; positive clearance after actual transfer lifecycle. Existing SQL regression suite retained. CI updated. NOT RUN in current execution; await actual CI results before merge.
- ST08-07/08: user guide in frontend docs/delivery, backend developer/deployment guide, API supplement. ST08-03/05/09: live role/HTTP/SQL/browser end-to-end and final acceptance remain open. No production deployment.

- Added DeliverySqlWorkflowTests.NewAdmissionToCompletedTransfusionAndReactionAcrossRealHandlers: fresh admission, verified identity/history, nurse/doctor request, technical equipment registration/service/independent release, temperature, bank inventory/lab/reserve/ready, dispatch, distinct nurse witness, monitoring/completion, late reaction/independent closure, administrative readiness and rejection of new request after readiness. Asserts links and scoped role grants. Real isolated SQL + handlers, synthetic actors (not HTTP). CI NOT OBSERVED for latest head. Backend PR #9; Frontend PR #8.
