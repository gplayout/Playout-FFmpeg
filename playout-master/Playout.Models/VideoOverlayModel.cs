using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{
    public class VideoOverlayMediaModel
    {
        public string FilePath { get; set; }
        public int PositionLeft { get; set; }
        public int PositionTop { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Enabled { get; set; }
        public VideoOverlayMediaModel()
        {
            this.FilePath = "";
            this.Width = 120;
            this.Height = 120;
        }
    }
}
