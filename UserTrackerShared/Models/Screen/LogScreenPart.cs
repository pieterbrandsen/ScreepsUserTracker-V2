using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.Screen
{
    public class LogScreenPart : ScreenPart
    {
        public LogScreenPart(bool enabled, int width, int startHeight, int height) : base(enabled, width, startHeight, height)
        {
        }
        private List<string> logEntries = new List<string>();

        public override void Draw()
        {
        }

        void DisplayLogs()
        {
            SetCursorPosition(5);
            Console.WriteLine(new string(' ', Console.WindowWidth)); // Clear log area
            for (int i = 0; i < logEntries.Count; i++)
            {
                if (i < Height) // Only display the maximum allowed logs
                {
                    SetCursorPosition(StartHeight + i);
                    Console.WriteLine(logEntries[i]); // Show current log
                }
            }
        }

        public void AddLog(string log)
        {
            // Log the input only if it's not empty
            if (!string.IsNullOrWhiteSpace(log)) // Check if the input is not empty or whitespace
            {
                logEntries.Add(log); // Add new log entry
            }

            // Remove the oldest log entry if we've exceeded the max logs
            if (logEntries.Count > Height)
            {
                logEntries.RemoveAt(0); // Remove the oldest log entry
            }

            if (Height > 0)
            {
                DisplayLogs();
            }
            else
            {
                Console.WriteLine(log);
            }
        }
    }
}
