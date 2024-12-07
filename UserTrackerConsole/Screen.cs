using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using UserTrackerShared.Models.Screen;

namespace UserTrackerConsole
{
    public class Screen : ScreenBase, IDisposable
    {
        public Screen(string name)
        {
            Name = name;
            Console.Title = Name;

            UpdateSize();

            CheckAndUpdateSizeTimer = new System.Timers.Timer();
            CheckAndUpdateSizeTimer.Interval = 5000;
            CheckAndUpdateSizeTimer.Elapsed += CheckAndUpdateSize;
            CheckAndUpdateSizeTimer.Enabled = true;
            CheckAndUpdateSizeTimer.AutoReset = true;
            CheckAndUpdateSizeTimer.Start();
        }

        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public TitleScreenPart TitlePart { get; set; } = new TitleScreenPart(false, 0, 0, 0);
        public LogScreenPart LogsPart { get; set; } = new LogScreenPart(false, 0, 0, 0);
        public FooterScreenPart FooterPart { get; set; } = new FooterScreenPart(false, 0, 0, 0);
        public System.Timers.Timer? CheckAndUpdateSizeTimer { get; set; } = new System.Timers.Timer();

        public void UpdateSize()
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

        public void CheckAndUpdateSize(object? source, ElapsedEventArgs e)
        {
            if (Console.WindowWidth != Width || Console.WindowHeight != Height)
            {
                UpdateSize();
            }
        }

        public void Dispose()
        {
            if (CheckAndUpdateSizeTimer != null)
            {
                CheckAndUpdateSizeTimer.Dispose();
                CheckAndUpdateSizeTimer = null;
            }
            throw new NotImplementedException();
        }
    }
}
