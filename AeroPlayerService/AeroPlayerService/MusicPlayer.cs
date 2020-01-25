using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace AeroPlayerService
{


    enum PlayLoop
    {
        SingleLoop,
        PlayListLoop,
        NoLoop
    }
    // Uses MusicQueue to select next song to play to audio.
    public class MusicPlayer : PropertyObject
    {
        const float MAX_VOLUME = 1F; // Max volume for NAudio WaveOutEvent..
        const bool DEFAULT_PLAYLIST_DIRECTION = true; // forward.

        private WaveOutEvent AudioOut = new WaveOutEvent();
        private Mp3FileReader mp3Reader;
        public readonly MusicManager SongManager = MusicManager.Instance;

        PlayLoop LoopType = PlayLoop.PlayListLoop;

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
                    AudioOut.Volume = 1;
                    return;
                }
                AudioOut.Volume = value;

            }
        }
        private string song_display;
        public string SongDisplay
        {
            get => song_display;

            set
            {
                song_display = Path.GetFileName(value).Split('.')[0];
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

        public void ToggleLooping()
        {

            if (LoopType == PlayLoop.PlayListLoop)
            {

                LoopType = PlayLoop.SingleLoop;
            }
            else if (LoopType == PlayLoop.SingleLoop)
            {
                LoopType = PlayLoop.NoLoop;
            }
            else if (LoopType == PlayLoop.NoLoop)
            {

                LoopType = PlayLoop.PlayListLoop;
            }


        }

        public void PlayNextSong(bool next)
        {
            SongManager.NextSong(next);
        }

        public void PauseToggle()
        {
            if (AudioOut.PlaybackState == PlaybackState.Playing)
                AudioOut.Pause();
            else
                AudioOut.Play();

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
                            player.ToggleLooping();
                            Console.WriteLine("Current looping is .. " + player.LoopType);
                            break;

                    }

                }
                else
                {

                    player.PlayNextSong(DEFAULT_PLAYLIST_DIRECTION);
                }
                Console.WriteLine("CurrentIndex= {0}", player.SongManager.CurrentPlayList.CurrentIndex);



            }


        }
        private void Play(string song_name)
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
            AudioOut.Play();

        }
        public void PlaySong(string playlist, string song_name)
        {
            Console.WriteLine("Now Playing " + Path.GetFileName(song_name).Split('.')[0]);
            Console.WriteLine("From playlist  =  " + playlist);

            AudioOut.PlaybackStopped -= MusicStoppedHandler;
            Play(song_name);
            AudioOut.PlaybackStopped += MusicStoppedHandler;



        }

        private void MusicStoppedHandler(object sender, StoppedEventArgs e)
        {
            //Essentially handels what to do automatically after a song ends..

            switch (LoopType)
            {
                case PlayLoop.SingleLoop:
                    Play(SongManager.CurrentSong);

                    break;
                case PlayLoop.PlayListLoop:
                    SongManager.LoopPlayList = true;
                    SongManager.NextSong(DEFAULT_PLAYLIST_DIRECTION);
                    break;
                case PlayLoop.NoLoop:
                    SongManager.LoopPlayList = false;
                    SongManager.NextSong(DEFAULT_PLAYLIST_DIRECTION);
                    break;
            }



        }


        public MusicPlayer()
        {
            SongManager.OnSongChange += delegate(object sender, MusicManagerEventArgs e) { PlaySong(e.PlayList, e.NewSong); };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
