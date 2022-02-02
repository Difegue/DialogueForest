using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.Models
{
    public class DialogueTree
    {
        public string Name { get; set; }

        public Dictionary<float, DialogueNode> Nodes { get; set; }

    }
}
