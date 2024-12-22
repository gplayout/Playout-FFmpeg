namespace Framework.UI
{
    using System.Windows;
    using System.Windows.Media;
    using Elysium;

    public class ElysiumApplication : Application
    {
        #region Fields

        private Theme theme = Theme.Light;

        #endregion

        #region Constructors

        public ElysiumApplication()
        {
            ThemeManager.AddExtraResourceDictionary(this.Resources);
        }

        #endregion

        #region Public Properties

        public SolidColorBrush AccentBrush
        {
            get { return (SolidColorBrush)this.Resources["AccentBrush"]; }
            set { ThemeManager.UpdateElysiumResourceDictionaries(this.Resources, "AccentBrush", value); }
        }

        public SolidColorBrush ContrastBrush
        {
            get 
            { 
                return (SolidColorBrush)this.Resources["ContrastBrush"]; 
            }

            set 
            { 
                ThemeManager.UpdateElysiumResourceDictionaries(this.Resources, "ContrastBrush", value);
                SolidColorBrush semitransparentContrastBrush = value.Clone();
                semitransparentContrastBrush.Opacity = 0.125;
                ThemeManager.UpdateElysiumResourceDictionaries(this.Resources, "SemitransparentContrastBrush", value);
            }
        }

        public Theme Theme
        {
            get
            {
                return this.theme;
            }

            set
            {
                if (this.theme != value)
                {
                    this.theme = value;
                    ThemeManager.UpdateElysiumResourceDictionaries(this.Resources, this.theme);
                }
            }
        }

        #endregion
    }
}
