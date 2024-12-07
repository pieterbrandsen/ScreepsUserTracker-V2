using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserTrackerShared.Models.Screen
{
    public class FooterScreenPart : ScreenPart
    {
        public FooterScreenPart(bool enabled, int width, int startHeight, int height) : base(enabled, width, startHeight, height)
        {
        }

        public override void Draw()
        {
        }
    }
}
