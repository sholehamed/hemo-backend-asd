# ST-04 — API
Base /Identity/BloodBank — Bearer، UserRole، scope مرکز، مجوز صریح مستقل برای همه عملیات. هیچ صدور فیزیکی ندارد.
| متد | مسیر | کاربرد |
|---|---|---|
| GET | /Capabilities | قابلیت نقش |
| POST | /Inventory | فیلتر و صفحه‌بندی واحدها |
| GET | /Unit/{guid} | جزئیات، تاریخچه، آزمایش‌ها و آخرین دما |
| POST | /Stats | آمار مرکز |
| POST | /TemperatureHistory | تاریخچه و فیلتر تجهیز/ارزیابی/زمان |
| POST | /Receive | یک کیسه با کد یکتا؛ ReceivedHold |
| POST | /Inspect | بازرسی مستند مسئول؛ Available |
| POST | /Test | نتیجه آزمایش از کارشناس؛ بدون محاسبه خودکار سازگاری |
| POST | /Reserve | رزرو واحد برای درخواست تأییدشده و قلم مطابق |
| POST | /Thaw | StartThaw / FinishThaw (انقضای مستند پس از ذوب) |
| POST | /Ready | آماده‌سازی نرم‌افزاری، نه صدور |
| POST | /Hold | قرنطینه |
| POST | /Unreserve | لغو رزرو، بازگشت به قرنطینه |
| POST | /Discard | ثبت امحای انجام‌شده، فقط بدون رزرو |
| POST | /Temperature | ثبت دما و ارزیابی مستند؛ نگرانی باعث توقف واحدها |
| POST | /ReturnRequest | برگشت درخواست بانک بدون رزرو فعال |
UnitAction: guid، version(base64)، action، requestGuid/requestVersion برای رزرو، reason (الزامی)، method/temperature/effectiveExpiry برای ذوب. LabInput: unitGuid/unitVersion، requestGuid/requestVersion، sampleReference، secondSample، labelGroup، method، immediateSpin، phase37، ahg، dat، antibodyScreen، reportReference، conclusion، notes. همه زمان‌های ورودی DateTimeOffset؛ ذخیره UTC. کنترل نسخه و انقضا و ارزیابی دمای ثبت‌شده؛ آستانه دما و عمر فرآورده بر اساس مصوبات بیمارستان تنظیم/تأیید شود، سامانه محاسبه نمی‌کند. درخواست برای صدور در ST-05 باید مجدداً کنترل شود.

`/LabHistory` (POST): گزارش صفحه‌بندی‌شده آزمایش‌ها با Search، Conclusion، From/To، RequestGuid. `/Stats` شامل Stock (تعداد موجودی آزاد به تفکیک گروه و فرآورده) و `/Inventory` فیلتر `ThawQueue` شامل رزروهای نیازمند ذوب و واحدهای در حال ذوب است.
