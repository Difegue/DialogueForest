using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.Models
{
    public class DialogueForest
    {

        public List<DialogueTree> Trees { get; set; }

        public List<float> PinnedIDs { get; set; }

        public DialogueTree Notes { get; set; }

        public DialogueTree Trash { get; set; }

        public Dictionary<string, MetadataKind> MetadataDefinitions { get; set; }

    }
}
