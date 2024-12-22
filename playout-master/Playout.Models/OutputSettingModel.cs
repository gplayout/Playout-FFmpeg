using Playout.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Models
{
    public class OutputSettingModel
    {
        public string VideoDeviceName { get; set; }

        public string AudioDeviceName { get; set; }
        public string VideoSize { get; set; }

        public string DefaultDir_Playlists { get; set; }
        public string DefaultDir_MediaFiles { get; set; }
        public bool PlayoutLog { get; set; }
        public string ChannelName { get; set; }
        public string AlternateTimeZone { get; set; }
        public bool OutputToVCam { get; set; }
        public VCamChannelType VCamChannel { get; set; }
    }
}
