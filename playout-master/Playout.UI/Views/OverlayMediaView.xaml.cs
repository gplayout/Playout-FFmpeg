using Playout.ApplicationService;
using Playout.Base;
using Playout.MediaFramework;
using Playout.UI.Controls;
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
    public partial class OverlayMediaView
    {
        bool _isGlobal = false;
        public MediaSourceViewModel ViewModel { get; private set; }
        public OverlayMediaView(MediaSourceViewModel item,bool isGlobal)
        {
            this._isGlobal = isGlobal;
            this.ViewModel = item;
            this.DataContext = this.ViewModel;
            //
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            this.DataContext = this.ViewModel;
            //
            this.canvas.Background = new ImageBrush();
            (this.canvas.Background as ImageBrush).ImageSource = new BitmapImage(new Uri(@"Resources/CheckBoard.png", UriKind.Relative));
            //
            this.canvas.HorizontalAlignment=System.Windows.HorizontalAlignment.Stretch;
            this.canvas.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

        }
        private Point ScalePoint(int top,int left,bool isin)
        {
            Point output = new Point();
            //
            var outFormatPars = Program.Dg.OutputVideoFormatPars;
            Point input = new Point(outFormatPars.Width, outFormatPars.Height);
            Point scalePoint = new Point(this.canvas.ActualWidth, this.canvas.ActualHeight);
            if(isin)
            {
                input = new Point(this.canvas.ActualWidth, this.canvas.ActualHeight);
                scalePoint = new Point(outFormatPars.Width, outFormatPars.Height);
            }
            //
            if (input.X != 0 && input.Y != 0)
            {
                output.Y = (int)(input.Y * top / scalePoint.Y);
                output.X = (int)(input.X * left / scalePoint.X);
            }
            return output;
        }
        private void ScalePositions(bool isin)
        {
            Point point = new Point();
            if (this.ViewModel.StringOverlay != null)
            {
                point = this.ScalePoint(this.ViewModel.StringOverlay.PositionTop, this.ViewModel.StringOverlay.PositionLeft, isin);
                this.ViewModel.StringOverlay.PositionTop = (int)point.Y;
                this.ViewModel.StringOverlay.PositionLeft = (int)point.X;
            }
            //
            if (this.ViewModel.ImageOverlay != null)
            {
                point = this.ScalePoint(this.ViewModel.ImageOverlay.PositionTop, this.ViewModel.ImageOverlay.PositionLeft, isin);
                this.ViewModel.ImageOverlay.PositionTop = (int)point.Y;
                this.ViewModel.ImageOverlay.PositionLeft = (int)point.X;
            }
            //
            if (this.ViewModel.CrawlOverlay != null)
            {
                point = this.ScalePoint(this.ViewModel.CrawlOverlay.PositionTop, this.ViewModel.CrawlOverlay.PositionLeft, isin);
                this.ViewModel.CrawlOverlay.PositionTop = (int)point.Y;
                this.ViewModel.CrawlOverlay.PositionLeft = (int)point.X;
            }
            //
            if (this.ViewModel.VideoOverlay != null)
            {
                point = this.ScalePoint(this.ViewModel.VideoOverlay.PositionTop, this.ViewModel.VideoOverlay.PositionLeft, isin);
                this.ViewModel.VideoOverlay.PositionTop = (int)point.Y;
                this.ViewModel.VideoOverlay.PositionLeft = (int)point.X;
            }
        }
      
        private void BaseWindowView_Loaded(object sender, RoutedEventArgs e)
        {
            this.AddDragControl(this.ViewModel.StringOverlay, DragableControl.ControlType.Text);
            this.AddDragControl(this.ViewModel.ImageOverlay, DragableControl.ControlType.Image);
            this.AddDragControl(this.ViewModel.CrawlOverlay, DragableControl.ControlType.Crawl);
            if (this.ViewModel.VideoOverlay != null)
                this.AddDragControl(this.ViewModel.VideoOverlay, DragableControl.ControlType.Video);
            //Scale Positions
            this.ScalePositions(true);
        }
        private void AddDragControl(MediaOverlayViewModel vm,DragableControl.ControlType ct)
        {
            //Text
            DragableControl.DragControlData textdcd = new DragableControl.DragControlData()
            {
                DropControl = this.canvas,
                Type = ct,
                InDropArea = vm.Enabled,
                ViewModel = vm,
            };
            DragableControl textdc = new DragableControl(textdcd);
            if (vm.Enabled)
            {
                if (vm.PositionLeft < 0)
                    vm.PositionLeft = 1;
                if (vm.PositionTop < 0)
                    vm.PositionTop = 1;
                //
                this.canvas.Children.Add(textdc);
            }
            else
            {
                this.controlBox.Children.Add(textdc);
            }
            /////////////////////////////////
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
            this.ScalePositions(false);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void BaseWindowView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            string fileName = Functions.ShowOpenFileDialog("Text File", Program.OPEN_TEXT_FILES, Program.DefaultDir_MediaFiles);
            this.ViewModel.CrawlOverlay.FilePath = fileName;
        }

        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.ViewModel.Clone();
            MediaOverlayPreviewView view = new MediaOverlayPreviewView(vm, this._isGlobal);
            view.ShowDialog();
        }

        //Drag & Drop
        private void canvas_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
                var dc = this.GetControlFromControlBox(Guid.Parse(dataString));
                //
                this.controlBox.Children.Remove(dc);
                dc.ControlData.InDropArea = true;
                dc.ControlData.ViewModel.Enabled = true;
                var pos = e.GetPosition(this.canvas);
                //
                dc.ControlData.ViewModel.PositionLeft = (int)pos.X;
                dc.ControlData.ViewModel.PositionTop = (int)pos.Y;
                this.canvas.Children.Add(dc);
            }
        }
        private void canvas_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string dataString = (string)e.Data.GetData(DataFormats.StringFormat);
                var dc = this.GetControlFromControlBox(Guid.Parse(dataString));
                //
                if (dc == null)
                    return;
                //
                e.Effects = DragDropEffects.Copy | DragDropEffects.Move;
            }
            var point = e.GetPosition(this.canvas);
            this.posx.Content = "X=" + point.X;
            this.posy.Content = "Y=" + point.Y;
        }
        private void canvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(this.canvas);
            this.posx.Content = "X=" + point.X;
            this.posy.Content = "Y=" + point.Y;
        }

        private DragableControl GetControlFromControlBox(Guid guid)
        {
            foreach (DragableControl dc in this.controlBox.Children)
            {
                if (dc.ControlData.Guid == guid)
                    return dc;
            }
            //
            return null;
        }

        private void canvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var items = this.canvas.GetSelectedElements();
                while(items.Count>0)
                {
                    var item = items[0];
                    if (!(item is DragableControl))
                        return;
                    //
                    var dc = (DragableControl)item;
                    dc.ControlData.ViewModel.Enabled = false;
                    dc.ControlData.InDropArea = false;
                    this.canvas.Children.Remove(dc);
                    this.controlBox.Children.Add(dc);
                }
                e.Handled = true;
            }
        }

        private void canvas_SelectionMoved(object sender, EventArgs e)
        {
            var items = this.canvas.GetSelectedElements();

            foreach (var item in items)
            {
                if (!(item is DragableControl))
                    return;
                //
                var dc = (DragableControl)item;
                //
                if (dc.ControlData.ViewModel.PositionLeft < 0)
                    dc.ControlData.ViewModel.PositionLeft = 0;
                if (dc.ControlData.ViewModel.PositionTop < 0)
                    dc.ControlData.ViewModel.PositionTop = 0;
                //
                if (dc.ControlData.ViewModel.PositionLeft >= this.canvas.ActualWidth - 30)
                    dc.ControlData.ViewModel.PositionLeft = (int)this.canvas.ActualWidth - 60;
                if (dc.ControlData.ViewModel.PositionTop >= this.canvas.ActualHeight - 30)
                    dc.ControlData.ViewModel.PositionTop = (int)this.canvas.ActualHeight - 60;
            }
        }
    }
}

