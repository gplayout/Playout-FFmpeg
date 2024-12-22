using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Playout.Log;

namespace Playout.MediaFramework
{
    public static class DecklinkWrapper
    {
        static string[] _decklinkDevNames = null;
        static Dictionary<string, List<string>> _deckOutVideoFormats = null;
        const string VideoOuputDirectShowName = "Decklink Video Render";
        const string AudioOuputDirectShowName = "Decklink Audio Render";

        public static string[] DecklinkDeviceNames
        {
            get
            {
                if (_decklinkDevNames != null)
                    return _decklinkDevNames;
                //
                string devNames = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(DecklinkNative.GetDeviceNames());
                if (String.IsNullOrEmpty(devNames))
                    return null;
                else
                {
                    _decklinkDevNames = devNames.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(m => m != "-1" && m != "-2").ToArray();
                    for (int i = 1; i <= _decklinkDevNames.Length; i++)
                    {
                        _decklinkDevNames[i - 1] = i + "-" + _decklinkDevNames[i - 1];
                    }
                    return _decklinkDevNames;
                }
            }
        }
        public static string GetOutputDirectShowName(string devName, bool isVideo)
        {
            if (DecklinkDeviceNames == null)
                return devName;
            //
            int index = DecklinkDeviceNames.ToList().IndexOf(devName);
            string preName = isVideo ? VideoOuputDirectShowName : AudioOuputDirectShowName;
            //
            if (index < 0)
                return devName;
            else if (index == 0)
                return preName;
            else
                return preName + String.Format(" ({0})", index + 1);
        }

        public static string GetInputDirectShowName(string devName, bool isVideo)
        {
            if (DecklinkDeviceNames == null)
                return devName;
            //
            int index = DecklinkDeviceNames.ToList().IndexOf(devName);
            //
            if (index < 0)
                return "dshow,video='" + devName + "'";
            else
                return "decklink," + devName.Substring(2);
        }

        public static int GetInputVideoCapabilityIndex(string devName, string format)
        {
            if (DecklinkDeviceNames == null || String.IsNullOrEmpty(format) || String.IsNullOrEmpty(devName))
                return -1;
            //
            int indexDev = DecklinkDeviceNames.ToList().IndexOf(devName);
            if (indexDev < 0)
                return -1;
            //
            int index = GetVideoFormats(devName, true).ToList().IndexOf(format);
            return index + 1;
        }

        public static string[] GetVideoFormats(string devName,bool isInput)
        {
            try
            {
                if (String.IsNullOrEmpty(devName))
                    return null;
                //
                devName = devName.Substring(2);
                //
                if (_deckOutVideoFormats == null)
                    _deckOutVideoFormats = new Dictionary<string, List<string>>();
                //
                if (_deckOutVideoFormats.ContainsKey(devName))
                    return _deckOutVideoFormats[devName].ToArray();
                //
                string formats = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(DecklinkNative.GetOutputDeviceFormats(devName));
                if (String.IsNullOrEmpty(formats))
                    return null;
                else
                {
                    string[] notCorrectVals = { "-1", "-2", "ntsc 23.98", "ntsc progressive", "pal progressive", "2160","2k","4k" };
                    if (isInput)
                        notCorrectVals = new string[] { "-1", "-2" };
                    //
                    var arr = formats.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(m => !notCorrectVals.Where(n=>m.ToLower().StartsWith(n)).Any()).ToArray();
                    //
                    _deckOutVideoFormats.Add(devName, arr.ToList());
                    return arr;
                }
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }
       
        private static class DecklinkNative
        {
            private const string DllFilePath = @"HamedDecklink.dll";

            [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
            public extern static IntPtr GetDeviceNames();

            [DllImport(DllFilePath, CallingConvention = CallingConvention.Cdecl)]
            public extern static IntPtr GetOutputDeviceFormats(string devName);
            
        }
    }
    
}
