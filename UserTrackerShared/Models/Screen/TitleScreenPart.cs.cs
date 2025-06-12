using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.Screen
{
    public class TitleScreenPart : ScreenPart
    {
        public TitleScreenPart(bool enabled, int width, int startHeight, int height) : base(enabled, width, startHeight, height)
        {
        }

        public override void Draw()
        {
            Utilities.Screen.SetCursorPosition(StartHeight);
            Console.WriteLine("User Tracker".PadLeft((Console.WindowWidth + 24) / 2)); // Centered title
        }
    }
}
