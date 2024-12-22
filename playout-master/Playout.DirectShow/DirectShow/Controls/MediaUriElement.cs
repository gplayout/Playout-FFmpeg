﻿using System;
using System.Windows;
using System.Windows.Threading;
using Playout.DirectShow.MediaPlayers;

namespace Playout.DirectShow.Controls
{
    /// <summary>
    /// The MediaUriElement is a WPF control that plays media of a given
    /// Uri. The Uri can be a file path or a Url to media.  The MediaUriElement
    /// inherits from the MediaSeekingElement, so where available, seeking is
    /// also supported.
    /// </summary>
    public class MediaUriElement : MediaSeekingElement
    {
        /// <summary>
        /// The current MediaUriPlayer
        /// </summary>
        protected MediaUriPlayer MediaUriPlayer
        {
            get
            {
                return MediaPlayerBase as MediaUriPlayer;
            }
        }

        #region VideoRenderer

        public static readonly DependencyProperty VideoRendererProperty =
            DependencyProperty.Register("VideoRenderer", typeof(VideoRendererType), typeof(MediaUriElement),
                new FrameworkPropertyMetadata(VideoRendererType.VideoMixingRenderer9,
                    new PropertyChangedCallback(OnVideoRendererChanged)));

        public VideoRendererType VideoRenderer
        {
            get { return (VideoRendererType)GetValue(VideoRendererProperty); }
            set { SetValue(VideoRendererProperty, value); }
        }

        private static void OnVideoRendererChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaUriElement)d).OnVideoRendererChanged(e);
        }

        protected virtual void OnVideoRendererChanged(DependencyPropertyChangedEventArgs e)
        {
            if (HasInitialized)
                PlayerSetVideoRenderer();
        }

        private void PlayerSetVideoRenderer()
        {
            var videoRendererType = VideoRenderer;
            MediaUriPlayer.Dispatcher.BeginInvoke((Action)delegate
            {
                MediaUriPlayer.VideoRenderer = videoRendererType;
            });
        }

        #endregion

        #region AudioRenderer

        public static readonly DependencyProperty AudioRendererProperty =
            DependencyProperty.Register("AudioRenderer", typeof(string), typeof(MediaUriElement),
                new FrameworkPropertyMetadata("Default DirectSound Device",
                    new PropertyChangedCallback(OnAudioRendererChanged)));

        /// <summary>
        /// The name of the audio renderer device to use
        /// </summary>
        public string AudioRenderer
        {
            get { return (string)GetValue(AudioRendererProperty); }
            set { SetValue(AudioRendererProperty, value); }
        }

        private static void OnAudioRendererChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaUriElement)d).OnAudioRendererChanged(e);
        }

        protected virtual void OnAudioRendererChanged(DependencyPropertyChangedEventArgs e)
        {
            if (HasInitialized)
                PlayerSetAudioRenderer();
        }

        private void PlayerSetAudioRenderer()
        {
            var audioDevice = AudioRenderer;

            MediaUriPlayer.Dispatcher.BeginInvoke((Action)delegate
            {
                /* Sets the audio device to use with the player */
                MediaUriPlayer.AudioRenderer = audioDevice;
            });
        }

        #endregion
        
        #region Source

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(MediaUriElement),
                new FrameworkPropertyMetadata(null,
                    new PropertyChangedCallback(OnSourceChanged)));

        /// <summary>
        /// The Uri source to the media.  This can be a file path or a
        /// URL source
        /// </summary>
        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //((MediaUriElement)d).OnSourceChanged(e);
        }

        public void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if(HasInitialized)
                PlayerSetSource();
        }

        private void PlayerSetSource()
        {
            var source = Source;
            var rendererType = VideoRenderer;

            MediaPlayerBase.Dispatcher.BeginInvoke((Action) delegate
            {
                /* Set the renderer type */
                MediaUriPlayer.VideoRenderer = rendererType;

                /* Set the source type */
                MediaUriPlayer.Source = source;

                Dispatcher.BeginInvoke((Action) delegate
                {
                    if (IsLoaded)
                        ExecuteMediaState(LoadedBehavior);
                    //else
                    //    ExecuteMediaState(UnloadedBehavior);
                });
            });
        }
        #endregion

        #region Loop

        public static readonly DependencyProperty LoopProperty =
            DependencyProperty.Register("Loop", typeof(bool), typeof(MediaUriElement),
                new FrameworkPropertyMetadata(false,
                    new PropertyChangedCallback(OnLoopChanged)));

        /// <summary>
        /// Gets or sets whether the media should return to the begining
        /// once the end has reached
        /// </summary>
        public bool Loop
        {
            get { return (bool)GetValue(LoopProperty); }
            set { SetValue(LoopProperty, value); }
        }

        private static void OnLoopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MediaUriElement)d).OnLoopChanged(e);
        }

        protected virtual void OnLoopChanged(DependencyPropertyChangedEventArgs e)
        {
            if (HasInitialized)
                PlayerSetLoop();
        }

        private void PlayerSetLoop()
        {
            var loop = Loop;
            MediaPlayerBase.Dispatcher.BeginInvoke((Action)delegate
            {
                MediaUriPlayer.Loop = loop;
            });
        }
        #endregion

        public override void EndInit()
        {
            PlayerSetVideoRenderer();
            PlayerSetAudioRenderer();
            PlayerSetLoop();
            PlayerSetSource();
            base.EndInit();
        }

        /// <summary>
        /// The Play method is overrided so we can
        /// set the source to the media
        /// </summary>
        public override void Play()
        {
            EnsurePlayerThread();
            base.Play();
        }

        /// <summary>
        /// The Pause method is overrided so we can
        /// set the source to the media
        /// </summary>
        public override void Pause()
        {
            EnsurePlayerThread();

            base.Pause();
        }

        /// <summary>
        /// Gets the instance of the media player to initialize
        /// our base classes with
        /// </summary>
        protected override MediaPlayerBase OnRequestMediaPlayer()
        {
            var player = new MediaUriPlayer();
            return player;
        }
    }
}
