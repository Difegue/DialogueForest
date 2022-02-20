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
            Nodes = new Dictionary<float, DialogueNode>();
        }

        public string Name { get; set; }

        public bool CannotAddNodes { get; set; }

        public Dictionary<float, DialogueNode> Nodes { get; set; }

        internal void AddNode(DialogueNode node)
        {
            Nodes.Add(node.ID, node);
        }

        internal void RemoveNode(DialogueNode node)
        {
            Nodes.Remove(node.ID);
        }
    }
}
