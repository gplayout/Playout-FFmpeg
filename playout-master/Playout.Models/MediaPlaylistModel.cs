using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{
    public class MediaPlaylistModel
    {
        public string Name { get; set; }
        public string FilePath { get; set; }
        public int Color { get; set; }
        public List<MediaSourceModel> MediaSources { get; set; }
        public bool Loop { get; set; }

        public MediaPlaylistModel()
        {
            this.MediaSources = new List<MediaSourceModel>();
        }
    }
}
