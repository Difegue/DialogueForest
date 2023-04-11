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
        Task DeleteFileAsync(string fileName, string parentFolder = "");
        Task DeleteFolderAsync(string folderName);

        void SetValue<T>(string key, T value);
        T GetValue<T>(string key, T defaultValue = default);

        Task<FileAbstraction> SaveDataToExternalFileAsync(byte[] bytes, FileAbstraction suggestedFile, bool promptUser = true);
        Task<Tuple<FileAbstraction, Stream>> LoadDataFromExternalFileAsync(string fileExtension);
        Task<FileAbstraction> GetExternalFolderAsync();
    }

    public class FileAbstraction
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }

        public string Type { get; set; }
        public string FullPath => Path == null ? null : System.IO.Path.Combine(Path, Name + Extension);
    }
}
