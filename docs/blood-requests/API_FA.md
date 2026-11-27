# قرارداد API ST-03
Base: /Identity/BloodRequest. Bearer و UserRole معتبر از همان Identity HEMO. Scope مرکز از PatientAdmission.

| Method | Suffix | کاربرد |
|---|---|---|
| GET | /Capabilities | قابلیت‌های نقش |
| POST | /BindPaging | فهرست، فیلتر جستجو/وضعیت/اولویت/بخش/پذیرش/درخواست خود، صفحه‌بندی |
| POST | /Stats | تعداد به تفکیک وضعیت؛ mine |
| GET | /{guid} | جزئیات، اقلام، داده بالینی، نسخه و تاریخچه |
| POST | /Nurse | ثبت پیش‌نویس پرستار (RequestInput) |
| POST | /Doctor | ثبت پیش‌نویس پزشک (RequestInput) |
| PUT | / | ویرایش مالک، {guid,version,input} |
| POST | /Submit | {guid,version,action:"Submit"} |
| POST | /Cancel | {guid,version,action:"Cancel",reason} |
| POST | /Decide | {guid,version,action:"Approve"/"Return"/"Reject",reason,physician} |
| POST | /BankQueue | فقط AwaitingBank |
| GET | /Bank/{guid} | فقط جزئیات درخواست تأییدشده |

RequestInput: patientAdmissionGuid، clientRequestId یکتا، formType، priority، items [{productCode,quantity,requestedBloodGroup,specialInstructions}]، clinical {diagnosis,clinicalIndication,hemoglobin,hematocrit,plateletCount,recentTransfusion,recentPregnancy,priorReaction,unexpectedAntibody,premedication,sampleStatus,notes,formDetails}. فیلدهای خاص در SpecialFormData تعریف شده‌اند؛ JSON ذخیره‌شده تایپ‌شده است. در Approve بخش physician شامل diagnosis، indication، medicalCouncilNumber، notes، confirmed=true الزامی است. ثبت تأیید حساب پزشک جایگزین امضای دیجیتال رسمی نیست. درخواست اورژانس یا اتولوگ خودکار تأیید نمی‌شود. شماره پرونده و هویت بیمار از پذیرش استخراج می‌شود. هیچ عملیات آزمایش سازگاری یا صدور خون در این API وجود ندارد.
