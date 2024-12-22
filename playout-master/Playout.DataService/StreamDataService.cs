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
    public interface IStreamDataService : IXMLDataService
    {
        StreamModel LoadXElement(XElement sett);
        XElement SaveToXElement(StreamModel model);
    }

    public class StreamDataService : XMLDataService, IStreamDataService
    {
        public StreamDataService()
        {
        }
        public StreamModel LoadXElement(XElement sett)
        {
            try
            {
                StreamModel model = new StreamModel()
                {
                    SelectedProfileName = sett.Attribute("SelectedProfileName").GetValue<string>(null),
                    Profiles = sett.Element("Profiles")
                                .Elements("Profile")
                                .Select(n => new StreamInstanceModel()
                                {
                                    FileFormat = n.Attribute("FileFormat").GetValue<string>("mp4"),
                                    FileName = n.Attribute("FileName").GetValue<string>(""),
                                    FrameRate = n.Attribute("FrameRate").GetValue<float>(30),
                                    SampleRate = n.Attribute("SampleRate").GetValue<int>(44100),
                                    VideoBitrate = sett.Attribute("VideoBitrate").GetValue<string>(""),
                                    AudioBitrate = sett.Attribute("AudioBitrate").GetValue<string>(""),
                                    SegmentSecond = n.Attribute("SegmentSecond").GetValue<int>(0),
                                    FrameSize = new Size
                                    (
                                        n.Attribute("FrameWidth").GetValue<int>(720),
                                        n.Attribute("FrameHeight").GetValue<int>(480)
                                    ),
                                    VCamChannel = n.Attribute("VCamChannel").GetEnumValue<VCamChannelType>(VCamChannelType.Channel1),
                                    EncoderProgram = n.Attribute("EncoderProgram").GetEnumValue<EncoderProgram>(EncoderProgram.FFMPEG),
                                    Url = n.Attribute("Url").GetValue<string>(""),
                                    ProfileName = n.Attribute("ProfileName").GetValue<string>("")
                                }).ToList()

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
        public XElement SaveToXElement(StreamModel model)
        {
            try
            {
                if (model.Profiles == null)
                    model.Profiles = new List<StreamInstanceModel>();
                //
                XElement sett = new XElement("Stream",
                    new XAttribute("SelectedProfileName", model.SelectedProfileName ?? ""),
                    new XElement("Profiles",
                        from item in model.Profiles
                        select new XElement("Profile",
                            new XAttribute("FileFormat", item.FileFormat ?? ""),
                            new XAttribute("FileName", item.FileName ?? ""),
                            new XAttribute("VideoBitrate", item.VideoBitrate ?? ""),
                            new XAttribute("AudioBitrate", item.AudioBitrate ?? ""),
                            new XAttribute("FrameRate", item.FrameRate),
                            new XAttribute("SampleRate", item.SampleRate),
                            new XAttribute("SegmentSecond", item.SegmentSecond),
                            new XAttribute("FrameWidth", item.FrameSize.Width),
                            new XAttribute("FrameHeight", item.FrameSize.Height),
                            new XAttribute("VCamChannel", item.VCamChannel),
                            new XAttribute("EncoderProgram", item.EncoderProgram),
                            new XAttribute("Url", item.Url),
                            new XAttribute("ProfileName", item.ProfileName)
                            )
                        ));
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
