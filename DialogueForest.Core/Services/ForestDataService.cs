using DialogueForest.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.Services
{
    public class ForestDataService
    {
        private DialogueDatabase _currentForest;

        public ForestDataService()
        {
            // TODO
            _currentForest = new DialogueDatabase();
        }

        public void LoadForestFromFile()
        {

        }

        public void LoadForestFromStorage()
        {

        }

        public void SaveForestToFile()
        {

        }

        public List<DialogueTree> GetDialogueTrees() => _currentForest.Trees;

        public DialogueNode GetNode(long id)
        {
            
            foreach (var tree in _currentForest.Trees)
                if (tree.Nodes.ContainsKey(id))
                    return tree.Nodes[id];

            if (_currentForest.Notes.Nodes.ContainsKey(id))
                return _currentForest.Notes.Nodes[id];

            if (_currentForest.Trash.Nodes.ContainsKey(id))
                return _currentForest.Trash.Nodes[id];

            return null;
        }

        public DialogueTree GetTrash() => _currentForest.Trash;

        public DialogueTree GetPins()
        {
            // TODO kinda inefficient and prone to bugs
            var t = new DialogueTree("Pins");

            foreach (var id in _currentForest.PinnedIDs)
                t.AddNode(GetNode(id));

            return t;
        }

        public DialogueTree GetNotes() => _currentForest.Notes;

        internal bool IsNodeTrashed(DialogueNode node) => _currentForest.Trash.Nodes.ContainsValue(node);

        internal void SetPinnedNode(DialogueNode node, bool isPinned) {
            if (isPinned)
                _currentForest.PinnedIDs.Add(node.ID);
            else
                _currentForest.PinnedIDs.Remove(node.ID);

            // TODO: Notify PinnedVM
        }

        internal void DeleteNode(DialogueNode node) => _currentForest.Trash.RemoveNode(node); // TODO update trash

        internal bool IsNodePinned(DialogueNode node) => _currentForest.PinnedIDs.Contains(node.ID);

        internal void MoveNodeToTrash(DialogueTree tree, DialogueNode node)
        {
            tree.RemoveNode(node);
            _currentForest.Trash.AddNode(node);
        }

        internal DialogueNode CreateNewNode()
        {
            var node = new DialogueNode(_currentForest.LastID);
            _currentForest.LastID++;

            return node;
        }

        internal DialogueTree CreateNewTree(string treeName)
        {
            var tree = new DialogueTree(treeName);
            _currentForest.Trees.Add(tree);

            return tree;
        }
    }
}
