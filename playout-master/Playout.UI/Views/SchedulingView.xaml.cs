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
using System.Windows.Shapes;

namespace Playout.UI.Views
{
    public partial class SchedulingView
    {
        public SchedulingsViewModel ViewModel { get; set; }
        public SchedulingView(SchedulingsViewModel svm)
        {
            this.ViewModel = svm;
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            this.DataContext = this.ViewModel;
            this.schedulesV.ViewModel = this.ViewModel;
        }
    }
}
