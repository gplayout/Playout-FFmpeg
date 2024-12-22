using System;
using System.Collections.Generic;
using System.Linq;
using DirectShowLib;

namespace Playout.DirectShow.Controls
{
    public class MultimediaUtil
    {
		#region Audio Renderer Methods
		/// <summary>
		/// The private cache of the audio renderer names
		/// </summary>
		private static string[] m_audioRendererNames;

		/// <summary>
		/// An array of audio renderer device names
		/// on the current system
		/// </summary>
		public static string[] AudioRendererNames
		{
			get
			{
				if (m_audioRendererNames == null)
				{
					m_audioRendererNames = (from a in GetDevices(FilterCategory.AudioRendererCategory)
					                        select a.Name).ToArray();
				}
				return m_audioRendererNames;
			}
		}
		#endregion

		#region Video Input Devices
		
		private static string[] m_videoInputNames;
        
		public static string[] VideoInputNames
		{
			get
			{
				if (m_videoInputNames == null)
				{
					m_videoInputNames = (from d in VideoInputDevices
										 select d.Name).ToArray();
				}
				return m_videoInputNames;
			}
		}

        private static string[] m_videoOutputNames;
        public static string[] VideoOutputNames
        {
            get
            {
                if (m_videoOutputNames == null)
                {
                    m_videoOutputNames = (from d in VideoOutputDevices
                                         select d.Name).ToArray();
                }
                return m_videoOutputNames;
            }
        }
		#endregion

		private static DsDevice[] GetDevices(Guid filterCategory)
		{
			return (from d in DsDevice.GetDevicesOfCat(filterCategory)
					select d).ToArray();
		}

    	public static DsDevice[] VideoInputDevices
		{
			get
			{
				if (m_videoInputDevices == null)
				{
					m_videoInputDevices = GetDevices(FilterCategory.VideoInputDevice);
				}
				return m_videoInputDevices;
			}
		}
		private static DsDevice[] m_videoInputDevices;

        public static DsDevice[] VideoOutputDevices
        {
            get
            {
                if (m_videoOutputDevices == null)
                {
                    m_videoOutputDevices = GetDevices(FilterCategory.TransmitCategory);
                }
                return m_videoOutputDevices;
            }
        }
        private static DsDevice[] m_videoOutputDevices;

		public static string[] VideoInputsDevicePaths
		{
			get
			{
				if (m_videoInputsDevicePaths == null)
				{
					m_videoInputsDevicePaths = (from d in VideoInputDevices
					                          select d.DevicePath).ToArray();
				}
				return m_videoInputsDevicePaths;
			}
		}
		private static string[] m_videoInputsDevicePaths;
    }
}
