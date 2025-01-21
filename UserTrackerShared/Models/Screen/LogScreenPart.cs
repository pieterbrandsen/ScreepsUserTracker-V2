using System.Timers;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.Models.Screen
{
    public class LogScreenPart : ScreenPart
    {
        public LogScreenPart(bool enabled, int width, int startHeight, int height) : base(enabled, width, startHeight, height)
        {
            _updateTimer = new Timer(100);
            _updateTimer.Elapsed += OnUpdateTimer;
            _updateTimer.AutoReset = true;
            _updateTimer.Enabled = true;
        }
        private Timer? _updateTimer;
        private bool isRenderingLogs = false;
        public bool HasNewLogs = false;
        public List<string> LogEntries = new List<string>();

        private void OnUpdateTimer(Object? source, ElapsedEventArgs e)
        {
            if (Height > 0 && HasNewLogs)
            {
                DisplayLogs();
            }
        }

        public override void Draw()
        {
        }

        void DisplayLogs()
        {
            if (isRenderingLogs) return;
            isRenderingLogs = true;
            HasNewLogs = false;

            // Remove the oldest log entry if we've exceeded the max logs
            while (LogEntries.Count > Height)
            {
                LogEntries.RemoveAt(0); // Remove the oldest log entry
            }

            UserTrackerShared.Screen.SetCursorPosition(5);
            Console.WriteLine(new string(' ', Console.WindowWidth)); // Clear log area
            for (int i = 0; i < LogEntries.Count; i++)
            {
                if (i < Height) // Only display the maximum allowed logs
                {
                    UserTrackerShared.Screen.SetCursorPosition(StartHeight + i);
                    Console.Write(new string(' ', Console.WindowWidth));
                    UserTrackerShared.Screen.SetCursorPosition(StartHeight + i);
                    Console.WriteLine(LogEntries[i]); // Show current log
                }
            }
            isRenderingLogs = false;
        }
    }
}
