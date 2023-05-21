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
using CommunityToolkit.Mvvm.DependencyInjection;
using DialogueForest.Core.ViewModels;
using System.IO;

namespace DialogueForest.Core.Services
{

    public class ForestDataService
    {
        private DialogueDatabase _currentForest;

        private IApplicationStorageService _storageService;
        private INotificationService _notificationService;
        private IInteropService _interopService;
        private IDialogService _dialogService;

        private const string STORAGE_NAME = "autosave.json";
        
        private bool _savedFileExists;
        private Timer _autoSaveTimer;

        public FileAbstraction LastSavedFile { get; private set; }
        public bool CurrentForestHasUnsavedChanges { get; private set; }

        public ForestDataService(IApplicationStorageService storageService, INotificationService notificationService, IInteropService interopService, IDialogService dialogService)
        {
            _storageService = storageService;
            _notificationService = notificationService;
            _interopService = interopService;
            _dialogService = dialogService;

            if (_storageService.GetValue<string>("lastSavedFolder", null) != null)
            {
                _savedFileExists = true;
                LastSavedFile = new FileAbstraction
                {
                    Extension = ".frst",
                    Type = "Dialogue Forest",
                    Name = _storageService.GetValue("lastSavedName", "MyForest"),
                    Path = _storageService.GetValue<string>("lastSavedFolder", null)
                };
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

        public void ResetDatabase()
        {
            _currentForest = new DialogueDatabase();
            _storageService.DeleteFileAsync(STORAGE_NAME);
            _storageService.SetValue<string>("lastSavedName", null);
            _storageService.SetValue<string>("lastSavedFolder", null);

            LastSavedFile = null;

            Ioc.Default.GetRequiredService<SettingsViewModel>().LoadCurrentForestSettings();
            CurrentForestHasUnsavedChanges = false;
            _storageService.SetValue(nameof(CurrentForestHasUnsavedChanges), false);

            WeakReferenceMessenger.Default.Send(new TreeUpdatedMessage(false));
        }

        #region FileSystem stuff

        public void SetForestDirty(bool isDirty)
        {
            CurrentForestHasUnsavedChanges = isDirty;
            _storageService.SetValue(nameof(CurrentForestHasUnsavedChanges), isDirty);
        }
        
        public async Task LoadForestFromFileAsync(FileAbstraction file = null)
        {
            // Load from the provided file if there's one, or open the filepicker
            var res = file != null ? Tuple.Create(file, (Stream)File.OpenRead(file.FullPath)) : 
                                     await _storageService.LoadDataFromExternalFileAsync(".frst");

            if (res != null)
            {
                try
                {
                    LastSavedFile = res.Item1;
                    _storageService.SetValue("lastSavedFolder", LastSavedFile.Path);
                    _storageService.SetValue("lastSavedName", LastSavedFile.Name);
                    _currentForest = await JsonSerializer.DeserializeAsync<DialogueDatabase>(res.Item2);

                    // Reload settings
                    Ioc.Default.GetRequiredService<SettingsViewModel>().LoadCurrentForestSettings();
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
            var unsavedStatus = _storageService.GetValue(nameof(CurrentForestHasUnsavedChanges), false);
            using (var stream = await _storageService.OpenFileAsync(STORAGE_NAME))
            {
                _currentForest = await JsonSerializer.DeserializeAsync<DialogueDatabase>(stream);
            }
            Ioc.Default.GetRequiredService<SettingsViewModel>().LoadCurrentForestSettings();
            CurrentForestHasUnsavedChanges = unsavedStatus; // Reset status here as it's likely been changed while loading everything
            WeakReferenceMessenger.Default.Send(new TreeUpdatedMessage(CurrentForestHasUnsavedChanges)); // Notify listeners we're loaded
        }

        public void SaveForestToStorage()
        {
            Task.Run(async () =>
            {
                await _semaphore.WaitAsync();
                var lastAutoSave = DateTime.Parse(_storageService.GetValue("lastAutoSave", DateTime.Today.ToString()));
                var diskLastModifiedTime = _interopService.GetLastModifiedTime(LastSavedFile);

                if (diskLastModifiedTime > lastAutoSave)
                {
                    var confirm = await _dialogService.ShowConfirmDialogAsync(Resources.ContentDialogOldAutosave, string.Format(Resources.ContentDialogOldAutosaveDesc, LastSavedFile.FullPath), 
                        Resources.ButtonYesText, Resources.ButtonNoText);

                    if (confirm)
                    {
                        await LoadForestFromFileAsync(LastSavedFile);
                        _storageService.SetValue("lastAutoSave", DateTime.UtcNow.ToString());
                        _semaphore.Release();
                        return;
                    }
                    _semaphore.Release();
                }

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

            // Don't prompt the user if the savedFile already exists
            var promptUser = promptNewFile ? true : !_savedFileExists;
            
            var savedFile = await _storageService.SaveDataToExternalFileAsync(bytes, LastSavedFile, promptUser);

            if (savedFile != null)
            {
                LastSavedFile = savedFile;
                _savedFileExists = true;
                _storageService.SetValue("lastSavedFolder", LastSavedFile.Path);
                _storageService.SetValue("lastSavedName", LastSavedFile.Name);

                // Send a message to inform VMs we saved to disk
                WeakReferenceMessenger.Default.Send(new SavedFileMessage(LastSavedFile));
            } 
            else
            {
                // If we used "Save As", we don't need to reset the lastSavedFile since the OG is still present
                if (!promptNewFile) 
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
        public long GetLastID() => _currentForest?.LastID ?? 0;

        internal bool IsNodeTrashed(DialogueNode node) => GetTrash().Nodes.ContainsValue(node);
        internal void DeleteNode(DialogueNode node) => GetTrash().RemoveNode(node);
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
