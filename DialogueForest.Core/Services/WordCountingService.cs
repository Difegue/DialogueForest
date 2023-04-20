using CommunityToolkit.Mvvm.Messaging;
using DialogueForest.Core.Interfaces;
using DialogueForest.Core.Messages;
using DialogueForest.Core.ViewModels;
using DialogueForest.Localization.Strings;
using System;
using System.Collections.Generic;
using System.Text;

namespace DialogueForest.Core.Services
{
    public class WordCountingService
    {
        private string defaultDate => DateTime.MinValue.Date.ToString();

        private IApplicationStorageService _storageService;
        private INotificationService _notificationService;

        private DateTime _streakLastDate;
        private bool _hasCompletedObjective;

        public int CurrentStreak { get; private set; }
        public int CurrentWordCount { get; private set; }
        public EventHandler WordCountUpdated;
        
        public int CurrentWordObjective => _storageService.GetValue(nameof(SettingsViewModel.DailyWordObjective), SettingsViewModel.DEFAULT_WORD_OBJECTIVE);
        public bool IsTrackingEnabled => _storageService.GetValue<bool>(nameof(SettingsViewModel.EnableWordTracking), true);

        public WordCountingService(IApplicationStorageService storageService, INotificationService notificationService)
        {
            _storageService = storageService;
            _notificationService = notificationService;
            
            // Check last streak date to see if it's still valid
            var currentStreak = _storageService.GetValue<int>("CurrentStreak");
            _streakLastDate = DateTime.Parse(_storageService.GetValue<string>("StreakLastDate", defaultDate));
            var isStreakValid = _streakLastDate.Date == DateTime.Today.Date || _streakLastDate.Date == DateTime.Today.Date.AddDays(-1);

            // Get today's word count.. if we're still today
            var currentWordCount = _storageService.GetValue<int>("CurrentWordCount");
            var currentWordDate = DateTime.Parse(_storageService.GetValue<string>("CurrentWordCountDate", defaultDate));
            var isCurrentCountValid = currentWordDate.Date == DateTime.Today.Date;

            CurrentWordCount = isCurrentCountValid ? currentWordCount : 0; // TODO: Could be interesting here to log the previous day's word count anyways for some kind of graph? 
            CurrentStreak = isStreakValid ? currentStreak : 0;

            WeakReferenceMessenger.Default.Register<WordCountingService, UnsavedModificationsMessage>(this, (r, m) => r.UpdateWordCount(m.AddedWords));
        }

        private void UpdateWordCount(int newWords)
        {
            if (!IsTrackingEnabled)
                return;

            // Special case when the app runs past midnight with a completed objective
            // (If the objective wasn't met before we rolled over, let's be nice and not reset the streak)
            if (_hasCompletedObjective && _streakLastDate != DateTime.Today.Date)
            {
                // Reset
                _hasCompletedObjective = false;
                CurrentWordCount = 0;
            }

            CurrentWordCount += newWords;

            if (CurrentWordCount < 0)
                CurrentWordCount = 0;

            if (CurrentWordCount >= CurrentWordObjective)
                WordObjectiveReached();

            _storageService.SetValue("CurrentWordCount", CurrentWordCount);
            _storageService.SetValue("CurrentWordCountDate", DateTime.Today.Date.ToString());

            WordCountUpdated?.Invoke(this, new());
        }

        private void WordObjectiveReached() 
        {
            if (_streakLastDate != DateTime.Today.Date)
            {
                CurrentStreak++;
                _hasCompletedObjective = true;
                _streakLastDate = DateTime.Today.Date;

                _storageService.SetValue("CurrentStreak", CurrentStreak);
                _storageService.SetValue("StreakLastDate", _streakLastDate.ToString());

                _notificationService.ShowBasicToastNotification(string.Format(Resources.DailyObjectiveStreakTitle, CurrentStreak), 
                    Resources.DailyObjectiveStreakDesc);

                // Remove the matching reminder notification if there's one since the objective has been reached
                _notificationService.RemoveScheduledNotifications(true);
            }
        }
    }
}
