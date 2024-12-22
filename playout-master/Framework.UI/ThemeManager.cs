namespace Framework.UI
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Media;
    using Elysium;
    using Framework;

    public static class ThemeManager
    {
        #region Dependency Properties

        public static readonly DependencyProperty ThemeProperty = DependencyProperty.RegisterAttached(
            "Theme", 
            typeof(Theme), 
            typeof(ThemeManager), 
            new PropertyMetadata(Theme.Light, OnThemePropertyChanged));

        #endregion

        #region Private Static Fields

        private static readonly Uri ElysiumExtraDictionaryUri = new Uri("/Framework.UI;component/Themes/Generic.xaml", UriKind.Relative);
        private static readonly Uri GenericDictionaryUri = new Uri("/Elysium;component/Themes/Generic.xaml", UriKind.Relative);
        private static readonly Uri BrushesDictionaryUri = new Uri("/Elysium;component/Themes/Brushes.xaml", UriKind.Relative);
        private static readonly Uri LightBrushesDictionaryUri = new Uri("/Elysium;component/Themes/LightBrushes.xaml", UriKind.Relative);
        private static readonly Uri DarkBrushesDictionaryUri = new Uri("/Elysium;component/Themes/DarkBrushes.xaml", UriKind.Relative); 

        #endregion

        #region Public Static Methods

        public static Theme GetTheme(DependencyObject obj)
        {
            return (Theme)obj.GetValue(ThemeProperty);
        }

        public static void SetTheme(DependencyObject obj, Theme value)
        {
            obj.SetValue(ThemeProperty, value);
        }

        #endregion

        #region Internal Static Methods

        internal static void AddExtraResourceDictionary(ResourceDictionary resourceDictionary)
        {
            if (!resourceDictionary.MergedDictionaries.Any(x => x.Source == ElysiumExtraDictionaryUri))
            {
                resourceDictionary.MergedDictionaries.Add(
                    new ResourceDictionary()
                    {
                        Source = ElysiumExtraDictionaryUri
                    });
            }
        }

        internal static void UpdateElysiumResourceDictionaries(ResourceDictionary resourceDictionary, Theme theme)
        {
            AddExtraResourceDictionary(resourceDictionary);

            ResourceDictionary newResourceDictionary = new ResourceDictionary()
                    {
                        Source = ElysiumExtraDictionaryUri
                    };
            foreach (ResourceDictionary genericResourceDictionary in newResourceDictionary.MergedDictionaries
                .Traverse(x => x.MergedDictionaries)
                .Where(x => x.Source == GenericDictionaryUri)
                .ToList())
            {
                UpdateGenericResourceDictionary(genericResourceDictionary, theme);
            }
            resourceDictionary.MergedDictionaries.Clear();
            resourceDictionary.MergedDictionaries.Add(newResourceDictionary);

            //foreach (ResourceDictionary genericResourceDictionary in resourceDictionary.MergedDictionaries
            //    .Traverse(x => x.MergedDictionaries)
            //    .Where(x => x.Source == GenericDictionaryUri)
            //    .ToList())
            //{
            //    UpdateGenericResourceDictionary(genericResourceDictionary, theme);
            //}
        }

        internal static void UpdateElysiumResourceDictionaries(ResourceDictionary resourceDictionary, string key, SolidColorBrush brush)
        {
            AddExtraResourceDictionary(resourceDictionary);

            foreach (ResourceDictionary brushesResourceDictionary in resourceDictionary.MergedDictionaries
                .Traverse(x => x.MergedDictionaries)
                .Where(x => x.Source == BrushesDictionaryUri)
                .ToList())
            {
                brushesResourceDictionary[key] = brush;
            }
        }

        #endregion

        #region Private Static Methods

        private static void OnThemePropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement frameworkElement = (FrameworkElement)dependencyObject;
            ThemeManager.UpdateElysiumResourceDictionaries(frameworkElement.Resources, GetTheme(frameworkElement));
        }

        private static void OnThemeChanged()
        {
            MethodInfo method = typeof(SystemColors).GetMethod("InvalidateCache", BindingFlags.NonPublic | BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(null, null);
            }
            MethodInfo info2 = typeof(SystemParameters).GetMethod("InvalidateCache", BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
            if (info2 != null)
            {
                info2.Invoke(null, null);
            }
            Assembly assembly = Assembly.GetAssembly(typeof(Window));
            if (assembly != null)
            {
                Type type = assembly.GetType("System.Windows.SystemResources");
                if (type != null)
                {
                    MethodInfo info3 = type.GetMethod("OnThemeChanged", BindingFlags.NonPublic | BindingFlags.Static);
                    if (info3 != null)
                    {
                        info3.Invoke(null, null);
                    }
                    MethodInfo info4 = type.GetMethod("InvalidateResources", BindingFlags.NonPublic | BindingFlags.Static);
                    if (info4 != null)
                    {
                        info4.Invoke(null, new object[] { false });
                    }
                }
            }
        }


        private static void UpdateGenericResourceDictionary(ResourceDictionary genericResourceDictionary, Theme theme)
        {
            genericResourceDictionary.BeginInit();

            if (theme == Theme.Dark)
            {
                if (!genericResourceDictionary.MergedDictionaries.Any(x => x.Source == DarkBrushesDictionaryUri))
                {
                    genericResourceDictionary.MergedDictionaries.Add(
                        new ResourceDictionary()
                        {
                            Source = DarkBrushesDictionaryUri
                        });
                }

                ResourceDictionary lightBrushesResourceDictionary = genericResourceDictionary.MergedDictionaries
                    .FirstOrDefault(x => x.Source == LightBrushesDictionaryUri);
                if (lightBrushesResourceDictionary != null)
                {
                    genericResourceDictionary.MergedDictionaries.Remove(lightBrushesResourceDictionary);
                }
            }
            else
            {
                if (!genericResourceDictionary.MergedDictionaries.Any(x => x.Source == LightBrushesDictionaryUri))
                {
                    genericResourceDictionary.MergedDictionaries.Add(
                        new ResourceDictionary()
                        {
                            Source = LightBrushesDictionaryUri
                        });
                }

                ResourceDictionary darkBrushesResourceDictionary = genericResourceDictionary.MergedDictionaries
                    .FirstOrDefault(x => x.Source == DarkBrushesDictionaryUri);
                if (darkBrushesResourceDictionary != null)
                {
                    genericResourceDictionary.MergedDictionaries.Remove(darkBrushesResourceDictionary);
                }
            }

            genericResourceDictionary.EndInit();
        }

        #endregion
    }
}
