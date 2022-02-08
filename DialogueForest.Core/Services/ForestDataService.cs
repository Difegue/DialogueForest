using DialogueForest.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.Services
{
    public class ForestDataService
    {
        private DialogueDatabase _currentForest;

        public void LoadForestFromFile()
        {

        }

        public void LoadForestFromStorage()
        {

        }

        public void SaveForestToFile()
        {

        }

        public DialogueNode GetNode(long id)
        {
            // TODO
            return new DialogueNode(id);
        }

        internal bool IsNodeTrashed(DialogueNode node) => _currentForest.Trash.Nodes.ContainsValue(node);

        internal void SetPinnedNode(DialogueNode node, bool isPinned)
        {
            throw new NotImplementedException();
        }

        internal void DeleteNode(DialogueNode node)
        {
            throw new NotImplementedException();
        }

        internal bool IsNodePinned(DialogueNode node)
        {
            throw new NotImplementedException();
        }

        internal void MoveNodeToTrash(DialogueNode node)
        {
            throw new NotImplementedException();
        }
    }
}
