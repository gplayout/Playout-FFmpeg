using DirectShowLib;
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
    public class ImageOverlay : BaseOverlay
    {
        bool oChromaKey;
        Color oChromaColor;
        float oChromaLeeway;
        public ImageOverlay(enOverlayIndex index, string filePath, int width, int height,
                int posLeft, int posTop, int alphBlend, bool chromaKey, Color chromaColor, int chromaLeeway)
            : base(index, posLeft, posTop)
        {
            this.oSize = new Size(width, height);
            this.oChromaKey = chromaKey;
            this.oChromaColor = chromaColor;
            this.oChromaLeeway = chromaLeeway;
            //
            float oAlphaBlend = (float)alphBlend / 255f;
            //
            if (!String.IsNullOrEmpty(filePath))
            {
                Image bitmapOriginal = this.resizeImage(Image.FromFile(filePath), this.oSize);
                bitmapOriginal.RotateFlip(RotateFlipType.Rotate180FlipX);
                this.bitmapOverlay = this.resizeImage(Image.FromFile(filePath), this.oSize);
                //
                Graphics gra = Graphics.FromImage(this.bitmapOverlay);
                gra.Clear(Color.Transparent);
                ColorMatrix cm = new ColorMatrix();
                cm.Matrix00 = cm.Matrix11 = cm.Matrix22 = cm.Matrix44 = 1;
                cm.Matrix33 = oAlphaBlend;
                ImageAttributes ia = new ImageAttributes();
                ia.SetColorMatrix(cm);
                gra.DrawImage(bitmapOriginal,
                    new Rectangle(0, 0, bitmapOriginal.Width, bitmapOriginal.Height),
                        0, 0, bitmapOriginal.Width, bitmapOriginal.Height, GraphicsUnit.Pixel, ia);
                //gra.ScaleTransform(1.0F, -1.0F);
                //
                gra.Save();
                //
                gra.Dispose();
            }
        }
        private Image resizeImage(Image imgToResize, SizeF size)
        {
            return MediaPlayers.MediaPlayerBase.ResizeBitmap(imgToResize, System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic,
                (int)size.Width, (int)size.Height);
        }
        public override void Dispose()
        {
            base.Dispose();
        }
        
        public override int ProcessBufferCB(ref Graphics g, ref Bitmap output, int videoWidth, int videoHeight)
        {
            lock (this)
            {
                if (this.bitmapOverlay == null)
                    return 0;
                //
                //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                //
                //Chroma Key
                if (this.oChromaKey)
                {
                    Bitmap output1 = (Bitmap)output.Clone();
                    g.Clear(Color.Transparent);
                    g.DrawImage(bitmapOverlay, this.oPosition.X, this.oPosition.Y, this.oSize.Width, this.oSize.Height);
                    // Iterate over all piels from top to bottom...
                    for (int y = 0; y < output1.Height; y++)
                    {
                        // ...and from left to right
                        for (int x = 0; x < output1.Width; x++)
                        {
                            // Determine the pixel color
                            Color camColor = output1.GetPixel(x, y);

                            bool replace =
                                (Math.Abs(camColor.R - this.oChromaColor.R) / 255f <= ((100f - this.oChromaLeeway) / 100f)) &&
                                (Math.Abs(camColor.G - this.oChromaColor.G) / 255f <= ((100f - this.oChromaLeeway) / 100f)) &&
                                (Math.Abs(camColor.B - this.oChromaColor.B) / 255f <= ((100f - this.oChromaLeeway) / 100f));

                            if (replace)
                                camColor = Color.Transparent;

                            // Set the output pixel
                            output1.SetPixel(x, y, camColor);
                        }
                    }
                    g.DrawImage(output1, 0, 0, videoWidth, videoHeight);
                    output1.Dispose();
                }
                else
                {
                    this.CheckOverlayScale(videoWidth, videoHeight);
                    //
                    g.DrawImage(bitmapOverlay, this.oPosition.X, videoHeight - this.bitmapOverlay.Height - this.oPosition.Y);//, this.oSize.Width+200, this.oSize.Height+400);
                }
            }
            //
            return 0;
        }
    }
}
