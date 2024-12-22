using Playout.DataService;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.ApplicationService
{
    public interface ISettingsAppService
    {
        SettingsModel LoadSettings();
        void SaveSettings(SettingsModel model);
    }
    public class SettingsAppService : ISettingsAppService
    {
        ISettingsDataService settDataService;

        public SettingsAppService(ISettingsDataService _settDataService)
        {
            this.settDataService = _settDataService;
        }

        public SettingsModel LoadSettings()
        {
            var result = this.settDataService.LoadSettings();
            return result;
        }

        public void SaveSettings(SettingsModel model)
        {
            this.settDataService.SaveSettings(model);
        }
    }
}
