namespace Playout.UI.WF
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.videoGrabber1 = new Playout.MediaFramework.MyVideoGrabber();
            this.SuspendLayout();
            // 
            // videoGrabber1
            // 
            this.videoGrabber1.AdjustOverlayAspectRatio = true;
            this.videoGrabber1.AdjustPixelAspectRatio = true;
            this.videoGrabber1.Aero = VidGrab.TAero.ae_Default;
            this.videoGrabber1.AnalogVideoStandard = -1;
            this.videoGrabber1.ApplicationPriority = VidGrab.TApplicationPriority.ap_default;
            this.videoGrabber1.ASFAudioBitRate = -1;
            this.videoGrabber1.ASFAudioChannels = -1;
            this.videoGrabber1.ASFBufferWindow = -1;
            this.videoGrabber1.ASFDeinterlaceMode = VidGrab.TASFDeinterlaceMode.adm_NotInterlaced;
            this.videoGrabber1.ASFFixedFrameRate = true;
            this.videoGrabber1.ASFMediaServerPublishingPoint = "";
            this.videoGrabber1.ASFMediaServerRemovePublishingPointAfterDisconnect = false;
            this.videoGrabber1.ASFMediaServerTemplatePublishingPoint = "";
            this.videoGrabber1.ASFNetworkMaxUsers = 5;
            this.videoGrabber1.ASFNetworkPort = 0;
            this.videoGrabber1.ASFProfile = -1;
            this.videoGrabber1.ASFProfileFromCustomFile = "";
            this.videoGrabber1.ASFProfileVersion = VidGrab.TASFProfileVersion.apv_ProfileVersion_8;
            this.videoGrabber1.ASFVideoBitRate = -1;
            this.videoGrabber1.ASFVideoFrameRate = 0D;
            this.videoGrabber1.ASFVideoHeight = -1;
            this.videoGrabber1.ASFVideoMaxKeyFrameSpacing = -1;
            this.videoGrabber1.ASFVideoQuality = -1;
            this.videoGrabber1.ASFVideoWidth = -1;
            this.videoGrabber1.AspectRatioToUse = -1D;
            this.videoGrabber1.AssociateAudioAndVideoDevices = false;
            this.videoGrabber1.AudioBalance = 0;
            this.videoGrabber1.AudioChannelRenderMode = VidGrab.TAudioChannelRenderMode.acrm_Normal;
            this.videoGrabber1.AudioCompressor = 0;
            this.videoGrabber1.AudioDevice = -1;
            this.videoGrabber1.AudioDeviceRendering = false;
            this.videoGrabber1.AudioFormat = VidGrab.TAudioFormat.af_default;
            this.videoGrabber1.AudioInput = -1;
            this.videoGrabber1.AudioInputBalance = 0;
            this.videoGrabber1.AudioInputLevel = 65535;
            this.videoGrabber1.AudioInputMono = false;
            this.videoGrabber1.AudioPeakEvent = false;
            this.videoGrabber1.AudioRecording = false;
            this.videoGrabber1.AudioRenderer = -1;
            this.videoGrabber1.AudioSource = VidGrab.TAudioSource.as_Default;
            this.videoGrabber1.AudioStreamNumber = -1;
            this.videoGrabber1.AudioSyncAdjustment = 0;
            this.videoGrabber1.AudioSyncAdjustmentEnabled = false;
            this.videoGrabber1.AudioVolume = 32767;
            this.videoGrabber1.AutoConnectRelatedPins = true;
            this.videoGrabber1.AutoFileName = VidGrab.TAutoFileName.fn_Sequential;
            this.videoGrabber1.AutoFileNameDateTimeFormat = "yymmdd_hhmmss_zzz";
            this.videoGrabber1.AutoFileNameMinDigits = 6;
            this.videoGrabber1.AutoFilePrefix = "vg";
            this.videoGrabber1.AutoRefreshPreview = false;
            this.videoGrabber1.AutoStartPlayer = true;
            this.videoGrabber1.AVIDurationUpdated = true;
            this.videoGrabber1.AVIFormatOpenDML = true;
            this.videoGrabber1.AVIFormatOpenDMLCompatibilityIndex = true;
            this.videoGrabber1.BackColor = System.Drawing.Color.DarkGray;
            this.videoGrabber1.BackgroundColor = 0;
            this.videoGrabber1.BufferCount = -1;
            this.videoGrabber1.BurstCount = 3;
            this.videoGrabber1.BurstInterval = 0;
            this.videoGrabber1.BurstMode = false;
            this.videoGrabber1.BurstType = VidGrab.TFrameCaptureDest.fc_TBitmap;
            this.videoGrabber1.BusyCursor = VidGrab.TCursors.cr_HourGlass;
            this.videoGrabber1.CameraControlSettings = true;
            this.videoGrabber1.CaptureFileExt = "";
            this.videoGrabber1.ColorKey = 1048592;
            this.videoGrabber1.ColorKeyEnabled = false;
            this.videoGrabber1.CompressionMode = VidGrab.TCompressionMode.cm_NoCompression;
            this.videoGrabber1.CompressionType = VidGrab.TCompressionType.ct_Video;
            this.videoGrabber1.Cropping_Enabled = false;
            this.videoGrabber1.Cropping_Height = 120;
            this.videoGrabber1.Cropping_Outbounds = true;
            this.videoGrabber1.Cropping_Width = 160;
            this.videoGrabber1.Cropping_X = 0;
            this.videoGrabber1.Cropping_Y = 0;
            this.videoGrabber1.Cropping_Zoom = 1D;
            this.videoGrabber1.Display_Active = true;
            this.videoGrabber1.Display_AlphaBlendEnabled = false;
            this.videoGrabber1.Display_AlphaBlendValue = 180;
            this.videoGrabber1.Display_AspectRatio = VidGrab.TAspectRatio.ar_Stretch;
            this.videoGrabber1.Display_AutoSize = false;
            this.videoGrabber1.Display_Embedded = true;
            this.videoGrabber1.Display_FullScreen = false;
            this.videoGrabber1.Display_Height = 240;
            this.videoGrabber1.Display_Left = 10;
            this.videoGrabber1.Display_Monitor = 0;
            this.videoGrabber1.Display_MouseMovesWindow = true;
            this.videoGrabber1.Display_PanScanRatio = 50;
            this.videoGrabber1.Display_StayOnTop = false;
            this.videoGrabber1.Display_Top = 10;
            this.videoGrabber1.Display_TransparentColorEnabled = false;
            this.videoGrabber1.Display_TransparentColorValue = 255;
            this.videoGrabber1.Display_VideoPortEnabled = true;
            this.videoGrabber1.Display_Visible = true;
            this.videoGrabber1.Display_Width = 320;
            this.videoGrabber1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.videoGrabber1.DoubleBuffered = false;
            this.videoGrabber1.DroppedFramesPollingInterval = -1;
            this.videoGrabber1.DualDisplay_Active = false;
            this.videoGrabber1.DualDisplay_AlphaBlendEnabled = false;
            this.videoGrabber1.DualDisplay_AlphaBlendValue = 180;
            this.videoGrabber1.DualDisplay_AspectRatio = VidGrab.TAspectRatio.ar_Stretch;
            this.videoGrabber1.DualDisplay_AutoSize = false;
            this.videoGrabber1.DualDisplay_Embedded = false;
            this.videoGrabber1.DualDisplay_FullScreen = false;
            this.videoGrabber1.DualDisplay_Height = 240;
            this.videoGrabber1.DualDisplay_Left = 20;
            this.videoGrabber1.DualDisplay_Monitor = 0;
            this.videoGrabber1.DualDisplay_MouseMovesWindow = true;
            this.videoGrabber1.DualDisplay_PanScanRatio = 50;
            this.videoGrabber1.DualDisplay_StayOnTop = false;
            this.videoGrabber1.DualDisplay_Top = 400;
            this.videoGrabber1.DualDisplay_TransparentColorEnabled = false;
            this.videoGrabber1.DualDisplay_TransparentColorValue = 255;
            this.videoGrabber1.DualDisplay_VideoPortEnabled = false;
            this.videoGrabber1.DualDisplay_Visible = true;
            this.videoGrabber1.DualDisplay_Width = 320;
            this.videoGrabber1.DVDateTimeEnabled = true;
            this.videoGrabber1.DVDiscontinuityMinimumInterval = 3;
            this.videoGrabber1.DVDTitle = 0;
            this.videoGrabber1.DVEncoder_VideoFormat = VidGrab.TDVVideoFormat.dvf_DVSD;
            this.videoGrabber1.DVEncoder_VideoResolution = VidGrab.TDVSize.dv_Full;
            this.videoGrabber1.DVEncoder_VideoStandard = VidGrab.TDVVideoStandard.dvs_Default;
            this.videoGrabber1.DVRecordingInNativeFormatSeparatesStreams = false;
            this.videoGrabber1.DVReduceFrameRate = false;
            this.videoGrabber1.DVRgb219 = false;
            this.videoGrabber1.DVTimeCodeEnabled = true;
            this.videoGrabber1.EventNotificationSynchrone = true;
            this.videoGrabber1.FixFlickerOrBlackCapture = false;
            this.videoGrabber1.FrameCaptureHeight = -1;
            this.videoGrabber1.FrameCaptureWidth = -1;
            this.videoGrabber1.FrameCaptureWithoutOverlay = false;
            this.videoGrabber1.FrameCaptureZoomSize = 100;
            this.videoGrabber1.FrameGrabber = VidGrab.TFrameGrabber.fg_BothStreams;
            this.videoGrabber1.FrameGrabberRGBFormat = VidGrab.TFrameGrabberRGBFormat.fgf_Default;
            this.videoGrabber1.FrameNumberStartsFromZero = false;
            this.videoGrabber1.FrameRate = 0D;
            this.videoGrabber1.FrameRateDivider = 0;
            this.videoGrabber1.HoldRecording = false;
            this.videoGrabber1.ImageOverlay_AlphaBlend = false;
            this.videoGrabber1.ImageOverlay_AlphaBlendValue = 180;
            this.videoGrabber1.ImageOverlay_ChromaKey = false;
            this.videoGrabber1.ImageOverlay_ChromaKeyLeewayPercent = 25;
            this.videoGrabber1.ImageOverlay_ChromaKeyRGBColor = 0;
            this.videoGrabber1.ImageOverlay_Height = -1;
            this.videoGrabber1.ImageOverlay_LeftLocation = 10;
            this.videoGrabber1.ImageOverlay_RotationAngle = 0D;
            this.videoGrabber1.ImageOverlay_StretchToVideoSize = false;
            this.videoGrabber1.ImageOverlay_TargetDisplay = -1;
            this.videoGrabber1.ImageOverlay_TopLocation = 10;
            this.videoGrabber1.ImageOverlay_Transparent = false;
            this.videoGrabber1.ImageOverlay_TransparentColorValue = 0;
            this.videoGrabber1.ImageOverlay_UseTransparentColor = false;
            this.videoGrabber1.ImageOverlay_Width = -1;
            this.videoGrabber1.ImageOverlayEnabled = false;
            this.videoGrabber1.ImageOverlaySelector = 0;
            this.videoGrabber1.IPCameraURL = "";
            this.videoGrabber1.JPEGPerformance = VidGrab.TJPEGPerformance.jpBestQuality;
            this.videoGrabber1.JPEGProgressiveDisplay = false;
            this.videoGrabber1.JPEGQuality = 100;
            this.videoGrabber1.LicenseString = "N/A";
            this.videoGrabber1.Location = new System.Drawing.Point(0, 0);
            this.videoGrabber1.LogoDisplayed = false;
            this.videoGrabber1.LogoLayout = VidGrab.TLogoLayout.lg_Stretched;
            this.videoGrabber1.MixAudioSamples_CurrentSourceLevel = 100;
            this.videoGrabber1.MixAudioSamples_ExternalSourceLevel = 100;
            this.videoGrabber1.Mixer_MosaicColumns = 1;
            this.videoGrabber1.Mixer_MosaicLines = 1;
            this.videoGrabber1.MotionDetector_CompareBlue = true;
            this.videoGrabber1.MotionDetector_CompareGreen = true;
            this.videoGrabber1.MotionDetector_CompareRed = true;
            this.videoGrabber1.MotionDetector_Enabled = false;
            this.videoGrabber1.MotionDetector_GreyScale = false;
            this.videoGrabber1.MotionDetector_Grid = "5555555555 5555555555 5555555555 5555555555 5555555555 5555555555 5555555555 5555" +
    "555555 5555555555 5555555555";
            this.videoGrabber1.MotionDetector_MaxDetectionsPerSecond = 0D;
            this.videoGrabber1.MotionDetector_ReduceCPULoad = 1;
            this.videoGrabber1.MotionDetector_ReduceVideoNoise = false;
            this.videoGrabber1.MotionDetector_Triggered = false;
            this.videoGrabber1.MouseWheelEventEnabled = true;
            this.videoGrabber1.MpegStreamType = VidGrab.TMpegStreamType.mpst_Default;
            this.videoGrabber1.MultiplexedInputEmulation = true;
            this.videoGrabber1.MultiplexedRole = VidGrab.TMultiplexedRole.mr_NotMultiplexed;
            this.videoGrabber1.MultiplexedStabilizationDelay = 100;
            this.videoGrabber1.MultiplexedSwitchDelay = 0;
            this.videoGrabber1.Multiplexer = -1;
            this.videoGrabber1.MuteAudioRendering = false;
            this.videoGrabber1.Name = "videoGrabber1";
            this.videoGrabber1.NetworkStreaming = VidGrab.TNetworkStreaming.ns_Disabled;
            this.videoGrabber1.NetworkStreamingType = VidGrab.TNetworkStreamingType.nst_AudioVideoStreaming;
            this.videoGrabber1.NormalCursor = VidGrab.TCursors.cr_Default;
            this.videoGrabber1.NotificationMethod = VidGrab.TNotificationMethod.nm_Thread;
            this.videoGrabber1.NotificationPriority = VidGrab.TThreadPriority.tpNormal;
            this.videoGrabber1.NotificationSleepTime = -1;
            this.videoGrabber1.OnFrameBitmapEventSynchrone = false;
            this.videoGrabber1.OpenURLAsync = true;
            this.videoGrabber1.OverlayAfterTransform = false;
            this.videoGrabber1.OwnerObject = null;
            this.videoGrabber1.PlayerAudioRendering = true;
            this.videoGrabber1.PlayerDuration = ((long)(1));
            this.videoGrabber1.PlayerDVSize = VidGrab.TDVSize.dv_Full;
            this.videoGrabber1.PlayerFastSeekSpeedRatio = 4;
            this.videoGrabber1.PlayerFileName = "";
            this.videoGrabber1.PlayerForcedCodec = "";
            this.videoGrabber1.PlayerFramePosition = ((long)(1));
            this.videoGrabber1.PlayerRefreshPausedDisplay = false;
            this.videoGrabber1.PlayerRefreshPausedDisplayFrameRate = 0D;
            this.videoGrabber1.PlayerSpeedRatio = 1D;
            this.videoGrabber1.PlayerTimePosition = ((long)(0));
            this.videoGrabber1.PlayerTrackBarSynchrone = false;
            this.videoGrabber1.PlaylistIndex = 0;
            this.videoGrabber1.PreallocCapFileCopiedAfterRecording = true;
            this.videoGrabber1.PreallocCapFileEnabled = false;
            this.videoGrabber1.PreallocCapFileName = "";
            this.videoGrabber1.PreallocCapFileSizeInMB = 100;
            this.videoGrabber1.PreviewZoomSize = 100;
            this.videoGrabber1.QuickDeviceInitialization = false;
            this.videoGrabber1.RawAudioSampleCapture = false;
            this.videoGrabber1.RawCaptureAsyncEvent = true;
            this.videoGrabber1.RawSampleCaptureLocation = VidGrab.TRawSampleCaptureLocation.rl_SourceFormat;
            this.videoGrabber1.RawVideoSampleCapture = false;
            this.videoGrabber1.RecordingAudioBitRate = -1;
            this.videoGrabber1.RecordingBacktimedFramesCount = 0;
            this.videoGrabber1.RecordingCanPause = false;
            this.videoGrabber1.RecordingFileName = "";
            this.videoGrabber1.RecordingInNativeFormat = true;
            this.videoGrabber1.RecordingMethod = VidGrab.TRecordingMethod.rm_AVI;
            this.videoGrabber1.RecordingOnMotion_Enabled = false;
            this.videoGrabber1.RecordingOnMotion_MotionThreshold = 0D;
            this.videoGrabber1.RecordingOnMotion_NoMotionPauseDelayMs = 5000;
            this.videoGrabber1.RecordingPauseCreatesNewFile = false;
            this.videoGrabber1.RecordingSize = VidGrab.TRecordingSize.rs_Default;
            this.videoGrabber1.RecordingTimer = VidGrab.TRecordingTimer.rt_Disabled;
            this.videoGrabber1.RecordingTimerInterval = 0;
            this.videoGrabber1.RecordingVideoBitRate = -1;
            this.videoGrabber1.Reencoding_IncludeAudioStream = true;
            this.videoGrabber1.Reencoding_IncludeVideoStream = true;
            this.videoGrabber1.Reencoding_Method = VidGrab.TRecordingMethod.rm_ASF;
            this.videoGrabber1.Reencoding_NewVideoClip = "";
            this.videoGrabber1.Reencoding_SourceVideoClip = "";
            this.videoGrabber1.Reencoding_StartFrame = ((long)(-1));
            this.videoGrabber1.Reencoding_StartTime = ((long)(-1));
            this.videoGrabber1.Reencoding_StopFrame = ((long)(-1));
            this.videoGrabber1.Reencoding_StopTime = ((long)(-1));
            this.videoGrabber1.Reencoding_UseAudioCompressor = false;
            this.videoGrabber1.Reencoding_UseFrameGrabber = true;
            this.videoGrabber1.Reencoding_UseVideoCompressor = false;
            this.videoGrabber1.Reencoding_WMVOutput = false;
            this.videoGrabber1.ScreenRecordingLayeredWindows = false;
            this.videoGrabber1.ScreenRecordingMonitor = 0;
            this.videoGrabber1.ScreenRecordingNonVisibleWindows = false;
            this.videoGrabber1.ScreenRecordingThroughClipboard = false;
            this.videoGrabber1.ScreenRecordingWithCursor = true;
            this.videoGrabber1.SendToDV_DeviceIndex = -1;
            this.videoGrabber1.Size = new System.Drawing.Size(284, 261);
            this.videoGrabber1.SpeakerBalance = 0;
            this.videoGrabber1.SpeakerControl = false;
            this.videoGrabber1.SpeakerVolume = 65535;
            this.videoGrabber1.StoragePath = "C:\\Windows\\system32";
            this.videoGrabber1.StoreDeviceSettingsInRegistry = true;
            this.videoGrabber1.SyncCommands = true;
            this.videoGrabber1.SynchronizationRole = VidGrab.TSynchronizationRole.sr_Master;
            this.videoGrabber1.Synchronized = false;
            this.videoGrabber1.SyncPreview = VidGrab.TSyncPreview.sp_Auto;
            this.videoGrabber1.TabIndex = 0;
            this.videoGrabber1.TextOverlay_Align = VidGrab.TTextOverlayAlign.tf_Left;
            this.videoGrabber1.TextOverlay_BkColor = 16777215;
            this.videoGrabber1.TextOverlay_Enabled = false;
// TODO: Code generation for '' failed because of Exception 'Invalid Primitive Type: System.IntPtr. Consider using CodeObjectCreateExpression.'.
            this.videoGrabber1.TextOverlay_FontColor = 16776960;
            this.videoGrabber1.TextOverlay_GradientColor = 8388608;
            this.videoGrabber1.TextOverlay_GradientMode = VidGrab.TTextOverlayGradientMode.gm_Disabled;
            this.videoGrabber1.TextOverlay_HighResFont = true;
            this.videoGrabber1.TextOverlay_Left = 0;
            this.videoGrabber1.TextOverlay_Right = -1;
            this.videoGrabber1.TextOverlay_Scrolling = false;
            this.videoGrabber1.TextOverlay_ScrollingSpeed = 1;
            this.videoGrabber1.TextOverlay_Selector = 0;
            this.videoGrabber1.TextOverlay_Shadow = true;
            this.videoGrabber1.TextOverlay_ShadowColor = 0;
            this.videoGrabber1.TextOverlay_ShadowDirection = VidGrab.TCardinalDirection.cd_Center;
            this.videoGrabber1.TextOverlay_String = resources.GetString("videoGrabber1.TextOverlay_String");
            this.videoGrabber1.TextOverlay_TargetDisplay = -1;
            this.videoGrabber1.TextOverlay_Top = 0;
            this.videoGrabber1.TextOverlay_Transparent = true;
            this.videoGrabber1.ThirdPartyDeinterlacer = "";
            this.videoGrabber1.TranslateMouseCoordinates = true;
            this.videoGrabber1.TunerFrequency = -1;
            this.videoGrabber1.TunerMode = VidGrab.TTunerMode.tm_TVTuner;
            this.videoGrabber1.TVChannel = 0;
            this.videoGrabber1.TVCountryCode = 0;
            this.videoGrabber1.TVTunerInputType = VidGrab.TTunerInputType.TunerInputCable;
            this.videoGrabber1.TVUseFrequencyOverrides = false;
            this.videoGrabber1.UseClock = true;
            this.videoGrabber1.VCRHorizontalLocking = false;
            this.videoGrabber1.Version = "v9.2.1.4 (build 140701) - Copyright ©2014 Datastead";
            this.videoGrabber1.VideoCompression_DataRate = -1;
            this.videoGrabber1.VideoCompression_KeyFrameRate = 15;
            this.videoGrabber1.VideoCompression_PFramesPerKeyFrame = 0;
            this.videoGrabber1.VideoCompression_Quality = 1D;
            this.videoGrabber1.VideoCompression_WindowSize = -1;
            this.videoGrabber1.VideoCompressor = 0;
            this.videoGrabber1.VideoControlSettings = true;
            this.videoGrabber1.VideoCursor = VidGrab.TCursors.cr_Default;
            this.videoGrabber1.VideoDevice = -1;
            this.videoGrabber1.VideoFormat = -1;
            this.videoGrabber1.VideoFromImages_BitmapsSortedBy = VidGrab.TFileSort.fs_TimeAsc;
            this.videoGrabber1.VideoFromImages_RepeatIndefinitely = false;
            this.videoGrabber1.VideoFromImages_SourceDirectory = "C:\\Windows\\system32";
            this.videoGrabber1.VideoFromImages_TemporaryFile = "SetOfBitmaps01.dat";
            this.videoGrabber1.VideoInput = -1;
            this.videoGrabber1.VideoProcessing_Brightness = 0;
            this.videoGrabber1.VideoProcessing_Contrast = 0;
            this.videoGrabber1.VideoProcessing_Deinterlacing = VidGrab.TVideoDeinterlacing.di_Disabled;
            this.videoGrabber1.VideoProcessing_FlipHorizontal = false;
            this.videoGrabber1.VideoProcessing_FlipVertical = false;
            this.videoGrabber1.VideoProcessing_GrayScale = false;
            this.videoGrabber1.VideoProcessing_Hue = 0;
            this.videoGrabber1.VideoProcessing_InvertColors = false;
            this.videoGrabber1.VideoProcessing_Pixellization = 1;
            this.videoGrabber1.VideoProcessing_Rotation = VidGrab.TVideoRotation.rt_0_deg;
            this.videoGrabber1.VideoProcessing_RotationCustomAngle = 45.5D;
            this.videoGrabber1.VideoProcessing_Saturation = 0;
            this.videoGrabber1.VideoQualitySettings = true;
            this.videoGrabber1.VideoRenderer = VidGrab.TVideoRenderer.vr_AutoSelect;
            this.videoGrabber1.VideoRendererExternal = VidGrab.TVideoRendererExternal.vre_None;
            this.videoGrabber1.VideoRendererExternalIndex = -1;
            this.videoGrabber1.VideoSize = -1;
            this.videoGrabber1.VideoSource = VidGrab.TVideoSource.vs_VideoCaptureDevice;
            this.videoGrabber1.VideoSource_FileOrURL = "";
            this.videoGrabber1.VideoSource_FileOrURL_StartTime = ((long)(-1));
            this.videoGrabber1.VideoSource_FileOrURL_StopTime = ((long)(-1));
            this.videoGrabber1.VideoSubtype = -1;
            this.videoGrabber1.VideoVisibleWhenStopped = false;
            this.videoGrabber1.VirtualAudioStreamControl = -1;
            this.videoGrabber1.VirtualVideoStreamControl = -1;
            this.videoGrabber1.VuMeter = VidGrab.TVuMeter.vu_Disabled;
            this.videoGrabber1.WebcamStillCaptureButton = VidGrab.TWebcamStillCaptureButton.wb_Disabled;
            this.videoGrabber1.ZoomCoeff = 1000;
            this.videoGrabber1.ZoomXCenter = 0;
            this.videoGrabber1.ZoomYCenter = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.videoGrabber1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "Form1";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Form1";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private MediaFramework.MyVideoGrabber videoGrabber1;
    }
}

