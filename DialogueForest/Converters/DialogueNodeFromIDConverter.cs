using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.Services;
using Windows.UI.Xaml.Data;

namespace DialogueForest.Converters
{
    public class DialogueNodeFromIDConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is long ID)
            {
                var dataService = Ioc.Default.GetRequiredService<ForestDataService>();
                var node = dataService.GetNode(ID);

                if (node != null)
                    return $"#{ID} - {node.Item2.Title} ({node.Item1.Name})";

                return "???";
            }

            return "???";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
