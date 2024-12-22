using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playout.UI.Controls
{
    public class DataGridDropCap
    {
        event EventHandler<OnDropFilesEventArgs> OnDropFiles;
        DataGrid dg { get; set; }
        

        public DataGridDropCap(DataGrid _dg,EventHandler<OnDropFilesEventArgs> onDrop)
        {
            this.dg=_dg;
            this.OnDropFiles=onDrop;
            this.dg.AllowDrop = true;
            this.dg.DragEnter += dg_DragEnter;
            this.dg.Drop += dg_Drop;
        }

        void dg_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && this.OnDropFiles != null)
                {
                    this.OnDropFiles(this, new OnDropFilesEventArgs(files));
                }
            }
        }

        void dg_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.None;
        }

    }

    public class OnDropFilesEventArgs:EventArgs
    {
        public string[] Files { get; set; }

        public OnDropFilesEventArgs(string[] files)
        {
            this.Files = files;
        }
    }
}
