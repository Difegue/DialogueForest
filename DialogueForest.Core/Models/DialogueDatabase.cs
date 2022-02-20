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
            Notes = new DialogueTree(Localization.Strings.Resources.NavigationNotes);
            Trash = new DialogueTree(Localization.Strings.Resources.NavigationTrash);
            Trash.CannotAddNodes = true;

            MetadataDefinitions = new Dictionary<string, MetadataKind>();
            CharacterDefinitions = new List<string>();
        }

        public long LastID { get; set; }

        public List<DialogueTree> Trees { get; set; }

        public List<long> PinnedIDs { get; set; }

        public DialogueTree Notes { get; set; }

        public DialogueTree Trash { get; set; }

        public Dictionary<string, MetadataKind> MetadataDefinitions { get; set; }

        public List<string> CharacterDefinitions { get; set; }

    }
}
