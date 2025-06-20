using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace MZDNETWORK.Helpers
{
    public static class EnumDisplayHelper
    {
        public static string ToDisplayName(this Enum value)
        {
            if (value == null) return string.Empty;
            var member = value.GetType().GetMember(value.ToString());
            if (member.Length > 0)
            {
                var attr = member[0].GetCustomAttribute<DisplayAttribute>();
                if (attr != null)
                    return attr.GetName();
            }
            return value.ToString();
        }
    }
} 