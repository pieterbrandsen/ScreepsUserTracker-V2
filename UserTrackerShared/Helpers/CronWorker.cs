using Cronos;
using Microsoft.Extensions.Hosting;

namespace UserTrackerShared.Helpers
{

    public class CronWorker : BackgroundService
    {
        private static readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.General);
        private readonly CronExpression _cron;
        private readonly Func<CancellationToken, Task> _job;
        private DateTimeOffset? _nextRun;
        private bool _isRunning;
        private string _name;

        public CronWorker(
            string name,
            string cronExpression,
            Func<CancellationToken, Task> job)
        {
            _cron = CronExpression.Parse(cronExpression);
            _name = name;
            _job = job;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _nextRun = _cron.GetNextOccurrence(DateTime.UtcNow);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_nextRun.HasValue && DateTime.UtcNow >= _nextRun && !_isRunning)
                {
                    _isRunning = true;
                    try
                    {
                        _logger.Information($"Starting {_name} job at {DateTime.UtcNow}");
                        await _job(stoppingToken);
                        _logger.Information($"Finished {_name} job at {DateTime.UtcNow}");
                    }
                    catch (OperationCanceledException)
                    {
                        // graceful shutdown
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                    finally
                    {
                        _isRunning = false;
                        _nextRun = _cron.GetNextOccurrence(DateTime.UtcNow);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
