# ST-03 — درخواست خون و تأیید پزشک

وضعیت: در حال توسعه، بر مبنای ST-02 Merge شده. نسخه عملیاتی هنوز ارائه نشده است.

## منابع دمو
- nurse-request.html: ویزارد ثبت درخواست پرستار، ۸ نوع درخواست، انتخاب بیمار و پیش‌نمایش.
- doctor-orders.html: HV.006, HV.007, HV.008, HV.010, HV.011 و PAD؛ ثبت مستقیم پزشک.
- doctor-approval.html و doctor-dashboard.html: تکمیل فیلدهای پزشک، کارتابل تصمیم و تاریخچه.
- doctor-patients.html و nurse-stats.html: نمای پرونده و گزارش درخواست‌ها.
- bloodbank-requests.html: فقط صف درخواست تأییدشده در ST-03؛ Crossmatch/Issue در مراحل بعدی.

## اسلایس‌ها
1. ST03-01: مدل داده، migration و قواعد انتقال وضعیت (در این commit).
2. ST03-02: CQRS ثبت/ویرایش پیش‌نویس، ثبت اقلام، ثبت اطلاعات شرطی و ارسال؛ اتصال به PatientAdmission و scope مرکز؛ idempotency و rowversion.
3. ST03-03: مجوز مستقل تصمیم پزشک، بازگشت/رد/تأیید، ثبت تاریخچه، صف بانک خون؛ جلوگیری از تأیید خودکار و بازنویسی درخواست ارسال‌شده.
4. ST03-04: نقش‌های پرستار/پزشک/بانک خون با seed آزمایشی و API grants مستقل، ویزارد و کارتابل Angular، reuse از AdmissionPicker و RequestState.
5. ST03-05: تست SQL/permission/concurrency، تست مرورگر و راهنمای استفاده، بازبینی بصری دمو.

## تصمیم‌های داده و ایمنی
- BloodRequest به PatientAdmission متصل است و مرکز از همان پذیرش استخراج می‌شود؛ شناسه بیمار جداگانه توسط کاربر وارد نمی‌شود.
- BloodRequestItem اقلام فرآورده با تعداد مثبت؛ RequestClinicalData داده بالینی ثبت‌شده؛ RequestApproval تاریخچه تصمیم با actor و attempt. مقادیر آزمایشگاهی فقط ثبت می‌شوند و تصمیم بالینی خودکار نیست.
- Draft/Returned → AwaitingDoctor → AwaitingBank یا Returned/Rejected؛ Cancel فقط قبل از ارسال به بانک. هیچ درخواست اورژانسی خودکار تأیید نمی‌شود. دستور مستقیم پزشک نیز اقدام صریح و مجاز برای تأیید می‌خواهد.
- وضعیت AwaitingBank به معنی آماده بودن خون، سازگاری یا مجوز تزریق نیست. هیچ فرآورده‌ای در ST-03 صادر نمی‌شود.
- SubmissionAttempt و AdmissionVersionAtSubmission برای کنترل تغییر اطلاعات پذیرش در مرحله بعد استفاده می‌شوند؛ مجوزها باید توسط Backend enforce شوند. فرم‌های ویژه و امضای رسمی پزشک پس از تکمیل قرارداد/سیاست بیمارستان پیاده‌سازی و ارزیابی می‌شوند.
