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

        public async Task<FileAbstraction> GetExternalFolderAsync()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker() {
                SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);

                return new FileAbstraction
                {
                    Name = null,
                    Type = folder.DisplayType,
                    Path = folder.Path
                };
            }

            return null;
        }

        public async Task<FileAbstraction> SaveDataToExternalFileAsync(byte[] bytes, FileAbstraction suggestedFile, bool promptUser = true)
        {
            StorageFile file;

            if (promptUser)
            {
                var savePicker = new Windows.Storage.Pickers.FileSavePicker
                {
                    SuggestedFileName = suggestedFile.Name,
                    SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
                };
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add(suggestedFile.Type, new List<string>() { suggestedFile.Extension });

                file = await savePicker.PickSaveFileAsync();
            }
            else
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(suggestedFile.Path);
                var fileName = suggestedFile.Name + suggestedFile.Extension;

                file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            } 
            
            if (file != null)
            {
                // Allow future access to the file
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);

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

                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    var abstraction = new FileAbstraction
                    {
                        Name = file.DisplayName,
                        Type = file.DisplayType,
                        Extension = suggestedFile.Extension,
                        Path = new FileInfo(file.Path).Directory.FullName
                    };

                    return abstraction;
                }
                else return null;
            }
            else
            {
                // Cancelled
                return suggestedFile;
            }
        }

        public async Task<Tuple<FileAbstraction,Stream>> LoadDataFromExternalFileAsync(string fileExtension)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(fileExtension);

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var abstraction = new FileAbstraction
                {
                    Name = file.DisplayName,
                    Type = file.DisplayType,
                    Extension = fileExtension,
                    Path = file.Path
                };
                return new Tuple<FileAbstraction, Stream>(abstraction, await file.OpenStreamForReadAsync());
            }
            else
            {
                return null;
            }
        }

    }
}
