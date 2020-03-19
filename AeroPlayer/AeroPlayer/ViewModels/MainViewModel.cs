using AeroPlayer.Services.MusicPlayerGuiLayer;
using AeroPlayerService;
using System;
using System.Windows.Input;

namespace AeroPlayer.ViewModels
{
    public class MainViewModel : PropertyObject
    {
        public static Player player;
        private string currentSong = "";
        private string currentPlayList = "";
        public float Volume
        {
            get => player.player.Volume;
            set
            {
                player.player.Volume = value;
                onPropertyChanged("Volume");
            }
        }
        public double PlayBackLength { get => player.player.AudioMaxLength; }
        public double PlaybackPos
        {
            get => player.player.PlaybackPos;
            set { player.player.PlaybackPos = value; onPropertyChanged("PlaybackPos"); }
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
            player = new Player();
            player.player.OnPlaybackChange += delegate (object sender, EventArgs e)
            {
                 onPropertyChanged("PlaybackPos");
            };
            player.player.SongManager.OnSongChange += delegate (object sender, MusicManagerEventArgs e)
            {
                CurrentSong = player.player.SongDisplay;
                CurrentPlayList = player.player.PlayListDisplay;
            };

            player.player.SongManager.RandomInPlayList();
            player.player.MusicPlayerEvent += MusicPlayerEventHandler;
        }

        private void MusicPlayerEventHandler(object sender, MusicPlayerEventArgs e)
        {
            //When song is played is the same as pressing pause/play button
            switch (e.EventType)
            {
                case MusicEventType.TogglePauseEvent:

                    onPropertyChanged("PlayingStatus");
                    break;
                case MusicEventType.Reset:

                    CurrentSong = player.player.SongDisplay;
                    CurrentPlayList = player.player.PlayListDisplay;
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
                    player.player.SongManager.NextSong(true);
                });
            }
        }
        public ICommand NextSongFalse
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player.player.SongManager.NextSong(false);
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

                        player.player.AudioPauseToggleStatus();

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
                return player.player.PlayingStatus ? "Pause" : "Play";
            }
        }




    }
}
