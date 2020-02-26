using AeroPlayerService;
using Microsoft.Win32;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace AeroPlayer.ViewModels
{
    public class SongSelectionViewModel : PropertyObject
    {
        PlayListDetail currentPlaylist;
        public PlayListDetail CurrentPlayList
        {

            get
            {
                return currentPlaylist;

            }
            set
            {

                currentPlaylist = value;
                onPropertyChanged("CurrentPlayList");
            }
        }
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
        private SongDetail song;
        public SongDetail Song { get => song; set { song = value; onPropertyChanged("Song"); } }
        DelegateCommand LoadSongCommand;
        DelegateCommand AddSongCommand;

        public ICommand LoadSong
        {
            get
            {


                return LoadSongCommand;

            }

        }
        public ICommand AddSong
        {
            get
            {
                return AddSongCommand;
            }
        }
        private void AddSongDelegate()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Mp3 Files (*.mp3)|*.mp3";


            if (fileDialog.ShowDialog() == true)
            {
                string[] filenames = fileDialog.FileNames;
                string playlist = this.CurrentPlayList.DisplayName;

              

               bool worked =  player.SongManager.AddSongs(playlist, filenames);
               if (worked)
                {
                    Console.WriteLine("Success");

                }
                else
                {
                    Console.WriteLine("Error..");
                }
            }

        }
        private void LoadSongDelegate()
        {
            if (Song == null)
                return;
            player.SongManager.SetSong(Song.PlayListName, Song.SongName);
        }

        private static MusicPlayer player;


        public SongSelectionViewModel()
        {
            player = MainViewModel.player;
            LoadSongCommand = new DelegateCommand(LoadSongDelegate);
            AddSongCommand = new DelegateCommand(AddSongDelegate);
            player.SongManager.OnAddSong += delegate (object sender, PlayListDetail playlist)
            {
                Console.WriteLine("delegate...");
                int index = 0;
                for (int i = 0; i < PlayLists.Count; i++)
                {
                    if (PlayLists[i].DisplayName == playlist.DisplayName)
                    {

                        index = i;
                        break;
                    }

                }
                PlayLists[index] = playlist;
                onPropertyChanged("PlayLists");


            };
            PlayLists = new ObservableCollection<PlayListDetail>(player.SongManager.GetPlayListDetails());

        }


    }
}
