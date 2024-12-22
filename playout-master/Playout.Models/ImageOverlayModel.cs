using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{
    public class ImageOverlayMediaModel
    {
        public string FilePath { get; set; }
        public int AlphaBlending { get; set; }
        public int PositionLeft { get; set; }
        public int PositionTop { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Enabled { get; set; }
        public bool ChromaKey { get; set; }
        public int ChromaLeeway { get; set; }
        public int ChromaColor { get; set; }
        public ImageOverlayMediaModel()
        {
            this.FilePath = "";
            this.AlphaBlending = 255;
            this.Width = 120;
            this.Height = 120;
            this.ChromaLeeway = 100;
        }
    }
}
