using DialogueForest.Core.Models;
using DialogueForest.Core.ViewModels;
using CommunityToolkit.WinUI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Dispatching;

namespace DialogueForest.Helpers
{
    public static class UWPHelpers
    {

        /// <summary>
        /// Handle dynamic node loading for links_to/linked_by menuflyouts for DialogueNodePage/DialogueTreePage.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="vm"></param>
        public static void LoadLinkedNodesIntoMenuFlyout(MenuFlyoutSubItem container, DialogueNodeViewModel vm)
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Needed so we can run dispatcherless
            var tag = container.Tag;

            container.Items.Clear();
            container.Items.Add(new MenuFlyoutItem { Text = "..." });

            Task.Run(async () =>
            {
                List<DialogueNode> nodes = null;
                switch (tag)
                {
                    case "links_to": nodes = vm.GetNodesLinkedByUs(); break;
                    case "linked_by": nodes = vm.GetNodesLinkingToUs(); break;
                    default: nodes = new List<DialogueNode>(); break;
                }

                await dispatcherQueue.EnqueueAsync(() =>
                {
                    container.Items.Clear();
                    foreach (var node in nodes)
                    {
                        var flyoutItem = new MenuFlyoutItem { Text = $"#{node.ID} - {node.Title}", Command = vm.OpenNodeCommand, CommandParameter = node.ID };
                        container.Items.Add(flyoutItem);
                    }

                    if (nodes.Count == 0)
                    {
                        container.Items.Add(new MenuFlyoutItem { Text = Localization.Strings.Resources.NoNodesText }); 
                    }

                });

            });
        }
    }
}
