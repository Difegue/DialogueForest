using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading;

namespace DialogueForest.Core.Services
{
    public class SavedFileMessage
    {
        public FileAbstraction FileAbstraction { get; set; }
    }

    public class ForestDataService
    {
        private DialogueDatabase _currentForest;

        private IApplicationStorageService _storageService;
        private INotificationService _notificationService;

        private const string STORAGE_NAME = "autosave.json";

        private bool _savedFileExists;
        private Timer _autoSaveTimer;

        public FileAbstraction LastSavedFile { get; private set; }

        public ForestDataService(IApplicationStorageService storageService, INotificationService notificationService)
        {
            _storageService = storageService;
            _notificationService = notificationService;

            LastSavedFile = new FileAbstraction
            {
                Extension = ".frst",
                Type = "Dialogue Forest",
                Name = _storageService.GetValue("lastSavedName", "MyForest"),
                Path = _storageService.GetValue<string>("lastSavedFolder", null)
            };


            if (_storageService.GetValue<string>("lastSavedFolder", null) != null)
            {
                _savedFileExists = true;
                WeakReferenceMessenger.Default.Send(new SavedFileMessage { FileAbstraction = LastSavedFile });
            }

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
                
                if (_currentForest == null) 
                {
                    _currentForest = new DialogueDatabase();
                }

                // Autosave to storage every 5 seconds
                _autoSaveTimer = new Timer((e) =>
                {
                    SaveForestToStorage();
                }, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                _notificationService.ShowInAppNotification("Couldn't load autosave!", false);
                _notificationService.ShowErrorNotification(ex);

                _currentForest = new DialogueDatabase();
            }
        }

        public async void LoadForestFromFile()
        {
            var res = await _storageService.LoadDataFromExternalFileAsync(".frst");

            if (res != null)
            {
                try
                {
                    LastSavedFile = res.Item1;
                    _currentForest = await JsonSerializer.DeserializeAsync<DialogueDatabase>(res.Item2);
                    WeakReferenceMessenger.Default.Send(new SavedFileMessage { FileAbstraction = LastSavedFile });
                }
                catch (Exception ex)
                {
                    _notificationService.ShowErrorNotification(ex);
                }

            }
        }

        public async Task LoadForestFromStorageAsync()
        {
            var stream = await _storageService.OpenFileAsync(STORAGE_NAME);
            _currentForest = await JsonSerializer.DeserializeAsync<DialogueDatabase>(stream);
        }

        public void SaveForestToStorage()
        {
            Task.Run(async () =>
            {
                try { await SaveForestToStorageAsync(); }
                catch (Exception ex)
                {
                    _notificationService.ShowInAppNotification("Couldn't autosave!", false);
                    _notificationService.ShowErrorNotification(ex);
                }

            });
        }

        public async Task SaveForestToStorageAsync()
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(_currentForest);
            await _storageService.SaveDataToFileAsync(STORAGE_NAME, bytes);
        }

        public async Task SaveForestToFileAsync()
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(_currentForest);

            // Don't prompt the user if the savedFile already exists
            LastSavedFile = await _storageService.SaveDataToExternalFileAsync(bytes, LastSavedFile, !_savedFileExists);

            if (LastSavedFile != null)
            {
                _savedFileExists = true;
                _storageService.SetValue("lastSavedFolder", LastSavedFile.Path);
                _storageService.SetValue("lastSavedName", LastSavedFile.Name);

                // Send a message to inform VMs we saved to disk
                WeakReferenceMessenger.Default.Send(new SavedFileMessage { FileAbstraction = LastSavedFile });
            }
            else
            {
                _notificationService.ShowInAppNotification("Couldn't save!");
                LastSavedFile = null;
            }

        }

        public List<DialogueTree> GetDialogueTrees() => _currentForest?.Trees;

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

        internal void SetPinnedNode(DialogueNode node, bool isPinned)
        {
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
        }
        public List<string> GetCharacters() => _currentForest.CharacterDefinitions;
        internal void SetCharacters(List<string> chars)
        {
            _currentForest.CharacterDefinitions = chars;
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
