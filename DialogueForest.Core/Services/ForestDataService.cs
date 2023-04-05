using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using System.Threading;
using System.Linq;
using DialogueForest.Core.Messages;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.Services
{

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
                WeakReferenceMessenger.Default.Send(new SavedFileMessage(LastSavedFile));
            }
        }

        public async void InitializeDatabase()
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
                _notificationService.ShowInAppNotification("Couldn't load autosave! " + ex, false);
            }
        }

        #region FileSystem stuff
        public async Task LoadForestFromFileAsync()
        {
            var res = await _storageService.LoadDataFromExternalFileAsync(".frst");

            if (res != null)
            {
                try
                {
                    LastSavedFile = res.Item1;
                    _currentForest = await JsonSerializer.DeserializeAsync<DialogueDatabase>(res.Item2);
                    WeakReferenceMessenger.Default.Send(new SavedFileMessage(LastSavedFile));
                    WeakReferenceMessenger.Default.Send(new TreeUpdatedMessage(false)); // Notify listeners we're loaded
                }
                catch (Exception ex)
                {
                    _notificationService.ShowErrorNotification(ex);
                }

            }
        }

        public async Task LoadForestFromStorageAsync()
        {
            using (var stream = await _storageService.OpenFileAsync(STORAGE_NAME))
            {
                _currentForest = await JsonSerializer.DeserializeAsync<DialogueDatabase>(stream);
            }   
            WeakReferenceMessenger.Default.Send(new TreeUpdatedMessage(false)); // Notify listeners we're loaded
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

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        public async Task SaveForestToStorageAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var bytes = JsonSerializer.SerializeToUtf8Bytes(_currentForest);
                await _storageService.SaveDataToFileAsync(STORAGE_NAME, bytes);
                _storageService.SetValue("lastAutoSave", DateTime.UtcNow.ToString());
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SaveForestToFileAsync(bool promptNewFile = false)
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(_currentForest);

            var promptUser = promptNewFile ? true : !_savedFileExists;

            // Don't prompt the user if the savedFile already exists
            LastSavedFile = await _storageService.SaveDataToExternalFileAsync(bytes, LastSavedFile, promptUser);

            if (LastSavedFile != null)
            {
                _savedFileExists = true;
                _storageService.SetValue("lastSavedFolder", LastSavedFile.Path);
                _storageService.SetValue("lastSavedName", LastSavedFile.Name);

                // Send a message to inform VMs we saved to disk
                WeakReferenceMessenger.Default.Send(new SavedFileMessage(LastSavedFile));
            }
            else
            {
                _notificationService.ShowInAppNotification("Couldn't save!");
                LastSavedFile = null;
            }

        }
        #endregion

        #region Tree/Node API
        public Dictionary<string, MetadataKind> GetMetadataDefinitions() => _currentForest.MetadataDefinitions;
        public List<string> GetCharacters() => _currentForest.CharacterDefinitions;
        public List<DialogueTree> GetDialogueTrees() => _currentForest?.Trees;
        public List<long> GetPinnedNodes() => _currentForest?.PinnedIDs;
        public DialogueTree GetTrash() => _currentForest.Trash;
        public DialogueTree GetNotes() => _currentForest.Notes;

        internal bool IsNodeTrashed(DialogueNode node) => GetTrash().Nodes.ContainsValue(node);
        internal void DeleteNode(DialogueNode node) => GetTrash().RemoveNode(node); // TODO update trash
        internal void DeleteTree(DialogueTree tree)
        {
            var nodeList = tree.Nodes.Values;
            while (nodeList.Count > 0)
                MoveNode(nodeList.First(), tree, GetTrash());

            GetDialogueTrees().Remove(tree);
            WeakReferenceMessenger.Default.Send(new TreeUpdatedMessage());
        }

        internal bool IsNodePinned(DialogueNode node) => _currentForest.PinnedIDs.Contains(node.ID);

        internal DialogueNode CreateNewNode()
        {
            var node = new DialogueNode(_currentForest.LastID);
            _currentForest.LastID++;

            return node;
        }
        internal DialogueTree CreateNewTree(string treeName)
        {
            var tree = new DialogueTree(treeName);
            GetDialogueTrees().Add(tree);

            return tree;
        }

        public Tuple<DialogueTree,DialogueNode> GetNode(long id)
        {
            // TODO super inefficient, needs a lookup table (ID => containing tree)

            foreach (var tree in _currentForest.Trees)
                if (tree.Nodes.ContainsKey(id))
                    return new(tree, tree.Nodes[id]);

            if (_currentForest.Notes.Nodes.ContainsKey(id))
                return new(_currentForest.Notes, _currentForest.Notes.Nodes[id]);

            if (_currentForest.Trash.Nodes.ContainsKey(id))
                return new(_currentForest.Trash, _currentForest.Trash.Nodes[id]);

            return null;
        }

        /// <summary>
        /// Get all nodes linking to a given ID. This only looks in the Tree hosting the node.
        /// TODO: Expand to all trees?
        /// </summary>
        /// <param name="id">ID to find links for</param>
        /// <returns></returns>
        public List<DialogueNode> GetNodesLinkingToID(long id)
        {
            var res = new List<DialogueNode>();
            var tuple = GetNode(id);

            if (tuple != null)
            {
                foreach (var node in tuple.Item1.Nodes)
                {
                    if (node.Value.Prompts.Select(p => p.LinkedID).Contains(id))
                        res.Add(node.Value);
                }
            }

            return res;
        }

        public void MoveNode(DialogueNode node, DialogueTree origin, DialogueTree destination)
        {
            // TODO update lookup table and 

            origin.RemoveNode(node);
            destination.AddNode(node);

            // Notify VMs (both tree and nodeVMs) with a message
            WeakReferenceMessenger.Default.Send(new NodeMovedMessage(origin, destination, node));
        }

        internal void SetPinnedNode(DialogueNode node, bool isPinned)
        {
            if (isPinned)
            {
                _currentForest.PinnedIDs.Add(node.ID);
                _notificationService.ShowInAppNotification(Resources.NotificationPinned);
            } 
            else
            {
                _currentForest.PinnedIDs.Remove(node.ID);
                _notificationService.ShowInAppNotification(Resources.NotificationUnpinned);
            }
            WeakReferenceMessenger.Default.Send(new NodePinnedMessage(node.ID, isPinned));
        }

        internal void SetMetadataDefinitions(Dictionary<string, MetadataKind> data)
        {
            _currentForest.MetadataDefinitions = data;
        }
        
        internal void SetCharacters(List<string> chars)
        {
            _currentForest.CharacterDefinitions = chars;
        }

        #endregion
    }
}
