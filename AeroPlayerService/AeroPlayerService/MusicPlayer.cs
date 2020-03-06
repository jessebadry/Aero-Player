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
        NewSongEvent
    }
    public delegate void MusicPlayerEvent(object sender, MusicPlayerEventArgs e);

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
    public class MusicPlayer : PropertyObject
    {
        const float MAX_VOLUME = 1F; // Max volume for NAudio WaveOutEvent..
        const bool DEFAULT_PLAYLIST_DIRECTION = true; // forward.



        WaveOutEvent AudioOut = new WaveOutEvent();
        Mp3FileReader mp3Reader;



        PlaybackState UserDefinedPlayState = PlaybackState.Stopped;

        //Object that manages the songs the player uses.
        public readonly MusicManager SongManager;


        //Events
        public event MusicPlayerEvent MusicPlayerEvent;
        public event NewPlaybackEventHandler OnPlaybackChange;
        //
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
                else
                {

                }
                AudioOut.Volume = value;

            }
        }
        public double AudioMaxLength
        {
            get
            {
                double seconds;
                if (!nullAudio())
                    seconds = mp3Reader.TotalTime.TotalSeconds;
                else return 0;

                return seconds;

            }
        }
        public double PlaybackPos
        {
            get
            {
                double time;
                if (!nullAudio())
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
                try
                {


                    if (!nullAudio())
                    {
                        TimeSpan time = TimeSpan.FromSeconds(value);
                        if (time.TotalSeconds < AudioMaxLength)
                            mp3Reader.CurrentTime = time;
                    }
                }
                catch (Exception)
                {
                }


            }

        }
        private Timer timer;
        //

        private void StartPositionListener()
        {

            timer = new Timer(300);
            timer.Elapsed += delegate (object sender, ElapsedEventArgs e)
            {
                OnPositionChange();
            };
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        protected virtual void OnPositionChange()
        {

            NewPlaybackEventHandler handler = OnPlaybackChange;
            handler?.Invoke(this, new EventArgs());
        }
        //INFORMATION
        public bool PlayingStatus
        {
            get
            {
                return AudioOut.PlaybackState == PlaybackState.Playing;
            }
        }


        private string song_display;
        public string SongDisplay
        {
            get => song_display;

            set
            {
                song_display = Path.GetFileNameWithoutExtension(value);
                onPropertyChanged("SongDisplay");
            }
        }
        private string playlist_display;
        public string PlayListDisplay
        {
            get => playlist_display;
            set
            {
                playlist_display = Path.GetFileName(value);

            }
        }



        public void PlayNextSong(bool next)
        {
            SongManager.NextSong(next);
        }

        //Toggles Audio playing=>paused, pause=>playing.
        //
        //If song is not initailized, music player will request new song from SongManager.
        public void PauseToggle()
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

        public bool nullAudio()
        {
            return mp3Reader == null;
        }
        private void LoadSong(string song_name)
        {
            if (song_name == null)
                return;
            AudioOut.Stop();
            AudioOut.Dispose();

            if (!nullAudio())
            {
                mp3Reader.Close();
                mp3Reader.Dispose();
            }


            mp3Reader = new Mp3FileReader(song_name);
            AudioOut.Init(mp3Reader);

            PlaybackPos = 0;
        }
        public void PlaySong(string playlist, string song_name)
        {



            AudioOut.PlaybackStopped -= MusicStoppedHandler; // Remove MusicHandler as Current instance of AudioOut is being disposed.
            LoadSong(song_name);
            AudioOut.PlaybackStopped += MusicStoppedHandler;

            if (UserDefinedPlayState == PlaybackState.Playing)
            {
                AudioOut.Play();
            }

            SongDisplay = song_name;
            PlayListDisplay = playlist;

        }

        private void MusicStoppedHandler(object sender, StoppedEventArgs e)
        {
            // handles what to do automatically after a song ends..
            Console.WriteLine("stopped");
            switch (SongManager.LoopType)
            {
                case PlayLoop.SingleLoop:
                    LoadSong(SongManager.SongAbsolute);
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


        public MusicPlayer()
        {

            SongManager = MusicManager.Instance;

            SongManager.OnSongChange += delegate (object sender, MusicManagerEventArgs e)
            {

                PlaySong(e.PlayList, e.NewSong);


            };
            StartPositionListener();
        }


        protected virtual void InvokeMusicPlayerEvent(MusicEventType type, object data)
        {
            MusicPlayerEvent handler = MusicPlayerEvent;
            handler?.Invoke(this, new MusicPlayerEventArgs(type, data));
        }


    }
}
