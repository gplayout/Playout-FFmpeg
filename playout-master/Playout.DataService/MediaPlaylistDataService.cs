using Playout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Base;
using Playout.MediaFramework;
using Playout.Log;
using Playout.DirectShow.MediaPlayers;

namespace Playout.DataService
{
    public interface IMediaPlaylistDataService : IXMLDataService
    {
        MediaPlaylistModel LoadFile(string filePath);
        MediaPlaylistModel LoadXElement(XElement pl);
        void SaveFile(string filePath, MediaPlaylistModel model);
        XElement SaveToXElement(MediaPlaylistModel model);
        //
        List<MediaPlaylistModel> LoadPlaylistsFromXElement(XElement pls);
        XElement SavePlaylistsToXElement(List<MediaPlaylistModel> pls);
    }
    public class MediaPlaylistDataService : XMLDataService, IMediaPlaylistDataService
    {
        public MediaPlaylistModel LoadFile(string filePath)
        {
            try
            {
                XElement root = this.LoadXFile(filePath);
                MediaPlaylistModel model = new MediaPlaylistModel();
                //
                string ext = System.IO.Path.GetExtension(filePath);
                if (ext == Program.PLAYLIST_Ext)
                {
                    model = LoadXElement(root);
                }
                else if (ext == Program.SCHEDULE_Ext)
                {
                    model = this.LoadXElement(root.Element("Playlist"));
                }
                else
                    throw new Exception("Could not open this file format " + ext);
                /////////////////////////////////////////////////////
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

        public MediaPlaylistModel LoadXElement(XElement pl)
        {
            try
            {
                MediaPlaylistModel model = new MediaPlaylistModel();
                model.Name = pl.Attribute("Name").GetValue<string>("Unnamed Playlist");
                model.Loop = pl.Attribute("Loop").GetValue<bool>(false);
                model.Color = pl.Attribute("Color").GetValue<int>(0);
                //
                if (pl.Element("Files") != null && pl.Element("Files").Elements("MediaSource") != null)
                {
                    model.MediaSources = new List<MediaSourceModel>(
                        (from file in pl.Element("Files").Elements("MediaSource")
                         let strov = file.Element("StringOverlay")
                         let crwov = file.Element("CrawlOverlay")
                         let imgov = file.Element("ImageOverlay")
                         let vidov = file.Element("VideoOverlay")
                         let nestpl = file.Element("NestedPlaylist")
                         let nestplpl = nestpl == null ? null : nestpl.Element("Playlist")
                         select new MediaSourceModel()
                         {
                             Category = file.Attribute("Category").GetValue<string>(""),
                             AliasName = file.Attribute("AliasName").GetValue<string>(""),
                             MediaType = file.Attribute("MediaType").GetEnumValue<enMediaType>(enMediaType.VideoFile),
                             ShowupEffect = file.Attribute("ShowupEffect").GetEnumValue<TransitionEffectType>(TransitionEffectType.None),
                             File_Url = file.Attribute("File_Url").GetValue<string>(""),
                             InputDeviceFormat = file.Attribute("InputDeviceFormat").GetValue<string>(""),
                             DeviceName = file.Attribute("DeviceName").GetValue<string>(""),
                             TrimStartSecond = file.Attribute("TrimStartSecond").GetValue<long>(0),
                             TrimStopSecond = file.Attribute("TrimStopSecond").GetValue<long>(0),
                             ThumbnailId = file.Attribute("ThumbnailId").GetGuid(Guid.Empty),
                             Loop = file.Attribute("Loop").GetValue<bool>(false),
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
                                    BackColor = strov.Attribute("BackColor").GetValue<int>(0),
                                    Shadow = strov.Attribute("Shadow").GetValue<bool>(false),
                                    ShadowColor = strov.Attribute("ShadowColor").GetValue<int>(0),
                                    TextAlign = strov.Attribute("TextAlign").GetValue<string>(""),
                                    Transparency = strov.Attribute("Transparency").GetValue<bool>(false),
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
                                     ChromaKey = imgov.Attribute("ChromaKey").GetValue<bool>(false),
                                     ChromaColor = imgov.Attribute("ChromaColor").GetValue<int>(0),
                                     ChromaLeeway = imgov.Attribute("ChromaLeeway").GetValue<int>(100),
                                     Width = imgov.Attribute("Width").GetValue<int>(120),
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
                                    BackColor = crwov.Attribute("BackColor").GetValue<int>(0),
                                    Shadow = crwov.Attribute("Shadow").GetValue<bool>(false),
                                    ShadowColor = crwov.Attribute("ShadowColor").GetValue<int>(0),
                                    TextAlign = crwov.Attribute("TextAlign").GetValue<string>(""),
                                    Transparency = crwov.Attribute("Transparency").GetValue<bool>(false),
                                    Scrolling = crwov.Attribute("Scrolling").GetValue<bool>(false),
                                    ScrollingSpeed = crwov.Attribute("ScrollingSpeed").GetValue<int>(1),
                                    FilePath = crwov.Attribute("FilePath").GetValue<string>(""),
                                    ReadFromFile = crwov.Attribute("ReadFromFile").GetValue<bool>(false),
                                    Direction = (CrawlDirection)Enum.Parse(typeof(CrawlDirection), crwov.Attribute("Direction").GetValue<string>(CrawlDirection.RightToLeft.ToString())),
                                }),
                             VideoOverlay = (vidov == null ? new VideoOverlayMediaModel() :
                                 new VideoOverlayMediaModel()
                                 {
                                     Enabled = vidov.Attribute("Enabled").GetValue<bool>(false),
                                     FilePath = vidov.Attribute("FilePath").GetValue<string>(""),
                                     Height = vidov.Attribute("Height").GetValue<int>(120),
                                     PositionLeft = vidov.Attribute("PositionLeft").GetValue<int>(0),
                                     PositionTop = vidov.Attribute("PositionTop").GetValue<int>(0),
                                     Width = vidov.Attribute("Width").GetValue<int>(120),
                                 }),
                             NestedPlaylist = (nestplpl == null ? null : this.LoadXElement(nestplpl)),                             
                         }).ToArray());
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

        public void SaveFile(string filePath, MediaPlaylistModel model)
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

        public XElement SaveToXElement(MediaPlaylistModel model)
        {
            try
            {
                XElement xpl = new XElement("Playlist");
                //
                xpl.SetAttributeValue("Name", model.Name);
                xpl.SetAttributeValue("Color", model.Color);
                xpl.SetAttributeValue("Loop", model.Loop);
                //
                if (model.MediaSources != null)
                {
                    xpl.Add(new XElement("Files",
                        from item in model.MediaSources
                        select new XElement("MediaSource",
                            new XAttribute("Category", item.Category ?? ""),
                            new XAttribute("AliasName", item.AliasName ?? ""),
                            new XAttribute("MediaType", item.MediaType),
                            new XAttribute("ShowupEffect", item.ShowupEffect),
                            new XAttribute("File_Url", item.File_Url ?? ""),
                            new XAttribute("InputDeviceFormat", item.InputDeviceFormat ?? ""),
                            new XAttribute("DeviceName", item.DeviceName),
                            new XAttribute("TrimStartSecond", item.TrimStartSecond),
                            new XAttribute("TrimStopSecond", item.TrimStopSecond),
                            new XAttribute("ThumbnailId", item.ThumbnailId),
                            new XAttribute("Loop", item.Loop),
                            new XElement("StringOverlay",
                                new XAttribute("Color", item.StringOverlay.Color),
                                new XAttribute("Enabled", item.StringOverlay.Enabled),
                                new XAttribute("FontName", item.StringOverlay.FontName ?? ""),
                                new XAttribute("Text", item.StringOverlay.Text ?? ""),
                                new XAttribute("FontSize", item.StringOverlay.FontSize),
                                new XAttribute("PositionLeft", item.StringOverlay.PositionLeft),
                                new XAttribute("PositionTop", item.StringOverlay.PositionTop),
                                new XAttribute("BackColor", item.StringOverlay.BackColor),
                                new XAttribute("Shadow", item.StringOverlay.Shadow),
                                new XAttribute("ShadowColor", item.StringOverlay.ShadowColor),
                                new XAttribute("TextAlign", item.StringOverlay.TextAlign ?? ""),
                                new XAttribute("Transparency", item.StringOverlay.Transparency)
                                ),
                            new XElement("ImageOverlay",
                                new XAttribute("AlphaBlending", item.ImageOverlay.AlphaBlending),
                                new XAttribute("Enabled", item.ImageOverlay.Enabled),
                                new XAttribute("FilePath", item.ImageOverlay.FilePath ?? ""),
                                new XAttribute("Height", item.ImageOverlay.Height),
                                new XAttribute("PositionLeft", item.ImageOverlay.PositionLeft),
                                new XAttribute("PositionTop", item.ImageOverlay.PositionTop),
                                new XAttribute("ChromaKey", item.ImageOverlay.ChromaKey),
                                new XAttribute("ChromaColor", item.ImageOverlay.ChromaColor),
                                new XAttribute("ChromaLeeway", item.ImageOverlay.ChromaLeeway),
                                new XAttribute("Width", item.ImageOverlay.Width)),
                            new XElement("CrawlOverlay",
                                new XAttribute("Color", item.CrawlOverlay.Color),
                                new XAttribute("Enabled", item.CrawlOverlay.Enabled),
                                new XAttribute("FontName", item.CrawlOverlay.FontName ?? ""),
                                new XAttribute("Text", item.CrawlOverlay.Text ?? ""),
                                new XAttribute("FontSize", item.CrawlOverlay.FontSize),
                                new XAttribute("PositionLeft", item.CrawlOverlay.PositionLeft),
                                new XAttribute("PositionTop", item.CrawlOverlay.PositionTop),
                                new XAttribute("Transparency", item.CrawlOverlay.Transparency),
                                new XAttribute("Shadow", item.CrawlOverlay.Shadow),
                                new XAttribute("ShadowColor", item.CrawlOverlay.ShadowColor),
                                new XAttribute("TextAlign", item.CrawlOverlay.TextAlign ?? ""),
                                new XAttribute("BackColor", item.CrawlOverlay.BackColor),
                                new XAttribute("Scrolling", item.CrawlOverlay.Scrolling),
                                new XAttribute("ScrollingSpeed", item.CrawlOverlay.ScrollingSpeed),
                                new XAttribute("FilePath", item.CrawlOverlay.FilePath ?? ""),
                                new XAttribute("ReadFromFile", item.CrawlOverlay.ReadFromFile),
                                new XAttribute("Direction", item.CrawlOverlay.Direction)),
                            new XElement("VideoOverlay",
                                new XAttribute("Enabled", item.VideoOverlay.Enabled),
                                new XAttribute("FilePath", item.VideoOverlay.FilePath ?? ""),
                                new XAttribute("Height", item.VideoOverlay.Height),
                                new XAttribute("PositionLeft", item.VideoOverlay.PositionLeft),
                                new XAttribute("PositionTop", item.VideoOverlay.PositionTop),
                                new XAttribute("Width", item.VideoOverlay.Width)),
                            new XElement("NestedPlaylist", item.NestedPlaylist == null ? new XElement("Playout") : this.SaveToXElement(item.NestedPlaylist))
                            )));
                }
                return xpl;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        public List<MediaPlaylistModel> LoadPlaylistsFromXElement(XElement pls)
        {
            try
            {
                var playlists = new List<MediaPlaylistModel>
                    (from pl in pls.Elements("Playlist")
                     select this.LoadXElement(pl)
                 );
                return playlists;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        public XElement SavePlaylistsToXElement(List<MediaPlaylistModel> pls)
        {
            try
            {
                XElement element = new XElement("Playlists",
                    from pl in pls
                    select this.SaveToXElement(pl));
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
