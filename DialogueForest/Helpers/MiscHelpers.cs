using System.Linq;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace DialogueForest.Helpers
{
    public static class UWPHelpers
    {

        public static string ConvertRtfWhiteTextToBlack(this string s)
        => s.Replace("\\red255\\green255\\blue255", "\\red0\\green0\\blue0");

        public static string ConvertRtfBlackTextToWhite(this string s)
            => s.Replace("\\red0\\green0\\blue0", "\\red255\\green255\\blue255");

        // If no items are selected, select the one underneath us.
        // https://github.com/microsoft/microsoft-ui-xaml/issues/911
        public static void SelectItemOnFlyoutRightClick<T>(ListView QueueList, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            // We can get the correct ViewModel by looking at the OriginalSource of the right-click event. 
            var s = (FrameworkElement)e.OriginalSource;
            var d = s.DataContext;

            if (d is T && !QueueList.SelectedItems.Contains(d))
            {
                var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);

                // Don't clear selectedItems if shift is pressed (multi-selection)
                if ((state & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down)
                {
                    var pos1 = QueueList.Items.IndexOf(QueueList.SelectedItems.First());
                    var pos2 = QueueList.Items.IndexOf(d);

                    if (pos2 < pos1)
                        QueueList.SelectRange(new ItemIndexRange(pos2, (uint)(pos1 - pos2)));
                    else
                        QueueList.SelectRange(new ItemIndexRange(pos1, (uint)(pos2 - pos1 + 1)));
                }
                else
                {
                    QueueList.SelectedItems.Clear();
                    QueueList.SelectedItems.Add(d);
                }
            }
        }
    }
}
