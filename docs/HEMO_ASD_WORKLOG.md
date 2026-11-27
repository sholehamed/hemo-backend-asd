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
