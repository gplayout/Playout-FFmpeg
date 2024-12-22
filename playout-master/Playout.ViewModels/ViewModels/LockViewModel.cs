//using ClientWinAppRoyalLock.BaseClasses;
using Playout.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Playout.ViewModels.ViewModels
{
    public class LockViewModel : Base.FormBaseViewModel
    {
        #region Construction
        public LockViewModel()
        {
            this.ProcessStatus();
        }

        #endregion

        #region Members
        string _serialNo;
        #endregion

        #region Properties

        [Required(ErrorMessage = "Serial Number is Required.")]
        [StringLength(19, MinimumLength = 19, ErrorMessage = "Serial Number is Invalid.")]
        public string SerialNo
        {
            get { return _serialNo; }
            set
            {
                _serialNo = value;
                this.NotifyOfPropertyChange(() => this.SerialNo);
            }
        }
        public RoyalLock.LockStatusEnum LockStatus
        {
            get { return Program.Lock.LockStatus; }
        }
        public bool IsRegistered
        {
            get { return Program.Lock.IsRegistered; }
        }
        public string StatusMessage { get; private set; }
        public SolidColorBrush StatusColor { get; private set; }
        public bool RetryVisible
        {
            get { return this.LockStatus == RoyalLock.LockStatusEnum.Timeout; }
        }

        #endregion

        private void ProcessStatus()
        {
            switch (this.LockStatus)
            {
                case RoyalLock.LockStatusEnum.Basic:
                case RoyalLock.LockStatusEnum.Max:
                case RoyalLock.LockStatusEnum.MaxPlus:
                    this.StatusMessage = this.LockStatus.ToString() + " Edition Activated.";
                    this.StatusColor = new SolidColorBrush(Colors.Green);
                    break;
                case RoyalLock.LockStatusEnum.Timeout:
                    this.StatusMessage = RoyalLock.MessageDialog_ConnectionError;
                    this.StatusColor = new SolidColorBrush(Colors.Red);
                    break;
                case RoyalLock.LockStatusEnum.Trial:
                    this.StatusMessage = "Product Not Activated. Trial Days Remaining:";
                    this.StatusColor = new SolidColorBrush(Colors.Orange);
                    break;
                case RoyalLock.LockStatusEnum.TrialExpired:
                    this.StatusMessage = RoyalLock.MessageDialog_TrialFailed;
                    this.StatusColor = new SolidColorBrush(Colors.Red);
                    break;
                default:
                    break;
            }
            this.NotifyOfPropertyChange(() => this.LockStatus);
            this.NotifyOfPropertyChange(() => this.StatusMessage);
            this.NotifyOfPropertyChange(() => this.StatusColor);
            this.NotifyOfPropertyChange(() => this.RetryVisible);
        }
        public bool DoActivation()
        {
            bool result = Program.Lock.DoActivation(this.SerialNo);
            this.ProcessStatus();
            return result;
        }

        public bool DoRetry()
        {
            Program.Lock.CheckTrialUsage();
            this.ProcessStatus();
            return Program.Lock.LockStatus == RoyalLock.LockStatusEnum.Trial;
        }

        public bool DoDeactivation()
        {
            bool result = Program.Lock.DoDeActivation(this.SerialNo);
            return result;
        }


    }
}
