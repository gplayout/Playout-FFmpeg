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
    public partial class VUMeter
    {
        private VUMeterViewModel _viewModel = null;
        public VUMeterViewModel ViewModel
        {
            get { return this._viewModel; }
            set
            {
                this._viewModel = value;
                this.DataContext = this._viewModel;
            }
        }
        public VUMeter()
        {
            InitializeComponent();
        }
    }
}
