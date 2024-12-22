using Playout.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Playout.Models
{
    public class RecordModel
    {
        public float FrameRate { get; set; }
        public int SampleRate { get; set; }
        public string FileName { get; set; }
        public string FileFormat { get; set; }
        public int SegmentSecond { get; set; }
        public Size FrameSize { get; set; }
        public VCamChannelType VCamChannel { get; set; }
        public EncoderProgram EncoderProgram { get; set; }
        public string VideoBitrate { get; set; }
        public string AudioBitrate { get; set; }
        public RecordModel()
        {

        }
    }
}
