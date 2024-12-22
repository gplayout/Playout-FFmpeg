using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playout.UI.Codes
{
    public class ExtendedDataGridTemplateColumn : Framework.UI.Controls.DataGridTemplateColumn
    {
        public event EventHandler<VisibilityChangedEventArgs> VisibilityChanged;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Visibility")
            {
                if (this.VisibilityChanged != null)
                {
                    this.VisibilityChanged(this, new VisibilityChangedEventArgs((Visibility)e.NewValue));
                }
            }

            base.OnPropertyChanged(e);
        }
    }

    public class VisibilityChangedEventArgs : EventArgs
    {
        public VisibilityChangedEventArgs(Visibility visibility)
        {
            this.Visibility = visibility;
        }

        public Visibility Visibility { get; private set; }
    }
}
