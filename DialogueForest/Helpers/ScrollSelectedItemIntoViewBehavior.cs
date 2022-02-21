using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DialogueForest.Helpers
{
    public class ScrollSelectedItemIntoViewBehavior : Behavior<ListViewBase>
    {

        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            base.OnAttached();
        }
        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            base.OnDetaching();
        }

        private void AssociatedObject_SelectionChanged(object sender,
                                                        SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.FirstOrDefault();
            if (item != null)
            {
                if (AssociatedObject.IsLoaded)
                    AssociatedObject.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
                else
                    AssociatedObject.Loaded += (s,e) => AssociatedObject.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
            }
        }
    }
}
