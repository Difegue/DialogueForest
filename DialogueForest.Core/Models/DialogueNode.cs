using System.Collections.Generic;

namespace DialogueForest.Core.Models
{
    public class DialogueNode
    {
        public long ID { get; set; }

        public string Title { get; set; }

        public string Character { get; set; }

        public List<string> DialogueLines { get; set; }

        public Dictionary<string, long> Prompts { get; set; }

        public Dictionary<string, object> Metadata { get; set; }

    }
}
