using System.Collections.Generic;

namespace DialogueForest.Core.Models
{
    public class DialogueNode
    {
        public long ID { get; set; }

        public string Title { get; set; }

        public List<DialogueText> DialogueLines { get; set; }

        public List<DialogueReply> Prompts { get; set; }

        public List<DialogueMetadataValue> Metadata { get; set; }

        public DialogueNode() { }

        public DialogueNode(long nodeId)
        {
            ID = nodeId;
            Title = "Dialogue #" + nodeId;

            DialogueLines = new List<DialogueText>
            {
                new DialogueText()
            };

            Prompts = new List<DialogueReply>();
            Metadata = new List<DialogueMetadataValue>();
        }

    }
}
