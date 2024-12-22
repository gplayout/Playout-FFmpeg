using Playout.MediaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Playout.Log;
using System.Management;

namespace Playout.Base
{
    public static class Program
    {
        public const string AUDIO_CLIPS = "*.mp3;*.wav;*.wma;*.mid;";
        public const string VIDEO_CLIPS = "*.mp*;*.vro;*.avi;*.asf;*.wmv;*.vob;*.avs;*.mov;*.divx;*.mp4;*.mts;*.3gp;*.swf;*.m2v;*.mkv;*.flv;*.webm;*.ts;*.m4v;*.mp4v;*.ogg;*.amv;*.rm;*.m2t*;";
        public const string IMAGE_FILES = "*.jpg;*.jpeg;*.jpe;*.bmp;*.gif;*.png;";
        public const string OPEN_MEDIA_FILES = "All media files|" + VIDEO_CLIPS + AUDIO_CLIPS + IMAGE_FILES
                        + "|Video clips|" + VIDEO_CLIPS
                        + "|Audio clips|" + AUDIO_CLIPS;
        public const string OPEN_ONLY_MEDIA_FILES = "All media files|" + VIDEO_CLIPS + AUDIO_CLIPS
                        + "|Video clips|" + VIDEO_CLIPS
                        + "|Audio clips|" + AUDIO_CLIPS;
        public const string OPEN_MEDIA_FILES_EXTENDED = OPEN_MEDIA_FILES
                        + "|Image files|" + IMAGE_FILES;
        public const string OPEN_PICTURE_FILES = "Image files|*.bmp;*.jpg;*.gif;*.png;*.jpeg";
        public const string OPEN_TEXT_FILES = "Text files|*.txt";
        //public const string OPEN_PICTURE_FILES = "Image files|*.bmp;*.jpg;*.gif;*.png;*.tif;*.tiff;*.wmf;*.emf;*.exf;*.jpe;*.jpeg";
        public const string OPEN_PROFILES = "WMV9 profiles (*.prx)|*.prx";
        public const string OPEN_COMPRESSOR_SETTINGS = "Compressor saved settings (*.txt)|*.txt";

        public const string PLAYLIST_FILES = "Playlist (*.spls)|*.spls";
        public const string SCHEDULE_FILES = "Schedule (*.schs)|*.schs";
        public const string PLAYLISTAndSCHEDULE_FILES = PLAYLIST_FILES + "|" + SCHEDULE_FILES;

        public const string PLAYLIST_Ext = ".spls";
        public const string SCHEDULE_Ext = ".schs";
        //
        public static bool PlayoutLog { get; set; }
        //
        public static DirectShowGrabber Dg { get; set; }
        public static string DefaultDir_MediaFiles { get; set; }
        public static string DefaultDir_Playlists { get; set; }

        public static readonly string AppPath = System.Windows.Forms.Application.StartupPath;
        public static RoyalLock Lock { get; set; }
        static Program()
        {
            Program.Dg = new DirectShowGrabber();
            //

            Program.Lock = new RoyalLock();
        }

        public static void DoEvents()
        {
            try
            {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(ExitFrame), frame);
                Dispatcher.PushFrame(frame);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        private static object ExitFrame(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }
    }
}
