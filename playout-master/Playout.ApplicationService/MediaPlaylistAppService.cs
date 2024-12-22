using Playout.Base;
using Playout.DataService;
using Playout.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Log;

namespace Playout.ApplicationService
{
    public interface IMediaPlaylistAppService
    {
        MediaPlaylistModel LoadFile(string filePath);
        MediaPlaylistModel LoadXElement(XElement pl);
        void SaveFile(MediaPlaylistModel model);
        MediaPlaylistModel OpenFile();
        XElement SaveToXElement(MediaPlaylistModel model);
    }

    public class MediaPlaylistAppService : IMediaPlaylistAppService
    {
        IMediaPlaylistDataService mpDataService;
        public MediaPlaylistAppService(IMediaPlaylistDataService _mpDataService)
        {
            this.mpDataService = _mpDataService;
        }

        public MediaPlaylistModel LoadFile(string filePath)
        {
            try
            {
                MediaPlaylistModel model = this.mpDataService.LoadFile(filePath);
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
                MediaPlaylistModel model = this.mpDataService.LoadXElement(pl);
                return model;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        public void SaveFile(MediaPlaylistModel model)
        {
            string filePath = Functions.ShowSaveFileDialog(model.Name + Program.PLAYLIST_Ext, Program.PLAYLIST_FILES, Program.DefaultDir_Playlists);
            //
            if (!String.IsNullOrEmpty(filePath))
            {
                this.mpDataService.SaveFile(filePath, model);
            }
        }

        public MediaPlaylistModel OpenFile()
        {
            string filePath = Functions.ShowOpenFileDialog("Playlist" + Program.PLAYLIST_Ext, Program.PLAYLISTAndSCHEDULE_FILES, Program.DefaultDir_Playlists);
            //
            if (!String.IsNullOrEmpty(filePath))
            {
                var model = this.LoadFile(filePath);
                return model;
            }
            else
                return null;
        }

        public XElement SaveToXElement(MediaPlaylistModel model)
        {
            try
            {
                XElement xpl = this.mpDataService.SaveToXElement(model);
                return xpl;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
    }
}
