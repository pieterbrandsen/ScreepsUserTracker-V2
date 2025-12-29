using UserTrackerShared.Helpers;
using Timer = System.Timers.Timer;
using System.Threading;

public class TimerScheduleHelper : IDisposable
{
    private readonly Serilog.ILogger _logger = Logger.GetLogger(LogCategory.General);
    private Timer? _timer;
    private readonly string _name;
    private readonly Func<Task> _action;
    private readonly int[] _hours;
    private readonly DayOfWeek[]? _daysOfWeek;
    private readonly int[]? _daysOfMonth;

    private int _isRunning = 0;

    public TimerScheduleHelper(
        string name,
        Func<Task> action,
        int[] hours,
        DayOfWeek[]? daysOfWeek = null,
        int[]? daysOfMonth = null)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _action = action ?? throw new ArgumentNullException(nameof(action));

        if (hours == null || hours.Length == 0)
            throw new ArgumentException("At least one hour must be specified.", nameof(hours));

        if (daysOfWeek != null && daysOfMonth != null)
            throw new ArgumentException("Cannot specify both daysOfWeek and daysOfMonth.");

        _hours = (int[])hours.Clone();
        _daysOfWeek = daysOfWeek?.Length > 0 ? (DayOfWeek[])daysOfWeek.Clone() : null;
        _daysOfMonth = daysOfMonth?.Length > 0 ? (int[])daysOfMonth.Clone() : null;

        ScheduleNext();
    }

    private void ScheduleNext()
    {
        DateTime now = DateTime.Now;
        Array.Sort(_hours);

        DateTime? next = null;

        if (_daysOfMonth != null)
        {
            Array.Sort(_daysOfMonth);

            for (int i = 0; i <= 31; i++)
            {
                var candidateDay = now.Date.AddDays(i);
                if (_daysOfMonth.Contains(candidateDay.Day))
                {
                    foreach (var h in _hours)
                    {
                        var candidate = candidateDay.AddHours(h);
                        if (candidate > now)
                        {
                            next = candidate;
                            break;
                        }
                    }
                }
                if (next != null) break;
            }

            if (next == null)
            {
                var firstValidDay = _daysOfMonth.Min();
                var candidateDay = new DateTime(now.Year, now.Month, 1)
                    .AddMonths(1)
                    .AddDays(firstValidDay - 1);

                next = candidateDay.AddHours(_hours[0]);
            }
        }
        else if (_daysOfWeek != null)
        {
            for (int i = 0; i <= 7; i++)
            {
                var candidateDay = now.Date.AddDays(i);
                if (_daysOfWeek.Contains(candidateDay.DayOfWeek))
                {
                    foreach (var h in _hours)
                    {
                        var candidate = candidateDay.AddHours(h);
                        if (candidate > now)
                        {
                            next = candidate;
                            break;
                        }
                    }
                }
                if (next != null) break;
            }
        }
        else
        {
            foreach (var h in _hours)
            {
                var candidate = now.Date.AddHours(h);
                if (candidate > now)
                {
                    next = candidate;
                    break;
                }
            }

            if (next == null)
                next = now.Date.AddDays(1).AddHours(_hours[0]);
        }

        if (next == null)
            throw new InvalidOperationException("Could not determine the next schedule.");

        double ms = (next.Value - now).TotalMilliseconds;
        _logger.Information($"Timer {_name} scheduled to run at {next.Value} (in {ms} ms)");

        _timer?.Dispose();
        _timer = new Timer(ms)
        {
            AutoReset = false
        };

        _timer.Elapsed += async (_, __) =>
        {
            if (Interlocked.Exchange(ref _isRunning, 1) == 1)
                return;

            try
            {
                _logger.Information($"Timer {_name} executing at {DateTime.Now}");
                await _action();
                _logger.Information($"Timer {_name} completed execution at {DateTime.Now}");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Timer {_name} failed");
            }
            finally
            {
                Interlocked.Exchange(ref _isRunning, 0);
                ScheduleNext();
            }
        };

        _timer.Start();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
