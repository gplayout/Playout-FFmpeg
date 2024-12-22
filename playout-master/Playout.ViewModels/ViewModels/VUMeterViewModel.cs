using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.ViewModels.ViewModels
{
    public class VUMeterViewModel: Base.FormBaseViewModel
    {
        System.Windows.Forms.Timer _tim = null;
        public MediaFramework.DirectShowGrabber Dg { get; private set; }
        //
        double _leftVU;
        double _rightVU;
        const int DbMultiplier = 6;
        const int DbOffset = 12;
        public double LeftVU
        {
            get { return this._leftVU; }
            set
            {
                if (this._leftVU != value)
                {
                    this._leftVU = value;
                    this.NotifyOfPropertyChange(() => this.LeftVU);
                }
            }
        }
        public double RightVU
        {
            get { return this._rightVU; }
            set
            {
                if (this._rightVU != value)
                {
                    this._rightVU = value;
                    this.NotifyOfPropertyChange(() => this.RightVU);
                }
            }
        }
        public VUMeterViewModel(MediaFramework.DirectShowGrabber dg)
        {
            this.Dg = dg;
            this._tim = new System.Windows.Forms.Timer();
            this._tim.Interval = 100;
            this._tim.Tick += _tim_Tick;
            this._tim.Start();
        }
        void _tim_Tick(object sender, EventArgs e)
        {
            var sgAudio = this.Dg.CurrentPlayer.sgAudio;
            if (sgAudio == null)
                return;
            int height = 200;
            //
            try
            {
                int avgR, avgL, peakR, peakL;
                if (!this.Dg.IsPlaying)
                {
                    this.LeftVU = 0;
                    this.RightVU = 0;
                    return;
                }
                if (sgAudio.volumeInfoBusy)
                {
                    //avgR = (int)((Math.Log10(sgAudio.volumeAvgR) * DbMultiplier) - DbOffset);
                    //avgL = (int)((Math.Log10(sgAudio.volumeAvgL) * DbMultiplier) - DbOffset);
                    //peakR = (int)((Math.Log10(sgAudio.volumePeakR) * DbMultiplier) - DbOffset);
                    //peakL = (int)((Math.Log10(sgAudio.volumePeakL) * DbMultiplier) - DbOffset);
                    //
                    this.LeftVU = sgAudio.volumeAvgL * height / sgAudio.volumePeakL;
                    this.RightVU = sgAudio.volumeAvgR * height / sgAudio.volumePeakR;
                    sgAudio.volumeInfoBusy = false;
                }
            }
            catch (Exception ex)
            {
                sgAudio.volumeInfoBusy = false;   
            }
        }
        
        
    }
}
