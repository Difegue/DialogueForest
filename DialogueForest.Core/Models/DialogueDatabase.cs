using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.Models
{
    public class DialogueDatabase
    {
        public DialogueDatabase()
        {
            LastID = 1;

            Trees = new List<DialogueTree>();
            PinnedIDs = new List<long>();
            Notes = new DialogueTree("NOtes");
            Trash = new DialogueTree("Tr4sh");

            MetadataDefinitions = new Dictionary<string, MetadataKind>();
            CharacterDefinitions = new Dictionary<string, string>();
        }

        public long LastID { get; set; }

        public List<DialogueTree> Trees { get; set; }

        public List<long> PinnedIDs { get; set; }

        public DialogueTree Notes { get; set; }

        public DialogueTree Trash { get; set; }

        public Dictionary<string, MetadataKind> MetadataDefinitions { get; set; }

        public Dictionary<string, string> CharacterDefinitions { get; set; }

    }
}
