using System;
using System.Globalization;

namespace HuatanApi.Tools
{
    public static class ToolsFormat
    {
        private static string _date = "yyyy-MM-dd";
        // 2020-09-11T05:00:00.000Z
        private static string _dateTypescript = "yyyy-MM-dd'T'HH:mm:ss.fffZ";

        public static DateTime ToDate(this string value)
        {
            return DateTime.Parse(value);
        }

        public static DateTime ToDateFromData(this string value, string format)
        {
            return DateTime.ParseExact(value, format, CultureInfo.InvariantCulture);
        }

        public static DateTime ToDateFromData(this string value)
        {
            return DateTime.ParseExact(value, _date, CultureInfo.InvariantCulture);
        }


        public static int GetBimester(this DateTime value)
        {
            int result = (value.Month - 1) / 2 + 1;
            return result;
        }

        public static int GetYear(this DateTime value)
        {
            return value.Year;
        }

        public static string ToDateString(this DateTime value)
        {
            return value.ToString(_date);
        }

        public static string ToHourString(this DateTime value)
        {
            return value.ToString("HH:mm ");
        }

        public static string ToHourString(this DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("HH:mm ") : "";
        }
    }
}