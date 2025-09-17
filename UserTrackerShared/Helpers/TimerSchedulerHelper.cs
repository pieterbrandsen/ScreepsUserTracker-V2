using UserTrackerShared.Helpers;
using Timer = System.Timers.Timer;

public class TimerScheduleHelper : IDisposable
{
    private Timer? _timer;
    private readonly Action _action;
    private readonly int[] _hours;

    public TimerScheduleHelper(Action action, params int[] hours)
    {
        if (hours == null || hours.Length == 0)
            throw new ArgumentException("At least one hour must be specified.");
        _action = action;
        _hours = (int[])hours.Clone();
        ScheduleNext();
    }

    private void ScheduleNext()
    {
        DateTime now = DateTime.Now;
        Array.Sort(_hours);

        DateTime next = now.Date.AddHours(_hours[0]);
        foreach (var h in _hours)
        {
            var t = now.Date.AddHours(h);
            if (t > now)
            {
                next = t;
                break;
            }
        }
        if (next <= now)
            next = now.Date.AddDays(1).AddHours(_hours[0]);

        double ms = (next - now).TotalMilliseconds;
        _timer?.Dispose();
        _timer = new Timer(ms);
        _timer.AutoReset = false;
        _timer.Elapsed += (s, e) =>
        {
            _action?.Invoke();
            ScheduleNext();
        };
        _timer.Start();
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
