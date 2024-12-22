using Playout.Base;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Log;
using System.Drawing;

namespace Playout.DataService
{
    public interface IRecordDataService : IXMLDataService
    {
        RecordModel LoadXElement(XElement sett);
        XElement SaveToXElement(RecordModel model);
    }

    public class RecordDataService : XMLDataService, IRecordDataService
    {
        public RecordDataService()
        {
        }
        public RecordModel LoadXElement(XElement sett)
        {
            try
            {
                RecordModel model = new RecordModel()
                {
                    FileFormat = sett.Attribute("FileFormat").GetValue<string>("mp4"),
                    FileName = sett.Attribute("FileName").GetValue<string>("output"),
                    FrameRate = sett.Attribute("FrameRate").GetValue<float>(30),
                    SampleRate = sett.Attribute("SampleRate").GetValue<int>(44100),
                    VideoBitrate = sett.Attribute("VideoBitrate").GetValue<string>(""),
                    AudioBitrate = sett.Attribute("AudioBitrate").GetValue<string>(""),
                    SegmentSecond = sett.Attribute("SegmentSecond").GetValue<int>(0),
                    FrameSize = new Size
                    (
                        sett.Attribute("FrameWidth").GetValue<int>(720),
                        sett.Attribute("FrameHeight").GetValue<int>(480)
                    ),
                    VCamChannel = sett.Attribute("VCamChannel").GetEnumValue<VCamChannelType>(VCamChannelType.Channel1),
                    EncoderProgram = sett.Attribute("EncoderProgram").GetEnumValue<EncoderProgram>(EncoderProgram.FFMPEG),
                };
                //
                return model;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public XElement SaveToXElement(RecordModel model)
        {
            try
            {
                XElement sett = new XElement("Record",
                    new XAttribute("FileFormat", model.FileFormat ?? ""),
                    new XAttribute("FileName", model.FileName ?? ""),
                    new XAttribute("FrameRate", model.FrameRate),
                    new XAttribute("SampleRate", model.SampleRate),
                    new XAttribute("SegmentSecond", model.SegmentSecond),
                    new XAttribute("FrameWidth", model.FrameSize.Width),
                    new XAttribute("VideoBitrate", model.VideoBitrate ?? ""),
                    new XAttribute("AudioBitrate", model.AudioBitrate ?? ""),
                    new XAttribute("FrameHeight", model.FrameSize.Height),
                    new XAttribute("VCamChannel", model.VCamChannel),
                    new XAttribute("EncoderProgram", model.EncoderProgram));
                //
                return sett;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

    }
}
