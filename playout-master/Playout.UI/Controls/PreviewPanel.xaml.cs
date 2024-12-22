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
    public partial class PreviewPanel
    {
        PlayerControlViewModel _pvViewModel = null;
        PreviewItemsViewModel _prevViewModel = null;
        string _currentItemPlayingFUD = null;
        public PlayerControlViewModel PVViewModel 
        {
            get { return this._pvViewModel; }
            set
            {
                this._pvViewModel = value;
                this._pvViewModel.OnBeforeSetMediaSource_ForPreview = PVViewModel_OnBeforeSetMediaSource;
            }
        }
        public PreviewItemsViewModel PrevItemsVM 
        {
            get { return this._prevViewModel; }
            set
            {
                this._prevViewModel = value;
                //
                this.spPrevItems.Children.Clear();
                foreach(var item in this._prevViewModel.Items)
                {
                    this.AddPreviewItem(item);
                }
            }
        }

        void PVViewModel_OnBeforeSetMediaSource(string fud)
        {
            this._currentItemPlayingFUD = fud;
            foreach (PreviewThumb item in this.spPrevItems.Children)
            {
                if (!String.IsNullOrEmpty(fud) && item.prevPlayer != null && item.prevPlayer.MediaViewModel != null
                    && item.prevPlayer.MediaViewModel.File_Url_DeviceName == fud)
                {
                    item.IsSelected = true;
                    item.Close();
                }
                else
                {
                    item.IsSelected = false;
                }
            }
        }
        public PreviewPanel()
        {
            InitializeComponent();
        }

        private PreviewThumb AddPreviewItem(MediaSourceViewModel vm)
        {
            PreviewThumb prevThumb = new PreviewThumb(vm);
            prevThumb.OnPlayNow = this.OnPlayNow;
            prevThumb.OnRemovePreviewItem = this.OnRemovePreviewItem;
            this.spPrevItems.Children.Add(prevThumb);
            prevThumb.IsSelected = this._currentItemPlayingFUD == vm.File_Url_DeviceName;
            return prevThumb;
        }
        private void btnAddPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MediaSourceViewModel vm = new MediaSourceViewModel(null);
                Views.MediaSourceView wnd = new Views.MediaSourceView(vm, true);
                bool? result = wnd.ShowDialog();
                if (result == true)
                {
                    var prevThumb = this.AddPreviewItem(vm);
                    this.PrevItemsVM.Items.Add(vm);
                    prevThumb.Run();
                }
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private void OnPlayNow(MediaSourceViewModel vm)
        {
            if (this.PVViewModel != null)
            {
                var pvm = new MediaPlaylistViewModel()
                {
                    MediaSourcesVM = new MediaSourcesViewModel()
                    {
                        Sources = new System.Collections.ObjectModel.ObservableCollection<MediaSourceViewModel>(
                            new List<MediaSourceViewModel>()
                            {
                                vm
                            })
                    }
                };
                this.PVViewModel.PlayScheduledPlaylist(pvm);
            }
        }
        private void OnRemovePreviewItem(PreviewThumb pt,MediaSourceViewModel vm)
        {
            int index = (int)pt.GetValue(Grid.ColumnProperty);
            this.spPrevItems.Children.RemoveAt(index);
            //
            this.PrevItemsVM.Items.Remove(vm);
        }
    }
}
