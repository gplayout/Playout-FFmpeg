using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Playout.Log;

namespace Playout.Base
{
    public static class CategoryWorker
    {
        static readonly string CategoryDirPath = Program.AppPath + @"\Resources\CategoryIcons";

        public static Dictionary<string, ImageSource> Categories { get; private set; }

        static CategoryWorker()
        {
            LoadCategories();
        }
        //
        static void LoadCategories()
        {
            try
            {
                if (!Directory.Exists(CategoryDirPath))
                    Directory.CreateDirectory(CategoryDirPath);
                //
                Categories = new Dictionary<string, ImageSource>();
                Categories.Add("", null);
                //
                foreach(string file in Directory.GetFiles(CategoryDirPath))
                {
                    string ext = System.IO.Path.GetExtension(file);
                    string filename = System.IO.Path.GetFileNameWithoutExtension(file);
                    //
                    if (!Program.OPEN_PICTURE_FILES.Contains(ext))
                        continue;
                    //
                    BitmapSource bitmapSource = new BitmapImage(new Uri(file));
                    if (Categories.ContainsKey(filename))
                        continue;
                    //
                    Categories.Add(filename, bitmapSource);
                }
                //
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

    }
}
