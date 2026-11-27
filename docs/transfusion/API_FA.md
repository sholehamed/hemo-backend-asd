# قرارداد ST-05
Base: /Identity/Transfusion. Bearer + UserRole، مجوز صریح، مرکز جاری.
| متد | مسیر | قرارداد |
|---|---|---|
| GET | /Capabilities | قابلیت‌های عملیات و شناسه کاربر |
| POST | /List | فیلتر Search/Status/Ward/AdmissionGuid/RequestGuid/From/To و صفحه‌بندی |
| GET | /Detail/{guid} | حواله، حمل، آزمایش، تاریخچه و علائم |
| POST | /Stats | شمارش وضعیت‌ها |
| POST | /Candidates | واحدهای آماده؛ اعتبارسنجی نهایی در صدور |
| POST | /Dispatch | DispatchInput؛ نسخه واحد، کدهای مشاهده‌شده، حامل، ظرف، دما، نشانگر، چک‌لیست و شرح |
| POST | /Receive, /ReceiptHold | دریافت منطبق یا ثبت مغایرت |
| POST | /Verify, /Witness, /Start | تطبیق مستقل دو حساب، ثبت T0 و آغاز |
| POST | /Vital | ثبت مشاهده با نوبت و زمان اندازه‌گیری |
| POST | /Complete, /Stop | پایان با مشاهده پایان، یا ثبت توقف بدون انتظار برای پایش |
| POST | /Return, /AcceptReturn, /Dispose | برگشت، قرنطینه ویژه و ثبت امحا |
| POST | /DoctorNote | یادداشت و علامت نگرانی، نه دستور درمان خودکار |
TransferActionInput: Guid, Version (base64), UnitCode, WristbandCode, Checks (31 در تطبیق)، Notes (الزامی)، Temperature, Condition, ReceiverName, NeedleReference, SuspectedReaction. VitalInput: Guid, Version, Phase, MeasuredAt (ISO offset), Systolic, Diastolic, Pulse, Temperature, Spo2, Respiration, UrineMl, Notes. نوبت‌ها T0/T15/Mid/End/Post1h؛ نام مرحله ثبت مشاهده است و زمان‌بندی بالینی خودکار اعمال نمی‌شود. زمان UTC، rowversion، رویدادهای append-only و تراکنش Serializable؛ خطا کاربر را به دریافت مجدد هدایت می‌کند. مقدار خارج از محدوده نمایش فنی رد می‌شود؛ هیچ تفسیر بالینی یا پیشنهاد درمانی وجود ندارد. 
واحد Issued/Transfused/TransfusionStopped/Returning/ReturnedHold/ReturnedDisposed از موجودی آزاد ST04 خارج است. نسبت به همان درخواست رزرو تاریخی نگه داشته می‌شود تا دوباره صادر نشود؛ جایگزینی واحد نیازمند درخواست جدید یا سیاست آتی تاییدشده. وضعیت AwaitingBank در ST03 وضعیت تأیید درخواست است و نباید به عنوان وضعیت فیزیکی تحویل تفسیر شود.

تکمیل فرم Raw: `VolumeMl` (حجم مشاهده‌شده ثبت‌شده) در پایان/توقف؛ `RateMlPerHour` (سرعت مشاهده‌شده، اختیاری) در پایش. بدون مقدار پیش‌فرض یا تعیین بالینی. نوبت‌های Start/T30/T45/T60/Post2h نیز برای ثبت مشاهده وجود دارند. `OccurredAt` اختیاری در عملیات فیزیکی: زمان واقعی با offset، پیش‌فرض زمان سرور؛ زمان ثبت در تایم‌لاین جداگانه می‌ماند. تطبیق/شاهد و یادداشت با زمان احراز حساب ثبت می‌شوند. زمان شروع بعد از شاهد و T0، پایان بعد از مشاهده End. گزارش انقضای موجودی ST04 فقط واحدهای هنوز در بانک را می‌شمارد.
