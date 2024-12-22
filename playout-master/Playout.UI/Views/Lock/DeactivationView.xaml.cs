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

namespace Playout.UI.Views.Lock
{
    public partial class DeactivationView
    {
        LockViewModel ViewModel { get; set; }
        public DeactivationView()
        {
            InitializeComponent();
            this.ViewModel = new LockViewModel();
            //this.ViewModel.SerialNo = Program.Lock.ReadSerialNo();
            this.DataContext = this.ViewModel;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            var result = this.ViewModel.DoDeactivation();
            if(result)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                this.ViewModel.SerialNo = "";
                this.txtSerialNo.Focus();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
