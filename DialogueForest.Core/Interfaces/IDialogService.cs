﻿using System.Threading.Tasks;

namespace DialogueForest.Core.Interfaces
{
    public interface IDialogService
    {
        Task ShowWhatsNewDialogIfAppropriateAsync();
        Task ShowFirstRunDialogIfAppropriateAsync();
        Task ShowRateAppDialogIfAppropriateAsync();
        Task<bool> ShowConfirmDialogAsync(string title, string text, string primaryButtonText, string cancelButtonText);
    }
}
