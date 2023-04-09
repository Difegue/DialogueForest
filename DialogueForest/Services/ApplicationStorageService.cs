using DialogueForest.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Path = System.IO.Path;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Nodes;

namespace DialogueForest.Services
{
    public sealed class ApplicationStorageService : IApplicationStorageService
    {
        private const string APP_IDENTIFIER = "DialogueForest";

        private string _appDataFolder;
        private FilePersistence _settingsStorage;

        public ApplicationStorageService()
        {
            // Create folder in %APPDATA% if there isn't one already
            // We can't use Windows.Storage since we're unpackaged
            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APP_IDENTIFIER);
            Directory.CreateDirectory(_appDataFolder);

            // Create a file to store key-value pairs
            _settingsStorage = new FilePersistence(Path.Combine(_appDataFolder, "config.json"));
            WinUIEx.WindowManager.PersistenceStorage = _settingsStorage;
        }

        public async Task<bool> DoesFileExistAsync(string fileName, string parentFolder = "")
        {
            var path = Path.Combine(_appDataFolder, parentFolder, fileName);
            return File.Exists(path);
        }

        public async Task SaveDataToFileAsync(string fileName, byte[] data, string parentFolder = "")
        {
            var path = Path.Combine(_appDataFolder, parentFolder, fileName);
            File.WriteAllBytes(path, data);
        }

        public async Task<Stream> OpenFileAsync(string fileName, string parentFolder = "")
        {
            var path = Path.Combine(_appDataFolder, parentFolder, fileName);
            return File.OpenRead(path);
        }

        public async Task DeleteFileAsync(string fileName, string parentFolder = "")
        {
            var path = Path.Combine(_appDataFolder, parentFolder, fileName);
            File.Delete(path);
        }

        public async Task DeleteFolderAsync(string folderName)
        {
            var path = Path.Combine(_appDataFolder, folderName);
            Directory.Delete(path, true);
        }

        public void SetValue<T>(string key, T value)
        {

            if (!_settingsStorage.ContainsKey(key)) _settingsStorage.Add(key, value);
            else if (value == null) _settingsStorage.Remove(key);
            else _settingsStorage[key] = value;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_settingsStorage.TryGetValue(key, out object value))
            {
                try
                {
                    Type listType = typeof(T);

                    // Special case for booleans and ints 
                    if (listType == typeof(bool))
                        return (T)(object)bool.Parse(value.ToString());

                    if (listType == typeof(int))
                        return (T)(object)int.Parse(value.ToString());

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
            var folderPicker = new FolderPicker() {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary
            };
            folderPicker.FileTypeFilter.Add("*");

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
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
            string file = suggestedFile?.FullPath;
            var type = suggestedFile?.Type ?? "Dialogue Forest";
            var extension = suggestedFile?.Extension ?? ".frst";

            // We'll prompt the user no matter what if there's no previous suggestedFile to use
            if (promptUser || file == null)
            {
                var savePicker = new FileSavePicker
                {
                    SuggestedFileName = suggestedFile?.Name ?? "",
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary
                };

                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.Window);
                WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add(type, new List<string>() { extension });

                var storageFile = await savePicker.PickSaveFileAsync();

                file = storageFile?.Path;
            }
            
            if (file != null)
            {
                // write to file
                File.WriteAllBytes(file, bytes);

                return new FileAbstraction
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Type = type,
                    Extension = extension,
                    Path = Path.GetDirectoryName(file)
                };
            }
            else
            {
                // Cancelled
                return null;
            }
        }

        public async Task<Tuple<FileAbstraction,Stream>> LoadDataFromExternalFileAsync(string fileExtension)
        {
            var picker = new FileOpenPicker();
            picker.ViewMode = PickerViewMode.List;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(fileExtension);

            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle((Application.Current as App)?.Window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                var abstraction = new FileAbstraction
                {
                    Name = file.DisplayName,
                    Type = file.DisplayType,
                    Extension = fileExtension,
                    Path = new FileInfo(file.Path).Directory.FullName
                };
                return new Tuple<FileAbstraction, Stream>(abstraction, await file.OpenStreamForReadAsync());
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Happily taken from WinUIEx
        /// </summary>
        private class FilePersistence : IDictionary<string, object>
        {
            private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
            private readonly string _file;

            public FilePersistence(string filename)
            {
                _file = filename;
                try
                {
                    if (File.Exists(filename))
                    {
                        var jo = JsonNode.Parse(File.ReadAllText(filename)) as JsonObject;
                        foreach (var node in jo)
                        {
                            if (node.Value is JsonValue jvalue && jvalue.TryGetValue<string>(out string value))
                                _data[node.Key] = value;
                        }
                    }
                }
                catch { }
            }
            private void Save()
            {
                JsonObject jo = new JsonObject();
                foreach (var item in _data)
                {
                    jo.Add(item.Key, item.Value?.ToString());
                }
                File.WriteAllText(_file, jo.ToJsonString());
            }
            public object this[string key] { get => _data[key]; set { _data[key] = value; Save(); } }

            public ICollection<string> Keys => _data.Keys;

            public ICollection<object> Values => _data.Values;

            public int Count => _data.Count;

            public bool IsReadOnly => false;

            public void Add(string key, object value)
            {
                _data.Add(key, value); Save();
            }

            public void Add(KeyValuePair<string, object> item)
            {
                _data.Add(item.Key, item.Value); Save();
            }

            public void Clear()
            {
                _data.Clear(); Save();
            }

            public bool Contains(KeyValuePair<string, object> item) => _data.Contains(item);

            public bool ContainsKey(string key) => _data.ContainsKey(key);

            public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotImplementedException(); // TODO

            public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => throw new NotImplementedException(); // TODO

            public bool Remove(string key) => _data.Remove(key);

            public bool Remove(KeyValuePair<string, object> item) => throw new NotImplementedException(); // TODO

            public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => ContainsKey(key) ? (value = _data[key]) != null : (value = null) != null;

            IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException(); // TODO
        }
    }
}
