using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.XDelSys
{
    public class Holiday
    {
        public DateTime[] Hols {  get; set; } = Array.Empty<DateTime>();

        public Holiday(Int64 AClientIDX = 0)
        {
            
        }

        public Boolean vIsHoliday(DateTime ADate)
        {
            if (ADate.Date <= new DateTime(2000, 01, 01) || ADate.Date >= new DateTime(2200, 12, 31))
                return true;

            if (Hols.Contains(ADate.Date))
                return true;

            // Below is to test for make up holiday due to holidays falling on Sunday but take into consideration of two or more day holidays
            Boolean IsHol = true;
            DateTime dd = ADate.Date.AddDays(-1);
            while (IsHol)
            {
                IsHol = Hols.Contains(dd);
                if (IsHol && dd.DayOfWeek == DayOfWeek.Sunday)
                    return true;
                if (IsHol)
                    dd = dd.AddDays(-1);
            }
            return IsHol;
        }

        public DateTime vGetWorkingDay(DateTime ADateTime, Int32 DaysAhead)
        {
            return vGetWorkingDay(ADateTime, false, false, DaysAhead);
        }

        public DateTime vGetWorkingDay(DateTime ADateTime, Boolean EnableSaturday = false, Boolean EnableSunday = false, Int32 DaysAhead = 0)
        {
            DateTime RetDateTime = ADateTime;
            Int32 DAH = DaysAhead;
            while ((!EnableSaturday && RetDateTime.DayOfWeek == DayOfWeek.Saturday) ||
                   (!EnableSunday && RetDateTime.DayOfWeek == DayOfWeek.Sunday) || (!EnableSunday && vIsHoliday(RetDateTime.Date)))
            {
                RetDateTime = RetDateTime.AddDays(1);
                //DAH--;
            }
            Int32 FS = 0;
            while (DAH >= 0 && FS < (DaysAhead * 5))
            {
                if (DAH > 0)
                    RetDateTime = RetDateTime.AddDays(1);
                if (EnableSaturday && !EnableSunday)
                {
                    if (RetDateTime.DayOfWeek != DayOfWeek.Sunday &&
                        !vIsHoliday(RetDateTime.Date))
                        DAH--;
                }
                else if (!EnableSaturday && EnableSunday)
                {
                    if (RetDateTime.DayOfWeek != DayOfWeek.Saturday &&
                        !vIsHoliday(RetDateTime.Date))
                        DAH--;
                }
                else if (EnableSaturday && EnableSunday)
                {
                    if (!vIsHoliday(RetDateTime.Date))
                        DAH--;
                }
                else if (RetDateTime.DayOfWeek != DayOfWeek.Saturday &&
                         RetDateTime.DayOfWeek != DayOfWeek.Sunday &&
                         !vIsHoliday(RetDateTime.Date))
                    DAH--;
                FS++;
            }
            return RetDateTime;
        }

        /// <summary>
        /// Gets the next working day, deducts DaysAhead if holiday or Sat/Sun is encountered
        /// </summary>
        /// <param name="ADateTime"></param>
        /// <param name="EnableSaturday"></param>
        /// <param name="EnableSunday"></param>
        /// <param name="DaysAhead"></param>
        /// <param name="IgnoreHolidays">If set to true, DaysAhead will not be deducted when encountering holiday or Sat/Sun</param>
        /// <returns>The next working day</returns>
        public DateTime vGetNextWorkingDay(DateTime ADateTime, Boolean EnableSaturday = false, Boolean EnableSunday = false, Int32 DaysAhead = 0, Boolean IgnoreHolidays = false)
        {
            DateTime RetDateTime = ADateTime;
            Int32 DAH = DaysAhead;
            while ((!EnableSaturday && RetDateTime.DayOfWeek == DayOfWeek.Saturday) ||
                   (!EnableSunday && RetDateTime.DayOfWeek == DayOfWeek.Sunday) || vIsHoliday(RetDateTime.Date))
            {
                RetDateTime = RetDateTime.AddDays(1);
                if (!IgnoreHolidays)
                    DAH--;
            }
            Int32 FS = 0;
            while (DAH >= 0 && FS < 10)
            {
                if (DAH > 0)
                    RetDateTime = RetDateTime.AddDays(1);
                if (EnableSaturday && !EnableSunday)
                {
                    if (RetDateTime.DayOfWeek != DayOfWeek.Sunday &&
                        !vIsHoliday(RetDateTime.Date) && !IgnoreHolidays)
                        DAH--;
                }
                else if (!EnableSaturday && EnableSunday)
                {
                    if (RetDateTime.DayOfWeek != DayOfWeek.Saturday &&
                        !vIsHoliday(RetDateTime.Date) && !IgnoreHolidays)
                        DAH--;
                }
                else if (EnableSaturday && EnableSunday)
                {
                    if (!vIsHoliday(RetDateTime.Date) && !IgnoreHolidays)
                        DAH--;
                }
                else if (RetDateTime.DayOfWeek != DayOfWeek.Saturday &&
                         RetDateTime.DayOfWeek != DayOfWeek.Sunday &&
                         !vIsHoliday(RetDateTime.Date))
                    DAH--;
                FS++;
            }
            return RetDateTime;
        }

        public DateTime vGetPrevWorkingDay(DateTime ADateTime, Int32 DaysBack)
        {
            return vGetPrevWorkingDay(ADateTime, false, false, DaysBack);
        }

        public DateTime vGetPrevWorkingDay(DateTime ADateTime, Boolean EnableSaturday = false, Boolean EnableSunday = false, Int32 DaysBack = 0)
        {
            DateTime RetDateTime = ADateTime;
            Int32 DBK = DaysBack;
            while ((!EnableSaturday && RetDateTime.DayOfWeek == DayOfWeek.Saturday) ||
                   (!EnableSunday && RetDateTime.DayOfWeek == DayOfWeek.Sunday) || vIsHoliday(RetDateTime.Date))
            {
                RetDateTime = RetDateTime.AddDays(-1);
                //DBK--;
            }
            Int32 FS = 0;
            while (DBK >= 0 && FS < 10)
            {
                if (DBK > 0)
                    RetDateTime = RetDateTime.AddDays(-1);
                if (EnableSaturday && !EnableSunday)
                {
                    if (RetDateTime.DayOfWeek != DayOfWeek.Sunday &&
                        !vIsHoliday(RetDateTime.Date))
                        DBK--;
                }
                else if (!EnableSaturday && EnableSunday)
                {
                    if (RetDateTime.DayOfWeek != DayOfWeek.Saturday &&
                        !vIsHoliday(RetDateTime.Date))
                        DBK--;
                }
                else if (EnableSaturday && EnableSunday)
                {
                    if (!vIsHoliday(RetDateTime.Date))
                        DBK--;
                }
                else if (RetDateTime.DayOfWeek != DayOfWeek.Saturday &&
                         RetDateTime.DayOfWeek != DayOfWeek.Sunday &&
                         !vIsHoliday(RetDateTime.Date))
                    DBK--;
                FS++;
            }
            return RetDateTime;
        }

        public DateTime vGet6WorkingDay(DateTime ADateTime, Int32 DaysAhead = 0)
        {
            DateTime RetDateTime = ADateTime;
            Int32 DAH = DaysAhead;
            while (RetDateTime.DayOfWeek == DayOfWeek.Sunday || vIsHoliday(RetDateTime.Date))
            {
                RetDateTime = RetDateTime.AddDays(1);
                //DAH--;
            }
            Int32 FS = 0;
            while (DAH >= 0 && FS < 10)
            {
                if (DAH > 0)
                    RetDateTime = RetDateTime.AddDays(1);
                if (RetDateTime.DayOfWeek != DayOfWeek.Sunday &&
                    !vIsHoliday(RetDateTime.Date))
                    DAH--;
                FS++;
            }
            return RetDateTime;
        }

        public DateTime vGetPrev6WorkingDay(DateTime ADateTime, Int32 DaysBack = 0)
        {
            DateTime RetDateTime = ADateTime;
            Int32 DBK = DaysBack;
            while (RetDateTime.DayOfWeek == DayOfWeek.Sunday || vIsHoliday(RetDateTime.Date))
            {
                RetDateTime = RetDateTime.AddDays(-1);
                //DBK--;
            }
            Int32 FS = 0;
            while (DBK >= 0 && FS < 10)
            {
                if (DBK > 0)
                    RetDateTime = RetDateTime.AddDays(-1);
                if (RetDateTime.DayOfWeek != DayOfWeek.Sunday &&
                    !vIsHoliday(RetDateTime.Date))
                    DBK--;
                FS++;
            }
            return RetDateTime;
        }

    }
}
