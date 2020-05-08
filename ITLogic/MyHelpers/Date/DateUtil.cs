using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyHelpers.Date
{
    public static class DateUtil
    {
        /// <summary>
        /// gets all dates between start date and end date
        /// </summary>
        /// <param name="start">start date</param>
        /// <param name="end">end date</param>
        /// <returns>all dates between start date and end date</returns>
        public static IEnumerable<DateTime> AllDatesBetween(DateTime start, DateTime end)
        {
            for (var _day = start.Date; _day <= end; _day = _day.AddDays(1))
                yield return _day;
        }

        /// <summary>
        /// calculate age form date
        /// </summary>
        /// <param name="date">the date</param>
        /// <returns>the age</returns>
        public static int CalculateAge(DateTime date)
        {
            DateTime _now = DateTime.Today;
            int _age = _now.Year - date.Year;
            if (date > _now.AddYears(-_age)) _age--;

            return _age;
        }

        /// <summary>
        /// Retorna la cantidad de milisegundos desde el Epoch (01/01/1970)
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static Double toTimeStamp(DateTime time)
        {
            return Math.Floor((time - new DateTime(1970, 1, 1)).TotalMilliseconds);
        }

        public static DateTime toDay(DateTime time)
        {
            return new DateTime(time.Year, time.Month, time.Day);
        }
    }
}
