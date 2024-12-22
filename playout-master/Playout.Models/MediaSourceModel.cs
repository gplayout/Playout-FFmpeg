using Playout.Base;
using Playout.DirectShow.MediaPlayers;
using Playout.MediaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{

    public class MediaSourceModel
    {
        public string Category { get; set; }
        public string AliasName { get; set; }
        public enMediaType MediaType { get; set; }
        public string File_Url { get; set; }
        public MediaPlaylistModel NestedPlaylist { get; set; }
        public string DeviceName { get; set; }
        public long TrimStartSecond { get; set; }
        public long TrimStopSecond { get; set; }
        public StringOverlayMediaModel StringOverlay { get; set; }
        public CrawlOverlayMediaModel CrawlOverlay { get; set; }
        public ImageOverlayMediaModel ImageOverlay { get; set; }
        public VideoOverlayMediaModel VideoOverlay { get; set; }
        public Guid ThumbnailId { get; set; }
        public TransitionEffectType ShowupEffect { get; set; }
        public bool Loop { get; set; }
        public string InputDeviceFormat { get; set; }
        public MediaSourceModel()
        {
            this.StringOverlay = new StringOverlayMediaModel();
            this.ImageOverlay = new ImageOverlayMediaModel();
            this.VideoOverlay = new VideoOverlayMediaModel();
            this.CrawlOverlay = new CrawlOverlayMediaModel();
            this.ThumbnailId = Guid.Empty;
            this.AliasName = "";
            this.Category = "";
            this.NestedPlaylist = null;
            this.Loop = false;
        }
    }
}
