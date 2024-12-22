using Playout.Base;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Log;

namespace Playout.DataService
{
    public interface IOutputSettingDataService : IXMLDataService
    {
        OutputSettingModel LoadXElement(XElement sett);
        XElement SaveToXElement(OutputSettingModel model);
    }

    public class OutputSettingDataService : XMLDataService, IOutputSettingDataService
    {
        public OutputSettingDataService()
        {
        }
        public OutputSettingModel LoadXElement(XElement sett)
        {
            try
            {
                OutputSettingModel model = new OutputSettingModel()
                {
                    VideoDeviceName = sett.Attribute("VideoDeviceName").GetValue<string>(""),
                    AudioDeviceName = sett.Attribute("AudioDeviceName").GetValue<string>(""),
                    AlternateTimeZone = sett.Attribute("AlternateTimeZone").GetValue<string>(""),
                    ChannelName = sett.Attribute("ChannelName").GetValue<string>(""),
                    VideoSize = sett.Attribute("VideoSize").GetValue<string>(""),
                    DefaultDir_MediaFiles = sett.Attribute("DefaultDir_MediaFiles").GetValue<string>(""),
                    DefaultDir_Playlists = sett.Attribute("DefaultDir_Playlists").GetValue<string>(""),
                    PlayoutLog = sett.Attribute("PlayoutLog").GetValue<bool>(false),
                    OutputToVCam = sett.Attribute("OutputToVCam").GetValue<bool>(false),
                    VCamChannel = sett.Attribute("VCamChannel").GetEnumValue<VCamChannelType>(VCamChannelType.Channel1),
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
        public XElement SaveToXElement(OutputSettingModel model)
        {
            try
            {
                XElement sett = new XElement("OutputSetting",
                    new XAttribute("VideoDeviceName", model.VideoDeviceName ?? ""),
                    new XAttribute("AudioDeviceName", model.AudioDeviceName ?? ""),
                    new XAttribute("ChannelName", model.ChannelName ?? ""),
                    new XAttribute("AlternateTimeZone", model.AlternateTimeZone ?? ""),
                    new XAttribute("DefaultDir_MediaFiles", model.DefaultDir_MediaFiles ?? ""),
                    new XAttribute("DefaultDir_Playlists", model.DefaultDir_Playlists ?? ""),
                    new XAttribute("PlayoutLog", model.PlayoutLog),
                    new XAttribute("OutputToVCam", model.OutputToVCam),
                    new XAttribute("VCamChannel", model.VCamChannel),
                    new XAttribute("VideoSize", model.VideoSize ?? ""));
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
