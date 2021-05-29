using System;

namespace YY.DBTools.Core.Helpers
{
    public static class StringExtensions
    {
        private static string _emptyGuid = Guid.Empty.ToString();

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        public static string NormalizeShortUUID(this string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length != 32)
                return _emptyGuid;

            string GUIDAsString = value.Substring(24, 8)
                                  + "-"
                                  + value.Substring(20, 4)
                                  + "-"
                                  + value.Substring(16, 4)
                                  + "-"
                                  + value.Substring(0, 4)
                                  + "-"
                                  + value.Substring(4, 12);

            if (Guid.TryParse(GUIDAsString, out _))
                return GUIDAsString;
            else
                return _emptyGuid;
        }
        public static string FixNetworkPath(this string sourceValue)
        {
            if (sourceValue.Length > 1 && sourceValue[0] == '\\' && sourceValue[1] != '\\')
                return "\\" + sourceValue;
            else
                return sourceValue;
        }
    }
}
