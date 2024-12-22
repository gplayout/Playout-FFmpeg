using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Playout.UI.Base
{
    public class BaseWindowView : Framework.UI.Controls.Window
    {
        public BaseWindowView()
        {
            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Icon = (ImageSource)this.FindResource("AppIconDrawingImage");
            this.TaskbarOverlayTemplate = (DataTemplate)this.FindResource("WindowOverlayTemplate");
            this.FlowDirection = FlowDirection.LeftToRight;
            //
            
        }
    }
}
