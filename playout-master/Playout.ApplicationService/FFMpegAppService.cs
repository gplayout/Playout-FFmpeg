using Playout.DataService;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.ApplicationService
{
    public interface IFFMpegAppService
    {
        FFMpegModel LoadSettings();
        bool SaveProfiles(FFMpegModel model);
    }

    public class FFMpegAppService : IFFMpegAppService
    {
        IFFMpegDataService ffDataService;

        public FFMpegAppService(IFFMpegDataService _ffDataService)
        {
            this.ffDataService = _ffDataService;
        }

        public FFMpegModel LoadSettings()
        {
            var result = this.ffDataService.LoadSettings();
            return result;
        }

        public bool SaveProfiles(FFMpegModel model)
        {
            return this.ffDataService.SaveProfiles(model);
        }
    }
}
