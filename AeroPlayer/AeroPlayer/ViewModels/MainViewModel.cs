using AeroPlayerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace AeroPlayer.ViewModels
{
    public class MainViewModel : PropertyObject
    {
        public static MusicPlayer player;
        private string currentSong = "";
        private string currentPlayList = "";
        public float Volume
        {
            get => player.Volume;
            set
            {
                player.Volume = value;
                onPropertyChanged("Volume");
            }
        }
        public double PlayBackLength { get => player.AudioMaxLength; }
        public double PlaybackPos
        {
            get => player.PlaybackPos;
            set { player.PlaybackPos = value; onPropertyChanged("PlaybackPos"); }
        }
        public string CurrentSong
        {
            get => currentSong;
            set
            {
                currentSong = value;
                onPropertyChanged("CurrentSong");
                onPropertyChanged("PlayBackLength");
            }
        }
        public string CurrentPlayList
        {
            get => currentPlayList;
            set
            {
                currentPlayList = value;
                onPropertyChanged("CurrentPlayList");
            }
        }


        

        public MainViewModel()
        {
            player = new MusicPlayer();
            player.OnPlaybackChange += delegate (object sender, EventArgs e)
             {
                 PlaybackPos = player.PlaybackPos;
             };
            player.SongManager.OnSongChange += delegate (object sender, MusicManagerEventArgs e)
            {
                CurrentSong = player.SongDisplay;
                CurrentPlayList = player.PlayListDisplay;
            };

            player.SongManager.RandomInPlayList();
            player.MusicPlayerEvent += MusicPlayerEventHandler;
        }

        private void MusicPlayerEventHandler(object sender, MusicPlayerEventArgs e)
        {
            //When song is played is the same as pressing pause/play button
            switch (e.EventType)
            {
                case MusicEventType.NewSongEvent:
                    CurrentSong = player.SongDisplay;
                    CurrentPlayList = player.PlayListDisplay;

                    goto case MusicEventType.TogglePauseEvent;
                case MusicEventType.TogglePauseEvent:
                    onPropertyChanged("PlayingStatus");


                    break;

                default:
                    return;
            }
        }

        public ICommand NextSongTrue
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player.SongManager.NextSong(true);
                });
            }
        }
        public ICommand NextSongFalse
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player.PlayNextSong(false);
                });
            }
        }
        public ICommand PlaySong
        {
            get
            {

                return new DelegateCommand(() =>
                {

                    try
                    {

                        player.PauseToggle();

                    }
                    catch (NullReferenceException e)
                    {
                        Console.WriteLine(e);
                    }
                });
            }
        }
        public string PlayingStatus
        {
            get
            {

                return player.PlayingStatus ? "Pause" : "Play";
            }

        }




    }
}
