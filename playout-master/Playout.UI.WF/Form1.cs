using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Playout.UI.WF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.videoGrabber1.PlayerFileName = @"H:\Demo\Tableau Software Overview.mp4";
            this.videoGrabber1.FrameCaptureHeight = 200;
            this.videoGrabber1.FrameCaptureWidth = 200;
            this.videoGrabber1.OpenPlayer();
        }
    }
}
