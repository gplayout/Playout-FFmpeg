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
    public interface IGlobalSettingDataService : IXMLDataService
    {
        GlobalSettingModel LoadXElement(XElement sett);
        XElement SaveToXElement(GlobalSettingModel model);
    }

    public class GlobalSettingDataService : XMLDataService, IGlobalSettingDataService
    {
        public GlobalSettingDataService()
        {
        }
        public GlobalSettingModel LoadXElement(XElement sett)
        {
            try
            {
                var strov = sett.Element("StringOverlay");
                var imgov = sett.Element("ImageOverlay");
                var crwov = sett.Element("CrawlOverlay");
                GlobalSettingModel model = new GlobalSettingModel()
                {
                    LoopOnPlaylists = sett.Attribute("LoopOnPlaylists").GetValue<bool>(false),
                    PlaybackMode = sett.Attribute("PlaybackMode").GetValue<bool>(false),
                    StringOverlay = (strov == null ? new StringOverlayMediaModel() :
                                new StringOverlayMediaModel()
                                {
                                    Color = strov.Attribute("Color").GetValue<int>(0),
                                    Enabled = strov.Attribute("Enabled").GetValue<bool>(false),
                                    FontName = strov.Attribute("FontName").GetValue<string>("Arial"),
                                    Text = strov.Attribute("Text").GetValue<string>(""),
                                    FontSize = strov.Attribute("FontSize").GetValue<int>(12),
                                    PositionLeft = strov.Attribute("PositionLeft").GetValue<int>(0),
                                    PositionTop = strov.Attribute("PositionTop").GetValue<int>(0),
                                    Transparency = strov.Attribute("Transparency").GetValue<bool>(false),
                                    TextAlign = strov.Attribute("TextAlign").GetValue<string>(""),
                                    Shadow = strov.Attribute("Shadow").GetValue<bool>(false),
                                    ShadowColor = strov.Attribute("ShadowColor").GetValue<int>(0),
                                    BackColor = strov.Attribute("BackColor").GetValue<int>(0),
                                }),
                    ImageOverlay = (imgov == null ? new ImageOverlayMediaModel() :
                        new ImageOverlayMediaModel()
                        {
                            AlphaBlending = imgov.Attribute("AlphaBlending").GetValue<int>(255),
                            Enabled = imgov.Attribute("Enabled").GetValue<bool>(false),
                            FilePath = imgov.Attribute("FilePath").GetValue<string>(""),
                            Height = imgov.Attribute("Height").GetValue<int>(120),
                            PositionLeft = imgov.Attribute("PositionLeft").GetValue<int>(0),
                            PositionTop = imgov.Attribute("PositionTop").GetValue<int>(0),
                            Width = imgov.Attribute("Width").GetValue<int>(120),
                            ChromaKey = imgov.Attribute("ChromaKey").GetValue<bool>(false),
                            ChromaColor = imgov.Attribute("ChromaColor").GetValue<int>(0),
                            ChromaLeeway = imgov.Attribute("ChromaLeeway").GetValue<int>(0),
                        }),
                    CrawlOverlay = (crwov == null ? new CrawlOverlayMediaModel() :
                            new CrawlOverlayMediaModel()
                            {
                                Color = crwov.Attribute("Color").GetValue<int>(0),
                                Enabled = crwov.Attribute("Enabled").GetValue<bool>(false),
                                FontName = crwov.Attribute("FontName").GetValue<string>("Arial"),
                                Text = crwov.Attribute("Text").GetValue<string>(""),
                                FontSize = crwov.Attribute("FontSize").GetValue<int>(12),
                                PositionLeft = crwov.Attribute("PositionLeft").GetValue<int>(0),
                                PositionTop = crwov.Attribute("PositionTop").GetValue<int>(0),
                                Transparency = crwov.Attribute("Transparency").GetValue<bool>(false),
                                TextAlign = crwov.Attribute("TextAlign").GetValue<string>(""),
                                Shadow = crwov.Attribute("Shadow").GetValue<bool>(false),
                                ShadowColor = crwov.Attribute("ShadowColor").GetValue<int>(0),
                                BackColor = crwov.Attribute("BackColor").GetValue<int>(0),
                                Scrolling = crwov.Attribute("Scrolling").GetValue<bool>(false),
                                ScrollingSpeed = crwov.Attribute("ScrollingSpeed").GetValue<int>(1),
                                FilePath = crwov.Attribute("FilePath").GetValue<string>(""),
                                ReadFromFile = crwov.Attribute("ReadFromFile").GetValue<bool>(false),
                                Direction = (CrawlDirection)Enum.Parse(typeof(CrawlDirection), crwov.Attribute("Direction").GetValue<string>(CrawlDirection.RightToLeft.ToString())),
                            }),
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
        public XElement SaveToXElement(GlobalSettingModel model)
        {
            try
            {
                XElement sett = new XElement("GlobalSetting",
                    new XAttribute("LoopOnPlaylists", model.LoopOnPlaylists),
                    new XAttribute("PlaybackMode", model.PlaybackMode),
                    new XElement("StringOverlay",
                        new XAttribute("Color", model.StringOverlay.Color),
                        new XAttribute("Enabled", model.StringOverlay.Enabled),
                        new XAttribute("FontName", model.StringOverlay.FontName ?? ""),
                        new XAttribute("Text", model.StringOverlay.Text ?? ""),
                        new XAttribute("FontSize", model.StringOverlay.FontSize),
                        new XAttribute("PositionLeft", model.StringOverlay.PositionLeft),
                        new XAttribute("PositionTop", model.StringOverlay.PositionTop),
                        new XAttribute("Transparency", model.StringOverlay.Transparency),
                        new XAttribute("Shadow", model.StringOverlay.Shadow),
                        new XAttribute("ShadowColor", model.StringOverlay.ShadowColor),
                        new XAttribute("TextAlign", model.StringOverlay.TextAlign ?? ""),
                        new XAttribute("BackColor", model.StringOverlay.BackColor)),
                    new XElement("ImageOverlay",
                        new XAttribute("AlphaBlending", model.ImageOverlay.AlphaBlending),
                        new XAttribute("Enabled", model.ImageOverlay.Enabled),
                        new XAttribute("FilePath", model.ImageOverlay.FilePath == null ? "" : model.ImageOverlay.FilePath),
                        new XAttribute("Height", model.ImageOverlay.Height),
                        new XAttribute("PositionLeft", model.ImageOverlay.PositionLeft),
                        new XAttribute("PositionTop", model.ImageOverlay.PositionTop),
                        new XAttribute("ChromaKey", model.ImageOverlay.ChromaKey),
                        new XAttribute("ChromaColor", model.ImageOverlay.ChromaColor),
                        new XAttribute("ChromaLeeway", model.ImageOverlay.ChromaLeeway),
                        new XAttribute("Width", model.ImageOverlay.Width)),
                    new XElement("CrawlOverlay",
                        new XAttribute("Color", model.CrawlOverlay.Color),
                        new XAttribute("Enabled", model.CrawlOverlay.Enabled),
                        new XAttribute("FontName", model.CrawlOverlay.FontName ?? ""),
                        new XAttribute("Text", model.CrawlOverlay.Text ?? ""),
                        new XAttribute("FontSize", model.CrawlOverlay.FontSize),
                        new XAttribute("PositionLeft", model.CrawlOverlay.PositionLeft),
                        new XAttribute("PositionTop", model.CrawlOverlay.PositionTop),
                        new XAttribute("Transparency", model.CrawlOverlay.Transparency),
                        new XAttribute("Shadow", model.CrawlOverlay.Shadow),
                        new XAttribute("ShadowColor", model.CrawlOverlay.ShadowColor),
                        new XAttribute("TextAlign", model.CrawlOverlay.TextAlign ?? ""),
                        new XAttribute("BackColor", model.CrawlOverlay.BackColor),
                        new XAttribute("Scrolling", model.CrawlOverlay.Scrolling),
                        new XAttribute("ScrollingSpeed", model.CrawlOverlay.ScrollingSpeed),
                        new XAttribute("FilePath", model.CrawlOverlay.FilePath??""),
                        new XAttribute("ReadFromFile", model.CrawlOverlay.ReadFromFile),
                        new XAttribute("Direction", model.CrawlOverlay.Direction)
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
