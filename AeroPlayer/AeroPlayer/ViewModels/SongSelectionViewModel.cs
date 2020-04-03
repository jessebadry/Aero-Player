using AeroPlayerService;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using AeroPlayer.Views.Dialogs;
using System.Linq;
using AeroPlayer.Services.MusicPlayerGuiLayer;
using System.Diagnostics;
using System.IO;

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
            get => MainViewModel.GuiPlayer.PlayLists;
            //set
            //{
            //    playLists = value;
            //    onPropertyChanged("PlayLists");
            //}
        }
        Song song;
        MusicManager SongManager;
        public Song Song { get => song; set { song = value; onPropertyChanged("Song"); } }
        DelegateCommand LoadSongCommand;
        DelegateCommand AddSongCommand;
        DelegateCommand AddPlayListCommand;
        DelegateCommand ChangeSongCommand;

        private void AddToPlayLists(PlayList playlist)
        {
           
            PlayLists.Add(playlist);
            CurrentPlayList = playlist;
        }
        public DelegateCommand<PlayList> ChangePlayList { get; }
        public DelegateCommand<PlayList> DeletePlayList { get; }
        public DelegateCommand<Song> DeleteSong { get; }
        public DelegateCommand<PlayList> OpenPlaylistFolder { get; }
        private static Player player;

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
            string playlistName = RunInputDialog("Enter new playlist name", "PlayList Change");
            if (!string.IsNullOrEmpty(playlistName))
            {
                var newPlaylist = SongManager.CreateNewPlayList(playlistName);
                PlayLists.Add(newPlaylist);
            }


        }
        public void ChangeSongDelegate()
        {
            string name = RunInputDialog("Enter new name for song", "Song Change");
            this.Song.SongDisplay = name;
        }


        private string RunInputDialog(string message, string title)
        {
            string output = null;
            var dialog = new InputDialog(message);
            dialog.Title = title;
            if (dialog.ShowDialog() == true)
            {
                output = dialog.ResponseBox.Text;
            }
            return output;
        }
        public void DeleteSongDelegate(Song song)
        {
            Console.WriteLine("Deleting song..");
            SongManager.DeleteSong(song.RelativePlayListPath, song.RelativePath);
        }
        public void DeletePlayListDelegate(PlayList PlayList)
        {
           
            SongManager.DeletePlaylist(PlayList.DisplayName);
        }
        private void AddSongDelegate()
        {
            string playlist;
            if (SongManager.CurrentPlayList == null)
            {
                playlist = RunInputDialog("Enter new playlist name", "Create PlayList Name");
                try
                {

                    AddToPlayLists(SongManager.CreateNewPlayList(playlist));
                }catch (ArgumentNullException)
                {

                    return;
                }
            }
            else
            {
                playlist = CurrentPlayList.DisplayName;
            }
            if (playlist != null)
            {

                OpenFileDialog fileDialog = new OpenFileDialog
                {
                    Multiselect = true,
                    Filter = "Mp3 Files (*.mp3)|*.mp3"
                };


                if (fileDialog.ShowDialog() == true)
                {
                    string[] filenames = fileDialog.FileNames;

                    bool worked = SongManager.AddSongs(playlist, filenames);
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

        }
        private void LoadSongDelegate()
        {
            if (Song == null)
                return;
            SongManager.SetCurrentlyPlaying(Song.RelativePlayListPath, Song.RelativePath);
        }
        private void ChangePlayListDelegate(PlayList oldplaylist)
        {
            string newName = RunInputDialog("Enter new playlist name", "Change PlayList name");
            SongManager.ChangePlaylistName(oldplaylist.DisplayName, newName);
        }
        private void OpenPlaylistFolderDelegate(PlayList playlist)
        {
            Process.Start("explorer.exe", Path.GetFullPath(playlist.RelativePathName));
        }
        private void AddSongChangeDelegate()
        {

            SongManager.OnPlaylistChange += SongManager_OnPlaylistChange;

        }

        private void SongManager_OnPlaylistChange(object sender, PlayList playlist, bool delete)
        {

        }

        public SongSelectionViewModel()
        {
            player = MainViewModel.GuiPlayer;
            SongManager = player.SongManager;
            ChangeSongCommand = new DelegateCommand(ChangeSongDelegate);
            LoadSongCommand = new DelegateCommand(LoadSongDelegate);
            AddSongCommand = new DelegateCommand(AddSongDelegate);
            AddPlayListCommand = new DelegateCommand(AddPlayListDelegate);
            DeletePlayList = new DelegateCommand<PlayList>(DeletePlayListDelegate);
            DeleteSong = new DelegateCommand<Song>(DeleteSongDelegate);
            ChangePlayList = new DelegateCommand<PlayList>(ChangePlayListDelegate);
            OpenPlaylistFolder = new DelegateCommand<PlayList>(OpenPlaylistFolderDelegate);

            AddSongChangeDelegate();

        }


    }
}
