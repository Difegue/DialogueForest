using SkiaSharp;
using System;
using SkiaSharp.Views.UWP;
using Windows.UI.Xaml.Data;

namespace DialogueForest.Converters
{
    /// <summary>
    ///     Converts objects to Windows.UI.Color
    /// </summary>
    public class ColorMetadataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Windows.UI.Color color)
                return color;
            else
                return Windows.UI.Color.FromArgb(1, 1, 1, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }

    /// <summary>
    ///     Converts objects to boolean
    /// </summary>
    public class BoolMetadataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool b)
                return b;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
