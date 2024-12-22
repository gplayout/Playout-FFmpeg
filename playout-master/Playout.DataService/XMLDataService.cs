using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Playout.Base;
using Playout.Log;

namespace Playout.DataService
{
    public abstract class XMLDataService
    {
        protected XElement LoadXFile(string filePath)
        {
            try
            {
                if (String.IsNullOrEmpty(filePath))
                    throw new Exception("File Path is empty.");
                //
                string content = File.ReadAllText(filePath);
                XElement root = XElement.Parse(content);
                //
                return root;
            }
            catch (Exception ex)
            {
                ex.Log();
                return null;
            }
        }

        protected bool SaveXFile(string filePath, XElement root)
        {
            try
            {
                if (String.IsNullOrEmpty(filePath))
                    throw new Exception("File Path is empty.");
                //
                if (root != null)
                    root.Save(filePath);
                return true;
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }

        protected bool SaveXElement(string filePath, XElement element)
        {
            try
            {
                if (String.IsNullOrEmpty(filePath))
                    throw new Exception("File Path is empty.");
                //
                XElement root = this.LoadXFile(filePath);
                if (root.Element(element.Name) != null)
                {
                    var el = root.Element(element.Name);
                    el.Remove();
                }
                //
                root.Add(element);
                this.SaveXFile(filePath, root);
                return true;
            }
            catch (Exception ex)
            {
                ex.Log();
                return false;
            }
        }
    }
}
