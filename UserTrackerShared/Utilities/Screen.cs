using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UserTrackerShared.Models.Screen;

namespace UserTrackerShared
{
    public static class Screen
    {
        public static void Init()
        {
            Console.Title = Name;

            UpdateSize();

            var checkAndUpdateSizeTimer = new System.Timers.Timer();
            checkAndUpdateSizeTimer.Interval = 5000;
            checkAndUpdateSizeTimer.Elapsed += CheckAndUpdateSize;
            checkAndUpdateSizeTimer.Enabled = true;
            checkAndUpdateSizeTimer.AutoReset = true;
            checkAndUpdateSizeTimer.Start();

            IsEnabled = true;
            TitlePart.Draw();
            FooterPart.Draw();

        }

        public static bool IsEnabled { get; set; } = false;
        public static string Name { get; set; } = "User Tracker Screen";
        public static int Width { get; set; }
        public static int Height { get; set; }
        public static TitleScreenPart TitlePart { get; set; } = new TitleScreenPart(false, 0, 0, 0);
        public static LogScreenPart LogsPart { get; set; } = new LogScreenPart(false, 0, 0, 0);
        public static FooterScreenPart FooterPart { get; set; } = new FooterScreenPart(false, 0, 0, 0);

        public static void UpdateSize()
        {
            Width = Console.WindowWidth;
            Height = Console.WindowHeight;
            Console.Clear();
            Console.WriteLine("\x1b[3J");

            int dividerCount = 3;
            int dividerHeight = 1;
            int titleHeight = 1;
            int footerHeight = 3;
            int logsHeight = Height - titleHeight - footerHeight - dividerHeight * 2;

            int logsStartHeight = titleHeight + dividerHeight * 2;
            int footerStartHeight = titleHeight + dividerHeight * dividerCount + logsHeight;

            if (Height > 7)
            {
                WriteDivider(0);
                TitlePart = new TitleScreenPart(true, Width, 1, titleHeight);
                WriteDivider(logsStartHeight - dividerHeight);
                LogsPart = new LogScreenPart(true, Width, logsStartHeight, logsHeight);
                if (Height > 10)
                {
                    FooterPart = new FooterScreenPart(true, Width, footerStartHeight, footerHeight);
                    WriteDivider(footerStartHeight - 1);
                }
            }
        }
        public static void CheckAndUpdateSize(object? source, ElapsedEventArgs e)
        {
            if (Console.WindowWidth != Width || Console.WindowHeight != Height)
            {
                UpdateSize();
            }
        }
        public static void SetCursorPosition(int y)
        {
            try
            {
                Console.SetCursorPosition(0, y);
            }
            catch (Exception)
            {
                // Accepted
            }
        }
        public static void WriteDivider(int location)
        {
            SetCursorPosition(location);
            Console.WriteLine(new string('=', Console.WindowWidth)); // Draw top border
        }
        public static void AddLog(string log)
        {
            if (!IsEnabled) return;
            // Log the input only if it's not empty
            if (!string.IsNullOrWhiteSpace(log)) // Check if the input is not empty or whitespace
            {
                LogsPart.LogEntries.Add(log); // Add new log entry
                LogsPart.HasNewLogs = true;
            }

            if (Height <= 0)
            {
                Console.WriteLine(log);
            }
        }
    }
}
