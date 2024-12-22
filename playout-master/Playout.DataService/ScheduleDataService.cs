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
    public interface IScheduleDataService : IXMLDataService
    {
        ScheduleModel LoadFile(string filePath);
        ScheduleModel LoadXElement(XElement sch);
        void SaveFile(string filePath, ScheduleModel model);
        XElement SaveToXElement(ScheduleModel model);

        List<ScheduleModel> LoadSchedulesFromXElement(XElement schs);
        XElement SaveSchedulesToXElement(List<ScheduleModel> schs);
    }

    public class ScheduleDataService : XMLDataService, IScheduleDataService
    {
        IMediaPlaylistDataService mpDataService;

        public ScheduleDataService(IMediaPlaylistDataService _mpDataService)
        {
            this.mpDataService = _mpDataService;
        }
        public ScheduleModel LoadFile(string filePath)
        {
            try
            {
                XElement root = this.LoadXFile(filePath);
                ScheduleModel model = new ScheduleModel()
                {
                    Days = Days2.None,
                    Enabled = false,
                    EveryDay = false,
                    IntervalMinutes = null,
                    StartDate = null,
                    StartTime = null
                };
                //
                string ext = System.IO.Path.GetExtension(filePath);
                if (ext == Program.SCHEDULE_Ext)
                {
                    model = LoadXElement(root);
                }
                else if (ext == Program.PLAYLIST_Ext)
                {
                    MediaPlaylistModel plModel = this.mpDataService.LoadXElement(root);
                    model.Playlist = plModel;
                }
                else
                    throw new Exception("Could not open this file format " + ext);
                //
                model.FilePath = filePath;
                //
                return model;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public ScheduleModel LoadXElement(XElement sch)
        {
            try
            {
                ScheduleModel model = new ScheduleModel()
                {
                    Days = (Days2)Enum.Parse(typeof(Days2), sch.Attribute("Days").GetValue<string>(Days2.None.ToString())),
                    StartTime = sch.Attribute("StartTime").GetTimespan(null),
                    Enabled = sch.Attribute("Enabled").GetValue<bool>(false),
                    EveryDay = sch.Attribute("EveryDay").GetValue<bool>(false),
                    StartDate = sch.Attribute("StartDate").GetDateTime(null),
                    IntervalMinutes = sch.Attribute("IntervalMinutes").GetValue<int?>(null),
                    Playlist = sch.Element("Playlist") == null ?
                        new MediaPlaylistModel() : this.mpDataService.LoadXElement(sch.Element("Playlist"))
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
        public void SaveFile(string filePath, ScheduleModel model)
        {
            try
            {
                XElement xpl = this.SaveToXElement(model);
                //
                if (xpl != null)
                    this.SaveXFile(filePath, xpl);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public XElement SaveToXElement(ScheduleModel model)
        {
            try
            {
                XElement sch = new XElement("Schedule",
                    new XAttribute("Days", model.Days),
                    new XAttribute("StartTime", model.StartTime.HasValue ? model.StartTime.ToString() : ""),
                    new XAttribute("StartDate", model.StartDate.HasValue ? model.StartDate.ToString() : ""),
                    new XAttribute("IntervalMinutes", model.IntervalMinutes.HasValue ? model.IntervalMinutes.ToString() : ""),
                    new XAttribute("Enabled", model.Enabled),
                    new XAttribute("EveryDay", model.EveryDay),
                    this.mpDataService.SaveToXElement(model.Playlist));
                //
                return sch;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        public List<ScheduleModel> LoadSchedulesFromXElement(XElement schs)
        {
            try
            {
                var schedules = new List<ScheduleModel>
                (
                    from sc in schs.Elements("Schedule")
                         select this.LoadXElement(sc)
                );
                //
                return schedules;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public XElement SaveSchedulesToXElement(List<ScheduleModel> schs)
        {
            try
            {
                XElement element = new XElement("Schedulings",
                    from sc in schs
                    select this.SaveToXElement(sc));
                //
                return element;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
    }
}
