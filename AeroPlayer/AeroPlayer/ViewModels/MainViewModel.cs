using AeroPlayer.Services.MusicPlayerGuiLayer;
using AeroPlayerService;
using System;
using System.Windows.Input;

namespace AeroPlayer.ViewModels
{
    public class MainViewModel : PropertyObject
    {

        //The Main view model contains the dashboard for the playing mechanisms from the player.
        public static readonly Player GuiPlayer = new Player();
        private string currentSong = "";
        private string currentPlayList = "";
        public PlayLoop LoopButtonImagePath
        {
            get
            {
                return GuiPlayer.SongManager.LoopType;
            }


        }
        private void ToggleLoopDelegate()
        {
            
            GuiPlayer.SongManager.ToggleLooping();
            onPropertyChanged("LoopButtonImagePath");
        }

        public DelegateCommand ToggleLoop { get; set; }

        public float Volume
        {
            get => GuiPlayer.Volume;
            set
            {
                GuiPlayer.Volume = value;
                onPropertyChanged("Volume");
            }
        }
        public double PlayBackLength { get => GuiPlayer.AudioMaxLength; }
        public double PlaybackPos
        {
            get => GuiPlayer.PlaybackPos;
            set { GuiPlayer.PlaybackPos = value; onPropertyChanged("PlaybackPos"); }
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
        private void MusicPlayerEventHandler(object sender, MusicPlayerEventArgs e)
        {
            //When song is played is the same as pressing pause/play button
            switch (e.EventType)
            {
                case MusicEventType.TogglePauseEvent:

                    onPropertyChanged("PlayingStatus");
                    break;
                case MusicEventType.Reset:
                    //Resets are when the current song can no longer be played for some reason (like user deletion ect.)
                    //When reset that means the current song and potentially playlist has changed.
                    CurrentSong = GuiPlayer.SongManager.CurrentSongDisplay;
                    CurrentPlayList = GuiPlayer.PlayListDisplay;
                    break;
                default:
                    return;
            }
        }

        public ICommand NextSongTrue { get; set; }
        public ICommand NextSongFalse { get; set; }
        public ICommand PlaySong { get; set; }
        public string PlayingStatus { get => GuiPlayer.PlayingStatus ? "Pause" : "Play"; }
        public MainViewModel()
        {
            NextSongTrue = new DelegateCommand(() => GuiPlayer.SongManager.NextSong(true));

            NextSongFalse = new DelegateCommand(() => GuiPlayer.SongManager.NextSong(false));

            PlaySong = new DelegateCommand(() =>
            {

                try
                {

                    GuiPlayer.AudioPauseToggleStatus();

                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e);
                }
            });
            ToggleLoop = new DelegateCommand(ToggleLoopDelegate);

            GuiPlayer.OnPlaybackChange += delegate (object sender, EventArgs e)
            {
                onPropertyChanged("PlaybackPos");
            };
            GuiPlayer.SongManager.OnSongChange += delegate (object sender, MusicManagerEventArgs e)
            {
                CurrentSong = e.SongDisplay;
                CurrentPlayList = e.PlayListDisplay;
            };

            GuiPlayer.SongManager.RandomInPlayList();
            GuiPlayer.MusicPlayerEvent += MusicPlayerEventHandler;
        }



    }
}
