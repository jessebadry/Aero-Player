using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    public delegate void NewPlaybackHandler(object sender, EventArgs e);

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
        public event NewPlaybackHandler OnPlaybackChange;
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
            get => mp3Reader.TotalTime.TotalSeconds;
        }
        public double PlaybackPos
        {
            get
            {
                double time = mp3Reader.CurrentTime.TotalSeconds;
                if (time > AudioMaxLength)
                    AudioOut.Stop();
                return time;
            }
            set
            {
                try
                {

                    if (mp3Reader != null)
                    {
                        TimeSpan time = TimeSpan.FromSeconds(value);

                        if (time.TotalSeconds < AudioMaxLength)
                            mp3Reader.CurrentTime = time;
                    }
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                }


            }

        }
        private Timer timer;
        //
        private void UpdatePlaybackPosition()
        {
            PlaybackPos = mp3Reader.CurrentTime.TotalSeconds;
            OnPositionChange();
        }
        private void StartPositionListener()
        {

            timer = new Timer(300);
            timer.Elapsed += delegate (object sender, ElapsedEventArgs e)
            {
                UpdatePlaybackPosition();
            };
            timer.AutoReset = true;
            timer.Enabled = true;
        }
        protected virtual void OnPositionChange()
        {

            NewPlaybackHandler handler = OnPlaybackChange;
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
            catch (Exception)
            {
                //Song not initialize so try and play new song
                SongManager.NextSong(true);
            }

            InvokeMusicPlayerEvent(MusicEventType.TogglePauseEvent, true);

        }
        public static void Main(string[] args)
        {
            //Debug method
            MusicPlayer player = new MusicPlayer();
            player.SongManager.NextSong(DEFAULT_PLAYLIST_DIRECTION);
            player.SongManager.LoopPlayList = false;

            Console.WriteLine("AeroPlayerService  Testing Interface....");

            while (true)
            {
                Console.WriteLine("Enter To Skip Song");
                string input = Console.ReadLine();

                if (!string.IsNullOrEmpty(input))
                {
                    var parameters = input.Split(' ');
                    string command = parameters[0];
                    switch (command)
                    {
                        case "vol":
                            bool worked = float.TryParse(parameters[1], out float volumeOffset);
                            if (worked)
                                player.Volume = volumeOffset;
                            else
                                Console.WriteLine("Invalid volume");
                            break;

                        case "p":
                            player.PauseToggle();
                            break;

                        case "skip":
                            bool next = (parameters[1] == "true");
                            player.PlayNextSong(next);
                            break;

                        case "device":
                            player.AudioOut.DeviceNumber = int.Parse(parameters[1]);
                            Console.WriteLine("new device = " + player.AudioOut.DeviceNumber);
                            break;
                        case "loop":
                            player.SongManager.ToggleLooping();
                            Console.WriteLine("Current looping is .. " + player.SongManager.LoopType);
                            break;


                    }

                }
                else
                {

                    player.SongManager.NextSong(DEFAULT_PLAYLIST_DIRECTION);
                }




            }


        }
        public bool NotPlaying()
        {
            return mp3Reader == null;
        }
        private void LoadSong(string song_name)
        {
            AudioOut.Stop();
            AudioOut.Dispose();

            if (mp3Reader != null)
            {
                mp3Reader.Close();
                mp3Reader.Dispose();
            }


            mp3Reader = new Mp3FileReader(song_name);
            AudioOut.Init(mp3Reader);

            PlaybackPos = mp3Reader.TotalTime.TotalSeconds;
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
            //Essentially handels what to do automatically after a song ends..

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
