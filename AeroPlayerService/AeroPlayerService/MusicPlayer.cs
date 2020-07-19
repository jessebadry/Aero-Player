using NAudio.Wave;
using System;
using System.IO;
using System.Timers;

namespace AeroPlayerService
{
    public enum MusicEventType
    {
        //Passes bool of toggle
        TogglePauseEvent,
        // Passes String of song name
        NewSongEvent,
        // Passes nothing
        Reset,
    }
    public delegate void MusicPlayerEventHandler(object sender, MusicPlayerEventArgs e);
    public delegate void MusicPlayerErrorHandler(object sender, string errorMsg);
    public delegate void NewPlaybackEventHandler(object sender, EventArgs e);

    public class MusicPlayerEventArgs : EventArgs
    {
        public MusicEventType EventType { get; }
        public object Data { get; }
        public MusicPlayerEventArgs(MusicEventType EventType, object data)
        {
            this.EventType = EventType;
            Data = data;
        }

    }

    // Uses MusicQueue to select next song to play to audio.
    public class MusicPlayer : PropertyObject, IDisposable
    {
        const double ZERO_AUDIO = 0.0;
        const float MAX_VOLUME = 1F; // Max volume for NAudio WaveOutEvent..
        const bool DEFAULT_PLAYLIST_DIRECTION = true; // forward.

        /// <summary>
        /// Controls the song that the player uses, implements Singleton.
        /// </summary>
        public MusicManager SongManager { get; }

        bool disposed = false;
        Timer PlayBackUpdateTimer;

        readonly WaveOutEvent AudioOut = new WaveOutEvent();
        MediaFoundationReader AudioReader;


        //Events
        public event MusicPlayerEventHandler MusicPlayerEvent;
        public event NewPlaybackEventHandler OnPlaybackChange;
        public event MusicPlayerErrorHandler OnError;

        public void InvokeError(string msg)
        {
            OnError.Invoke(this, msg);
        }

        PlaybackState UserDefinedPlayState = PlaybackState.Stopped;

        /// <summary>
        /// Wipes currently playing song from audioOut and settings.
        /// </summary>
        private void AudioReset()
        {
            Console.WriteLine("Resetting audio");
            AudioOut.Stop();
            AudioOut.Dispose();
            if (!AudioIsNull())
            {
                AudioReader.Close();
                AudioReader.Dispose();
            }

            PlayListDisplay = "";

            InvokeMusicPlayerEvent(MusicEventType.Reset, null);

        }

        //SETTINGS
        public float Volume
        {
            get
            {
                return AudioOut.Volume;
            }
            set
            {
                if (value > MAX_VOLUME)
                {
                    AudioOut.Volume = MAX_VOLUME;
                    return;
                }
                AudioOut.Volume = value;

            }
        }
        /// <summary>
        /// returns the Length of Loaded Audio in seconds
        /// </summary>
        public double AudioMaxLength
        {
            get
            {
                double seconds;
                if (!AudioIsNull())
                    seconds = AudioReader.TotalTime.TotalSeconds;
                else return ZERO_AUDIO;

                return seconds;
            }
        }
        public double PlaybackPos
        {
            get
            {
                double time;
                if (!AudioIsNull() && AudioReader.CurrentTime != null)
                {
                    time = AudioReader.CurrentTime.TotalSeconds;
                }
                else
                {
                    return 0;
                }
                return time;
            }
            set
            {
                if (!AudioIsNull())
                {
                    try
                    {
                        TimeSpan time = TimeSpan.FromSeconds(value);
                        if (time.TotalSeconds < AudioMaxLength && AudioReader != null)
                            AudioReader.CurrentTime = time;
                    }
                    catch (NullReferenceException)
                    {

                    }
                }
            }
        }
        //
        private void StartPositionListener()
        {

            PlayBackUpdateTimer = new Timer(50);
            PlayBackUpdateTimer.Elapsed += delegate (object sender, ElapsedEventArgs e)
            {
                OnPositionChange();
            };
            PlayBackUpdateTimer.AutoReset = true;
            PlayBackUpdateTimer.Enabled = true;
        }
        //INFORMATION
        public bool PlayingStatus
        {
            get
            {
                return AudioOut.PlaybackState == PlaybackState.Playing;
            }
        }



        private string playlistDisplay;
        public string PlayListDisplay
        {
            get => playlistDisplay;
            set
            {

                playlistDisplay = Path.GetFileName(value);

            }
        }
        //System Logic
        /// <summary>
        /// Toggles Audio playing=>paused, pause=>playing.
        /// <br/>If song is not initailized, music player will request new song from SongManager.
        /// </summary>
        public void AudioPauseToggleStatus()
        {
            try
            {
                if (AudioOut.PlaybackState == PlaybackState.Playing)
                {
                    AudioOut.Pause();
                    UserDefinedPlayState = PlaybackState.Paused;
                }
                else
                {
                    AudioOut.Play();
                    UserDefinedPlayState = PlaybackState.Playing;
                }
            }
            catch (Exception e)
            {
                //Song not initialize so try and play new song
                Console.WriteLine(e);
                SongManager.NextSong(true);
            }

            InvokeMusicPlayerEvent(MusicEventType.TogglePauseEvent, true);

        }

        /// <summary>
        /// Returns if mp3Reader is unusable
        /// </summary>
        /// <returns></returns>
        public bool AudioIsNull() => AudioReader == null;
        private void LoadSong(string songName)
        {
            if (songName == null)
                return;

            AudioOut.Stop();
            AudioOut.Dispose();

            if (!AudioIsNull())
            {
                AudioReader.Close();
                AudioReader.Dispose();
            }

            AudioReader = new MediaFoundationReader(songName);
            AudioOut.Init(AudioReader);

            PlaybackPos = 0;
        }
        public void PlaySong(string playlist, string songName)
        {
            // Remove MusicHandler because  Current instance of AudioOut is being disposed.

            AudioOut.PlaybackStopped -= MusicStoppedHandler;
            LoadSong(songName);
            AudioOut.PlaybackStopped += MusicStoppedHandler;

            if (UserDefinedPlayState == PlaybackState.Playing)
            {
                try
                {
                    AudioOut.Play();
                }
                catch (Exception e)
                {
                    InvokeError(e.ToString());
                }
            }

            PlayListDisplay = playlist;

        }
        private void MusicStoppedHandler(object sender, StoppedEventArgs e)
        {
            // handles what to do automatically after a song ends..
            switch (SongManager.LoopType)
            {
                case PlayLoop.SingleLoop:
                    PlaybackPos = 0;
                    AudioOut.Play();
                    break;
                case PlayLoop.PlayListLoop:
                    SongManager.NextSong(DEFAULT_PLAYLIST_DIRECTION);
                    break;
                case PlayLoop.NoLoop:
                    SongManager.NextSong(DEFAULT_PLAYLIST_DIRECTION);
                    break;
                case PlayLoop.Shuffle:
                    SongManager.RandomSongInPlayList();
                    break;
            }

        }

        protected void OnPositionChange()
        {
            NewPlaybackEventHandler handler = OnPlaybackChange;
            handler?.Invoke(this, new EventArgs());
        }
        protected void InvokeMusicPlayerEvent(MusicEventType type, object data)
        {
            MusicPlayerEventHandler handler = MusicPlayerEvent;
            handler?.Invoke(this, new MusicPlayerEventArgs(type, data));
        }


        public MusicPlayer()
        {
            SongManager = MusicManager.Instance;

            SongManager.OnPlaylistChange += delegate (object sender, PlayList playlist, bool deleted)
            {
                if (PlayListDisplay == playlist.DisplayName && deleted)
                    AudioReset();
            };
            SongManager.OnSongChange += delegate (object sender, MusicManagerEventArgs e)
            {
                PlaySong(e.PlayList.RelativePathName, e.PlayList.CurrentSong.FilePath);
            };
            StartPositionListener();

            Song current = SongManager.CurrentPlayList?.CurrentSong;
            if (current != null)
                PlaySong(current.RelativePlayListPath, current.FilePath);
        }
        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                AudioReader.Dispose();
                AudioOut.Dispose();
            }

            disposed = true;
        }
        public void Dispose()
        {

            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
