using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace File_Exchange_Manager
{
    public class GlobalUtils
    {
        public static DateTime GetNowUtcDateTime()
        {
            return DateTime.UtcNow;
        }

        public static eWorkInterval ParseWorkIntervalEnum(string _s)
        {
            return GlobalUtils.ToEnum<eWorkInterval>(_s, eWorkInterval._Other);
        }

        public static eDateRange ParseDateRangeEnum(string _s)
        {
            return GlobalUtils.ToEnum<eDateRange>(_s, eDateRange._Other);
        }

        public static T ToEnum<T>(string value, T defaultValue)
        {
            T ret;
            try
            {
                ret = (T)Enum.Parse(typeof(T), value, true);
            }
            catch (Exception)
            {
                ret = defaultValue;
            }
            return ret;
        }
    }
}
