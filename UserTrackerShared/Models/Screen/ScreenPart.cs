using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.Screen
{
    public abstract class ScreenPart
    {
        public ScreenPart(bool enabled, int width, int startHeight, int height)
        {
            Enabled = enabled;
            Width = width;
            StartHeight = startHeight;
            Height = height;
        }
        public bool Enabled { get; set; }
        public int Width { get; set; }
        public int StartHeight { get; set; }
        public int Height { get; set; }
        public abstract void Draw();
    }
}
