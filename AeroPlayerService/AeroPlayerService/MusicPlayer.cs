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
        public MusicManager SongManager {get;}

        bool disposed = false;
        Timer PlayBackUpdateTimer;

        readonly WaveOutEvent AudioOut = new WaveOutEvent();
        Mp3FileReader mp3Reader;

        //Object that manages the songs the player uses.

        //Events
        public event MusicPlayerEventHandler MusicPlayerEvent;
        public event NewPlaybackEventHandler OnPlaybackChange;

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
                mp3Reader.Close();
                mp3Reader.Dispose();
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
                    seconds = mp3Reader.TotalTime.TotalSeconds;
                else return ZERO_AUDIO;

                return seconds;
            }
        }
        public double PlaybackPos
        {
            get
            {
                double time;
                if (!AudioIsNull())
                {
                    time = mp3Reader.CurrentTime.TotalSeconds;
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
                        if (time.TotalSeconds < AudioMaxLength)
                            mp3Reader.CurrentTime = time;
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

            PlayBackUpdateTimer = new Timer(300);
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

        public string SongDisplay
        {
            get => SongManager.CurrentSongDisplay;

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
        public bool AudioIsNull() => mp3Reader == null;
        private void LoadSong(string songName)
        {
            if (songName == null)
                return;

            AudioOut.Stop();
            AudioOut.Dispose();

            if (!AudioIsNull())
            {
                mp3Reader.Close();
                mp3Reader.Dispose();
            }

            mp3Reader = new Mp3FileReader(songName);
            AudioOut.Init(mp3Reader);

            PlaybackPos = 0;
            onPropertyChanged("SongDisplay");
        }
        public void PlaySong(string playlist, string songName)
        {
            AudioOut.PlaybackStopped -= MusicStoppedHandler; // Remove MusicHandler as Current instance of AudioOut is being disposed.
            LoadSong(songName);
            AudioOut.PlaybackStopped += MusicStoppedHandler;

            if (UserDefinedPlayState == PlaybackState.Playing)
            {
                try
                {
                    AudioOut.Play();
                }
                catch (Exception) { }
            }

            PlayListDisplay = playlist;

        }
        private void MusicStoppedHandler(object sender, StoppedEventArgs e)
        {
            // handles what to do automatically after a song ends..
            Console.WriteLine("stopped");
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
                PlaySong(e.PlayList, e.NewSong);
            };
            StartPositionListener();
        }
        protected void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                mp3Reader.Dispose();
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
