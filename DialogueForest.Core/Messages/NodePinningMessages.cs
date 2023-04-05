using DialogueForest.Core.Models;

namespace DialogueForest.Core.Messages
{
    public record AskToPinNodeMessage(DialogueNode node, bool pinStatus);
    public record NodePinnedMessage(long nodeId, bool pinStatus);
}
