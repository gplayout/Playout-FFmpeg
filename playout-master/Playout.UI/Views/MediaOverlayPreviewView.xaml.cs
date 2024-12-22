using Playout.ApplicationService;
using Playout.Base;
using Playout.MediaFramework;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using enOverlayIndex = Playout.DirectShow.Overlays.enOverlayIndex;


namespace Playout.UI.Views
{
    public partial class MediaOverlayPreviewView
    {
        DirectShowGrabber Dg = null;
        bool _isGlobal = false;
        public MediaSourceViewModel ViewModel { get; private set; }
        public MediaOverlayPreviewView(MediaSourceViewModel item,bool isGlobal)
        {
            this.ViewModel = item;
            this._isGlobal = isGlobal;
            //
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            this.DataContext = this.ViewModel;
            //
            this.playerControl.BeforePlay = this.CheckOverlay;
            //
            this.Dg = new DirectShowGrabber();
            this.Dg.ForPreview = true;
            this.Dg.ChangeElement(DirectShowGrabberType.Uri);
            //
            IMediaPlaylistAppService mpAppService = (IMediaPlaylistAppService)DI.NinjectConfig.kernel.GetService(typeof(IMediaPlaylistAppService));
            var currentPlaylist = new MediaPlaylistViewModel()
                {
                    MediaSourcesVM = new MediaSourcesViewModel()
                    {
                        Sources = new ObservableCollection<MediaSourceViewModel>(
                            new List<MediaSourceViewModel>(){
                                this.ViewModel
                            })
                    }
                };
            //
            var playlistsVM = new MediaPlaylistsViewModel(mpAppService)
            {
                Playlists = new ObservableCollection<MediaPlaylistViewModel>(
                    new List<MediaPlaylistViewModel>()
                {   
                    currentPlaylist
                })
            };
            this.playerControl.ViewModel = new PlayerControlViewModel(this.Dg, playlistsVM)
            {
                CurrentPlaylist = currentPlaylist
            };
            //
            this.LoadVideo();
        }

        private void CheckOverlay()
        {
            var so = this.ViewModel.StringOverlay;
            this.Dg.SetTextOverlay(enOverlayIndex.ItemText, so.Enabled, so.FontName, so.FontSize, 
                Functions.ConvertWColorToDColor(so.Color),so.PositionLeft, so.PositionTop, so.Text,
                so.Shadow, Functions.ConvertWColorToDColor(so.ShadowColor),
                so.Transparency, Functions.ConvertWColorToDColor(so.BackColor), so.TextAlign);
            //
            var si = this.ViewModel.ImageOverlay;
            this.Dg.SetImageOverlay(enOverlayIndex.ItemImage, si.Enabled, si.FilePath, si.Width, si.Height, si.PositionLeft, si.PositionTop,
                si.AlphaBlending, si.ChromaKey, Functions.ConvertWColorToDColor(si.ChromaColor), si.ChromaLeeway);
            //
            var co = this.ViewModel.CrawlOverlay;
            this.Dg.SetCrawlOverlay(enOverlayIndex.ItemCrawl, co.Enabled, co.FontName, co.FontSize,
                Functions.ConvertWColorToDColor(co.Color), co.PositionLeft, co.PositionTop, co.Text,
                co.Shadow, Functions.ConvertWColorToDColor(co.ShadowColor),
                co.Transparency, Functions.ConvertWColorToDColor(co.BackColor), co.TextAlign, co.Scrolling,
                co.ScrollingSpeed, co.ReadFromFile, co.FilePath, co.Direction.ToString(), this._isGlobal);
            //
            var vo = this.ViewModel.VideoOverlay;
            Program.Dg.SetVideoOverlay(vo.Enabled, vo.FilePath, vo.Width, vo.Height, vo.PositionLeft, vo.PositionTop);
        }
        
        private void BaseWindowView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Dg.CurrentPlayer.Close();
        }

        
        private void btnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Functions.ShowOpenFileDialog("Image", Program.OPEN_PICTURE_FILES, Program.DefaultDir_MediaFiles);
            this.ViewModel.ImageOverlay.FilePath = fileName;
        }

        private void LoadVideo()
        {
            this.CheckOverlay();
            this.playerControl.PerformPlay();
        }

    }
}
