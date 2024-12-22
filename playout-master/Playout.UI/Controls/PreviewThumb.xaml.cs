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

namespace Playout.UI.Controls
{
    public partial class PreviewThumb
    {
        public Action<MediaSourceViewModel> OnPlayNow;
        public Action<PreviewThumb, MediaSourceViewModel> OnRemovePreviewItem;
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected",typeof(bool),typeof(PreviewThumb),null);

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public PreviewThumb(MediaSourceViewModel vm)
        {
            InitializeComponent();
            this.prevPlayer.MediaViewModel = vm;
            this.prevPlayer.OnPreviewStateChanged = this.OnPreviewStateChanged;
            //
            this.OnPreviewStateChanged(DirectShow.MediaPlayers.MediaState.Stop);
        }

        public void Run()
        {
            if (!this.IsSelected)
                this.prevPlayer.Run();
        }
        public void Close()
        {
            this.prevPlayer.Close();
        }


        private void OnPreviewStateChanged(Playout.DirectShow.MediaPlayers.MediaState ms)
        {
            switch (ms)
            {
                case DirectShow.MediaPlayers.MediaState.Play:
                    this.iconStop.IsEnabled = true;
                    this.iconPlay.IsEnabled = false;
                    break;
                case DirectShow.MediaPlayers.MediaState.Stop:
                    this.iconStop.IsEnabled = false;
                    this.iconPlay.IsEnabled = true;
                    break;
                default:
                    return;
            }
        }

        private void iconPlay_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.prevPlayer.Run();
        }

        private void iconStop_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.prevPlayer.Close();
        }

        private void iconEdit_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var vm = this.prevPlayer.MediaViewModel.Clone();
            Views.MediaSourceView wnd = new Views.MediaSourceView(vm, false);
            bool? result = wnd.ShowDialog();
            if (result == true)
            {
                this.prevPlayer.MediaViewModel.CopyValues(wnd.ViewModel, wnd.txtThumbImage.Text);
            }
        }

        private void iconPlayNow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.prevPlayer.Close();
            //
            if (this.OnPlayNow != null)
            {
                this.OnPlayNow(this.prevPlayer.MediaViewModel);
            }
        }

        private void iconDelete_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
            if (this.OnRemovePreviewItem != null)
                this.OnRemovePreviewItem(this, this.prevPlayer.MediaViewModel);
        }

        private void iconCopy_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.prevPlayer == null || this.prevPlayer.MediaViewModel == null)
                return;
            //
            var arr = new List<MediaSourceViewModel>() { this.prevPlayer.MediaViewModel };
            this.prevPlayer.MediaViewModel.CopeidItem = arr;
        }
    }
}
