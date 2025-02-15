using System;
using System.Threading;
using System.Threading.Tasks;

namespace PuppeteerSharp.Helpers
{
    internal sealed class TaskQueue : IDisposable, IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;
        private int _disposed;

        internal TaskQueue() => _semaphore = new SemaphoreSlim(1);

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _semaphore.Wait();
            _semaphore.Dispose();
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            await _semaphore.WaitAsync().ConfigureAwait(false);

            _semaphore.Dispose();
        }

        internal async Task<T> Enqueue<T>(Func<Task<T>> taskGenerator)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                return await taskGenerator().ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        internal async Task Enqueue(Func<Task> taskGenerator)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                await taskGenerator().ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
