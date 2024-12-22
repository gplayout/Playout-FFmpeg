using Playout.Base;
using Playout.UI.Base;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using enMediaType = Playout.DirectShow.MediaPlayers.enMediaType;

namespace Playout.UI.Controls
{
    public partial class MediaSourcesView
    {
        Controls.DataGridDropCap ddCap = null;
        BaseWindowView wndPreview = null;
        //
        public MediaSourcesViewModel ViewModel
        {
            get { return (MediaSourcesViewModel)this.DataContext; }
        }

        public MediaSourcesView()
        {
            InitializeComponent();
            //
            this.ddCap = new DataGridDropCap(this.dgMediaItems, this.dgMediaItems_OnDropFiles);
        }
        private void BaseUserControlView_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
        public void PerformAddMediaItem(enMediaType? mediaType = null)
        {
            if (this.ViewModel == null)
            {
                if (this.Name == "filesSchV")
                    return;
                //
                var pl = MainViewModel.AddNewMediaPlaylist();
                this.ViewModel.ColorPlaylist = pl.Color;
            }
            //
            var vm = new MediaSourceViewModel(this.ViewModel.Sources);
            if (mediaType != null)
                vm.MediaType = mediaType.Value;
            //
            Views.MediaSourceView wnd = new Views.MediaSourceView(vm, true);
            if (wnd.ShowDialog() == true)
            {
                if (wnd.ViewModel.MediaType == enMediaType.Playlist)
                {
                    var item = MediaSourceViewModel.AddFileSource(wnd.ViewModel.File_Url, this.ViewModel.Sources);
                    this.ViewModel.AddFileSource(item);
                }
                else
                    this.ViewModel.AddFileSource(wnd.ViewModel);
            }
        }
        private void MenuItemAdd_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.PerformAddMediaItem();
        }
        private async void MenuItemRemove_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ViewModel == null || this.ViewModel.SelectedSource == null)
                return;
            //
            bool result = await Functions.ConfirmationForDelete();
            if (result)
            {
                var selitems = this.dgMediaItems.SelectedItems;
                if (selitems != null && selitems.Count > 0)
                {
                    var arr = new MediaSourceViewModel[selitems.Count];
                    selitems.CopyTo(arr, 0);
                    foreach (var item in arr)
                    {
                        this.ViewModel.RemoveSource(item);
                    }
                }
            }
        }
        private void expMenuUp_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            //
            var selitems = this.dgMediaItems.SelectedItems;
            if (selitems != null && selitems.Count > 0)
            {
                MediaSourceViewModel[] arr = new MediaSourceViewModel[selitems.Count];
                this.dgMediaItems.SelectedItems.CopyTo(arr, 0);
                this.ViewModel.ChangeOrderMediaSource(arr, true);
                foreach(var item in arr)
                    this.dgMediaItems.SelectedItems.Add(item);
            }
        }
        private void expMenuDown_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            //
            var selitems = this.dgMediaItems.SelectedItems;
            if (selitems != null && selitems.Count > 0)
            {
                MediaSourceViewModel[] arr = new MediaSourceViewModel[this.dgMediaItems.SelectedItems.Count];
                this.dgMediaItems.SelectedItems.CopyTo(arr, 0);
                this.ViewModel.ChangeOrderMediaSource(arr, false);
                foreach (var item in arr)
                    this.dgMediaItems.SelectedItems.Add(item);
            }
        }
        private void dgMediaItems_OnDropFiles(object sender,OnDropFilesEventArgs e)
        {
            if (e.Files == null)
                return;
            //
            if (this.ViewModel == null)
            {
                if (this.Name == "filesSchV")
                    return;
                //
                var pl = MainViewModel.AddNewMediaPlaylist();
                this.ViewModel.ColorPlaylist = pl.Color;
            }
            //
            foreach(string file in e.Files)
            {
                this.ViewModel.AddFileSource(file);
            }
        }
        public void MenuItemTrim_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            //
            var selitem = this.ViewModel.SelectedSource;
            if (selitem != null)
            {
                if (selitem.MediaType == enMediaType.Playlist)
                    return;
                //
                var vmClone = selitem.Clone();
                //
                Views.TrimMediaView view = new Views.TrimMediaView(vmClone);
                if (view.ShowDialog() == true)
                {
                    selitem.TrimStartSecond = view.ViewModel.TrimStartSecond;
                    if (selitem.Duration.HasValue && view.ViewModel.TrimStopSecond == (long)selitem.Duration.Value.TotalSeconds)
                        selitem.TrimStopSecond = 0;
                    else
                        selitem.TrimStopSecond = view.ViewModel.TrimStopSecond;
                    //
                    selitem.ImageOverlay = view.ViewModel.ImageOverlay.Clone();
                    selitem.StringOverlay = view.ViewModel.StringOverlay.Clone();
                    selitem.CrawlOverlay = view.ViewModel.CrawlOverlay.Clone();
                    selitem.VideoOverlay = view.ViewModel.VideoOverlay.Clone();
                }
            }
        }
        private void dgMediaItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MenuItemRemove.Visibility =
                this.MenuItemTrim.Visibility =
                this.MenuItemEdit.Visibility =
                this.MenuItemSepEdit.Visibility =
                this.MenuItemLoop.Visibility =
                this.MenuItemMove.Visibility =
                this.MenuItemSepMove.Visibility =
                    (this.dgMediaItems.SelectedItem == null ? Visibility.Collapsed : Visibility.Visible);
            //
            if (this.ViewModel != null && this.ViewModel.SelectedSource != null && this.ViewModel.SelectedSource.MediaType == enMediaType.Playlist)
            {
                this.MenuItemTrim.Visibility = Visibility.Collapsed;
            }
        }
        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
             if (this.ViewModel == null)
                return;
            //
            var selitem = this.ViewModel.SelectedSource;
            if (selitem != null)
            {
                var vm = selitem.Clone();
                Views.MediaSourceView wnd = new Views.MediaSourceView(vm, false);
                bool? result = wnd.ShowDialog();
                if (result == true)
                {
                    selitem.CopyValues(wnd.ViewModel, wnd.txtThumbImage.Text);
                }
            }
        }
        public void MenuItemAddFiles_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
            {
                if (this.Name == "filesSchV")
                    return;
                //
                var pl = MainViewModel.AddNewMediaPlaylist();
                this.ViewModel.ColorPlaylist = pl.Color;
            }
            //
            this.ViewModel.AddMediaFiles();
        }

        private void MenuItemCopy_Click(object sender, RoutedEventArgs e)
        {
            var selitem = this.ViewModel.SelectedSource;
            var selitems = this.dgMediaItems.SelectedItems;
            if (selitems != null && selitems.Count > 0)
            {
                var arr = new MediaSourceViewModel[selitems.Count];
                selitems.CopyTo(arr, 0);
                var arr2 = new List<MediaSourceViewModel>();
                foreach (var item in arr)
                    arr2.Add(item.Clone());
                //
                selitem.CopeidItem = arr2;
            }
        }

        private void MenuItemPaste_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlaylistViewModel._copiedItem != null)
            {
                int index = this.dgMediaItems.SelectedIndex;
                if (index == -1)
                    index = this.ViewModel.Sources.Count - 1;
                this.ViewModel.AddObject(MediaPlaylistViewModel._copiedItem, index);
                MediaPlaylistViewModel._copiedItem = null;
            }
        }

        private void MenuItemLoop_Click(object sender, RoutedEventArgs e)
        {
             if (this.ViewModel == null)
                return;
            //
            var selitem = this.ViewModel.SelectedSource;
            if (selitem != null)
            {
                selitem.Loop = !selitem.Loop;
            }
        }

        private void dgMediaItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.MenuItemTrim.Visibility == System.Windows.Visibility.Visible)
                this.MenuItemTrim_Click(null, null);
        }

        private void dgMediaItems_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            DependencyObject dep = (DependencyObject)e.OriginalSource;

            // iteratively traverse the visual tree until get a row or null
            while ((dep != null) && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            //if this is a row (item)
            if (dep is DataGridRow)
            {
                //if the pointed item is already selected do not reselect it, so the previous multi-selection will remain
                if (this.dgMediaItems.SelectedItems.Contains((dep as DataGridRow).Item))
                {
                    // now the drag will drag all selected files
                    e.Handled = true;
                }
            }

        }

        public void SelectAllMediaItems()
        {
            if (this.ViewModel == null || this.ViewModel.Sources==null)
                return;
            //
            this.dgMediaItems.SelectedItems.Clear();
            foreach (var item in this.ViewModel.Sources)
                this.dgMediaItems.SelectedItems.Add(item);
        }

        private void MenuItemMove_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            //
            var selitems = this.dgMediaItems.SelectedItems;
            if (selitems != null && selitems.Count > 0)
            {
                Views.MoveToDialogView wnd = new Views.MoveToDialogView("New Position:",
                    this.ViewModel.SelectedSource.Index, this.ViewModel.Sources.Min(m => m.Index),
                    this.ViewModel.Sources.Max(m => m.Index));
                if (wnd.ShowDialog() == true)
                {
                    MediaSourceViewModel[] arr = new MediaSourceViewModel[selitems.Count];
                    this.dgMediaItems.SelectedItems.CopyTo(arr, 0);
                    int val = wnd.val.Value.Value - 1;
                    if (val < 0)
                        val = 0;
                    this.ViewModel.ChangeOrderMediaSource(arr, val);
                    //
                    foreach (var item in arr)
                        this.dgMediaItems.SelectedItems.Add(item);
                }
            }
        }

        private void MenuItemOVerlay_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            //
            var selitem = this.ViewModel.SelectedSource;
            if (selitem != null)
            {
                if (selitem.MediaType == enMediaType.Playlist)
                    return;
                //
                var vmClone = selitem.Clone();
                //
                Views.OverlayMediaView view = new Views.OverlayMediaView(vmClone,false);
                if (view.ShowDialog() == true)
                {
                    selitem.ImageOverlay = view.ViewModel.ImageOverlay.Clone();
                    selitem.StringOverlay = view.ViewModel.StringOverlay.Clone();
                    selitem.CrawlOverlay = view.ViewModel.CrawlOverlay.Clone();
                    selitem.VideoOverlay = view.ViewModel.VideoOverlay.Clone();
                }
            }
        }

        private void thumbVideoTemplate_MouseEnter(object sender, MouseEventArgs e)
        {
            var vm = (MediaSourceViewModel)((FrameworkElement)((ContentControl)((ContentPresenter)((FrameworkElement)((ContentControl)((ContentPresenter)((FrameworkElement)(sender as MediaElement).Parent).TemplatedParent).TemplatedParent).Parent).TemplatedParent).TemplatedParent).Parent).DataContext;
            //
            var me = (sender as MediaElement);
            //
            me.Play();
            me.Position = vm.PreviewOfVideoFilePosition;
        }

        private void thumbVideoTemplate_MouseLeave(object sender, MouseEventArgs e)
        {
            try
            {
                var vm = (MediaSourceViewModel)((FrameworkElement)((ContentControl)((ContentPresenter)((FrameworkElement)((ContentControl)((ContentPresenter)((FrameworkElement)(sender as MediaElement).Parent).TemplatedParent).TemplatedParent).Parent).TemplatedParent).TemplatedParent).Parent).DataContext;
                //
                var me = (sender as MediaElement);
                vm.PreviewOfVideoFilePosition = me.Position;
                //
                me.Pause();
                //
                Size dpi = new Size(96, 96);
                RenderTargetBitmap bmp =
                  new RenderTargetBitmap((int)me.ActualWidth, (int)me.ActualHeight,
                    dpi.Width, dpi.Height, PixelFormats.Pbgra32);
                bmp.Render(me);
                //
                vm.ThumbnailImageForVideoFile = bmp;
                //

            }
            catch(Exception ex)
            {

            }
        }

        private void thumbVideoTemplate_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as MediaElement).Pause();
        }

        private void thumbVideoOpenPopup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.wndPreview != null)
                {
                    this.wndPreview.Close();
                    this.wndPreview = null;
                }
                var btn = (FrameworkElement)sender;
                var pme = (MediaElement)((Grid)btn.Parent).Children[0];
                this.wndPreview = new BaseWindowView();
                this.wndPreview.WindowStartupLocation = WindowStartupLocation.Manual;
                var pos = Mouse.GetPosition(Application.Current.MainWindow);
                this.wndPreview.Top = pos.Y;
                this.wndPreview.Left = pos.X;
                this.wndPreview.Width = 400;
                this.wndPreview.Height = 225;
                this.wndPreview.Title = "Preview";
                MediaElement me = new MediaElement()
                {
                    Source = pme.Source,
                    IsMuted = true,
                    LoadedBehavior = MediaState.Play,
                    Position = pme.Position,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                    VerticalAlignment = System.Windows.VerticalAlignment.Stretch
                };
                this.wndPreview.Content = me;
                this.wndPreview.Topmost = true;
                this.wndPreview.Show();
                //<Popup Name="popup" StaysOpen="True" IsOpen="True">
                //                    <MediaElement  Name="mediaElementPopup" 
                //                        LoadedBehavior="Play" IsMuted="True"
                //                        Source="{Binding Path=DataContext.File_Url,UpdateSourceTrigger=PropertyChanged, RelativeSource={RelativeSource FindAncestor, AncestorType=ContentControl}}"
                //                        >
                //                    </MediaElement>
                //                    <Popup.Style>
                //                        <Style TargetType="{x:Type Popup}">
                //                            <Style.Triggers>
                //                                <DataTrigger Binding="{Binding Path=IsMouseOver,UpdateSourceTrigger=PropertyChanged, 
                //                                        RelativeSource={RelativeSource FindAncestor, AncestorType=ContentControl}}"
                //                                    Value="True">
                //                                    <Setter Property="Popup.IsOpen"  Value="True" />
                //                                </DataTrigger>
                //                            </Style.Triggers>
                //                        </Style>
                //                    </Popup.Style>
                //                </Popup>
            }
            catch { }
        }
    }
}
