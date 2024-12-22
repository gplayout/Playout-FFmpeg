using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playout.Installer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Setup Started.");
            //
            string playoutPath = System.Windows.Forms.Application.StartupPath;
            playoutPath = Directory.GetParent(playoutPath).FullName;
            List<string> files = new List<string>();
            //
            Console.WriteLine("Base Path:" + playoutPath);
            files.Add(Path.Combine(playoutPath, "HamedSource.ax"));
            //VCams
            if (Directory.Exists(Path.Combine(playoutPath, "VCam")))
            {
                string path = Path.Combine(playoutPath, "VCam");
                foreach (var file in Directory.GetFiles(path))
                {
                    if (Path.GetExtension(file).ToLower() == ".ax")
                        files.Add(file);
                }
            }
            foreach(var file in files)
            {
                RegisterFile(file, true);
                RegisterFile(file, false);
            }
            //
            Console.WriteLine("Setup Completed.");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        static bool RegisterFile(string filePath, bool u)
        {
            try
            {
                string fileName = Path.GetFileName(filePath);
                ProcessStartInfo psi = new ProcessStartInfo(System.Environment.SystemDirectory + "\\regsvr32.exe ", fileName + " -s " + (u ? "-u" : ""));
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;
                psi.WorkingDirectory = Path.GetDirectoryName(filePath);
                var proc = Process.Start(psi);
                proc.WaitForExit();
                bool result = proc.ExitCode == 0;
                if (result)
                    Console.WriteLine(fileName + " " + (u ? "uninstalled" : "installed") + " successfully.");
                else
                    Console.WriteLine((u ? "Uninstalling " : "Installing ") + fileName + " failed. Exit Code:" + proc.ExitCode);
                //
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
