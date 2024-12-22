using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playout.DI
{
    public class NinjectConfig
    {
        public static IKernel kernel;

        public void ConfigureContainer()
        {
            kernel = new StandardKernel();
            //
            kernel.Bind<DataService.IMediaPlaylistDataService>().To<DataService.MediaPlaylistDataService>().InTransientScope();
            kernel.Bind<DataService.IScheduleDataService>().To<DataService.ScheduleDataService>().InTransientScope();
            kernel.Bind<DataService.ISettingsDataService>().To<DataService.SettingsDataService>().InTransientScope();
            kernel.Bind<DataService.IOutputSettingDataService>().To<DataService.OutputSettingDataService>().InTransientScope();
            kernel.Bind<DataService.IGlobalSettingDataService>().To<DataService.GlobalSettingDataService>().InTransientScope();
            kernel.Bind<DataService.IPreviewDataService>().To<DataService.PreviewDataService>().InTransientScope();
            kernel.Bind<DataService.IRecordDataService>().To<DataService.RecordDataService>().InTransientScope();
            kernel.Bind<DataService.IStreamDataService>().To<DataService.StreamDataService>().InTransientScope();
            //
            kernel.Bind<ApplicationService.IMediaPlaylistAppService>().To<ApplicationService.MediaPlaylistAppService>().InTransientScope();
            kernel.Bind<ApplicationService.IScheduleAppService>().To<ApplicationService.ScheduleAppService>().InTransientScope();
            kernel.Bind<ApplicationService.ISettingsAppService>().To<ApplicationService.SettingsAppService>().InTransientScope();
            
        }
    }
}
