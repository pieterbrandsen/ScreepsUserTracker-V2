using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.Screen
{
    public abstract class ScreenBase
    {
        public void SetCursorPosition(int y)
        {
            try
            {
                Console.SetCursorPosition(0, y);
            }
            catch (Exception)
            {
            }
        }
        public void WriteDivider(int location)
        {
            SetCursorPosition(location);
            Console.WriteLine(new string('=', Console.WindowWidth)); // Draw top border
        }
    }
}
