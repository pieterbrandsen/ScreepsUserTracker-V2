using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models
{
    public abstract class ScreenPart : ScreenBase
    {
        public ScreenPart(bool enabled, int width, int startHeight, int height)
        {
            Enabled = enabled;
            Width = width;
            StartHeight = startHeight;
            Height = height;

            Draw();
        }
        public bool Enabled { get; set; }
        public int Width { get; set; }
        public int StartHeight { get; set; }
        public int Height { get; set; }
        public abstract void Draw();
    }
}
