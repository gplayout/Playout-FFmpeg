using DirectShowLib;
using Playout.DirectShow.MediaPlayers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playout.DirectShow.Overlays
{
    public class TextOverlay : BaseOverlay
    {

        bool _isTrial = false;
        Random rndTrial = new Random((int)DateTime.Now.Ticks & 0x0000FFFF);
        int trialCounter = 1;
        int posX, posY;

        public TextOverlay(enOverlayIndex index, string fontName, float fontSize, Color color,
                int posLeft, int posTop, string text, bool shadow, Color shadowColor,Size shadowOffset, 
            bool transparency, Color backcolor, string textAlign,bool isTrial)
            : base(index, posLeft, posTop)
        {
            this._isTrial = isTrial;
            //
            var font = new Font(fontName, fontSize);
            Brush backBrush = new SolidBrush(backcolor);
            Brush forBrush = new SolidBrush(color);
            Brush shadowBrush = new SolidBrush(shadowColor);
            //
            this.oSize = System.Windows.Forms.TextRenderer.MeasureText(text, font);
            var rect = new RectangleF(this.oPosition.X, this.oPosition.Y, this.oSize.Width, this.oSize.Height);
            //
            this.bitmapOverlay = new Bitmap((int)rect.Width, (int)rect.Height, PixelFormat.Format32bppArgb);
            var gra = Graphics.FromImage(this.bitmapOverlay);
            gra.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //
            StringFormat strFormat = new StringFormat();
            if (!String.IsNullOrEmpty(textAlign) && textAlign == "Right")
            {
                strFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;
            }
            //
            if(!transparency)
                gra.FillRectangle(backBrush, rect);
            //
            if(shadow)
                gra.DrawString(text, font, shadowBrush, shadowOffset.Width, shadowOffset.Height, strFormat);
            //
            gra.DrawString(text, font, forBrush, strFormat.FormatFlags == StringFormatFlags.DirectionRightToLeft ? oSize.Width : 0, 0, strFormat);
            this.bitmapOverlay.RotateFlip(RotateFlipType.RotateNoneFlipY);
            //
            gra.Save();
            //
            gra.Dispose();
        }
        public override void Dispose()
        {
            base.Dispose();
        }

        public override int ProcessBufferCB(ref Graphics g, ref Bitmap output, int videoWidth, int videoHeight)
        {
            lock (this)
            {
                
                //
                this.CheckOverlayScale(videoWidth, videoHeight);
                //
                if (_isTrial)
                {
                    if (trialCounter++ > 200)
                    {
                        posX = this.rndTrial.Next(1, videoWidth - this.bitmapOverlay.Width);
                        posY = (this.rndTrial.Next(1, videoHeight - this.bitmapOverlay.Height));
                        //
                        trialCounter = 1;
                    }
                    else
                    {

                    }
                }
                else
                {
                    this.posX = this.oPosition.X;
                    this.posY = videoHeight - this.bitmapOverlay.Height - this.oPosition.Y;
                }
                //
                g.DrawImage(bitmapOverlay, posX, posY);//, this.oSize.Width+200, this.oSize.Height+400);
            }
            return 0;
        }


    }
}
