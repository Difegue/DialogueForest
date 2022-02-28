using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Services;

namespace DialogueForest.Core.ViewModels
{
    public partial class ExportViewModel : ObservableObject
    {
        private IApplicationStorageService _applicationStorageService;
        private IInteropService _interop;
        private ForestDataService _dataService;

        private bool _hasInstanceBeenInitialized;

        public ExportViewModel(IApplicationStorageService appStorage, IInteropService interop, ForestDataService dataService)
        {
            _applicationStorageService = appStorage;
            _interop = interop;
            _dataService = dataService;

            /*PropertyChanged += SaveSettings;
            ForestMetadata.CollectionChanged += (s, e) => OnPropertyChanged(nameof(MetadataCount));
            ForestCharacters.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CharacterCount));*/
        }


        [ObservableProperty]
        private string _exportFileLocation;

        [ObservableProperty]
        private OutputFormat _rtfConversionParameter;

        [ICommand]
        private async Task PickExportLocation()
        {
            var suggestedFile = _dataService.LastSavedFile;
            suggestedFile.Extension = ".json";
            suggestedFile.Type = "DialogueForest Export";

            var file = await _applicationStorageService.GetExternalFileAsync(suggestedFile);

            if (file != null)
            {
                ExportFileLocation = file.FullPath;
            }
        }

        [ICommand]
        private async Task Export()
        {

        }

    }
}
