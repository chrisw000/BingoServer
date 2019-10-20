using System.Threading;
using System.Threading.Tasks;

namespace BlueCheese.HostedServices
{
    public interface IHostedServiceProvider
    {
        /// <summary>
        /// Number of milliseconds between periodic calls to do work
        /// </summary>
        int Delay { get; }

        Task DoPeriodicWorkAsync();
        Task StartupAsync();
        Task StopAsync(CancellationToken stoppingToken);
    }
}