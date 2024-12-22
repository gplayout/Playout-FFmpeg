using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Playout.Base
{
    public static class XMLExtentionsHelper
    {
        public static T GetValue<T>(this XAttribute att, T defValue)
        {
            try
            {
                string val = att.Value;
                if (String.IsNullOrEmpty(val))
                    return defValue;
                //
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch
            {
                return defValue;
            }
        }

        public static T GetEnumValue<T>(this XAttribute att, T defValue)
        {
            try
            {
                string val = att.Value;
                if (String.IsNullOrEmpty(val))
                    return defValue;
                //
                return (T)Enum.Parse(typeof(T), val);
            }
            catch
            {
                return defValue;
            }
        }

        public static TimeSpan? GetTimespan(this XAttribute att, TimeSpan? defValue = null)
        {
            try
            {
                string val = att.Value;
                if (String.IsNullOrEmpty(val))
                    return defValue;
                //
                return TimeSpan.Parse(val);
            }
            catch
            {
                return defValue;
            }
        }

        public static DateTime? GetDateTime(this XAttribute att, DateTime? defValue = null)
        {
            try
            {
                string val = att.Value;
                if (String.IsNullOrEmpty(val))
                    return defValue;
                //
                return DateTime.Parse(val);
            }
            catch
            {
                return defValue;
            }
        }

        public static Guid GetGuid(this XAttribute att, Guid defValue)
        {
            try
            {
                string val = att.Value;
                if (String.IsNullOrEmpty(val))
                    return defValue;
                //
                return Guid.Parse(val);
            }
            catch
            {
                return defValue;
            }
        }
    }
}
