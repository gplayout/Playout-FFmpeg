using Playout.Base;
using Playout.DataService;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Log;

namespace Playout.ApplicationService
{
    public interface IScheduleAppService
    {
        ScheduleModel LoadFile(string filePath);
        ScheduleModel LoadXElement(XElement sch);
        void SaveFile(ScheduleModel model);
        ScheduleModel OpenFile();
        XElement SaveToXElement(ScheduleModel model);

        void KillThread();
        void SchedulingStartThread(Action fn);
    }

    public class ScheduleAppService:IScheduleAppService
    {
        CancellationTokenSource cTokenSource;
        IScheduleDataService schDataService;
        public ScheduleAppService(IScheduleDataService _schDataService)
        {
            this.schDataService = _schDataService;
        }

        public ScheduleModel LoadFile(string filePath)
        {
            try
            {
                ScheduleModel model = this.schDataService.LoadFile(filePath);
                return model;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public ScheduleModel LoadXElement(XElement sch)
        {
            try
            {
                ScheduleModel model = this.schDataService.LoadXElement(sch);
                return model;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public void SaveFile(ScheduleModel model)
        {
            string filePath = Functions.ShowSaveFileDialog(model.Playlist.Name + Program.SCHEDULE_Ext, Program.SCHEDULE_FILES, Program.DefaultDir_Playlists);
            //
            if (!String.IsNullOrEmpty(filePath))
            {
                this.schDataService.SaveFile(filePath, model);
            }
        }
        public ScheduleModel OpenFile()
        {
            string filePath = Functions.ShowOpenFileDialog("SchedulePlaylist" + Program.SCHEDULE_Ext, Program.PLAYLISTAndSCHEDULE_FILES, Program.DefaultDir_Playlists);
            //
            if (!String.IsNullOrEmpty(filePath))
            {
                var model = this.LoadFile(filePath);
                return model;
            }
            else
                return null;
        }
        public XElement SaveToXElement(ScheduleModel model)
        {
            try
            {
                XElement xpl = this.schDataService.SaveToXElement(model);
                return xpl;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        public void KillThread()
        {
            if (this.cTokenSource != null)
                this.cTokenSource.Cancel();
        }

        public void SchedulingStartThread(Action fn)
        {
            this.cTokenSource = new CancellationTokenSource();
            var ct = this.cTokenSource.Token;
            //
            var UISyncContext = TaskScheduler.FromCurrentSynchronizationContext();
            Task schTask = Task.Factory.StartNew(() => SchedulingThreadDriver(ct, UISyncContext, fn), ct);
        }

        private void SchedulingThreadDriver(CancellationToken token, TaskScheduler uiTask, Action fn)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    return;
                //
                Task.Factory.StartNew(() =>
                {
                    fn();
                }, token, TaskCreationOptions.AttachedToParent, uiTask);
                Thread.Sleep(1000);
            }
        }
    }
}
