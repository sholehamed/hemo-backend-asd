using System.Globalization;

namespace Application.Common.Exceptions;

public static class PersianDate
{
    public static int GetDayOfWeekNumber(this DateTime date)
    {
        int dayOfWeekNumber;

        switch (date.DayOfWeek)
        {
            case DayOfWeek.Sunday:
                dayOfWeekNumber = 1;
                break;
            case DayOfWeek.Monday:
                dayOfWeekNumber = 2;
                break;
            case DayOfWeek.Tuesday:
                dayOfWeekNumber = 3;
                break;
            case DayOfWeek.Wednesday:
                dayOfWeekNumber = 4;
                break;
            case DayOfWeek.Thursday:
                dayOfWeekNumber = 5;
                break;
            case DayOfWeek.Friday:
                dayOfWeekNumber = 6;
                break;
            case DayOfWeek.Saturday:
                dayOfWeekNumber = 0;
                break;
            default:
                dayOfWeekNumber = -1; // یک مقدار پیش‌فرض برای موارد نامعتبر
                break;
        }

        return dayOfWeekNumber;
    }

    public static string GetDayOfWeekName(this int DayOfWeek)
    {
        try
        {
            string[] persianDays =
            {
                "شنبه",
                "یک‌شنبه",
                "دوشنبه",
                "سه‌شنبه",
                "چهارشنبه",
                "پنج‌شنبه",
                "جمعه"
            };
            return persianDays[DayOfWeek];
        }
        catch (Exception e)
        {
            return "";
        }
    }

    public static string GetDayOfWeekName(this int? DayOfWeek)
    {
        if (DayOfWeek == null) return "";

        var _DayOfWeek = (int)DayOfWeek;
        return _DayOfWeek.GetDayOfWeekName();
    }

    public static string ToShamsi(this DateTime value)
    {
        var pc = new PersianCalendar();
        return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" +
               pc.GetDayOfMonth(value).ToString("00");
    }

    public static string ToShamsiWhitTime(this DateTime value)
    {
        var pc = new PersianCalendar();
        return pc.GetYear(value) + "/" + pc.GetMonth(value).ToString("00") + "/" +
               pc.GetDayOfMonth(value).ToString("00") + "  " + pc.GetHour(value).ToString("00") + ":" +
               pc.GetMinute(value).ToString("00") + ":" + pc.GetSecond(value).ToString("00");
    }

    public static string ToTime(this DateTime value)
    {
        var pc = new PersianCalendar();
        return pc.GetHour(value).ToString("00") + ":" + pc.GetMinute(value).ToString("00") + ":" +
               pc.GetSecond(value).ToString("00");
    }

    public static DateTime shamsiToMiladi(this string date)
    {
        var pc = new PersianCalendar();
        var dt = new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)),
            int.Parse(date.Substring(6, 2)), pc);
        return dt;
    }


    public static long ToTimeTicks(this string date)
    {
        long Time = 0;
        Time = Time + int.Parse(date.Substring(0, 2)) * TimeSpan.TicksPerHour;
        Time = Time + int.Parse(date.Substring(3, 2)) * TimeSpan.TicksPerMinute;
        return Time;
    }

    public static string ToTimeString(this long date)
    {
        var Time = new DateTime(date).ToString("HH:mm");
        return Time;
    }

    public static long RemoveTimeFromDate(this long date)
    {
        var dateTime = new DateTime(date);
        var T2 = DateTime.Parse(dateTime.ToShortDateString()).Ticks;
        return T2;
    }


    public static int ToWeekDayNumber(this long date)
    {
        var dateTime = new DateTime(date);
        var DayOfWeek = dateTime.DayOfWeek;
        var Day = DayOfWeek.ToString();
        var dayNumber = (int)Enum.Parse(typeof(DayOfWeek), Day);

        return dayNumber;
    }

    public static DateTime GetTehranTime()
    {
        TimeZoneInfo timeZoneInfo;

        DateTime dateTime;

        timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Iran Standard Time");


        dateTime = TimeZoneInfo.ConvertTime(DateTime.Now.ToUniversalTime(), timeZoneInfo);


        return dateTime;
    }

    public static string GetDayOfWeekName(this DateTime date)
    {
        switch (date.DayOfWeek)
        {
            case DayOfWeek.Saturday: return "شنبه";
            case DayOfWeek.Sunday: return "يکشنبه";
            case DayOfWeek.Monday: return "دوشنبه";
            case DayOfWeek.Tuesday: return "سه‏ شنبه";
            case DayOfWeek.Wednesday: return "چهارشنبه";
            case DayOfWeek.Thursday: return "پنجشنبه";
            case DayOfWeek.Friday: return "جمعه";
            default: return "";
        }
    }
    //How to use it:
}