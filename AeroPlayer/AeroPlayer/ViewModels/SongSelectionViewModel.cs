using AeroPlayerService;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AeroPlayer.Views.Dialogs;

namespace AeroPlayer.ViewModels
{
    public class SongSelectionViewModel : PropertyObject
    {
        PlayList currentPlaylist;
        public PlayList CurrentPlayList
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
        ObservableCollection<PlayList> playLists;
        public ObservableCollection<PlayList> PlayLists
        {
            get => playLists;
            set
            {
                playLists = value;
                onPropertyChanged("PlayLists");
            }
        }
        private Song song;
        public Song Song { get => song; set { song = value; onPropertyChanged("Song"); } }
        DelegateCommand LoadSongCommand;
        DelegateCommand AddSongCommand;
        DelegateCommand AddPlayListCommand;
        DelegateCommand ChangeSongCommand;

        public ICommand LoadSong
        {
            get
            {


                return LoadSongCommand;

            }

        }
        public ICommand ChangeSong
        {
            get
            {


                return ChangeSongCommand;

            }

        }
        public ICommand AddPlaylist
        {
            get
            {
                return AddPlayListCommand;
            }
        }
        public ICommand AddSong
        {
            get
            {
                return AddSongCommand;
            }
        }
        public void AddPlayListDelegate()
        {
            var newPlaylist = player.SongManager.CreateNewPlayList(RunInputDialog("Enter new playlist name"));
            PlayLists.Add(newPlaylist);

        }
        public void ChangeSongDelegate()
        {
            string name = RunInputDialog("Enter new name for song");
            this.Song.SongDisplay = name;
        }
        private string RunInputDialog(string message)
        {
            string output =  null;
            var dialog = new InputDialog(message);
            if (dialog.ShowDialog() == true)
            {
                output = dialog.ResponseBox.Text;
            }
            return output;
        }
        private void AddSongDelegate()
        {
            string playlist;


            if (player.SongManager.CurrentPlayList == null)
            {
                playlist = RunInputDialog("Enter new playlist name" );
            }
            else
            {
                playlist = this.CurrentPlayList.DisplayName;
            }
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Mp3 Files (*.mp3)|*.mp3"
            };


            if (fileDialog.ShowDialog() == true)
            {
                string[] filenames = fileDialog.FileNames;

                bool worked = player.SongManager.AddSongs(playlist, filenames);
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
            player.SongManager.SetCurrentlyPlaying(Song.PlayListName, Song.AbsoluteName);
        }

        private static MusicPlayer player;

        private void AddSongChangeDelegate()
        {
            player.SongManager.OnAddSong += delegate (object sender, PlayList playlist)
            {
                
                int index = 0;
                for (int i = 0; i < PlayLists.Count; i++)
                {
                    if (PlayLists[i].DisplayName == playlist.DisplayName)
                    {

                        index = i;
                        break;
                    }

                }
                Console.WriteLine("index = " + index);

                if (index > PlayLists.Count - 1)
                    PlayLists.Add(playlist);
                else
                    PlayLists[index] = playlist;
                onPropertyChanged("PlayLists");


            };
        }
        public SongSelectionViewModel()
        {
            player = MainViewModel.player;
            ChangeSongCommand = new DelegateCommand(ChangeSongDelegate);
            LoadSongCommand = new DelegateCommand(LoadSongDelegate);
            AddSongCommand = new DelegateCommand(AddSongDelegate);
            AddPlayListCommand = new DelegateCommand(AddPlayListDelegate);
            AddSongChangeDelegate();
            PlayLists = new ObservableCollection<PlayList>(player.SongManager.PlayLists);

        }


    }
}
