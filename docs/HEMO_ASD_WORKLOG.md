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
