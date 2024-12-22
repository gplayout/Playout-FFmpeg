using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{
    public class GlobalSettingModel
    {
        public StringOverlayMediaModel StringOverlay { get; set; }
        public CrawlOverlayMediaModel CrawlOverlay { get; set; }
        public ImageOverlayMediaModel ImageOverlay { get; set; }
        public bool LoopOnPlaylists { get; set; }
        public bool PlaybackMode { get; set; }
        public GlobalSettingModel()
        {
            this.StringOverlay = new StringOverlayMediaModel();
            this.ImageOverlay = new ImageOverlayMediaModel();
            this.CrawlOverlay = new CrawlOverlayMediaModel();
            this.LoopOnPlaylists = false;
            this.PlaybackMode = false;
        }
    }
}
