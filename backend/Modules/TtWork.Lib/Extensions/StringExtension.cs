using System;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using TtWork.Lib;

namespace TtWork.Lib.Extensions {
    public static class StringEx {
        public static string FormatWith(this string str, params object[] args) {
            return string.Format(str, args);
        }

        /// <summary>
        /// unix时间转换为datetime
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeToTime(this string timeStamp) {
            var dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var lTime = long.Parse(timeStamp + "0000000");
            var toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }


        public static DateTime UnixTimeToTime2(string timeStamp) {
            var dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var lTime = long.Parse(timeStamp + "0000");
            var toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }

        public static DateTime UnixTimeToTime2(long timeStamp) {
            var dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            var lTime = timeStamp * 10000;
            var toNow = new TimeSpan(lTime);
            return dtStart.Add(toNow);
        }


        public static string MaskPhoneNumber(this string PhoneNumber) {
            if (!PhoneNumber.IsNullOrEmptyOrWhiteSpace() && PhoneNumber.Length == 11) {
                return Regex.Replace(PhoneNumber, "(\\d{3})\\d{5}(\\d{3})", "$1*****$2");
            }

            return PhoneNumber;
        }


        public static int TimeToJsTime(this DateTime time) {
            var startTime = new System.DateTime(1970, 1, 1);
            return (int)(time - startTime).TotalSeconds;
        }

        /// <summary>
        /// datetime转换为unixtime
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static int TimeToUnixTime(this DateTime time) {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds;
        }

        public static DateTime JavascriptTimeToTime(this long timestamp) {
            var startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            return startTime.AddMilliseconds(timestamp);
        }

        public static Random random = new Random();

        /// <summary>
        /// 取随机数
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string BuildRandomStr(int length) {
            int num;

            lock (random) {
                num = random.Next();
            }

            string str = num.ToString();

            if (str.Length > length) {
                str = str.Substring(0, length);
            }
            else if (str.Length < length) {
                int n = length - str.Length;
                while (n > 0) {
                    str = str.Insert(0, "0");
                    n--;
                }
            }

            return str;
        }

        public static string AdvSubString(this string text, int num) {
            try {
                return text.Substring(0, num) + "..";
            }
            catch {
                return text;
            }
        }

        /// <summary>
        /// Adds a char to end of given string if it does not ends with the char.
        /// </summary>
        public static string EnsureEndsWith(this string str, char c) {
            return EnsureEndsWith(str, c, StringComparison.Ordinal);
        }

        /// <summary>
        /// Adds a char to end of given string if it does not ends with the char.
        /// </summary>
        public static string EnsureEndsWith(this string str, char c, StringComparison comparisonType) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.EndsWith(c.ToString(), comparisonType)) {
                return str;
            }

            return str + c;
        }

        /// <summary>
        /// Adds a char to end of given string if it does not ends with the char.
        /// </summary>
        public static string EnsureEndsWith(this string str, char c, bool ignoreCase, CultureInfo culture) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.EndsWith(c.ToString(culture), ignoreCase, culture)) {
                return str;
            }

            return str + c;
        }

        /// <summary>
        /// Adds a char to beginning of given string if it does not starts with the char.
        /// </summary>
        public static string EnsureStartsWith(this string str, char c) {
            return EnsureStartsWith(str, c, StringComparison.Ordinal);
        }

        /// <summary>
        /// Adds a char to beginning of given string if it does not starts with the char.
        /// </summary>
        public static string EnsureStartsWith(this string str, char c, StringComparison comparisonType) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.StartsWith(c.ToString(), comparisonType)) {
                return str;
            }

            return c + str;
        }

        /// <summary>
        /// Adds a char to beginning of given string if it does not starts with the char.
        /// </summary>
        public static string EnsureStartsWith(this string str, char c, bool ignoreCase, CultureInfo culture) {
            if (str == null) {
                throw new ArgumentNullException(nameof(str));
            }

            if (str.StartsWith(c.ToString(culture), ignoreCase, culture)) {
                return str;
            }

            return c + str;
        }
    }
}