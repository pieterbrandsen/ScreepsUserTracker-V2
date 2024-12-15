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
        private bool hasNewLogs = false;
        private List<string> logEntries = new List<string>();

        private void OnUpdateTimer(Object? source, ElapsedEventArgs e)
        {
            if (Height > 0 && hasNewLogs)
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
            hasNewLogs = false;

            // Remove the oldest log entry if we've exceeded the max logs
            while (logEntries.Count > Height)
            {
                logEntries.RemoveAt(0); // Remove the oldest log entry
            }

            UserTrackerShared.Screen.SetCursorPosition(5);
            Console.WriteLine(new string(' ', Console.WindowWidth)); // Clear log area
            for (int i = 0; i < logEntries.Count; i++)
            {
                if (i < Height) // Only display the maximum allowed logs
                {
                    UserTrackerShared.Screen.SetCursorPosition(StartHeight + i);
                    Console.Write(new string(' ', Console.WindowWidth));
                    UserTrackerShared.Screen.SetCursorPosition(StartHeight + i);
                    Console.WriteLine(logEntries[i]); // Show current log
                }
            }
            isRenderingLogs = false;
        }

        public void AddLog(string log)
        {
            // Log the input only if it's not empty
            if (!string.IsNullOrWhiteSpace(log)) // Check if the input is not empty or whitespace
            {
                logEntries.Add(log); // Add new log entry
                hasNewLogs = true;
            }

            if (Height <= 0)
            {
                Console.WriteLine(log);
            }
        }
    }
}
