using DirectShowLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playout.DirectShow.Overlays
{
    public class CrawlOverlay : BaseOverlay
    {
        bool oScrolling;
        int oScrollingSpeed;
        int frameCount;
        string oDirection;
        bool _isGlobal;

        public static bool ScrollGlobalReset = true;
        public static int ScrollGlobalframeCount = 0;

        public static bool ScrollMediaReset = true;
        public static int ScrollMediaframeCount = 0;
        public CrawlOverlay(enOverlayIndex index, string fontName, float fontSize, Color color,
                int posLeft, int posTop, string text, bool shadow, Color shadowColor,
                Size shadowOffset, bool transparency, Color backcolor, string textAlign,
                string filePath, bool readFromFile, bool scrolling, int scrollingSpeed,string direction,
                bool isGlobal)
            : base(index, posLeft, posTop)
        {
            this.oScrolling = scrolling;
            this.oScrollingSpeed = scrollingSpeed;
            this.oDirection = direction;
            this._isGlobal = isGlobal;
            //
            if (isGlobal)
            {
                if (!ScrollGlobalReset)
                {
                    frameCount = ScrollGlobalframeCount;
                }
                ScrollGlobalReset = false;
            }
            else
            {
                if (!ScrollMediaReset)
                {
                    frameCount = ScrollMediaframeCount;
                }
                ScrollMediaReset = true;
            }
            //
            if (readFromFile && File.Exists(filePath))
                text = File.ReadAllText(filePath);
            //
            var font = new Font(fontName, fontSize);
            Brush backBrush = new SolidBrush(backcolor);
            Brush forBrush = new SolidBrush(color);
            Brush shadowBrush = new SolidBrush(shadowColor);
            //
            this.oSize = System.Windows.Forms.TextRenderer.MeasureText(text, font);
            var rect = new RectangleF(this.oPosition.X, this.oPosition.Y, this.oSize.Width+1000, this.oSize.Height);
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
            if (!transparency)
                gra.FillRectangle(backBrush, rect);
            //
            if (shadow)
                gra.DrawString(text, font, shadowBrush, shadowOffset.Width, shadowOffset.Height, strFormat);
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
            try
            {
                lock (this)
                {
                    //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    //
                    int posX = this.oPosition.X;
                    if (this.oScrolling)
                    {
                        if (oScrollingSpeed < 1)
                            oScrollingSpeed = 1;
                        //
                        if (!String.IsNullOrEmpty(this.oDirection) && this.oDirection == "LeftToRight")
                        {
                            posX = (oScrollingSpeed * this.frameCount) - (int)this.oSize.Width;
                            if (posX > videoWidth)
                            {
                                posX = 0;
                                frameCount = 0;
                                return 0;
                            }
                        }
                        else
                        {
                            posX = videoWidth - oScrollingSpeed * this.frameCount;
                            if (posX < -this.oSize.Width)
                            {
                                posX = 0;
                                frameCount = 0;
                                return 0;
                            }
                        }
                    }
                    g.DrawImage(bitmapOverlay, posX, videoHeight - this.oSize.Height - this.oPosition.Y);//, this.oSize.Width+200, this.oSize.Height+400);
                    //
                    this.frameCount++;
                    //
                    if (this._isGlobal)
                    {
                        ScrollGlobalframeCount = this.frameCount;
                    }
                    else
                    {
                        ScrollMediaframeCount = this.frameCount;
                    }
                }
                return 0;
            }
            catch(Exception ex)
            {
                return 0;
            }
        }
    }
}
