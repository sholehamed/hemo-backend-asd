# HEMO-ASD — سند راهبرد توسعه و انتقال دمو

**نسخه:** 0.1.0 (ST-00)  
**تاریخ ثبت:** 2026-11-27  
**وضعیت:** Baseline / مستندات؛ هنوز هیچ قابلیت جدیدی پیاده‌سازی یا تست نشده است.

**تصمیم الزام‌آور مالک:** [عدم تغییر زیرساخت تثبیت‌شده](HEMO_ASD_SCOPE_OVERRIDE_FA.md). موارد قبلی اصلاح عمومی Backend صرفاً finding تاریخی و خارج از Scope هستند.

## 1. هدف و منابع قطعی

هدف، تبدیل دموی ۶۲ صفحه‌ای HEMO Raw به سامانه عملیاتی با داده پایدار، امنیت سمت سرور، API واقعی و رابط Angular است؛ بدون بازنویسی غیرضروری زیرساخت موجود.

| مرجع | کاربرد | شاخه / Snapshot |
|---|---|---|
| [HEMO Raw](https://github.com/sholehamed/hemo-raw) | مبنای دیداری، صفحات و سناریوها | `main` — `5ebfc2e1101491d1162853a449ce947c7cc01941` |
| [Backend ASD](https://github.com/sholehamed/hemo-backend-asd) | **کد پایه قطعی Backend** | `main` — `382b763c90332bb79b52113dd21ff601691d0244` |
| [Frontend ASD](https://github.com/sholehamed/hemo-frontend-asd) | **مقصد قطعی Frontend**؛ در زمان تحلیل فقط `.gitignore` | `main` — `7a168d786920749c699d6b72cc270b506547657f` |
| [HEMO Backend قدیمی](https://github.com/sholehamed/hemo-backend) | مرجع تحلیل فرآیند و نمونه آزمون؛ نه معماری اجباری | `main` — `d0a0ab719f5f17aba1a0cedcf6c11266edd04acb` |
| [HEMO Frontend قدیمی](https://github.com/sholehamed/hemo-frontend) | مرجع الگوهای Angular و تست‌های قبلی | `main` — `c242f700de1ea1026521553e85d47daf51e09875` |

**مرجع تفصیلی تحلیل کسب‌وکار:** [reference/HEMO_WORKFLOWS_AND_DATA_MODEL_FA.md](reference/HEMO_WORKFLOWS_AND_DATA_MODEL_FA.md). این سند از پروژه قبلی وارد شده است؛ **پیشنهادهای Schema/جدول و وضعیت‌های آن قرارداد قطعی دیتابیس جدید نیستند**. هر قسمت باید با کد ASD و تأیید دامنه تطبیق داده شود. فرم‌ها و کدهای نمایشی HEMO Raw به‌خودی‌خود اثبات‌کننده اتصال واقعی، قانون درمانی یا سطح دسترسی نیستند.

## 2. نتیجه بررسی واقعی مخزن‌های مقصد

### Backend: وضعیت موجود و قرارداد توسعه

- پروژه: `Hemo.sln` و لایه‌های `Domain`، `Application`، `Infrastructure`، `web` به همراه `Generator`. فایل‌های csproj با `net9.0` پیکربندی شده‌اند؛ ارتقای نسخه بدون دلیل و آزمون سازگاری ممنوع.
- مدل دامنه: `Domain/Entities` و `Domain/Common/BaseEntity.cs`؛ کلید `GUID`، `AuditedEntity`، `IsDelete` و رویدادهای موجود.
- EF Core: `Infrastructure/Data/ApplicationDbContext.cs` و `Infrastructure/Data/Configurations/*Configuration.cs`؛ `ApplyConfigurationsFromAssembly` و سیاست `DeleteBehavior.Restrict`. مایگریشن‌های موجود در `Infrastructure/SQLServer` هستند.
- Application: `Application/Core/{Entity}_core/Commands|Queries`، `IRequest`/handler مبتنی بر MediatR، `FluentValidation`، نگاشت `AutoMapper` و سرویس‌های موجود.
- API: `web/Endpoints/*Endpoints.cs` با `EndpointGroupBase`، `app.MapGroup(this)`، `ISender` و `TypedResults`. از این قرارداد برای حوزه جدید Hemo استفاده شود.
- هویت/دسترسی: جداول و Entityهای `User`، `Role`، `UserRole`، `RoleAccess`، `Menu`، `MenuApi` و `Api` وجود دارند. قبل از افزودن RBAC جدید، معنای واقعی `OrgLocation` و مجوز API در همین سیستم بررسی شود.
- **یک Backend واحد:** Identity و Hemo داخل همین Host، Service Registration، `ApplicationDbContext` و چرخه انتشار قرار می‌گیرند. سرویس مستقل Identity، Gateway یا مخزن Backend دوم ایجاد نشود.
- **شکاف فعلی:** تست/CI مخصوص این ریپو در snapshot اصلی دیده نشد. Seed در `Infrastructure/Data/InitialiserExtensions.cs` مسیر `TrySeedAsync()` را اجرا نمی‌کند. در برخی EndpointGroupها `RequireAuthorization()` کامنت شده و AuthorizationBehaviour نیز کامنت است. وجود Entity دسترسی به معنای اعمال شدن کامل آن نیست.

### Frontend: وضعیت موجود و قرارداد انتخاب‌شده

- `hemo-frontend-asd` در زمان تحلیل فاقد Angular workspace است و فقط `.gitignore` دارد. پس **هیچ استاندارد پیاده‌سازی Angular در مقصد قابل استخراج نیست**.
- پایه پیشنهادی برای ST-01: استفاده انتخابی از الگوی تست‌شده `sholehamed/hemo-frontend`: Angular 22 standalone، lazy routing، Reactive Forms، Angular Material، Signals، HttpClient services، route guards و تست‌ها. انتقال کامل فایل‌ها بدون تطبیق قرارداد API ممنوع.
- خروجی HTML/CSS/JS دمو صرفاً مرجع UI/UX، متن فرم‌ها و سناریوست، نه کدی که مستقیم داخل Angular کپی شود.
- استاندارد دقیق Frontend در سند هم‌نامِ ریپوی Frontend ثبت خواهد شد.

## 3. حوزه‌های کسب‌وکار و مرزبندی مسئولیت

| شناسه حوزه | نقش مرجع | زیرروندهای مورد نیاز |
|---|---|---|
| ADM | پذیرش | پرونده بیمار، شناسایی/استعلام، پذیرش، سابقه، HIS، ترخیص، آمار |
| NUR | پرستار | بیماران بخش، درخواست فرآورده، کارتابل، کنترل علائم، تزریق، گزارش واکنش |
| DOC | پزشک | دستورات، تأیید درخواست، پایش، سوابق تزریق، واکنش‌ها، گزارش دوره‌ای |
| BB | کارشناس بانک خون | درخواست‌ها، موجودی، آزمایش سازگاری، ذوب، ثبت دما، گزارش |
| SUP | سرپرست بانک خون | دریافت، کراس‌مچ، تحویل، برگشت، انقضا، امحا، QC، کمیته، آمار |
| HV | هموویژیلانس | واکنش‌ها، تحلیل RCA، ممیزی، کمیته، آموزش، گزارش و تنظیمات |
| EQ | تجهیزات پزشکی | تجهیزات، PM/کالیبراسیون، گزارش رخداد و ثبت دما |

هفت حوزه بالا **حوزه‌های نمایشی/مجوز** هستند؛ هفت دیتابیس یا هفت موجودیت مستقل بیمار تعریف نمی‌شوند. شناسه بیمار، پذیرش، درخواست، کیسه، کراس‌مچ، تحویل و تزریق باید چرخه داده واحد داشته باشند. دسترسی واقعی تابع نقش **به علاوه** مرکز، بخش و دامنه داده است. نقش `SYSTEM_ADMIN` نیز برای مدیریت سامانه وجود خواهد داشت، هرچند نقش مستقلی در منوی Raw نیست.

## 4. گردش‌های انتها به انتها (Traceability)

| شناسه | گردش اصلی | خروجی/شواهد پذیرش |
|---|---|---|
| FLOW-01 | ورود، تعیین نقش و محدوده مرکز/بخش | کاربر فقط داده و صفحات مجاز را می‌بیند؛ API هم همان مجوز را اعمال می‌کند |
| FLOW-02 | یافتن یا ایجاد بیمار و ثبت پذیرش | جلوگیری از پرونده تکراری، ثبت تراکنشی، نگهداری تاریخچه |
| FLOW-03 | شناسایی و تطبیق بیمار، مچ‌بند، استعلام HIS | لاگ تصمیمات، مدیریت بیمار مجهول‌الهویه و عدم موفقیت سرویس بیرونی |
| FLOW-04 | ثبت درخواست فرآورده و بررسی پزشک | تأیید/رد با تاریخچه و مجوز؛ ممانعت از اقدام غیرمجاز |
| FLOW-05 | بررسی بانک خون و آزمون سازگاری | تخصیص فقط برای درخواست/کیسه معتبر؛ ثبت آزمایش و مسئول |
| FLOW-06 | دریافت/رزرو موجودی و مدیریت زنجیره سرد | موجودی غیرمنفی، تاریخ انقضا، رهگیری کیسه و تغییر مکان |
| FLOW-07 | صدور، تحویل، برگشت، قرنطینه، امحا | انتقال وضعیت کنترل‌شده، جلوگیری از مصرف/صدور دوباره |
| FLOW-08 | شناسایی پیش از تزریق، ثبت علائم و اجرای تزریق | ثبت فرد، زمان، فرآورده و رویدادهای بالینی با Audit |
| FLOW-09 | واکنش نامطلوب و پیگیری هموویژیلانس | ارتباط واکنش به بیمار/پذیرش/کیسه/تزریق و جریان بررسی |
| FLOW-10 | مدیریت کیفیت، تجهیزات، کالیبراسیون و دما | هشدار و تاریخچه ثبت‌ها و موارد عدم انطباق |
| FLOW-11 | ترخیص، بستن پرونده و گزارش‌های چندنقشی | عدم تضاد با فرایندهای باز، گزارش بر اساس سطح دسترسی |

**ملاحظه بالینی:** معیارهای سازگاری خون، محدوده علائم حیاتی، امکان مرجوعی و ضوابط واکنش باید از سیاست مصوب مرکز و متخصص بالینی اخذ شوند؛ فرضیات HTML نباید خودکار به Rule درمانی تبدیل شوند.

## 5. مراحل توسعه و شرایط عبور

| مرحله | تمرکز | خروجی ضروری | Gate |
|---|---|---|---|
| ST-00 | شناسایی + سند + برنامه | همین سند، تحلیل مرجع ۶۲ صفحه، Worklog و تصمیم‌های معماری | سندها قابل پیگیری در PR |
| ST-01 | ایمن‌سازی/Bootstrap یکپارچه | اجرای قابل‌تکرار .NET 9، دیتابیس و Frontend Angular، auth/RBAC پایه، seed قابل تکرار، CI | build + smoke + تست ممنوعیت دسترسی |
| ST-02 | هویت بیمار و پذیرش | Entity/Configuration/Service/CQRS/API/فرانت واقعی | تست ثبت مجدد، ناشناس، محدودیت مرکز و تراکنش |
| ST-03 | درخواست و تأیید | درخواست چندنوع فرآورده، پزشک، تغییر وضعیت/تاریخچه | تست اجازه/ممانعت انتقال وضعیت |
| ST-04 | بانک خون و فرآورده | دریافت/موجودی/کراس‌مچ/رزرو/ذوب/انقضا | تست رقابت همزمان برای رزرو و سازگاری قواعد |
| ST-05 | تحویل، برگشت و تزریق | جریان تحویل تا تزریق و پایش | تست چرخه کامل کیسه، برگشت و خطا |
| ST-06 | واکنش و هموویژیلانس | گزارش واکنش، RCA، ممیزی، کمیته | تست ردیابی، سطح دسترسی و Audit |
| ST-07 | تجهیزات، گزارش‌ها و مدیریت | تجهیزات/PM/دما، گزارش‌ها، منو/مجوز، آمار | تست فیلتر مرکز و درستی تجمع داده |
| ST-08 | سخت‌سازی و تحویل | E2E بین نقش‌ها، مستند راهنمای کاربر/توسعه‌دهنده/استقرار | همه Checks سبز یا استثنای مستند |

این ترتیب برنامه اجرای جدید است؛ مراحل قدیمی ST-01/ST-02 مخزن‌های دیگر به معنای تکمیل این ریپوها نیست.

## 6. واحد توسعه: Vertical Slice قابل تست

برای **هر عملیات واقعی** یک شناسه `HEMO-<DOMAIN>-NNN` تعریف می‌شود (مثلاً `HEMO-ADM-001`). مسیر توسعه:

1. **Discovery:** استخراج صفحه/کنترل/سناریو از Raw؛ تعیین بازیگر، precondition، قوانین و transition؛ الصاق ردیف traceability.
2. **Contract:** تعریف request/response، خطاها، مجوز، idempotency/concurrency و اثر داده؛ ثبت قرارداد API.
3. **Backend:** Entity و Configuration/Index در صورت نیاز؛ DbSet و migration در DbContext واحد؛ Query/Command/Validator/Handler با همان DI/Repository pattern موجود؛ EndpointGroup.
4. **Frontend:** route lazy، component، form/state، HTTP service، guard، loading/error/empty؛ بدون داده عملیاتی hardcoded.
5. **Tests:** Domain/handler، integration EF+SQL Server، API authorization و status codes، Angular logic، و E2E مرورگر برای هر جریان پرتکرار.
6. **Review:** build، migration clean، تست سبز، تغییرات فایل‌به‌فایل، به‌روزرسانی worklog، PR و لینک Commit.

**Definition of Done:** نه فقط نمایش صفحه؛ API و DB حقیقی، کنترل مجوز، Seed مناسب، تست شواهد، مستندات و قابلیت بازتولید باید کامل باشند.

## 7. قواعد پیاده‌سازی اجباری

### Backend
- تغییرات کسب‌وکار جدید فقط داخل `hemo-backend-asd`؛ **بدون microservice/Identity جداگانه**.
- نام‌گذاری و نوع Entityها مطابق ساختار موجود؛ برای `Patient`/ `PatientAdmission` از تصمیم‌های قبلی و کد فعلی بازبینی‌شده استفاده شود، نه کپی کور از DB قدیمی.
- `CancellationToken` در handler و پرس‌وجوها، `FluentValidation` برای input و `AsNoTracking` برای خواندن. عملیات چندموجودیتی تراکنشی و رخدادها Audit می‌شوند.
- یکتایی/ایندکس/رابطه و DeleteBehavior به شکل صریح در EF Configuration؛ تغییر schema با migration، نه SQL دستی بدون ثبت.
- `RequireAuthorization` یا سیاست معادل در endpointهای محافظت‌شده؛ شرط دسترسی مرکز/بخش در خود handler/query enforce شود. Guard فرانت کافی نیست.
- برای عملیات حساس concurrency token و شرط انتقال معتبر لازم است؛ تکرار درخواست نباید داده تکراری یا موجودی منفی ایجاد کند.
- برای گزارش‌دهی و Dapper، از سرویس و الگوی موجود در Application استفاده شود؛ raw SQL پارامتری، محدودیت Scope و timeout.

### Frontend
- Angular standalone و ساختار `core` / `shared` / `features`؛ lazy route؛ API service typed، Reactive Forms و status state؛ زبان فارسی و RTL.
- منو از مجوزهای واقعی Backend تولید شود؛ نه صرفاً نقش hardcoded در HTML.
- هر feature چند action مستقل دارد؛ هر action دارای Contract، UI states و test scenario مستقل است، نه یک تسک مبهم برای «کل صفحه».
- نگاشت Route↔API از طریق interceptor ثبت درخواست‌های **واقعاً اجراشده** و ابزار export/import قابل تعریف است؛ این مکانیزم به‌تنهایی کشف همه endpointها یا مجوزدهی خودکار نیست. پیش از import باید قابل بازبینی و idempotent باشد.
- هیچ توکن، اطلاعات بیمار یا credential در LocalStorage/Log یا Mock production نگهداری نشود.

## 8. مسائل شناسایی‌شده در ST-00 (قبل از feature اول)

1. **Security:** در `Infrastructure/DependencyInjection.cs` رشته اتصال و رمز عبور hardcoded مشاهده شد؛ باید حذف و credential مربوطه rotate شود. Config از secret store/env با startup validation خوانده شود. مقدار فعلی در مستندات تکرار نشود.
2. **Authorization:** `RequireAuthorization()` در چند نمونه EndpointGroup غیرفعال است و `AuthorizationBehaviour` نیز کامنت شده؛ مسیر واقعی policy enforcement و Scope را قبل از اولین API بالینی تست کنیم.
3. **CORS:** `web/Program.cs` سیاست `AllowAnyOrigin` دارد؛ محدودسازی origin هنگام اتصال واقعی و اعتبارسنجی auth ضروری است.
4. **Seed:** `TrySeedAsync` در seed فعلی کامنت شده؛ seed نقش‌ها، منوها، مراکز نمونه و مقادیر مرجع باید idempotent و بدون PII واقعی باشد.
5. **Migrations:** `InitialiseDatabaseAsync()` در startup migration اجرا می‌کند؛ در deployment باید کنترل خطا/همزمانی/rollback روشن باشد.
6. **Tests/CI:** در snapshot مقصد test project/workflow مشاهده نشد. برای هر PR تست خودکار و Build Gate اضافه شود؛ سبز بودن CI قدیمی به معنای سبز بودن این ریپو نیست.
7. **Frontend:** مقصد تنها `.gitignore` دارد؛ scaffold Angular 22 باید پس از تطبیق API موجود ساخته شود. انتقال کد Frontend قبلی بدون بازنگری endpointها ممنوع.
8. **Demo links:** `assets/menu.js` به ۱۲ HTML غایب ارجاع می‌دهد؛ فایل‌های جایگزین را کورکورانه ایجاد نکنیم؛ براساس صفحات موجود unify شوند. نمونه‌های تکراری `crossmatch-updated.html`, `supervisor-crossmatch1.html`, `nurse-tasks - Copy.html` نیز برای ادغام بررسی شوند.

## 9. استراتژی تست و Seed

- **Static/Build:** `dotnet restore/build` برای `Hemo.sln`؛ Angular `npm ci` و production build، strict TypeScript.
- **Unit:** Validator/handler، شناسه بیمار، تغییر وضعیت و محاسبات مستقل از DB.
- **Integration:** SQL Server واقعیِ تستی یا سرویس تست مناسب؛ migration، unique constraints، filterها، transaction و concurrent updates.
- **API:** 401/403، tenant/OrgLocation isolation، ولیدیشن 400، صفحه‌بندی، contract و correlation ID.
- **E2E:** FLOW-02→04→05→07→08→09 و سناریوهای شکست/برگشت؛ roleهای مختلف؛ بررسی RTL و دسترس‌پذیری پایه.
- **Seed:** system role + نقش‌های هفت‌گانه، یک ساختار سازمانی نمونه، reference catalogs، user test با secret از environment، بیمار کاملاً ساختگی. Seed ایمن و تکرارپذیر باشد؛ اطلاعات شناسایی واقعی ممنوع.
- تا زمانی که اجرای واقعی تست انجام نشده، در Worklog وضعیت `NOT RUN` درج می‌شود؛ نتیجه سبز یا کارکرد واقعی ادعا نمی‌شود.

## 10. ثبت دائمی تصمیم و پیشرفت

- **منبع حقیقت:** همین فایل + `docs/HEMO_ASD_WORKLOG.md` + PR/commitهای دو ریپو.
- پیش از هر slice: `ID`، flow، ورودی/خروجی، scope، قرارداد API، فایل‌های درگیر، تست‌های برنامه‌ریزی‌شده ثبت شود.
- پس از هر slice: فایل‌های تغییر کرده، migration/seed، commit SHA، نتایج واقعی تست، اشکالات باز و تصمیم‌های جدید به Worklog افزوده شود.
- برای تغییر بین دو ریپو از یک `Slice ID` مشترک در عنوان PR و commit استفاده شود؛ دو PR متقابل به هم لینک شوند.
- تغییرات جدید روی branch مجزا؛ merge به `main` صرفاً بعد از بررسی و gate. هیچ force-push یا حذف تاریخچه بدون الزام روشن و هماهنگی.
- راهنمای نهایی توسعه‌دهنده و کاربر در `docs/developer` و `docs/user` بر اساس functionality واقعی تکمیل خواهد شد.

## 11. نخستین Vertical Slice پیشنهادی پس از ST-01

**HEMO-ADM-001 — ثبت بیمار و پذیرش:** بازیگر پذیرش؛ استعلام/یافتن بیمار؛ ثبت بیمار جدید فقط در صورت نیاز؛ ایجاد پذیرش در همان تراکنش؛ عدم ثبت تکراری؛ سطح دسترسی به مرکز؛ ورود بیمار مجهول‌الهویه؛ response شامل شناسه پرونده/پذیرش. UI منبع: `admission-new-patient.html` و `admission-patient-list.html`. پیاده‌سازی و تست قبلی مخزن‌های Hemo قدیمی صرفاً **منبع نمونه** است و باید با GUID/Identity/OrgLocation و API قرارداد ASD بازطراحی شود.

**قانون راهبردی:** اول جریان واقعی کوچک و کامل، بعد توسعه افقی صفحات. تعداد زیاد صفحات نمایشی بدون گردش عملیاتی متصل، پیشرفت قابل قبول محسوب نمی‌شود.
