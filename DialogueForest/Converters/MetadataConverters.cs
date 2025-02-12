﻿using System;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml.Data;

namespace DialogueForest.Converters
{
    /// <summary>
    ///     Converts objects to Windows.UI.Color
    /// </summary>
    public partial class ColorMetadataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Windows.UI.Color color)
                return color;
            else if (value is string s)
                return s.ToColor();
            else
                return Windows.UI.Color.FromArgb(1, 1, 1, 1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Windows.UI.Color color)
                return color.ToHex();
            else
                return value;
        }
    }

    /// <summary>
    ///     Converts objects to boolean
    /// </summary>
    public partial class BoolMetadataConverter : IValueConverter
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

    /// <summary>
    ///     Converts objects to string
    /// </summary>
    public partial class StringMetadataConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string s)
                return s;
            else
                return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
