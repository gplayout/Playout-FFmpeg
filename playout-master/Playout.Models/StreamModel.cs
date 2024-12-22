using Playout.Base;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Playout.Models
{
    public class StreamModel
    {
        public List<StreamInstanceModel> Profiles { get; set; }
        public string SelectedProfileName { get; set; }
        public StreamModel()
        {
            this.Profiles = new List<StreamInstanceModel>();
        }
    }
    public class StreamInstanceModel:RecordModel
    {
        public string Url { get; set; }
        public string ProfileName{get;set;}

        public StreamInstanceModel()
        {

        }
    }
}
