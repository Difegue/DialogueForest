
namespace DialogueForest.Core.Messages
{
    public record UnsavedModificationsMessage
    {
        public UnsavedModificationsMessage(int words = 0)
        {
            AddedWords = words;
        }

        public int AddedWords { get; init; }
    }
}
