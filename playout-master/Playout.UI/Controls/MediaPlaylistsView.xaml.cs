using Playout.Base;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Playout.UI.Codes;

namespace Playout.UI.Controls
{
    public partial class MediaPlaylistsView
    {
        MediaPlaylistsViewModel _viewModel;
        Controls.DataGridDropCap ddCap = null;
        
        public MediaPlaylistsViewModel ViewModel 
        {
            get { return this._viewModel; }
            set 
            { 
                this._viewModel = value;
                //
                this.DataContext = this._viewModel;
                if (this.dgPlaylist.SelectedItem == null && this.ViewModel.Playlists.Count > 0)
                    this.dgPlaylist.SelectedIndex = 0;
                //
                this.ViewModel.OnPlaybackModeChanged = this.OnPlaybackModeChanged;
                this.ViewModel.OnSelectedItemChangedInPlaybackMode = this.OnSelectedItemChangedInPlaybackMode;
                this.LoadThumbColumnInfo();
                this.ChangePlaybackMode();
            }
        }
        
        public MediaPlaylistsView()
        {
            InitializeComponent();
            //
            this.ddCap = new DataGridDropCap(this.dgPlaylist, this.dgPlaylist_OnDropFiles);
        }

        private void OnPlaybackModeChanged()
        {
            if (this.ViewModel.PlaybackMode && this.dgPlaylist.ActualWidth >= 0 && this.dgPlaylist.ActualWidth <= 5)
            {
                this.filesV.dgMediaItems.ItemsSource = this.ViewModel.AllSources;
            }
            else
            {
                this.filesV.dgMediaItems.SetBinding(DataGrid.ItemsSourceProperty, new Binding("Sources"));
            }
        }
        private void dgPlaylist_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.OnPlaybackModeChanged();
        }       
        private void expMenuUp_Click(object sender, RoutedEventArgs e)
        {
            var selitem = this.dgPlaylist.SelectedItem;
            var selitems = this.dgPlaylist.SelectedItems;
            if (selitems != null && selitems.Count > 0)
            {
                MediaPlaylistViewModel[] arr = new MediaPlaylistViewModel[selitems.Count];
                this.dgPlaylist.SelectedItems.CopyTo(arr, 0);
                this.ViewModel.ChangeOrderMediaSource(arr, true);
                this.dgPlaylist.SelectedItem = selitem;
            }

        }
        private void expMenuDown_Click(object sender, RoutedEventArgs e)
        {
            var selitem = this.dgPlaylist.SelectedItem;
            var selitems = this.dgPlaylist.SelectedItems;
            if (selitems != null && selitems.Count > 0)
            {
                MediaPlaylistViewModel[] arr = new MediaPlaylistViewModel[selitems.Count];
                this.dgPlaylist.SelectedItems.CopyTo(arr, 0);
                this.ViewModel.ChangeOrderMediaSource(arr, false);
                this.dgPlaylist.SelectedItem = selitem;
            }
        }
        public void MenuItemNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var pvm = this.ViewModel.NewPlaylist();
            if (pvm != null)
                this.dgPlaylist.SelectedItem = pvm;
        }      
        private void MenuItemOpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var pvm = this.ViewModel.OpenPlaylist();
            if (pvm != null)
                this.dgPlaylist.SelectedItem = pvm;
        }
        private void MenuItemSavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selitem = this.ViewModel.SelectedPlaylist;
            if (selitem != null)
                this.ViewModel.SavePlaylist(selitem);
        }
        private void MenuItemRemovePlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selitems = this.dgPlaylist.SelectedItems;
            if (selitems != null && selitems.Count > 0)
            {
                var arr = new MediaPlaylistViewModel[selitems.Count];
                selitems.CopyTo(arr, 0);
                foreach (var item in arr)
                {
                    this.ViewModel.RemovePlaylist((MediaPlaylistViewModel)item);
                }
            }
        }
        private void dgPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.MenuItemRemovePlaylist.Visibility =
                this.MenuItemSavePlaylist.Visibility =
                this.MenuItemChangeColorPlaylist.Visibility =
                    (this.dgPlaylist.SelectedItem == null ? Visibility.Collapsed : Visibility.Visible);
        }
        private void dgPlaylist_OnDropFiles(object sender, OnDropFilesEventArgs e)
        {
            if (e.Files == null)
                return;
            //
            foreach (string file in e.Files)
            {
                this.ViewModel.LoadFile(file);
            }
        }
        private void MenuItemCopyPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selitem = this.ViewModel.SelectedPlaylist;
            var selitems = this.dgPlaylist.SelectedItems;
            if (selitems != null && selitems.Count > 0)
            {
                var arr = new MediaPlaylistViewModel[selitems.Count];
                selitems.CopyTo(arr, 0);
                var arr2 = new List<MediaPlaylistViewModel>();
                foreach (var item in arr)
                    arr2.Add(item.Clone());
                //
                selitem.CopeidItem = arr2;
            }

        }
        private void MenuItemPastePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (MediaPlaylistViewModel._copiedItem != null && MediaPlaylistViewModel._copiedItem is List<MediaPlaylistViewModel>)
            {
                int index = this.dgPlaylist.SelectedIndex;
                if (index == -1)
                    index = this.ViewModel.Playlists.Count - 1;
                foreach(var item in (List<MediaPlaylistViewModel>)MediaPlaylistViewModel._copiedItem)
                {
                    this.ViewModel.AddPlaylist(item, index++);
                }
                //
                MediaPlaylistViewModel._copiedItem = null;
            }
        }
        private void MenuItemChangeColorPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selitem = this.ViewModel.SelectedPlaylist;
            if (selitem != null)
            {
                System.Windows.Forms.ColorDialog colDialog = new System.Windows.Forms.ColorDialog();
                if (colDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    selitem.Color = Functions.ConvertColorToInt(colDialog.Color);
                }
            }
        }
        private void MenuItemOpenPlaylists_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            //
            this.ViewModel.AddPlaylists();
        }
        private void MenuItemLoopPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var selitem = this.ViewModel.SelectedPlaylist;
            if (selitem != null)
            {
                selitem.Loop = !selitem.Loop;
            }
        }
        private void MenuItemLoopAllPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            //
            this.ViewModel.LoopOnPlaylists = !this.ViewModel.LoopOnPlaylists;
        }
        private void MenuItemPlaybackMode_Click(object sender, RoutedEventArgs e)
        {
            if (this.ViewModel == null)
                return;
            //
            this.ViewModel.PlaybackMode = !this.ViewModel.PlaybackMode;
            this.ChangePlaybackMode();
        }

        private void ChangePlaybackMode()
        {
            if(this.ViewModel.PlaybackMode)
            {
                this.filesV.dgMediaItems.SetValue(ContextMenuService.IsEnabledProperty, false);
                this.filesV.dgMediaItems.Columns[0].Visibility = System.Windows.Visibility.Collapsed;
                this.dgPlaylist.Columns[0].Visibility = System.Windows.Visibility.Collapsed;
                this.MenuItemChangeColorPlaylist.IsEnabled =
                    this.MenuItemCopyPlaylist.IsEnabled =
                    this.MenuItemNewPlaylist.IsEnabled =
                    this.MenuItemOpenPlaylist.IsEnabled =
                    this.MenuItemOpenPlaylists.IsEnabled =
                    this.MenuItemPastePlaylist.IsEnabled =
                    this.MenuItemRemovePlaylist.IsEnabled =
                    this.MenuItemSavePlaylist.IsEnabled = false;
            }
            else
            {
                this.filesV.dgMediaItems.SetValue(ContextMenuService.IsEnabledProperty, true);
                this.filesV.dgMediaItems.Columns[0].Visibility = System.Windows.Visibility.Visible;
                this.dgPlaylist.Columns[0].Visibility = System.Windows.Visibility.Visible;
                //
                this.MenuItemChangeColorPlaylist.IsEnabled =
                    this.MenuItemCopyPlaylist.IsEnabled =
                    this.MenuItemNewPlaylist.IsEnabled =
                    this.MenuItemOpenPlaylist.IsEnabled =
                    this.MenuItemOpenPlaylists.IsEnabled =
                    this.MenuItemPastePlaylist.IsEnabled =
                    this.MenuItemRemovePlaylist.IsEnabled =
                    this.MenuItemSavePlaylist.IsEnabled = true;
            }
        }

        private void OnSelectedItemChangedInPlaybackMode()
        {
            if (this.dgPlaylist.SelectedItem != null)
                this.dgPlaylist.ScrollIntoView(this.dgPlaylist.SelectedItem);
            if (this.filesV.dgMediaItems.SelectedItem != null)
                this.filesV.dgMediaItems.ScrollIntoView(this.filesV.dgMediaItems.SelectedItem);
        }

        private void LoadThumbColumnInfo()
        {
            Codes.emDataGridOptions.LoadGridOptions(this.dgPlaylist);
            Codes.emDataGridOptions.LoadGridOptions(this.filesV.dgMediaItems);
            //
            var thumbCol = this.filesV.dgMediaItems.Columns.Where(m => m.Header != null && m.Header.ToString() == "Thumbnail").FirstOrDefault();
            //
            if (thumbCol != null && thumbCol.Visibility == System.Windows.Visibility.Visible)
            {
                Parallel.ForEach(this.ViewModel.Playlists, (pl) =>
                {
                    if (pl.MediaSourcesVM == null)
                        return;
                    Parallel.ForEach(pl.MediaSourcesVM.Sources, (ms) =>
                    {
                        ms.LoadThumb = true;
                    });
                });
            }
            //
            if (thumbCol != null)
            {
                ((Codes.ExtendedDataGridTemplateColumn)thumbCol).VisibilityChanged += (s, e) =>
                {
                    Parallel.ForEach(this.ViewModel.Playlists, (pl) =>
                    {
                        if (pl.MediaSourcesVM == null || pl.MediaSourcesVM.Sources == null)
                            return;
                        Parallel.ForEach(pl.MediaSourcesVM.Sources, (ms) =>
                        {
                            ms.LoadThumb = e.Visibility == System.Windows.Visibility.Visible;
                        });
                    });
                };
            }
        }

        private void dgPlaylist_PreviewMouseDown(object sender, MouseButtonEventArgs e)
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
                if (this.dgPlaylist.SelectedItems.Contains((dep as DataGridRow).Item))
                {
                    // now the drag will drag all selected files
                    e.Handled = true;
                }
            }
        }

        private void dgPlaylist_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var nameField = this.dgPlaylist.Columns.Where(m => m.Header!=null && m.Header.ToString() == "Name").FirstOrDefault();
            if (nameField == null)
                return;
            //
            int index = this.dgPlaylist.Columns.IndexOf(nameField);
            DataGridCell cell = this.dgPlaylist.GetCell(this.dgPlaylist.SelectedIndex, index);
            if (cell != null)
            {
                cell.Focus();
                this.dgPlaylist.BeginEdit();
            }
        }

      
    }
}
