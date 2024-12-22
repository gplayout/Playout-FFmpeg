using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Base
{
    [Flags]
    public enum Days2
    {
        None = 0x0,
        Sunday = 0x1,
        Monday = 0x2,
        Tuesday = 0x4,
        Wednesday = 0x8,
        Thursday = 0x10,
        Friday = 0x20,
        Saturday = 0x40
    }

    public enum TimersMode
    {
        None,
        Specific,
        CountDown
    }
    public enum TextOverlayAlign
    {
        Left,Right,Center
    }

    public enum CrawlDirection
    {
        RightToLeft = 0,
        LeftToRight = 1
    }

    
    public enum TransitionEffectType
    {
        None,
        Fade,
        SlideToDown,
        SlideToUp,
        SlideToLeft,
        SlideToRight,
        SlideToCenter
    }

    public enum VCamChannelType
    {
        Channel1 = 1,
        Channel2 = 2,
        Channel3 = 3,
        Channel4 = 4
    }

    public enum EncoderProgram
    {
        FFMPEG = 1,
        FMLE = 2
    }
}
