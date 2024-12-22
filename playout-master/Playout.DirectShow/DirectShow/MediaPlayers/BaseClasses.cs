#region Includes
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using DirectShowLib;
using Size=System.Windows.Size;
#endregion

namespace Playout.DirectShow.MediaPlayers
{
    public enum enPlayStatus { None,Playing, Paused, Stopped }

    public enum enMediaType
    {
        VideoFile,
        Url,
        ImageFile,
        Device,
        Playlist,
        DVD
    }

    public enum MediaState
    {
        Manual,
        Play,
        Stop,
        Close,
        Pause
    }

    /// <summary>
    /// The types of position formats that
    /// are available for seeking media
    /// </summary>
    public enum MediaPositionFormat
    {
        MediaTime,
        Frame,
        Byte,
        Field,
        Sample,
        None
    }
    
    /// <summary>
    /// The arguments that store information about a failed media attempt
    /// </summary>
    public class MediaFailedEventArgs : EventArgs
    {
        public MediaFailedEventArgs(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        public Exception Exception { get; protected set; }
        public string Message { get; protected set; }
    }

   

}