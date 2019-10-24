using System.Threading;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices
{
    public abstract class AbstractHostedServiceProvider
    {
        public virtual async Task StartupAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public virtual async Task DoPeriodicWorkAsync()
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public virtual async Task StopAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }
    }
}