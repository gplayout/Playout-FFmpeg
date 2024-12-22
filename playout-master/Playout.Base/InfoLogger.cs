
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playout.Log;

namespace Playout.Base
{
    public class InfoLogger
    {
        string filePath="";
        public void Initial(string filename,string text)
        {
            try
            {
                filePath = Program.AppPath + @"\Logs\" + filename;
                if (!File.Exists(filePath))
                    File.WriteAllText(filePath, text ?? "");
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
        public void InfoLog(string message)
        {
            File.AppendAllText(filePath, message + Environment.NewLine);
        }

    }
}