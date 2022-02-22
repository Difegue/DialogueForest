using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DialogueForest.Core.Services
{
    public class ForestDataService
    {
        private DialogueDatabase _currentForest;

        private IApplicationStorageService _storageService;
        private INotificationService _notificationService;

        private const string STORAGE_NAME = "autosave.json";

        public ForestDataService(IApplicationStorageService storageService, INotificationService notificationService)
        {
            _storageService = storageService;
            _notificationService = notificationService;

            InitializeDatabase();
        }

        private async void InitializeDatabase()
        {
            try
            {
                if (await _storageService.DoesFileExistAsync(STORAGE_NAME))
                {
                    await LoadForestFromStorageAsync();
                }
                else
                {
                    _currentForest = new DialogueDatabase();
                }  
            } 
            catch (Exception ex)
            {
                _notificationService.ShowErrorNotification(ex);
            }
        }

        public async void LoadForestFromFile()
        {
            var stream = await _storageService.LoadDataFromExternalFileAsync(".frst");

            if (stream != null)
            {
                _currentForest = await JsonSerializer.DeserializeAsync<DialogueDatabase>(stream);
            }
        }

        public async Task LoadForestFromStorageAsync()
        {
            var stream = await _storageService.OpenFileAsync(STORAGE_NAME);
            _currentForest = await JsonSerializer.DeserializeAsync<DialogueDatabase>(stream);
        }

        public async Task SaveForestToStorageAsync()
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(_currentForest);
            await _storageService.SaveDataToFileAsync(STORAGE_NAME, bytes);
        }

        public async void SaveForestToFile()
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(_currentForest);
            // TODO use suggested name/location if set
            var res = await _storageService.SaveDataToExternalFileAsync(bytes, ".frst");

            if (res.HasValue)
            {
                if (res.GetValueOrDefault())
                    _notificationService.ShowInAppNotification("Saved!");
                else
                    _notificationService.ShowInAppNotification("Couldn't save!");
            }
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
            t.CannotAddNodes = true;

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

        public Dictionary<string, MetadataKind> GetMetadataDefinitions() => _currentForest.MetadataDefinitions;
        internal void SetMetadataDefinitions(Dictionary<string, MetadataKind> data)
        {
            _currentForest.MetadataDefinitions = data;
            SaveForestToStorageAsync();
        }
        public List<string> GetCharacters() => _currentForest.CharacterDefinitions;
        internal void SetCharacters(List<string> chars)
        {
            _currentForest.CharacterDefinitions = chars;
            SaveForestToStorageAsync();
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
