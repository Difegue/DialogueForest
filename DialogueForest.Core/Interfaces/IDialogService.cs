using System.Threading.Tasks;

namespace DialogueForest.Core.Interfaces
{
    public interface IDialogService
    {
        Task ShowWhatsNewDialogIfAppropriateAsync(bool forceShow = false);
        Task ShowFirstRunDialogIfAppropriateAsync();
        Task ShowRateAppDialogIfAppropriateAsync();
        Task<string> ShowTreeNameDialogAsync();
        Task<bool> ShowConfirmDialogAsync(string title, string text, string primaryButtonText, string cancelButtonText);
    }
}
