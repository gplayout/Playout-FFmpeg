using Playout.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playout.UI.Codes
{
    public static class emDataGridOptions
    {
        // We declare a certain path in which we'll store the grid's options file
        static readonly string _DATAGRID_OPTIONS_DIR = Program.AppPath + "\\Data\\MediaSourceGrid";
        // As file extension, we'll use ".di"
        const string _DATAGRID_OPTIONS_EXT = ".di";

        // ===============================================================================
        // Private function to return the full path of options file
        // ===============================================================================
        private static string ComposeGridOptionsFile(string gridName)
        {
            // Simple concatenation of directory, grid's name, and default extension
            return _DATAGRID_OPTIONS_DIR + gridName + _DATAGRID_OPTIONS_EXT;
        }

        // ===============================================================================
        // Public shared sub to save DataGrid's option
        // ===============================================================================
        public static void SaveGridOptions(DataGrid dg)
        {
            // We'll create a DataSet with the same name of the Grid, generating inside it a DataTable named "columns"
            DataSet columns = new DataSet(dg.Name);
            DataTable coltable = new DataTable("columns");

            // Here we'll save only some parameters, such as the visibility, the sort direction, etc.
            // we must create in the DataTable a number of columns equals to the number of property we intend to save.
            var _with1 = coltable;
            _with1.Columns.Add("DisplayIndex", Type.GetType("System.Int32"));
            _with1.Columns.Add("Width", Type.GetType("System.Double"));
            _with1.Columns.Add("Visibility", Type.GetType("System.Int32"));

            columns.Tables.Add(coltable);

            // We execute a loop on the DataGrid's columns, adding to the DataTable a number of rows equals to the
            // count of DataGrid's columns, each one of them exposing the value of the property itself        
            foreach (DataGridColumn c in dg.Columns)
            {
                coltable.Rows.Add(new object[] {
				c.DisplayIndex,
				c.Width.DisplayValue,
				c.Visibility,
			});
            }

            // Then, using the WriteXml() method, we save an XML file which contains the columns extracted values
            columns.WriteXml(ComposeGridOptionsFile(dg.Name));
        }

        // ===============================================================================
        // Public shared sub to load DataGrid's option
        // ===============================================================================
        public static void LoadGridOptions(DataGrid dg)
        {
            // We check if the options file exists...
            if (!(System.IO.File.Exists(ComposeGridOptionsFile(dg.Name))))
                return;

            // A new DataSet will be generated, and then populated using the readXml() method
            DataSet columns = new DataSet(dg.Name);
            columns.ReadXml(ComposeGridOptionsFile(dg.Name));

            // We execute a loop on grid's columns: for each of them, we read and apply the corresponding property's value from the table in DataSet
            int ii = 0;
            foreach (DataGridColumn c in dg.Columns)
            {
                if (ii >= columns.Tables[0].Rows.Count)
                    break;
                //
                c.DisplayIndex = columns.Tables[0].Rows[ii]["DisplayIndex"] == null ? c.DisplayIndex : int.Parse(columns.Tables[0].Rows[ii]["DisplayIndex"].ToString());
                if (c.Header!=null && c.Header.ToString() != "FA")
                {
                    double a = 0;
                    if(columns.Tables[0].Rows[ii]["Width"] != null)
                        double.TryParse(columns.Tables[0].Rows[ii]["Width"].ToString(), out a);
                    //
                    c.Width = a == 0 ? c.Width : new DataGridLength(a);
                }
                c.Visibility = columns.Tables[0].Rows[ii]["Visibility"] == null ? c.Visibility :
                    (Visibility)Enum.Parse(typeof(Visibility), columns.Tables[0].Rows[ii]["Visibility"].ToString());

                ii += 1;
            }
        }
    }
}
