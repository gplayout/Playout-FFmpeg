using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using enMediaType = Playout.DirectShow.MediaPlayers.enMediaType;

namespace Playout.UI.Codes
{
    public class MainShortcuts
    {
        public MainShortcuts(Window ownd = null)
        {
            this.OtherWnd = ownd;
            if (ownd == null)
            {
                this.AddShortcut(ShortcutPlay_Executed, new KeyGesture(Key.Space));
                this.AddShortcut(ShortcutNext_Executed, new KeyGesture(Key.OemOpenBrackets));
                this.AddShortcut(ShortcutPervious_Executed, new KeyGesture(Key.OemCloseBrackets));

                this.AddShortcut(ShortcutAddMedia_Executed, new KeyGesture(Key.I, ModifierKeys.Control));
                this.AddShortcut(ShortcutAddMediaVideo_Executed, new KeyGesture(Key.V, ModifierKeys.Control));
                this.AddShortcut(ShortcutAddMediaUrl_Executed, new KeyGesture(Key.U, ModifierKeys.Control));

                this.AddShortcut(ShortcutAddMediaFiles_Executed, new KeyGesture(Key.M, ModifierKeys.Control));
                this.AddShortcut(ShortcutOpenMediaSettings_Executed, new KeyGesture(Key.E, ModifierKeys.Control));

                this.AddShortcut(ShortcutAddNewPlaylist_Executed, new KeyGesture(Key.P, ModifierKeys.Control));

                this.AddShortcut(ShortcutOpenSettings_Executed, new KeyGesture(Key.F2));
                this.AddShortcut(ShortcutOpenScheduler_Executed, new KeyGesture(Key.S, ModifierKeys.Alt));
                this.AddShortcut(ShortcutOpenStreamOut_Executed, new KeyGesture(Key.R, ModifierKeys.Alt));
                this.AddShortcut(ShortcutOpenTimer_Executed, new KeyGesture(Key.T, ModifierKeys.Alt));
                this.AddShortcut(ShortcutOpenGlobalSettings_Executed, new KeyGesture(Key.G, ModifierKeys.Alt));

                this.AddShortcut(ShortcutLoopOnSelectedItem_Executed, new KeyGesture(Key.L, ModifierKeys.Alt));
            }
            else if (ownd is UI.Views.TrimMediaView)
            {
                this.AddShortcut(TrimShortcutPlay_Executed, new KeyGesture(Key.Space));
                this.AddShortcut(TrimShortcutFF_Executed, new KeyGesture(Key.OemPeriod));
                this.AddShortcut(TrimShortcutFR_Executed, new KeyGesture(Key.OemComma));
            }

        }
        private System.Windows.Window OtherWnd { get; set; }
        private UI.Views.MainView MainWnd
        {
            get { return (UI.Views.MainView)App.Current.MainWindow; }
        }
        private void AddShortcut(ExecutedRoutedEventHandler ev, KeyGesture kg)
        {
            ICommand command = new RoutedCommand();
            CommandBinding cb = new CommandBinding(command, ev);
            InputBinding ib = new InputBinding(command, kg);
            if (OtherWnd != null)
            {
                OtherWnd.CommandBindings.Add(cb);
                OtherWnd.InputBindings.Add(ib);
            }
            else
            {
                MainWnd.CommandBindings.Add(cb);
                MainWnd.InputBindings.Add(ib);
            }
        }

        #region Main Window Shortcuts
        private void ShortcutPlay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (!this.MainWnd.playerControl.ViewModel.Dg.IsPlaying || this.MainWnd.playerControl.ViewModel.CurrentMediaSource == null)
                this.MainWnd.playerControl.PerformPlay();
            else
                this.MainWnd.playerControl.PerformPause();
        }
        private void ShortcutNext_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.playerControl.PerformNext(false);
        }
        private void ShortcutPervious_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.playerControl.PerformNext(true);
        }
        private void ShortcutAddMedia_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.playlistsV.filesV.PerformAddMediaItem();
        }
        private void ShortcutAddMediaVideo_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.playlistsV.filesV.PerformAddMediaItem(enMediaType.VideoFile);
        }
        private void ShortcutAddMediaUrl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.playlistsV.filesV.PerformAddMediaItem(enMediaType.Url);
        }
        private void ShortcutAddMediaFiles_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.playlistsV.filesV.MenuItemAddFiles_Click(null, null);
        }
        private void ShortcutOpenMediaSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.playlistsV.filesV.MenuItemTrim_Click(null, null);
        }
        private void ShortcutAddNewPlaylist_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.playlistsV.MenuItemNewPlaylist_Click(null, null);
        }
        private void ShortcutOpenSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.btnSettings_Click(null, null);
        }
        private void ShortcutOpenStreamOut_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.btnStreamOut_Click(null, null);
        }
        private void ShortcutOpenScheduler_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.btnSchedule_Click(null, null);
        }
        private void ShortcutOpenTimer_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.btnTimerSettings_Click(null, null);
        }
        private void ShortcutOpenGlobalSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            this.MainWnd.btnGlobalSettings_Click(null, null);
        }
        private void ShortcutLoopOnSelectedItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.MainWnd.playlistsV.filesV.dgMediaItems.SelectedItem != null)
                ((MediaSourceViewModel)this.MainWnd.playlistsV.filesV.dgMediaItems.SelectedItem).Loop = !((MediaSourceViewModel)this.MainWnd.playlistsV.filesV.dgMediaItems.SelectedItem).Loop;
            else if (this.MainWnd.playlistsV.dgPlaylist.SelectedItem != null)
                ((MediaPlaylistViewModel)this.MainWnd.playlistsV.dgPlaylist.SelectedItem).Loop = !((MediaPlaylistViewModel)this.MainWnd.playlistsV.dgPlaylist.SelectedItem).Loop;

        }
        #endregion Main Window Shortcuts

        #region TrimWindow Shortcuts
        private void TrimShortcutPlay_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            UI.Views.TrimMediaView ownd = (UI.Views.TrimMediaView)this.OtherWnd;
            //
            if (ownd.MediaState!=System.Windows.Controls.MediaState.Play)
                ownd.PlayMedia();
            else
                ownd.PauseMedia();
        }
        private void TrimShortcutFF_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //UI.Views.TrimMediaView ownd = (UI.Views.TrimMediaView)this.OtherWnd;
            //
            //ownd.playerControl.PerformFF(1);
        }
        private void TrimShortcutFR_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //UI.Views.TrimMediaView ownd = (UI.Views.TrimMediaView)this.OtherWnd;
            ////
            //ownd.playerControl.PerformFR();
        }
        #endregion TrimWindow Shortcuts
    }
}
