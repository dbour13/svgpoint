using System;
using System.Collections.Generic;
using System.Text;

namespace svgpoints.Extensions
{
    public static class EnumExtensions
    {
        public static TEnum? NullableEnumTryParseOrDefault<TEnum>(string value) where TEnum : struct
        {
            TEnum result;
            if (Enum.TryParse(value, out result))
            {
                return result;
            }

            return null;
        }
    }
}
