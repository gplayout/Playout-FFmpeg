using Framework.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Bitmap = System.Drawing.Bitmap;

namespace Playout.Base
{
    public static class Functions
    {
        public async static Task ShowMessage(string title, string message)
        {
            var msg = await MessageDialog.ShowAsync(
                title, message, System.Windows.MessageBoxButton.OK
               , MessageDialogType.Light);
        }
        public async static Task<bool> ConfirmationForDelete(string title = "Delete Item", string message = "Are you sure to delete selected item(s)?")
        {
            var msgBox = await MessageDialog.ShowAsync(title, message, System.Windows.MessageBoxButton.OKCancel, MessageDialogType.Light);
            //
            return msgBox == System.Windows.MessageBoxResult.OK;
        }
        public static void ShowMessageErrorClassic(string title, string message)
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        public static void ShowMessageInfoClassic(string title, string message)
        {
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        public static string ShowSaveFileDialog(string fileName,string filter,string defDir)
        {
            System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.FileName = fileName;
            sfd.Filter = filter;
            sfd.FilterIndex = 1;
            //sfd.RestoreDirectory = true;
            sfd.InitialDirectory = defDir;

            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return sfd.FileName;
            }
            else
                return null;
        }
        public static string ShowOpenFileDialog(string fileName,string filter,string defDir)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.FileName = fileName;
            ofd.Filter = filter;
            ofd.FilterIndex = 1;
            //ofd.RestoreDirectory = true;
            ofd.InitialDirectory = defDir;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return ofd.FileName;
            }
            else
                return null;
        }
        public static string ShowOpenFolderDialog(string defDir)
        {
            System.Windows.Forms.FolderBrowserDialog ofd = new System.Windows.Forms.FolderBrowserDialog();
            ofd.SelectedPath = defDir;

            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return ofd.SelectedPath;
            }
            else
                return null;
        }

        public static int ConvertColorToInt(System.Windows.Media.Color wcolor)
        {
            System.Drawing.Color col = System.Drawing.Color.FromArgb(wcolor.A, wcolor.R, wcolor.G, wcolor.B);
            int code = System.Drawing.ColorTranslator.ToWin32(col);
            return code;
        }
        public static int ConvertColorToInt(System.Drawing.Color wcolor)
        {
            System.Drawing.Color col = System.Drawing.Color.FromArgb(wcolor.A, wcolor.R, wcolor.G, wcolor.B);
            int code = System.Drawing.ColorTranslator.ToWin32(col);
            return code;
        }
        public static System.Windows.Media.Color ConvertIntToColor(int code)
        {
            try
            {
                System.Drawing.Color col = System.Drawing.ColorTranslator.FromWin32(code);
                System.Windows.Media.Color wcolor = System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B); ;
                return wcolor;
            }
            catch
            {
                return System.Windows.Media.Colors.Black;
            }
        }
        public static System.Drawing.Color ConvertIntToDColor(int code)
        {
            try
            {
                System.Drawing.Color col = System.Drawing.ColorTranslator.FromWin32(code);
                return col;
            }
            catch
            {
                return System.Drawing.Color.Black;
            }
        }

        public static System.Drawing.Color ConvertWColorToDColor(System.Windows.Media.Color wcolor)
        {
            System.Drawing.Color col = System.Drawing.Color.FromArgb(wcolor.A, wcolor.R, wcolor.G, wcolor.B);
            return col;
        }
        public static int GetRandomColorCode()
        {
            Random randomGen = new Random();
            System.Drawing.KnownColor[] names = (System.Drawing.KnownColor[])Enum.GetValues(typeof(System.Drawing.KnownColor));
            System.Drawing.KnownColor randomColorName = names[randomGen.Next(names.Length)];
            System.Drawing.Color randomColor = System.Drawing.Color.FromKnownColor(randomColorName);
            return randomColor.ToArgb();
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static System.Windows.Media.Imaging.BitmapSource ConvertToBitmapSource(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = System.Windows.Media.Imaging.BitmapSource.Create(
                bitmapData.Width, bitmapData.Height, 96, 96, System.Windows.Media.PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        public static float[] Stream_GetFrameRates()
        {
            return new float[] { 24, 25, 29.97f, 30 };
        }
        public static int[] Stream_GetSampleRates()
        {
            return new int[] { 22050, 44100, 48000 };
        }
        public static string[] Stream_GetFileFormats()
        {
            return new string[] { "mp4", "mkv", "flv" };
        }
        public static string[] Stream_GetVideoBitrates()
        {
            return new string[] {
                "",
                "50M",
                "25M",
                "15M",
                "10M",
                "8M",
                "6M",
                "4M",
                "2M",
                "1500k" };
        }
        public static string[] Stream_GetAudioBitrates()
        {
            return new string[] {
                "",
              "384k",
              "192k",
              "128k",
              "96k",
              "64k" };
        }
        public static string[] Stream_GetFrameSizes()
        {
            return new string[]
                {
                    "320x240",
                    "720x480",
                    "720x576",
                    "1280x720",
                    "1920x1080"
                };
        }

        public static string GetPlayoutVersion()
        {
            return String.Format("{0}.{1}",
                Assembly.GetEntryAssembly().GetName().Version.Major,
                Assembly.GetEntryAssembly().GetName().Version.Minor);
        }
    }
}
