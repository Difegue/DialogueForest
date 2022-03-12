using DialogueForest.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.Messages
{
    public class NodeMovedMessage
    {
        public DialogueTree SourceTree;
        public DialogueTree DestinationTree;

        public DialogueNode NodeMoved;
    }
}
