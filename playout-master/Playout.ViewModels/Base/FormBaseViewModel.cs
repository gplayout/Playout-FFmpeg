using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.ViewModels.Base
{
    public class FormBaseViewModel : ValidationViewModelBase
    {
        private bool isValid;

        public bool IsValid
        {
            get
            {
                return this.isValid;
            }
            protected set
            {
                this.isValid = value;
                this.NotifyOfPropertyChange(() => this.IsValid);
            }
        }

        public override void NotifyOfPropertyChange(string propertyName = "")
        {
            base.NotifyOfPropertyChange(propertyName);
            //
            // test prevent infinite loop while settings IsValid 
            // (which causes an PropertyChanged to be raised)
            if (propertyName != "IsValid")
            {
                // update the isValid status
                if (string.IsNullOrEmpty(this.Error) && this.ValidPropertiesCount == this.TotalPropertiesWithValidationCount)
                {
                    this.IsValid = true;
                }
                else
                {
                    this.IsValid = false;
                }
            }
        }
    }
}
