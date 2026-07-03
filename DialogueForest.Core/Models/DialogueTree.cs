using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.Models
{
    public class DialogueTree
    {
        public DialogueTree(string name)
        {
            Name = name;
            Nodes = new OrderedDictionary<long, DialogueNode>();
            Markers = new List<DialogueMarker>();
        }

        public string Name { get; set; }

        public bool CannotAddNodes { get; set; }

        public OrderedDictionary<long, DialogueNode> Nodes { get; set; }

        public List<DialogueMarker> Markers { get; set; }

        internal void AddNode(DialogueNode node)
        {
            Nodes.Add(node.ID, node);
        }

        internal void MoveNode(DialogueNode node, int position)
        {
            Nodes.Remove(node.ID);
            Nodes.Insert(position, node.ID, node);
        }

        internal void RemoveNode(DialogueNode node)
        {
            Nodes.Remove(node.ID);
        }

        internal void AddMarker(DialogueMarker marker)
        {
            Markers.Add(marker);
        }

        internal void RemoveMarker(DialogueMarker marker)
        {
            Markers.Remove(marker);
        }

        // Fix markers that point to non-existent nodes (e.g. after a node deletion).
        internal void NormalizeMarkers()
        {
            if (Markers == null)
                return;

            foreach (var marker in Markers)
            {
                if (marker.BeforeNodeId != 0 && !Nodes.ContainsKey(marker.BeforeNodeId))
                    marker.BeforeNodeId = 0;
            }
        }
    }
}
