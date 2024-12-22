using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Playout.Log;

namespace Playout.Base
{
    public static class ThumbnailWorker
    {
        static readonly string ThumbnailDirPath = Program.AppPath + @"\Thumbnails";
        
        public static bool CreateThumbnail(Guid id,string filePath)
        {
            try
            {
                if (!Directory.Exists(ThumbnailDirPath))
                    Directory.CreateDirectory(ThumbnailDirPath);
                //
                var bs = GetThumbnail(filePath);
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                BitmapFrame outputFrame = CreateResizedImage(bs, 70, 70);
                encoder.Frames.Add(outputFrame);
                string fileName = System.IO.Path.Combine(ThumbnailDirPath, id.ToString() + ".png");
                using (FileStream file = File.OpenWrite(fileName))
                {
                    encoder.Save(file);
                    file.Flush();
                }
                return true;
            }
            catch(Exception ex)
            {
                ex.Log();
                return false;
            }
        }
        public static BitmapSource GetThumbnail(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return null;
                //
                string fileName = System.IO.Path.Combine(ThumbnailDirPath, id.ToString() + ".png");
                //
                return GetThumbnail(fileName);
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public static BitmapSource GetThumbnail(string fileName)
        {
            try
            {
                
                //Stream imageStreamSource = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                //PngBitmapDecoder decoder = new PngBitmapDecoder(imageStreamSource, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                BitmapSource bitmapSource = new BitmapImage(new Uri(fileName));//decoder.Frames[0];
                //
                return bitmapSource;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public static bool DeleteThumbnail(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                    return false;
                //
                string fileName = System.IO.Path.Combine(ThumbnailDirPath, id.ToString() + ".png");
                //
                File.Delete(fileName);
                return true;
            }
            catch(Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        private static BitmapFrame CreateResizedImage(BitmapSource source, int width, int height)
        {
            var rect = new Rect(0, 0, width, height);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(source, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }
    }
}
