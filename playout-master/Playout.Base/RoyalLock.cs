//using ClientWinAppRoyalLock.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Playout.Log;

namespace Playout.Base
{
    public class RoyalLock
    {
        public enum LockStatusEnum { Basic, Max, MaxPlus, Trial, TrialExpired, Timeout }

        //ClientWinAppRoyalLock.Control MyLock = new ClientWinAppRoyalLock.Control();

        const string MessageDialog_Title = "License Validation";
        public const string MessageDialog_ConnectionError = "Operation Failed. Please Check Internet Connectivity.";
        public const string MessageDialog_GeneralError = "Operation Failed. Please Check License Info and/or Internet Connectivity.";
        public const string MessageDialog_TrialFailed = "Trial Period Expired. Please Enter A Valid Serial Number To Continue.";
        const string MessageDialog_AckSuccd = "SmarterDataAgent Activated Successfully. Thank you.\r\n Please close and reopen this application for the changes to become effective.";
        const string MessageDialog_DeackSuccd = "SmarterDataAgent Deactivated Successfully.";
        const string MessageDialog_AckOtherMachine = "This Serial Number Already In Use On Another Machine!";
        const string MessageDialog_AckFailed = "Activation Failed. Please Check Serial Number And Try Again.";
        const string MessageDialog_DeackFailed = "Deactivation Failed. Please Check Serial Number And Try Again.";

        const string ValidationKey = "76b9b7e7-b616-4f83-9a07-4332f7d573cb";

        const int VersionCode = 1019;
        string serialNoFilePath = Program.AppPath + @"\Data\CoreSettings.bin";

        Thread trialCheckThread = null;
        int tiralCheckCounter = 0;
        public Action trialCheckOnExceed { get; set; }
        public bool IsRegistered
        {
            get
            {
                return (Program.Lock.LockStatus == RoyalLock.LockStatusEnum.Max
                    || Program.Lock.LockStatus == RoyalLock.LockStatusEnum.Basic
                    || Program.Lock.LockStatus == LockStatusEnum.MaxPlus);
            }
        }

        public LockStatusEnum LockStatus
        {
            get;
            private set;
        }

        public RoyalLock()
        {
           // this.LockStatus = LockStatusEnum.Trial;
            this.LockStatus = LockStatusEnum.MaxPlus;
        }

        public bool DoActivation(string serialNo)
        {

            return true;
            /*
            AckMessage MsgResult = MyLock.Activation(serialNo, ValidationKey, true);
            if (MsgResult == null)
            {
                Functions.ShowMessageErrorClassic(MessageDialog_Title, MessageDialog_GeneralError);
                return false;
            }
            if (MsgResult.StatusCode == enStatusCodes.Activated || MsgResult.StatusCode == enStatusCodes.Succeed)
            {
                Functions.ShowMessageInfoClassic(MessageDialog_Title, MessageDialog_AckSuccd);
                //
                this.LockStatus = (LockStatusEnum)Enum.Parse(typeof(LockStatusEnum), MsgResult.EditionKey);
                this.SaveSerialNo(serialNo);
                this.TrialCheckKillThread();
                return true;
            }
            else if (MsgResult.StatusCode == enStatusCodes.ActivatedOnOtherMachine)
                Functions.ShowMessageErrorClassic(MessageDialog_Title, MessageDialog_AckOtherMachine);
            else
                Functions.ShowMessageErrorClassic(MessageDialog_Title, MessageDialog_AckFailed);
            return false;*/
        }

        public bool DoDeActivation(string serialNo)
        {
            throw new NotImplementedException();
            /*try
            {
                DckMessage MsgResult = MyLock.Disactivation(serialNo, ValidationKey);
                if (MsgResult == null)
                {
                    Functions.ShowMessageErrorClassic(MessageDialog_Title, MessageDialog_GeneralError);
                    return false;
                }
                else if (MsgResult.StatusCode == enStatusCodes.Succeed)
                {
                    Functions.ShowMessageInfoClassic(MessageDialog_Title, MessageDialog_DeackSuccd);
                    //
                    if (File.Exists(this.serialNoFilePath))
                        File.Delete(this.serialNoFilePath);
                    this.LockStatus = LockStatusEnum.Trial;
                    return true;
                }
                else
                {
                    Functions.ShowMessageErrorClassic(MessageDialog_Title, MessageDialog_DeackFailed);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Functions.ShowMessageErrorClassic(MessageDialog_Title, MessageDialog_GeneralError);
                return false;
            }*/
        }

        public void CheckTrialUsage()
        {
            /*try
            {
                this.LockStatus = LockStatusEnum.Trial;
                return;
                //
                TrialMessage MsgResult = MyLock.TrialUsage(VersionCode);
                Logger.InfoLog("Trial:" + (MsgResult == null ? "timeout" : MsgResult.StatusCode.ToString()));
                if (MsgResult == null)
                {
                    this.LockStatus = LockStatusEnum.Timeout;
                }
                else if (MsgResult.StatusCode == enStatusCodes.Succeed)
                {
                    this.LockStatus = LockStatusEnum.Trial;
                }
                else
                {
                    this.LockStatus = LockStatusEnum.TrialExpired;
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                this.LockStatus = LockStatusEnum.Timeout;
            }*/
        }

        public bool CheckOffAck()
        {
            this.LockStatus = LockStatusEnum.MaxPlus;
            return true;
            /*
            try
            {
                string serialNo = this.ReadSerialNo();
                if (String.IsNullOrEmpty(serialNo))
                    return false;
                //
                AckMessage MsgResult = MyLock.CheckOfflineActivation(serialNo);
                if (MsgResult == null)
                {
                    return false;
                }
                else if (MsgResult.StatusCode == enStatusCodes.Succeed
                    || MsgResult.StatusCode == enStatusCodes.Activated)
                {
                    this.LockStatus = (LockStatusEnum)Enum.Parse(typeof(LockStatusEnum), MsgResult.EditionKey);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
             */
        }

        private void SaveSerialNo(string serialNo)
        {
            try
            {
                string content = Functions.Base64Encode(serialNo);
                File.WriteAllText(this.serialNoFilePath, content);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public string ReadSerialNo()
        {
            try
            {
                if (!File.Exists(this.serialNoFilePath))
                    return "";
                //
                string content = File.ReadAllText(this.serialNoFilePath);
                content = Functions.Base64Decode(content);
                return content;
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        private void TrialCheckKillThread()
        {
            try
            {
                if (this.trialCheckThread != null)
                    this.trialCheckThread.Abort();
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public void TrialCheckStartThread()
        {
            this.TrialCheckKillThread();
            //
            this.trialCheckThread = new Thread(this.TrialCheckThreadDriver);
            this.tiralCheckCounter = 0;
            this.trialCheckThread.Start();
        }
        private void TrialCheckThreadDriver()
        {
            while (true)
            {
                this.CheckTrialUsage();
                if (this.LockStatus == LockStatusEnum.Trial)
                    return;
                //
                this.tiralCheckCounter++;
                if (this.tiralCheckCounter >= 4 || this.LockStatus == LockStatusEnum.TrialExpired)
                {
                    this.trialCheckOnExceed();
                    return;
                }
                //
                Thread.Sleep(5 * 60 * 1000); //5 Min
            }
        }
    }
}
