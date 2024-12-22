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
    public interface IPreviewDataService : IXMLDataService
    {
        PreviewItemsModel LoadItemsFromXElement(XElement node);
        XElement SaveItemsToXElement(PreviewItemsModel pim);
    }

    public class PreviewDataService : XMLDataService, IPreviewDataService
    {
        IMediaPlaylistDataService mpDataService;

        public PreviewDataService(IMediaPlaylistDataService _mpDataService)
        {
            this.mpDataService = _mpDataService;
        }

        public PreviewItemsModel LoadItemsFromXElement(XElement node)
        {
            try
            {
                MediaPlaylistModel mpm = this.mpDataService.LoadXElement(node.Element("Playlist"));
                if (mpm == null)
                    return null;
                else
                    return new PreviewItemsModel() { Items = mpm.MediaSources };
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
        public XElement SaveItemsToXElement(PreviewItemsModel pim)
        {
            try
            {
                MediaPlaylistModel mpm = new MediaPlaylistModel() { MediaSources = pim.Items };
                //
                XElement element = new XElement("PreviewItems", this.mpDataService.SaveToXElement(mpm));
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
