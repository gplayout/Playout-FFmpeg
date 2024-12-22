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
    public enum enOverlayIndex
    {
        GlobalText = 1, GlobalCrawl = 2, GlobalImage = 3,
        ItemText = 4, ItemCrawl = 5, ItemImage = 6,
        Trial = 20
    }
    public abstract class BaseOverlay: IDisposable
    {
        public enOverlayIndex oOrder;
        protected Point oPosition;
        protected SizeF oSize;
        protected Image bitmapOverlay = null;

        public BaseOverlay(enOverlayIndex index,int posLeft,int posTop)
        {
            this.oOrder = index;
            this.oPosition = new Point(posLeft, posTop);
        }
        public virtual int ProcessBufferCB(ref Graphics g,ref Bitmap output,int videoWidth,int videoHeight)
        {
            throw new NotImplementedException();
        }
        
        public virtual void Dispose()
        {
            if (this.bitmapOverlay != null)
                this.bitmapOverlay.Dispose();
        }

        int currentWidth, currentHeight;

        protected void CheckOverlayScale(int videoWidth,int videoHeight)
        {
            return;//No Need Anymore
            //
            if ((currentWidth != videoWidth || currentHeight != videoHeight)
                   && MediaPlayerBase.OutputVideoFormat_Height != 0 && MediaPlayerBase.OutputVideoFormat_Width != 0
                   && (MediaPlayerBase.OutputVideoFormat_Width != videoWidth || MediaPlayerBase.OutputVideoFormat_Height != videoHeight))
            {
                currentWidth = videoWidth;
                currentHeight = videoHeight;
                int ow = bitmapOverlay.Width * videoWidth/MediaPlayerBase.OutputVideoFormat_Width;
                if (ow > videoWidth)
                    ow = videoWidth;
                int oh = bitmapOverlay.Height * videoHeight / MediaPlayerBase.OutputVideoFormat_Height;
                if (oh > videoHeight)
                    oh = videoHeight;
                bitmapOverlay = ResizeImage((Bitmap)bitmapOverlay, new Size(ow,oh));
            }
        }
        public Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {

                return MediaPlayers.MediaPlayerBase.ResizeBitmap(imgToResize, System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic,
                    (int)size.Width, (int)size.Height, PixelFormat.Format32bppArgb);
            }
            catch { return imgToResize; }
        }
    }
}