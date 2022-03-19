using DialogueForest.Core.Models;

namespace DialogueForest.Core.Messages
{
    public record NodeMovedMessage(DialogueTree SourceTree, DialogueTree DestinationTree, DialogueNode NodeMoved);
}
