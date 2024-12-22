using Playout.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Base;
using Playout.Log;

namespace Playout.DataService
{
    public interface ISettingsDataService : IXMLDataService
    {
        SettingsModel LoadSettings();
        void SaveSettings(SettingsModel model);
    }

    public class SettingsDataService : XMLDataService, ISettingsDataService
    {
        readonly string SettingsDirPath = Program.AppPath + @"\Data";
        readonly string SettingsFilePath = Program.AppPath + @"\Data\Settings.xml";

        IScheduleDataService schDataService;
        IMediaPlaylistDataService mpDataService;
        IOutputSettingDataService osDataService;
        IGlobalSettingDataService gsDataService;
        IPreviewDataService prevDataService;
        IRecordDataService recDataService;
        IStreamDataService streamDataService;
       
        public SettingsDataService(IScheduleDataService _schDataService,IMediaPlaylistDataService _mpDataService,
            IOutputSettingDataService _osDataSetvice,IGlobalSettingDataService _gsDataService,
            IPreviewDataService _prevDataService,IRecordDataService _recDataService,IStreamDataService _streamDataService)
        {
            this.schDataService = _schDataService;
            this.mpDataService = _mpDataService;
            this.osDataService = _osDataSetvice;
            this.gsDataService = _gsDataService;
            this.prevDataService = _prevDataService;
            this.recDataService = _recDataService;
            this.streamDataService = _streamDataService;
        }

        public SettingsModel LoadSettings()
        {
            try
            {
                SettingsModel model = new SettingsModel()
                {
                    Playlists = new List<MediaPlaylistModel>(),
                    Schedules = new List<ScheduleModel>(),
                    OutputSetting = new OutputSettingModel(),
                    GlobalSetting = new GlobalSettingModel(),
                    PreviewItems = new PreviewItemsModel(),
                    Record = new RecordModel(),
                    Stream = new StreamModel()
                };
                //
                if (!Directory.Exists(SettingsDirPath))
                    Directory.CreateDirectory(SettingsDirPath);
                //
                XElement root = this.LoadXFile(this.SettingsFilePath);
                //
                if (root.Element("Playlists") != null && root.Element("Playlists").Elements("Playlist") != null)
                {
                    model.Playlists = this.mpDataService.LoadPlaylistsFromXElement(root.Element("Playlists")); ;
                }
                //
                if (root.Element("Schedulings") != null)
                {
                    model.Schedules = this.schDataService.LoadSchedulesFromXElement(root.Element("Schedulings"));
                }
                //
                if (root.Element("OutputSetting") != null)
                {
                    model.OutputSetting = this.osDataService.LoadXElement(root.Element("OutputSetting"));
                }
                //
                if (root.Element("GlobalSetting") != null)
                {
                    model.GlobalSetting = this.gsDataService.LoadXElement(root.Element("GlobalSetting"));
                }
                //
                if (root.Element("PreviewItems") != null)
                {
                    model.PreviewItems = this.prevDataService.LoadItemsFromXElement(root.Element("PreviewItems"));
                }
                //
                if (root.Element("Record") != null)
                {
                    model.Record = this.recDataService.LoadXElement(root.Element("Record"));
                }
                //
                if (root.Element("Stream") != null)
                {
                    model.Stream = this.streamDataService.LoadXElement(root.Element("Stream"));
                }
                //
                return model;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        public void SaveSettings(SettingsModel model)
        {
            try
            {
                if (!Directory.Exists(SettingsDirPath))
                    Directory.CreateDirectory(SettingsDirPath);
                //
                XElement root = new XElement("Settings",
                    this.mpDataService.SavePlaylistsToXElement(model.Playlists),
                    this.schDataService.SaveSchedulesToXElement(model.Schedules),
                    this.osDataService.SaveToXElement(model.OutputSetting),
                    this.gsDataService.SaveToXElement(model.GlobalSetting),
                    this.prevDataService.SaveItemsToXElement(model.PreviewItems),
                    this.recDataService.SaveToXElement(model.Record),
                    this.streamDataService.SaveToXElement(model.Stream));
                //
                root.Save(this.SettingsFilePath);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
    }
}
