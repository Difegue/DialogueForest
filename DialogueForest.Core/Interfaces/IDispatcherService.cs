using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DialogueForest.Core.Interfaces
{
    public interface IDispatcherService
    {
        Task ExecuteOnUIThreadAsync(Action p);
        Task<T> EnqueueAsync<T>(Func<Task<T>> function);
        void Initialize();
    }
}
