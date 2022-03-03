using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Models;
using DialogueForest.Core.Services;
using DialogueForest.Localization.Strings;

namespace DialogueForest.Core.ViewModels
{
    public partial class ExportViewModel : ObservableObject
    {
        private IApplicationStorageService _applicationStorageService;
        private INotificationService _notificationService;
        private IDialogService _dialogService;
        private ForestDataService _dataService;

        public ExportViewModel(IApplicationStorageService appStorage, INotificationService notificationService, IDialogService dialogService, ForestDataService dataService)
        {
            _applicationStorageService = appStorage;
            _notificationService = notificationService;
            _dataService = dataService;
            _dialogService = dialogService;

            PropertyChanged += SaveExportSettings;

            ExportFolder = _applicationStorageService.GetValue<string>(nameof(ExportFolder), null);
            ExportSeparateFiles = _applicationStorageService.GetValue(nameof(ExportSeparateFiles), false);

            var rtfParameter = _applicationStorageService.GetValue(nameof(RtfConversionParameter), OutputFormat.HTML.ToString());
            Enum.TryParse(rtfParameter, out OutputFormat format);
            RtfConversionParameter = format;
        }

        private void SaveExportSettings(object sender, PropertyChangedEventArgs e)
        {
            _applicationStorageService.SetValue(nameof(ExportFolder), _exportFolder);
            _applicationStorageService.SetValue(nameof(ExportSeparateFiles), _exportSeparateFiles);
            _applicationStorageService.SetValue(nameof(RtfConversionParameter), _rtfConversionParameter.ToString());
        }

        [ObservableProperty]
        private string _exportFolder;

        [ObservableProperty]
        private string _currentExportedTree;

        [ObservableProperty]
        private bool _exportSeparateFiles;

        [ObservableProperty]
        private bool _isExportInProgress;

        [ObservableProperty]
        private OutputFormat _rtfConversionParameter;

        [ICommand]
        private async Task PickExportLocation()
        {
            var folder = await _applicationStorageService.GetExternalFolderAsync();

            if (folder != null)
            {
                ExportFolder = folder.Path;
            }
        }

        [ICommand(AllowConcurrentExecutions = false)]
        private async Task Export()
        {
            if (_exportFolder == null)
            {
                await PickExportLocation();

                if (_exportFolder == null) return;
            }

            IsExportInProgress = true;

            try
            {
                // Craft a nodelist we'll export to a JSON file
                List<Dictionary<string, object>> nodes = new();

                foreach (var tree in _dataService.GetDialogueTrees())
                {
                    // Indicate to the UI we're handling this tree
                    CurrentExportedTree = tree.Name;

                    var convertedNodes = tree.Nodes.Values.Select(n => ExportNode(n));
                    nodes.AddRange(convertedNodes);

                    // If export to split files is on, export to a JSON now and clear out the nodelist
                    if (_exportSeparateFiles)
                    {
                        var treeBytes = JsonSerializer.SerializeToUtf8Bytes(nodes);
                        var savedFile = new FileAbstraction
                        {
                            Name = tree.Name,
                            Extension = ".json",
                            Path = _exportFolder
                        };
                        await _applicationStorageService.SaveDataToExternalFileAsync(treeBytes, savedFile, false);
                        nodes.Clear();
                    }
                }

                // Export the entire nodelist in one file
                if (!_exportSeparateFiles)
                {
                    var forestBytes = JsonSerializer.SerializeToUtf8Bytes(nodes);
                    var savedFile = new FileAbstraction
                    {
                        Name = _dataService.LastSavedFile.Name,
                        Extension = ".json",
                        Path = _exportFolder
                    };
                    await _applicationStorageService.SaveDataToExternalFileAsync(forestBytes, savedFile, false);
                }

                IsExportInProgress = false;
                _notificationService.ShowInAppNotification(Resources.NotificationExportSuccess);
            } catch (Exception ex)
            {
                IsExportInProgress = false;
                _notificationService.ShowInAppNotification(Resources.NotificationExportFailed, false);
                _notificationService.ShowErrorNotification(ex);
            }
            
        }

        private Dictionary<string, object> ExportNode(DialogueNode node)
        {
            var export = new Dictionary<string, object>();
            var exportedLines = new List<Dictionary<string, object>>();

            // Flatten all of the node's parameters in our dictionary
            export.Add("ID", node.ID);
            export.Add("Title", node.Title);

            foreach (var line in node.DialogueLines)
            {
                var convertedLine = new Dictionary<string, object>();
                convertedLine.Add("Text", RtfHelper.Convert(line.RichText, _rtfConversionParameter));
                convertedLine.Add("Character", line.Character);

                exportedLines.Add(convertedLine);
            }

            export.Add("Lines", exportedLines);
            export.Add("Prompts", node.Prompts);

            foreach (var metadata in node.Metadata)
            {
                export.Add(metadata.Key, metadata.Value);
            }

            return export;
        }
    }
}
