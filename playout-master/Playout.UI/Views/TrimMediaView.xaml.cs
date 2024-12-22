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
using System.Reflection;
using enMediaType = Playout.DirectShow.MediaPlayers.enMediaType;

namespace Playout.UI.Views
{
    public partial class TrimMediaView
    {
        Codes.MainShortcuts shorts = null;
        public MediaSourceViewModel ViewModel { get; private set; }
        public TrimMediaView(MediaSourceViewModel item)
        {
            this.ViewModel = item;
            //
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            this.DataContext = this.ViewModel;
            //
            long duration = item.Duration == null ? 0 : (long)item.Duration.Value.TotalSeconds;
            //
            this.tsStart.Minimum = new TimeSpan(0, 0, 0);
            this.tsStart.Maximum = TimeSpan.FromSeconds(duration);
            this.tsStart.Value = TimeSpan.FromSeconds(this.ViewModel.TrimStartSecond);
            //
            this.tsStop.Minimum = new TimeSpan(0, 0, 0);
            this.tsStop.Maximum = TimeSpan.FromSeconds(duration);
            this.tsStop.Value = TimeSpan.FromSeconds(this.ViewModel.TrimStopSecond == 0 ? duration : this.ViewModel.TrimStopSecond);
            //
            this.tsStart.ValueChanged += this.tsStart_ValueChanged;
            this.tsStop.ValueChanged += this.tsStop_ValueChanged;
            //
            this.slider.Maximum = duration;
            this.slider.Minimum = 0;
            this.slider.UpperValue = this.ViewModel.TrimStopSecond == 0 ? duration : this.ViewModel.TrimStopSecond;
            this.slider.LowerValue = this.ViewModel.TrimStartSecond;
            //
            if (this.ViewModel.MediaType == enMediaType.VideoFile)
            {
                this.brdTrim.Visibility = Visibility.Visible;
            }
            else
            {
                this.brdTrim.Visibility = Visibility.Collapsed;
                this.Height = this.Height - 100;
            }
            //
            this.shorts = new Codes.MainShortcuts(this);
            this.mediaPlayer.Source = new Uri(this.ViewModel.File_Url);
            this.mediaPlayer.Position = TimeSpan.FromSeconds(this.ViewModel.TrimStartSecond);
            //
            this.mediaPlayer.ScrubbingEnabled = true;
            this.mediaPlayer.Pause();
            this.btnPlay.IsEnabled = true;
            this.btnPause.IsEnabled = this.btnStop.IsEnabled = false;
        }

        public MediaState MediaState
        {
            get
            {
                FieldInfo hlp = typeof(MediaElement).GetField("_helper", BindingFlags.NonPublic | BindingFlags.Instance);
                object helperObject = hlp.GetValue(this.mediaPlayer);
                FieldInfo stateField = helperObject.GetType().GetField("_currentState", BindingFlags.NonPublic | BindingFlags.Instance);
                MediaState state = (MediaState)stateField.GetValue(helperObject);
                return state;
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(this.ViewModel.StringOverlay.FontName))
            {
                try
                {
                    System.Drawing.Font fd = new System.Drawing.Font(this.ViewModel.StringOverlay.FontName, 12);
                }
                catch (Exception ex)
                {
                    Functions.ShowMessageErrorClassic("Media Settings", ex.Message);
                    return;
                }
            }
            //
            this.DialogResult = true;
            this.Close();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void BaseWindowView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.mediaPlayer.Close();
        }

        private void tsStop_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!this.tsStop.Value.HasValue)
                return;
            //
            if (this.tsStart.Value != null)
            {
                if (this.tsStop.Value.Value.CompareTo(this.tsStart.Value.Value) < 0)
                    this.tsStop.Value = this.tsStart.Value.Value;
            }
            //
            this.slider.UpperValue = this.tsStop.Value.Value.TotalSeconds;
            this.SetTrimStop();
        }

        private void tsStart_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!this.tsStart.Value.HasValue)
                return;
            //
            if (this.tsStop.Value != null)
            {
                if (this.tsStart.Value.Value.CompareTo(this.tsStop.Value.Value) > 0)
                    this.tsStart.Value = this.tsStop.Value.Value;
            }
            //
            this.slider.LowerValue = this.tsStart.Value.Value.TotalSeconds;
            this.SetTrimStart();
        }
        private void SetTrimStart()
        {
            this.ViewModel.TrimStartSecond = (long)this.tsStart.Value.Value.TotalSeconds;
            //
            this.mediaPlayer.Position = TimeSpan.FromSeconds(this.ViewModel.TrimStartSecond);
        }
        private void SetTrimStop()
        {
            this.ViewModel.TrimStopSecond = (long)this.tsStop.Value.Value.TotalSeconds;
            //
            this.mediaPlayer.Position = TimeSpan.FromSeconds(this.ViewModel.TrimStopSecond);

        }
        

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            this.StopMedia();
        }

        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            this.PauseMedia();
        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            this.PlayMedia();
        }
        public void StopMedia()
        {
            this.mediaPlayer.Stop();
            this.mediaPlayer_MediaEnded(null, null);
        }
        public void PauseMedia()
        {
            this.mediaPlayer.Pause();
            this.mediaPlayer_MediaEnded(null, null);
        }
        public void PlayMedia()
        {
            this.mediaPlayer.Play();
            this.mediaPlayer_MediaOpened();
        }
        private void mediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.btnPlay.IsEnabled = true;
            this.btnPause.IsEnabled = this.btnStop.IsEnabled = false;
        }

        private void mediaPlayer_MediaOpened()
        {
            this.btnPlay.IsEnabled = false;
            this.btnPause.IsEnabled = this.btnStop.IsEnabled = true;
        }

        private void slider_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            //
            long val = (long)this.slider.LowerValue;
            this.slider.LowerValue = val;
            if (this.tsStart.Value == null || val != (long)this.tsStart.Value.Value.TotalSeconds)
            {
                this.tsStart.Value = TimeSpan.FromSeconds(val);
                this.SetTrimStart();
            }
            //
            val = (long)this.slider.UpperValue;
            this.slider.UpperValue = val;
            if (this.tsStop.Value == null || val != (long)this.tsStop.Value.Value.TotalSeconds)
            {
                this.tsStop.Value = TimeSpan.FromSeconds(val);
                this.SetTrimStop();
            }
        }
    }
}
