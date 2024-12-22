using Playout.Base;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Playout.ViewModels.ViewModels
{
    public class StringOverlayMediaViewModel : BaseTextOverlayMediaViewModel
    {

        public override string MediaOverlayIcon
        {
            get
            {
                return this.Enabled && !String.IsNullOrEmpty(this.Text) ? "T" : "";
            }
        }
        public StringOverlayMediaViewModel() : base() { }
        public StringOverlayMediaViewModel(StringOverlayMediaModel model)
        {
            this.Color = Functions.ConvertIntToColor(model.Color);
            this.FontName = model.FontName;
            this.FontSize = model.FontSize;
            this.Enabled = model.Enabled;
            this.PositionLeft = model.PositionLeft;
            this.PositionTop = model.PositionTop;
            this.Text = model.Text;
            this.Shadow = model.Shadow;
            this.Transparency = model.Transparency;
            this.TextAlign = model.TextAlign;
            this.BackColor = Functions.ConvertIntToColor(model.BackColor);
            this.ShadowColor = Functions.ConvertIntToColor(model.ShadowColor);
        }

        public StringOverlayMediaViewModel Clone()
        {
            StringOverlayMediaViewModel so = new StringOverlayMediaViewModel()
            {
                Color = this.Color,
                Enabled = this.Enabled,
                FontName = this.FontName,
                FontSize = this.FontSize,
                PositionLeft = this.PositionLeft,
                PositionTop = this.PositionTop,
                Text = this.Text,
                BackColor = this.BackColor,
                Shadow = this.Shadow,
                ShadowColor = this.ShadowColor,
                TextAlign = this.TextAlign,
                Transparency = this.Transparency
            };
            return so;
        }
        public StringOverlayMediaModel GetModel()
        {
            StringOverlayMediaModel model = new StringOverlayMediaModel()
            {
                Color = Functions.ConvertColorToInt(this.Color),
                Enabled = this.Enabled,
                FontName = this.FontName,
                FontSize = this.FontSize,
                PositionLeft = this.PositionLeft,
                PositionTop = this.PositionTop,
                Text = this.Text,
                BackColor = Functions.ConvertColorToInt(this.BackColor),
                Transparency = this.Transparency,
                Shadow = this.Shadow,
                ShadowColor = Functions.ConvertColorToInt(this.ShadowColor),
                TextAlign = this.TextAlign
            };
            return model;
        }
    }
}
