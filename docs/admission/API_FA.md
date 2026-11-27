# HEMO-ADM API
مبنای آدرس: /Identity/PatientAdmission؛ Bearer token و UserRole طبق قرارداد فعلی شرکت. مجوز APIهای جدید مطابق Api/RoleAccess/MenuApi موجود ثبت شود؛ فایل manifest و PROVISION_FA.md را ببینید. در ST-02 فقط مرکز نقش جاری قابل مشاهده است.

| Method | Path | عملیات |
|---|---|---|
| GET | /Identity/PatientAdmission/Capabilities | مجوزهای نقش جاری برای UI (بدون افشای اطلاعات بیمار) |
| POST | /Identity/PatientAdmission | ثبت بیمار/پذیرش در یک SaveChanges |
| PUT | /Identity/PatientAdmission | ویرایش اطلاعات بستری با version |
| PUT | /Identity/PatientAdmission/PatientIdentity | اصلاح مشخصات هویتی / تعیین هویت موقت، بدون ادغام خاموش پرونده و با ثبت دلیل |
| POST | /Identity/PatientAdmission/BindPaging | فیلتر نام/کد/بخش/گروه/وضعیت/تاریخ، صفحه‌بندی |
| GET | /Identity/PatientAdmission/{guid} | جزئیات |
| GET | /Identity/PatientAdmission/FindPatient?identificationType=NationalId&identificationNumber=... | استعلام داخلی پرونده مرکز، نه ثبت احوال/HIS |
| GET | /Identity/PatientAdmission/{guid}/History | پذیرش‌های قبلی همان بیمار |
| GET | /Identity/PatientAdmission/{guid}/Reviews | تاریخچه بررسی |
| POST | /Identity/PatientAdmission/Stats | آمار و توزیع |
| POST | /Identity/PatientAdmission/Review | Identity/History/Clearance با checklist و version |

پذیرش مجهول‌الهویه: شناسه یکتای سرور. بارکد منطقی: WristbandCode. چاپ لیبل باید با سخت‌افزار و نمونه واقعی مرکز آزمون شود. مقادیر BloodGroup صرفاً سابقه ثبت‌شده، نه نتیجه آزمون جدید. وضعیت ReadyForDischarge نتیجه بررسی مستند است، نه ترخیص قطعی/ارسال HIS. اتصال HIS، ثبت احوال و IBTO نیاز به قرارداد خارجی دارد.

## کنترل دسترسی
مکانیزم موجود تغییر نکرده است. ثبت Api/MenuApi و تخصیص RoleAccess جدید توسط مدیر مجاز شرکت ضروری است. برای کاربر بالینی، scope مرکز نقش انتخاب‌شده در سرور enforce می‌شود.
