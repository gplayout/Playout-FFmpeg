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
    public interface IFFMpegDataService : IXMLDataService
    {
        FFMpegModel LoadSettings();
        bool SaveProfiles(FFMpegModel model);
    }
    public class FFMpegDataService : XMLDataService, IFFMpegDataService
    {
        readonly string FFMpegFilePath = Program.AppPath + @"\Data\FFMpegSettings.xml";
        public FFMpegDataService()
        {
        }
        public FFMpegModel LoadSettings()
        {
            try
            {
                XElement root = this.LoadXFile(this.FFMpegFilePath);
                FFMpegModel model = new FFMpegModel();
                //
                model.Profiles = (from profileXl in root.Element("StreamSettings").Element("Profiles").Elements("Profile")
                                  select new FFMpegProfileModel()
                                  {
                                      Name = profileXl.Attribute("Name").GetValue<string>(""),
                                      Args = profileXl.Value
                                  }).ToList();
                model.RecordArgs = root.Element("StreamSettings").Element("Record") == null ? "" : root.Element("StreamSettings").Element("Record").Value;
                //
                return model;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public bool SaveProfiles(FFMpegModel model)
        {
            try
            {
                if (model==null || model.Profiles==null)
                    return false;
                //
                XElement streamSett = new XElement("StreamSettings",
                    new XElement("Profiles",
                    from pf in model.Profiles
                    select new XElement("Profile",
                        new XAttribute("Name", pf.Name), pf.Args))
                    ,
                    new XElement("Record", model.RecordArgs ?? "")
                    );
                //
                bool result = this.SaveXElement(this.FFMpegFilePath, streamSett);
                return result;
            }
            catch(Exception ex)
            {
                ex.Log();
                return false;
            }
        }
    }
}
