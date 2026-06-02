using System.Globalization;
using Microsoft.Maui.Controls;

namespace FoodDrinkApp.Converters;

public class CategoryToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string category)
        {
            return category.ToLower() switch
            {
                "breakfast" => Color.FromArgb("#FFD504"),
                "lunch" => Color.FromArgb("#DDF49C"),
                "dinner" => Color.FromArgb("#BCB4EF"),
                "snack" => Color.FromArgb("#FFD4DB"),
                "drink" => Color.FromArgb("#D0E8F2"),
                _ => Color.FromArgb("#D9472B")  // 默认红色
            };
        }
        return Color.FromArgb("#D9472B");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
