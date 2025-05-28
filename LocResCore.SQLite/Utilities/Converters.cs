using System;

namespace LocResCore.SQLite.Utilities
{
    public static class Converters
    {
        public static int ConvertToInt32(this object obj)
        {
            if (obj == DBNull.Value)
                return default;
            else
                return Convert.ToInt32(obj);
        }

        public static long ConvertToInt64(this uint obj)
        {
            return Convert.ToInt64(obj);
        }

        public static string ConvertToString(this object obj)
        {
            if (obj == DBNull.Value)
                return string.Empty;
            else
                return Convert.ToString(obj);
        }

        public static uint ConvertToUInt32(this object obj)
        {
            if (obj == DBNull.Value)
                return default;
            else
                return Convert.ToUInt32(obj);
        }
    }
}
