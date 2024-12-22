using Playout.DirectShow.Controls;
using Playout.MediaFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Playout.Log;
using Playout.UI.Base;
using Playout.DirectShow.MediaPlayers;
using Playout.Base;
using Playout.DirectShow.DirectShow.MediaPlayers;
using System.IO;
using Bitmap = System.Drawing.Bitmap;
using Playout.ViewModels.ViewModels;
using System.Runtime.InteropServices;

namespace Playout.UI.Controls
{
    public partial class PreviewPlayer
    {
        MediaPreviewPlayer _prevPlayer = null;
        System.Windows.Forms.Timer timer = null;
        public Action MouseClick;
        public Action<Playout.DirectShow.MediaPlayers.MediaState> OnPreviewStateChanged;
        public MediaSourceViewModel MediaViewModel { get; set; }
        public PreviewPlayer()
        {
            InitializeComponent();
        }
        public void Run()
        {
            try
            {
                this.Close();
                //
                var pp = new MediaPreviewPlayer();
                string inputName = this.MediaViewModel.File_Url_DeviceName;
                int devIndex = 0;
                if (this.MediaViewModel.MediaType == enMediaType.Device)
                {
                    inputName = DecklinkWrapper.GetInputDirectShowName(inputName, true);
                    devIndex = DecklinkWrapper.GetInputVideoCapabilityIndex(this.MediaViewModel.DeviceName, this.MediaViewModel.InputDeviceFormat);
                    inputName += (devIndex == -1 ? "" : "@" + devIndex);
                }
                pp.OnPreviewStateChanged = this.OnPreviewStateChanged;

                pp.Run(inputName, this.MediaViewModel.MediaType, (int)this.Width, (int)this.Height, devIndex);
                _prevPlayer = pp;
                //
                timer = new System.Windows.Forms.Timer();
                timer.Interval = 10;
                timer.Tick += this.onFrameIsReady;
                timer.Start();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public void Close()
        {
            try
            {
                if (this._prevPlayer != null)
                    this._prevPlayer.Close();
                //
                if (this.timer != null)
                {
                    this.timer.Tick -= this.onFrameIsReady;
                    this.timer.Stop();
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        private void onFrameIsReady(object sender, EventArgs e)
        {
            try
            {
                if (this._prevPlayer == null || this._prevPlayer.frameQueue.Count == 0)
                    return;
                //
                IntPtr buffer = IntPtr.Zero;
                while (this._prevPlayer.frameQueue.Count > 0)
                {
                    buffer = this._prevPlayer.frameQueue.Dequeue();
                    if (buffer == null)
                        return;
                    //
                    if(this._prevPlayer.frameQueue.Count > 0)
                    {
                        Marshal.FreeHGlobal(buffer);
                    }
                }
                //
                int width = (int)this.Width;
                int height = (int)this.Height;
                if (width != this._prevPlayer.InputWidth)
                    width = this._prevPlayer.InputWidth;
                if (height != this._prevPlayer.InputHeight)
                    height = this._prevPlayer.InputHeight;
                //
                Bitmap output = new Bitmap(width, height, width * 4, System.Drawing.Imaging.PixelFormat.Format32bppRgb, buffer);
                output.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipY);
                this.picbox.Image = output;
                Marshal.FreeHGlobal(buffer);
            }
            catch (Exception ex)
            {
            }
        }

        private void picbox_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (MouseClick != null)
                MouseClick();
        }
    }
}
