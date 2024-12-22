using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Framework.UI.Converters
{
    public sealed class IntToColorConverter : IValueConverter
    {
        #region Public Methods

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <param name="targetType"> The target type. </param>
        /// <param name="parameter"> The parameter. </param>
        /// <param name="culture"> The culture. </param>
        /// <returns> The <see cref="object"/>. </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            //
            int code = (int)value;
            System.Drawing.Color col = System.Drawing.ColorTranslator.FromWin32(code);
            System.Windows.Media.Color wcolor = System.Windows.Media.Color.FromArgb(col.A, col.R, col.G, col.B); ;
            return wcolor;
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value"> The value. </param>
        /// <param name="targetType"> The target type. </param>
        /// <param name="parameter"> The parameter. </param>
        /// <param name="culture"> The culture. </param>
        /// <returns> The <see cref="object"/>. </returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            //
            int code = (int)value;
            return code;
        }

        #endregion
    }
}
