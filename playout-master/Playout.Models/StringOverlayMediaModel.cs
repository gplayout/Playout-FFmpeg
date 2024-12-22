using Playout.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{
    public class StringOverlayMediaModel
    {
        public string Text { get; set; }
        public int Color { get; set; }
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public int PositionLeft { get; set; }
        public int PositionTop { get; set; }
        public bool Enabled { get; set; }
        public bool Transparency { get; set; }
        public int BackColor { get; set; }
        public bool Shadow { get; set; }
        public int ShadowColor { get; set; }
        public string TextAlign { get; set; }
        public StringOverlayMediaModel()
        {
            this.FontName = "Arial";
            this.FontSize = 12;
            this.Text = "";
            this.BackColor = Functions.ConvertColorToInt(System.Drawing.Color.Brown);
            this.Color = Functions.ConvertColorToInt(System.Drawing.Color.White);
        }
    }
}
