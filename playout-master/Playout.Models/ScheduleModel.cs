using Playout.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{
    public class ScheduleModel
    {
        public Days2 Days { get; set; }
        public TimeSpan? StartTime { get; set; }
        public bool Enabled { get; set; }
        public bool EveryDay { get; set; }
        public DateTime? StartDate { get; set; }
        public int? IntervalMinutes { get; set; }
        public MediaPlaylistModel Playlist { get; set; }
        public string FilePath { get; set; }
    }
}
