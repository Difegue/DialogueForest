using DialogueForest.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using Windows.Foundation.Collections;
using System.Collections.Generic;

namespace DialogueForest.Services
{
    public sealed class ApplicationStorageService : IApplicationStorageService
    {
        /// <summary>
        /// The <see cref="IPropertySet"/> with the settings targeted by the current instance.
        /// </summary>
        private readonly IPropertySet SettingsStorage = ApplicationData.Current.LocalSettings.Values;

        private async Task<StorageFolder> GetFolderAsync(string name)
        {
            var localFolder = ApplicationData.Current.LocalFolder;

            if (name != "")
            {
                localFolder = await localFolder.CreateFolderAsync(name, CreationCollisionOption.OpenIfExists);
            }

            return localFolder;
        }

        public async Task<bool> DoesFileExistAsync(string fileName, string parentFolder = "")
        {
            var folder = await GetFolderAsync(parentFolder);
            return await StorageFileHelper.FileExistsAsync(folder, fileName);
        }

        public async Task SaveDataToFileAsync(string fileName, byte[] data, string parentFolder = "")
        {
            var folder = await GetFolderAsync(parentFolder);
            await StorageFileHelper.WriteBytesToFileAsync(folder, data, fileName);
        }

        public async Task<Stream> OpenFileAsync(string fileName, string parentFolder = "")
        {
            var folder = await GetFolderAsync(parentFolder);
            var file = await folder.GetFileAsync(fileName);

            return await file.OpenStreamForReadAsync();
        }

        public async Task DeleteFolderAsync(string folderName)
        {
            var folder = await GetFolderAsync(folderName);

            if (folder != ApplicationData.Current.LocalFolder)
            {
                await folder.DeleteAsync();
            }
        }

        public void SetValue<T>(string key, T value)
        {
            if (!SettingsStorage.ContainsKey(key)) SettingsStorage.Add(key, value);
            else SettingsStorage[key] = value;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (SettingsStorage.TryGetValue(key, out object value))
            {
                try
                {
                    return (T)value;
                }
                catch
                {
                    // Corrupted storage, return default
                }
            }

            return defaultValue;
        }

        public async Task<bool?> SaveDataToExternalFileAsync(byte[] bytes, string fileExtension)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker
            {
                SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add(fileExtension + " File", new List<string>() { fileExtension });
            // Default file name if the user does not type one in or select a file to replace
            //savePicker.SuggestedFileName = "New Document";

            StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                await FileIO.WriteBytesAsync(file, bytes);

                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await CachedFileManager.CompleteUpdatesAsync(file);

                return status == Windows.Storage.Provider.FileUpdateStatus.Complete;
            }
            else
            {
                return null;
            }
        }

        public async Task<Stream> LoadDataFromExternalFileAsync(string fileExtension)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(fileExtension);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                return await file.OpenStreamForReadAsync();
            }
            else
            {
                return null;
            }
        }
    }
}
