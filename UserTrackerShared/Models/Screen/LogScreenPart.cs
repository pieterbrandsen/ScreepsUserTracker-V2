using System.Timers;
using Timer = System.Timers.Timer;

namespace UserTrackerShared.Models.Screen
{
    public class LogScreenPart : ScreenPart
    {
        public LogScreenPart(bool enabled, int width, int startHeight, int height) : base(enabled, width, startHeight, height)
        {
            var updateTimer = new Timer(100);
            updateTimer.Elapsed += OnUpdateTimer;
            updateTimer.AutoReset = true;
            updateTimer.Enabled = true;
        }
        private bool isRenderingLogs = false;
        public bool HasNewLogs { get; set; } = false;
        public List<string> LogEntries { get; set; } = new List<string>();

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

            Utilities.Screen.SetCursorPosition(5);
            Console.WriteLine(new string(' ', Console.WindowWidth)); // Clear log area
            for (int i = 0; i < LogEntries.Count; i++)
            {
                if (i < Height) // Only display the maximum allowed logs
                {
                    Utilities.Screen.SetCursorPosition(StartHeight + i);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Utilities.Screen.SetCursorPosition(StartHeight + i);
                    Console.WriteLine(LogEntries[i]); // Show current log
                }
            }
            isRenderingLogs = false;
        }
    }
}
