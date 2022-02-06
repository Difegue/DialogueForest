using SkiaSharp;
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
        UnityRichText = 3
    }

    public interface IInteropService
    {
        SKColor GetAccentColor();
        Version GetAppVersion();
        Task SetThemeAsync(Theme param);
        Task OpenStoreReviewUrlAsync();

        string ConvertRtf(string rtf, OutputFormat format);
    }
}
