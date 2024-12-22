using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{
    public class SettingsModel
    {
        public List<ScheduleModel> Schedules { get; set; }
        public List<MediaPlaylistModel> Playlists { get; set; }
        public OutputSettingModel OutputSetting { get; set; }
        public GlobalSettingModel GlobalSetting { get; set; }

        public PreviewItemsModel PreviewItems { get; set; }
        public RecordModel Record { get; set; }
        public StreamModel Stream { get; set; }
    }
}
