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

namespace Playout.UI.Controls
{
    public partial class Player 
    {
        public static readonly DependencyProperty IsPausedProperty = DependencyProperty.Register(
            "IsPaused",typeof(bool),typeof(Player));

        public bool IsPaused
        {
            get { return (bool)GetValue(IsPausedProperty); }
            set { SetValue(IsPausedProperty, value); }
        }

        System.Windows.Forms.Timer timer = null;
        public Player()
        {
            InitializeComponent();
            //
            this.IsPaused = false;
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10;
            timer.Tick += this.onFrameIsReady;
            timer.Start();
        }
        private void onFrameIsReady(object sender,EventArgs e)
        {
            try
            {
                var queue = Playout.DirectShow.Overlays.SampleGrabberOverlays.frameQueue;
                //
                if (queue.Count == 0)
                    return;
                //
                System.Drawing.Bitmap buffer = null;
                while (queue.Count > 0)
                {
                    buffer = queue.Dequeue();
                    if (buffer == null || this.IsPaused)
                        return;
                }
                //
                MediaPlayerBase.OutputPreview_Width = (int)this.imgPreview.ActualWidth;
                MediaPlayerBase.OutputPreview_Height = (int)this.imgPreview.ActualHeight;
                MediaPlayerBase.OutputPreview_IsPaused = this.IsPaused;
                //
                this.imgPreview.Source = Functions.ConvertToBitmapSource(buffer);
                //this.imgPreview.Dispatcher.BeginInvoke(new Action(() => {
                    
                //}), null);
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        
       
        private void ChangeSize(int size)
        {
            this.Height = size;
            this.Width = 16 * size / 9;
        }
        private void expMenuItemBig_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeSize(280);
        }

        private void expMenuItemNormal_Click(object sender, RoutedEventArgs e)
        {
            this.ChangeSize(200);
        }

        private void expMenuItemPopup_Click(object sender, RoutedEventArgs e)
        {
            BaseWindowView wnd = new BaseWindowView();
            wnd.Title = "Playout - Play";
            this.gridMain.Children.Remove(this.imgPreview);
            //
            Grid wndGrid = new Grid();
            wndGrid.RowDefinitions.Clear();
            wndGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
            wndGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            CheckBox chkTop = new CheckBox()
            {
                Content = "Stay On Top",
                Foreground=new SolidColorBrush(Colors.White),
                Margin = new Thickness(7, 7, 7, 7),
            };
            chkTop.SetValue(Grid.RowProperty, 0);
            chkTop.Checked += chkTop_Checked;
            chkTop.Unchecked += chkTop_Checked;
            //
            wndGrid.Children.Add(chkTop);
            this.imgPreview.BeginInit();
            wndGrid.Children.Add(this.imgPreview);
            
            wnd.Content = wndGrid;
            this.imgPreview.SetValue(Grid.RowProperty, 1);
            wnd.Show();
            this.imgPreview.EndInit();
            //
            int preSize = (int)this.Height;
            this.Width = this.Height = 0;
            //
            wnd.Closing += (cs, ce) =>
            {
                wndGrid.Children.Remove(this.imgPreview);
                this.imgPreview.SetValue(Grid.RowProperty, 0);
                this.gridMain.Children.Add(this.imgPreview);
                this.ChangeSize(preSize);
            };
        }

        void chkTop_Checked(object sender, RoutedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(sender as CheckBox);
            while (!(parent is Window))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            if (parent == null)
                return;
            (parent as Window).Topmost = (sender as CheckBox).IsChecked.Value;
        }

        private void expMenuItemPause_Click(object sender, RoutedEventArgs e)
        {
            this.IsPaused = !this.IsPaused;
            //
            if (this.IsPaused)
                this.timer.Stop();
            else
                this.timer.Start();
        }

    }
}
