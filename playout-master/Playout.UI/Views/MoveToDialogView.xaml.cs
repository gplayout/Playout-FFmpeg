using Playout.Base;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
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

namespace Playout.UI.Views
{
    public partial class MoveToDialogView
    {
        public MoveToDialogView(string lblText,int value,int minVal,int MaxVal)
        {
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            //
            this.lbl.Content = lblText;
            this.val.Value = value;
            this.val.Minimum = minVal;
            this.val.Maximum = MaxVal;
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if(this.val.Value==null)
            {
                this.val.Focus();
                return;
            }
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
