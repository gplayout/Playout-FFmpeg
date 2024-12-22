using Playout.MediaFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Playout.DI;
using Ninject;
using Playout.Base;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;
using Playout.Log;

namespace Playout.UI
{
    public partial class App
    {
        private DI.NinjectConfig ninject;
        private static string appGuid = "ba82cd54-733b-4e1f-bc28-3359bbf78fdb";

        Mutex mutex = new Mutex(false, "Global\\" + appGuid);
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            //
            Logger.Initial();
            Application.Current.DispatcherUnhandledException += Dispatcher_UnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.Exit += Current_Exit;
            //
            Logger.InfoLog("Application Started.");
            //
            if (System.Windows.Forms.Control.ModifierKeys == System.Windows.Forms.Keys.Control)
            {
                Views.Lock.DeactivationView wnd = new Views.Lock.DeactivationView();
                wnd.ShowDialog();
                Application.Current.Shutdown();
                return;
            }
            //
            //Program.Lock.trialCheckOnExceed = this.TrialCheckOnExceed;
            //bool resultLock = Program.Lock.CheckOffAck();
            //if (!resultLock)
           // {
            //    Program.Lock.TrialCheckStartThread();
           // }

           ///if (Program.Lock.LockStatus == RoyalLock.LockStatusEnum.Basic)
           // {
                if (!mutex.WaitOne(0, false))
                {
                    MessageBox.Show("Instance already running");
                    Process.GetCurrentProcess().Kill();
                    return;
                }
           // }

            //
            ninject = new DI.NinjectConfig();
            ninject.ConfigureContainer();
            //
            ComposeObjects();
            //
            Current.MainWindow.Show();
            //Microsoft.Win32.SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged);
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            
        }
        public static void CheckScale(IEnumerable<Window> wnds)
        {
            var screen = System.Windows.Forms.Screen.PrimaryScreen.Bounds;
            double baseW = 1366.0;
            double baseH = baseW * ((double)screen.Height / (double)screen.Width);
            double wZarib = screen.Width / baseW;
            double hZarib = screen.Height / baseH;
            //foreach (Window wnd in wnds )
            //{
            //    ((System.Windows.Controls.Grid)wnd.Content).LayoutTransform = new System.Windows.Media.ScaleTransform(wZarib, hZarib, 0, 0);
            //    if (wnd.WindowState == WindowState.Normal)
            //    {
            //        wnd.Width *= wZarib;
            //        wnd.Height *= hZarib;
            //    }
            //}
        }
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Logger.InfoLog("Application Exit.");
        }
        public Window MyMainWindow
        {
            get { return DI.NinjectConfig.kernel.Get<Views.MainView>(); }
        }

        private void ComposeObjects()
        {
            Current.MainWindow = this.MyMainWindow;
            Current.ShutdownMode = System.Windows.ShutdownMode.OnMainWindowClose;
        }

        void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Exception.Log("Crash");
        }
        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ((Exception)e.ExceptionObject).Log("DomainCrash");
        }
        private void TrialCheckOnExceed()
        {
            this.Dispatcher.Invoke(() =>
            {
                Views.Lock.ActivationView ackView = new Views.Lock.ActivationView();
                ackView.ForTrialCheckExpired();
                if (ackView.ShowDialog() != true)
                    Process.GetCurrentProcess().Kill();
            });


        }
    }
}
