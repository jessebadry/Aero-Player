using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace AeroPlayerService
{



    // Uses MusicQueue to select next song to play to audio.
    public class MusicPlayer
    {
        const float MAX_VOLUME = 1F; // Max volume for NAudio WaveOutEvent..


        private WaveOutEvent AudioOut = new WaveOutEvent();
        private Mp3FileReader mp3Reader;
        public readonly MusicManager SongManager = MusicManager.Instance;


        public void PlayNextSong()
        {
            SongManager.NextSong();
        }
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
            player.SongManager.NextSong();
            while (true)
            {
                Console.WriteLine("Press Enter to skip ");
                string input = Console.ReadLine();

                if (!string.IsNullOrEmpty(input))
                {
                    var parameters = input.Split(' ');
                    string command = parameters[0];
                    if (command == "vol")
                    {

                        float volumeOffset = float.Parse(parameters[1]);

                        player.Volume = volumeOffset;
                    }
                    else if (command == "pause")
                    {
                        player.PauseToggle();
                    }
                }
                else
                {
                    player.PlayNextSong();
                }



            }

        }
        public void PlaySong(string playlist, string song_name)
        {
            Console.WriteLine("Now Playing " + Path.GetFileName(song_name).Split('.')[0]);


            AudioOut.PlaybackStopped -= MusicStoppedHandler;
            AudioOut.Stop();


            AudioOut.Dispose();


            mp3Reader = new Mp3FileReader(song_name);
            AudioOut.Init(mp3Reader);
            AudioOut.Play();
            AudioOut.PlaybackStopped += MusicStoppedHandler;

        }

        private void MusicStoppedHandler(object sender, StoppedEventArgs e)
        {
            SongManager.NextSong();
        }


        public MusicPlayer()
        {

            SongManager.NewSongOutput += (object sender, MusicQueueEventArgs e) => { PlaySong(e.PlayList, e.NewSong); };

            SongManager.LoopPlayList = true;




        }
    }
}
