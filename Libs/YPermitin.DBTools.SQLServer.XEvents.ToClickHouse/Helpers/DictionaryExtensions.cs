using System;
using System.Collections.Generic;

namespace YPermitin.DBTools.SQLServer.XEvents.ToClickHouse.Helpers
{
    internal static class DictionaryExtensions
    {
        public static string GetStringValueByKey(this IReadOnlyDictionary<string, object> props, string name)
        {
            if (props.TryGetValue(name, out object resultValue))
                return resultValue as string;
            return null;
        }
        public static long? GetLongValueByKey(this IReadOnlyDictionary<string, object> props, string name)
        {

            if (props.TryGetValue(name, out object resultValue))
            {
                try
                {
                    return Convert.ToInt64(resultValue);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
        public static bool? GetBoolValueByKey(this IReadOnlyDictionary<string, object> props, string name)
        {
            if (props.TryGetValue(name, out object resultValue))
                return resultValue as bool?;
            return null;
        }
        public static byte[] GetByteArrayValueByKey(this IReadOnlyDictionary<string, object> props, string name)
        {
            if (props.TryGetValue(name, out object resultValue))
                return resultValue as byte[];
            return null;
        }
        public static Guid? GetGuidValueByKey(this IReadOnlyDictionary<string, object> props, string name)
        {
            if (props.TryGetValue(name, out object resultValue))
                return resultValue as Guid?;
            return null;
        }
    }
}
