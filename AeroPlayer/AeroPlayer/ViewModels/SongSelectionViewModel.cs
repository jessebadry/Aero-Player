using AeroPlayerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace AeroPlayer.ViewModels
{
    public class SongSelectionViewModel : PropertyObject
    {
        private SongDetail song;
        public SongDetail Song { get => song; set { song = value; onPropertyChanged("Song"); } }
        DelegateCommand LoadSongCommand;
        public ICommand LoadSong
        {
            get
            {


                return LoadSongCommand;

            }

        }
        private void LoadSongDelegate()
        {
            if (Song == null)
                return;
            player.SongManager.SetSong(Song.PlayListName, Song.SongName);
        }

        private static MusicPlayer player;

        ObservableCollection<PlayListDetail> playLists;
        public ObservableCollection<PlayListDetail> PlayLists
        {
            get => playLists;
            set
            {
                playLists = value;
                onPropertyChanged("PlayLists");
            }
        }
        public SongSelectionViewModel()
        {
            player = MainViewModel.player;
            LoadSongCommand = new DelegateCommand(LoadSongDelegate);

            PlayLists = new ObservableCollection<PlayListDetail>(player.SongManager.GetPlayListDetails());

        }


    }
}
