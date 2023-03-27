using System;
using System.Threading.Tasks;

namespace DialogueForest.Core.Interfaces
{
    public enum Theme
    {
        Default = 0,
        Light = 1,
        Dark = 2
    }

    public enum OutputFormat
    {
        PlainText = 0,
        BBCode = 1,
        HTML = 2,
        Markdown = 3
    }

    public interface IInteropService
    {
        Version GetAppVersion();
        Task SetThemeAsync(Theme param);
        Task OpenStoreReviewUrlAsync();

        void UpdateAppTitle(string v);
    }
}
