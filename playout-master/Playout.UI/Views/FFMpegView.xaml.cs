using Playout.ApplicationService;
using Playout.Base;
using Playout.MediaFramework;
using Playout.Models;
using Playout.ViewModels.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Playout.Log;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Playout.UI.Views
{
    public partial class FFMpegView
    {
        public StreamViewModel ViewModel { get; set; }
        //
        public static event EventHandler OnFFMpegChangedStatus;
        public static Process encProgProc = null;
        public static Action<string> OnLogReceived = null;
        static string LatestLogs = "";
        static string LogFilePath = "";
        static System.Windows.Forms.Timer timLog = null;

        public static bool ffmpegProcStarted 
        { 
            get
            {
                try
                {
                    var proc1 = Process.GetProcessesByName("sbffmpeg");
                    var proc2 = Process.GetProcessesByName("fmlecmd");
                    return proc1.Length > 0 || proc2.Length > 0;
                }
                catch (Exception ex)
                {
                    ex.Log();
                    return false;
                }
            }
        }
        static readonly string ffmpegPath = System.Windows.Forms.Application.StartupPath + @"\stream\sbffmpeg.exe";
        public static readonly string streamDirPath = System.Windows.Forms.Application.StartupPath + @"\stream";
        static readonly string fmleProfilePath = System.Windows.Forms.Application.StartupPath + @"\stream\profile.xml";
        static readonly string fmleP1Path = System.Windows.Forms.Application.StartupPath + @"\stream\p1.xml";

        public FFMpegView(StreamViewModel vm)
        {
            this.ViewModel = vm;
            this.DataContext = this.ViewModel;
            //
            InitializeComponent();
            App.CheckScale(new Window[] { this });
            //
            this.CheckButtonStatus();
        }
        private void CheckButtonStatus()
        {
            this.Dispatcher.Invoke(() =>
                {
                    this.btnStart.IsEnabled = !ffmpegProcStarted;
                    this.btnStop.IsEnabled = ffmpegProcStarted;
                });
        }
        private bool SelectedProfile_IsValid()
        {
            var sp = this.ViewModel.SelectedProfile;
            if (sp != null &&
                (
                    (sp.DoRecord && !sp.IsValid)
                    ||
                    (!sp.DoRecord && !sp.IsValid &&
                        (String.IsNullOrEmpty(sp.ProfileName) || String.IsNullOrEmpty(sp.Url)))
                ))
            {
                return false;
            }
            else
                return true;
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CloseAllFFMPEGS();
                //
                this.Dispatcher.Invoke(() =>
                {
                    this.CheckButtonStatus();
                });
            }
            catch (Exception ex)
            {

            }
        }
        private async void btnStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sp = this.ViewModel.SelectedProfile;
                if (sp == null || !this.SelectedProfile_IsValid())
                {
                    await Functions.ShowMessage("Stream", "Please input all data fields.");
                    return;
                }
                //
                FFMpegView.CloseAllFFMPEGS();
                //
                string args =
                    sp.EncoderProgram == EncoderProgram.FFMPEG ? GetFFMPEGArgs(sp) : GetFMLEArgs(sp);
                //
                FFMpegView.RunEncoder(sp.EncoderProgram, args);
                //
                this.Dispatcher.Invoke(() =>
                {
                    this.CheckButtonStatus();
                });
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        private string GetFFMPEGArgs(StreamInstanceViewModel sp)
        {
            if (File.Exists(streamDirPath + @"\ffmpegProfile.txt"))
            {
                string xml = File.ReadAllText(streamDirPath + @"\ffmpegProfile.txt");
                return " " + xml;
            }
            //
            int vi = sp.VCamChannel.GetHashCode();
            bool isSeg = sp.SegmentSecond != 0;
            string fm = sp.FileFormat;
            //
            string path = FFMpegView.streamDirPath + @"\Record\" + DateTime.Now.ToString("yyyyMMdd_HHmmss"); ;
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            //
            //string args = " -y -loglevel warning -re -rtbufsize 1500M -f dshow -i video=\"Playout VCam" +
            //    vi + "\" -f dshow -i audio=\"Playout VSound" +
            //    vi + "\" -r " +
            //    sp.FrameRate + " -threads 4 -vcodec libx264 -pix_fmt yuv420p "+
            //    (String.IsNullOrEmpty(sp.VideoBitrate)?"":"-b:v "+sp.VideoBitrate)+
            //    " -vf scale=" + sp.FrameSizeString +
            //    " -c:a aac -strict -2 -q:a 1.1 -ar " + sp.SampleRate + " -ac 2 " +
            //    (String.IsNullOrEmpty(sp.AudioBitrate) ? "" : "-b:a " + sp.AudioBitrate) +
            //    " -f flv \"" + sp.Url + "\"" +
            //    (sp.DoRecord ?
            //        (isSeg ?
            //            " -f segment -segment_time " + sp.SegmentSecond +
            //            " -segment_format " + fm + " \"" + path + "\\" + sp.FileName + "_%03d." + fm + "\""
            //            :
            //            " \"" + path + "\\" + sp.FileName + "." + fm + "\"")
            //        : ""
            //    );

            string args = " -report -copyts -rtbufsize 1500M -f dshow -i video=\"Playout VCam" + vi + "\":audio=\"Playout VSound" + vi + "\"" +
                " -framerate " + sp.FrameRate +
                " -codec:v libx264 -s:v " + sp.FrameSizeString +
                " -pix_fmt:v yuv420p -threads 4 -bufsize:v 1500k -g:v 60" +
                " -b:v " + (String.IsNullOrEmpty(sp.VideoBitrate) ? "1500K" : sp.VideoBitrate) +
                (sp.SampleRate == 0 ? "" : " -ar " + sp.SampleRate) +
                " -b:a " + (String.IsNullOrEmpty(sp.AudioBitrate) ? "96k" : sp.AudioBitrate) +
                " -preset:v veryfast -profile:v baseline -level:v 3.1 -maxrate:v 1650k -codec:a libfaac -strict -2 -f flv \"" + sp.Url + "\"" +
                (sp.DoRecord ?
                    (isSeg ?
                        " -f segment -segment_time " + sp.SegmentSecond +
                        " -segment_format " + fm + " \"" + path + "\\" + sp.FileName + "_%03d." + fm + "\""
                        :
                        " \"" + path + "\\" + sp.FileName + "." + fm + "\"")
                    : ""
                );
            return args;
        }
        private string GetFMLEArgs(StreamInstanceViewModel sp)
        {
            if(File.Exists(streamDirPath+@"\fmleProfile.xml"))
            {
                string xml= " /p \"" + streamDirPath + @"\fmleProfile.xml" + "\""; ;
                return xml;
            }
            //Create Profile
            if (File.Exists(fmleP1Path))
                File.Delete(fmleP1Path);
            //
            XElement root = XElement.Load(fmleProfilePath);
            string fmleProfile = root.Element("flashmedialiveencoder_profile").ToString();
            //
            int vi = sp.VCamChannel.GetHashCode();
            //
            int outFrameRate = Program.Dg.OutputVideoFormatPars.FrameRateScale / Program.Dg.OutputVideoFormatPars.FrameRateDuration;
            //
            fmleProfile = fmleProfile.Replace("#VIDEODEVICE#", "Playout VCam" + vi);
            fmleProfile = fmleProfile.Replace("#FRAMERATE#", outFrameRate.ToString() + (outFrameRate.ToString().IndexOf(".") == -1 ? ".00" : ""));
            fmleProfile = fmleProfile.Replace("#WIDTH#", Program.Dg.OutputVideoFormatPars.Width.ToString());
            fmleProfile = fmleProfile.Replace("#HEIGHT#", Program.Dg.OutputVideoFormatPars.Height.ToString());
            fmleProfile = fmleProfile.Replace("#AUDIODEVICE#", "Playout VSound" + vi);
            fmleProfile = fmleProfile.Replace("#SAMPLERATE#", "44100");
            fmleProfile = fmleProfile.Replace("#VIDEODATARATES#", (String.IsNullOrEmpty(sp.VideoBitrate) ? "200" : sp.VideoBitrate.ToLower().Replace("k","").Replace("m","")));
            fmleProfile = fmleProfile.Replace("#AUDIODATARATE#", (String.IsNullOrEmpty(sp.AudioBitrate) ? "96" : sp.AudioBitrate.ToLower().Replace("k", "").Replace("m", "")));
            fmleProfile = fmleProfile.Replace("#VIDEOSIZE#", sp.FrameSizeString);
            fmleProfile = fmleProfile.Replace("#URL#", sp.Url.Substring(0,sp.Url.LastIndexOf("/")));
            fmleProfile = fmleProfile.Replace("#STREAM#", sp.Url.Substring(sp.Url.LastIndexOf("/")+1));
            fmleProfile = fmleProfile.Replace("#LOGDIRECTORY#", streamDirPath + @"\Logs");

            fmleProfile = "<?xml version=\"1.0\" encoding=\"UTF-16\"?>" + Environment.NewLine + fmleProfile;
            File.WriteAllText(fmleP1Path, fmleProfile, Encoding.Unicode);

            string args = " /p \"" + fmleP1Path + "\"";
                
            return args;
        }
        private void btnNewProfile_Click(object sender, RoutedEventArgs e)
        {
            string upName = this.ViewModel.GetNewProfileName();
            var vm = new StreamInstanceViewModel(upName);
            //
            this.ViewModel.Profiles.Add(vm);
            this.ViewModel.SelectedProfileName = vm.ProfileName;
        }
        private async void btnDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(this.ViewModel.SelectedProfileName))
                return;
            //
            bool result = await Functions.ConfirmationForDelete("Stream", "Are you sure to delete the selected profile?");
            if (result == false)
                return;
            //
            this.ViewModel.Profiles.Remove(this.ViewModel.SelectedProfile);
            //
            if (this.ViewModel.Profiles.Count > 0)
                this.ViewModel.SelectedProfileName = this.ViewModel.Profiles[0].ProfileName;
        }
        private async void btnOk_Click(object sender, RoutedEventArgs e)
        {
            var sp = this.ViewModel.SelectedProfile;
            if (sp != null && !this.SelectedProfile_IsValid())
            {
                await Functions.ShowMessage("Stream", "Please fill all fields for selected profile.");
                return;
            }
            //
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        public static void CloseAllFFMPEGS()
        {
            try
            {
                if (timLog != null)
                {
                    timLog.Stop();
                    timLog = null;
                }
                //
                if (encProgProc != null && !encProgProc.HasExited)
                {
                    StopProgramByAttachingToItsConsoleAndIssuingCtrlCEvent(encProgProc, 3000);
                }
                //
                var procs = Process.GetProcessesByName("sbffmpeg");
                foreach (var p in procs)
                {
                    p.Kill();
                    p.WaitForExit();
                }
                //
                streamProc_Exited(null, null);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }

        private static void StopProgramByAttachingToItsConsoleAndIssuingCtrlCEvent(Process process, int waitForExitTimeout = 2000)
        {
            if (!AttachConsole((uint)process.Id))
            {
                return;
            }

            // Disable Ctrl-C handling for our program
            SetConsoleCtrlHandler(null, true);

            // Sent Ctrl-C to the attached console
            GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);

            // Wait for the graceful end of the process.
            // If the process will not exit in time specified by 'waitForExitTimeout', the process will be killed
            using (new System.Threading.Timer((dummy => 
            { 
                if (!process.HasExited) 
                    process.Kill(); 
            }), null, waitForExitTimeout, 2000))
            {
                // Must wait here. If we don't wait and re-enable Ctrl-C handling below too fast, we might terminate ourselves.
                process.WaitForExit(waitForExitTimeout);
            }
            process.StandardInput.WriteLine("y");

            FreeConsole();

            // Re-enable Ctrl-C handling or any subsequently started programs will inherit the disabled state.
            SetConsoleCtrlHandler(null, false);
        }
        private static string GetFMLEPath()
        {
            var root = XElement.Load(fmleProfilePath);
            string path = root.Element("FMLEPath").Value + "fmlecmd.exe";
            return path;
        }
        public static void RunEncoder(EncoderProgram encPorg,string args)
        {
            if (ffmpegProcStarted)
                return;
            //
            encProgProc = new Process();
            ProcessStartInfo psi = new ProcessStartInfo(encPorg == EncoderProgram.FMLE ? GetFMLEPath() : ffmpegPath, args);
            psi.CreateNoWindow = true;
            psi.RedirectStandardInput = true;
            psi.RedirectStandardError = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            
            ////////////////////////////////////////
            encProgProc.EnableRaisingEvents = true;
            encProgProc.Exited += streamProc_Exited;
            encProgProc.OutputDataReceived += streamProc_OutputDataReceived;
            encProgProc.ErrorDataReceived += streamProc_ErrorDataReceived;
            Logger.InfoLog("Stream Args:" + args);
            
            encProgProc.StartInfo = psi;
            //
            encProgProc.Start();
            //
            encProgProc.BeginOutputReadLine();
            encProgProc.BeginErrorReadLine();
            //
            EncoderProgramChangedStatus();
            //
            if (timLog != null)
                timLog.Stop();
            //
            string path = FFMpegView.streamDirPath + @"\Logs"; 
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            LogFilePath = path + @"\StreamingLog_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
            //
            timLog = new System.Windows.Forms.Timer();
            timLog.Interval = 1000;
            timLog.Tick += (e, s) =>
            {
                string logs = "";
                lock (LatestLogs)
                {
                    logs = LatestLogs;
                    LatestLogs = "";
                }
                if (!String.IsNullOrEmpty(logs))
                {
                    SaveLogInFile(logs);
                    if (OnLogReceived != null)
                    {
                        OnLogReceived(logs);
                    }
                }
            };
            timLog.Start();
        }
        static void EncoderProgramChangedStatus()
        {
            if (OnFFMpegChangedStatus != null)
            {
                OnFFMpegChangedStatus(null, null);
            }
        }
        static void streamProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    lock (LatestLogs)
                    {
                        LatestLogs = e.Data + Environment.NewLine;
                    }
                }
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }

        static void streamProc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            
        }

        static void streamProc_Exited(object sender, EventArgs e)
        {
            EncoderProgramChangedStatus();
        }

        static void SaveLogInFile(string logs)
        {
            try
            {
                if (String.IsNullOrEmpty(LogFilePath))
                    return;
                //
                string msg = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss -> ") + logs;
                File.AppendAllText(LogFilePath, msg);
            }
            catch(Exception ex)
            {
                ex.Log();
            }
        }
        

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();

        // Enumerated type for the control messages sent to the handler routine
        enum CtrlTypes : uint
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint dwProcessGroupId);

        public void StopProgram(Process proc)
        {
            //This does not require the console window to be visible.
            if (AttachConsole((uint)proc.Id))
            {
                // Disable Ctrl-C handling for our program
                SetConsoleCtrlHandler(null, true);
                GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0);
                
                // Must wait here. If we don't and re-enable Ctrl-C
                // handling below too fast, we might terminate ourselves.
                proc.WaitForExit(2000);

                FreeConsole();

                //Re-enable Ctrl-C handling or any subsequently started
                //programs will inherit the disabled state.
                SetConsoleCtrlHandler(null, false);
            }
        }

       
    }
}
