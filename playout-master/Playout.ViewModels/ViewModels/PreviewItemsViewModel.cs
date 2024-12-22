using Playout.ApplicationService;
using Playout.Base;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Models;
using Playout.Log;
using System.Timers;
using Playout.MediaFramework;
using Playout.DirectShow.MediaPlayers;

namespace Playout.ViewModels.ViewModels
{
    public class PreviewItemsViewModel : Base.FormBaseViewModel
    {
        ObservableCollection<MediaSourceViewModel> _items;

        public ObservableCollection<MediaSourceViewModel> Items
        {
            get
            { return this._items; }
            set
            {
                if (this._items != value)
                {
                    this._items = value;
                    this.NotifyOfPropertyChange(() => this.Items);
                }
            }
        }

        public PreviewItemsViewModel()
        {

        }

        public void Load(PreviewItemsModel model)
        {
            try
            {
                if (model == null)
                    return;
                if (model.Items == null)
                    model.Items = new List<MediaSourceModel>();
                //
                var pl = new MediaPlaylistViewModel(
                    new MediaPlaylistModel() { MediaSources = model.Items }
                    );
                this.Items = pl.MediaSourcesVM.Sources;
            }
            catch(Exception ex)
            {

            }
        }

        public PreviewItemsModel GetModel()
        {
            try
            {
                MediaPlaylistViewModel mp = new MediaPlaylistViewModel();
                mp.MediaSourcesVM.Sources = this.Items;
                var mpModel = mp.GetModel();
                if (mpModel == null)
                    return null;
                //

                PreviewItemsModel model = new PreviewItemsModel()
                {
                    Items = mpModel.MediaSources
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
    }
}
