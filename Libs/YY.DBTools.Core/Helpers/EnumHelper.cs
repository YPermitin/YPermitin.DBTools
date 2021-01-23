using System;
using System.Runtime.CompilerServices;

namespace YY.DBTools.Core.Helpers
{    
    public sealed class EnumHelper
    {
        public static T GetEnumValueByName<T>(string storageTypeByName) where T : struct, Enum
        {
            Enum.TryParse(storageTypeByName, true, out T EnumValue);
            return EnumValue;
        }
    }
}
