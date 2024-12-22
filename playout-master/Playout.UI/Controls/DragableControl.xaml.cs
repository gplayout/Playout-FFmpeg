using Playout.UI.Base;
using Playout.ViewModels.ViewModels;
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

namespace Playout.UI.Controls
{
    public partial class DragableControl
    {
        Point lastPos;
        public DragControlData ControlData { get; set; }
        public DragableControl(DragControlData data)
        {
            this.ControlData = data;
            this.DataContext = this.ControlData;
            this.ControlData.InDropAreaChanged = this.InDropAreaChanged;
            InitializeComponent();
            this.InDropAreaChanged(this.ControlData.InDropArea);
        }
        private void InDropAreaChanged(bool inDA)
        {
            if(inDA)
            {
                this.SetBinding(BaseUserControlView.WidthProperty,
                    new Binding("ViewModel.Width")
                    {
                        UpdateSourceTrigger=UpdateSourceTrigger.PropertyChanged,
                        Mode=BindingMode.TwoWay,
                    });
                this.SetBinding(BaseUserControlView.HeightProperty,
                    new Binding("ViewModel.Height")
                    {
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Mode = BindingMode.TwoWay,
                    });
            }
            else
            {
                switch(this.ControlData.Type)
                {
                    case ControlType.Crawl:
                    case ControlType.Text:
                        this.Width = 120;
                        this.Height = 50;
                        break;
                    case ControlType.Image:
                    case ControlType.Video:
                        this.Width = 120;
                        this.Height = 120;
                        break;
                    default:
                        break;
                }
            }
        }

        private void UserControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.lastPos = e.GetPosition(this.ControlData.DropControl);
            this.Focus();
        }
        private void UserControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                return;
            //
            if (!this.ControlData.InDropArea)
            {
                DragDrop.DoDragDrop(this, this.ControlData.Guid.ToString(), DragDropEffects.Move);
            }
            //else
            //{
            //    var pos0 = e.GetPosition(this.ControlData.DropControl);
            //    var pos = new Point((double)this.GetValue(Canvas.LeftProperty), (double)this.GetValue(Canvas.TopProperty));
            //    //
            //    pos.X = pos.X - (lastPos.X - pos0.X);
            //    pos.Y = pos.Y - (lastPos.Y - pos0.Y);
            //    //
            //    this.ControlData.ViewModel.PositionLeft = (int)pos.X;
            //    this.ControlData.ViewModel.PositionTop = (int)pos.Y;
            //    //
            //    lastPos = pos0;
            //}
        }

        private void UserControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!this.ControlData.InDropArea)
                return;
            //
            BaseWindowView wnd = null;
            switch (this.ControlData.Type)
            {
                case ControlType.Text:
                    wnd = new Controls.OverlayEditors.TextEditor((StringOverlayMediaViewModel)this.ControlData.ViewModel);
                    break;
                case ControlType.Image:
                    wnd = new Controls.OverlayEditors.ImageEditor((ImageOverlayMediaViewModel)this.ControlData.ViewModel);
                    break;
                case ControlType.Video:
                    wnd = new Controls.OverlayEditors.VideoEditor((VideoOverlayMediaViewModel)this.ControlData.ViewModel);
                    break;
                case ControlType.Crawl:
                    wnd = new Controls.OverlayEditors.CrawlEditor((CrawlOverlayMediaViewModel)this.ControlData.ViewModel);
                    break;
                default:
                    return;
            }
            //
            wnd.ShowDialog();
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            
        }
        ///////////////////////////////////////////////////////////////////////////////////////
        public class DragControlData
        {
            bool _inDropArea;
            public Action<bool> InDropAreaChanged;
            public DragControlData()
            {
                this.Guid = Guid.NewGuid();
            }
            public bool InDropArea 
            {
                get { return this._inDropArea; }
                set
                {
                    if(this._inDropArea!=value)
                    {
                        this._inDropArea = value;
                        if (this.InDropAreaChanged != null)
                            this.InDropAreaChanged(this._inDropArea);
                    }
                }
            }
            public ControlType Type { get; set; }
            public string TypeText { get { return Type.ToString() + " Overlay"; } }
            public InkCanvas DropControl { get; set; }
            public Guid Guid { get; private set; }
            public MediaOverlayViewModel ViewModel { get; set; }
        }
        public enum ControlType
        {
            Text, Image, Crawl, Video
        }

    }

}
