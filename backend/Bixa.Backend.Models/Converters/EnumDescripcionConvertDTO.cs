using Bixa.Backend.Models.Enums;
using System.ComponentModel;

namespace Bixa.Backend.Models.Converters
{
    public static class EnumDescripcionConvertDTO
    {
        public static string? GetEnumDescription(BudgetaryItemEnum value)
        {
            var type = typeof(BudgetaryItemEnum);
            var memInfo = type.GetMember(value.ToString());
            if (memInfo.Length > 0)
            {
                var attrs = memInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attrs.Length > 0)
                    return ((System.ComponentModel.DescriptionAttribute)attrs[0]).Description;
            }
            return value.ToString();
        }
    }
}