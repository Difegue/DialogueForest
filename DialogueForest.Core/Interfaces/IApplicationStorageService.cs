using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DialogueForest.Core.Interfaces
{
    public interface IApplicationStorageService
    {
        Task<bool> DoesFileExistAsync(string fileName, string parentFolder = "");
        Task SaveDataToFileAsync(string fileName, byte[] data, string parentFolder = "");
        Task<Stream> OpenFileAsync(string fileName, string parentFolder = "");
        Task DeleteFolderAsync(string folderName);

        void SetValue<T>(string key, T value);
        T GetValue<T>(string key, T defaultValue = default);

        Task<bool?> SaveDataToExternalFileAsync(byte[] bytes, string fileExtension);
        Task<Stream> LoadDataFromExternalFileAsync(string fileExtension);
    }
}
