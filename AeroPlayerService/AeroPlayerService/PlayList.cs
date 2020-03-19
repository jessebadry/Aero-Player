using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AeroPlayerService
{
    // Playlist of songs defined from folders.
    public class PlayList : PropertyObject
    {
        private int currentIndex;
        public List<Song> Songs { get; set; }
        public int CurrentIndex
        {
            get
            {
                return currentIndex;
            }
            set
            {
                //Index change of current index changes CurrentSong, so must update currentsong...
                if (value > Songs.Count - 1)
                    currentIndex = 0;
                else if (value < 0)
                    currentIndex = Songs.Count - 1;
                else
                {

                    currentIndex = value;
                }
                onPropertyChanged("CurrentSong");
            }
        }
        public string DisplayName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(RelativePathName);
            }

        }
        private string absoluteName;
        public string RelativePathName
        {
            get
            {
                return absoluteName;
            }
            set
            {
                absoluteName = value;
                LoadSongs();
                onPropertyChanged("DisplayName");
            }

        }
        public string CurrentSong
        {
            get
            {
                if (!ValidSongIndex(CurrentIndex))
                    return null;
                return Songs[CurrentIndex].RelativePath;
            }

        }
        private bool ValidSongIndex(int index) => (index <= Songs.Count - 1 && index >= 0);
        public static string CreateValidPlayListPath(string playlistName)
        {
            return Path.Join(MusicManager.MusicPlayerPath, playlistName);
        }
        public static string CreateValidSongPath(string playlistName, string SongRelativeName)
        {
            return Path.Join(MusicManager.MusicPlayerPath, playlistName, SongRelativeName + ".mp3");
        }
        //String version of SetSongIndex
        public void SetCurrentSong(Song song)
        {
            int index = Songs.IndexOf(song);
            SetSongByIndex(index);
        }
        private void SetSongByIndex(int newIndex)
        {
            if (ValidSongIndex(newIndex))
            {
                CurrentIndex = newIndex;
            }
            else
            {
                throw new IndexOutOfRangeException(string.Format("Out of range for Songslist playlist = {0}, index supplied = {1}", RelativePathName, newIndex));
            }
        }
        private int GetIndexOfSong(string song) => Songs.FindIndex(s => s.RelativePath == song);
        public void SetCurrentSong(string song)
        {
            int newIndex = GetIndexOfSong(song);
            SetSongByIndex(newIndex);
        }
        public bool ChangeSongName(string oldSongDisplay, string newSong)
        {

            string oldSongAbsolute = CreateValidSongPath(DisplayName, oldSongDisplay);
            int index = GetIndexOfSong(oldSongAbsolute);
            if (ValidSongIndex(index))
            {
                string oldSongName = Songs[index].RelativePath;
                try
                {
                    string newSongName = CreateValidSongPath(DisplayName, newSong);
                    File.Move(oldSongName, newSongName);
                    Songs[index] = new Song(newSongName, RelativePathName);
                }
                catch (IOException e)
                {
                    return false;
                }


                return true;
            }

            return false;

        }
        public void SetSongIndex(int index)
        {

            if (ValidSongIndex(index))
            {
                //Change Low level index of songs so next time current song is retrieved, songs will start from the beginning of the playlist.
                // As Changing CurrentIndex Property will invoke calls to PropertyChanged..

                currentIndex = index;
            }
            else
            {
                throw new IndexOutOfRangeException(string.Format("Out of range for Songslist playlist = {0}, index supplied = {1}", RelativePathName, index));
            }
        }
        public bool NextSong(bool next)
        {
            if (CurrentIndex >= Songs.Count - 1 && next)
            {
                CurrentIndex = 0;
                return true;
            }
            else if (CurrentIndex <= 0 && !next)
            {
                CurrentIndex = Songs.Count - 1;
                return true;
            }
            else if (next)
            {
                CurrentIndex++;
            }
            else
            {
                CurrentIndex--;
            }
            Console.WriteLine("play list index = " + CurrentIndex);
            return false;
        }
        private Song FindSong(string SongAbsoluteName) => Songs.Where(s => s.RelativePath == SongAbsoluteName).Take(1).FirstOrDefault();
        public bool DeleteSong(string SongAbsoluteName)
        {
            var song = FindSong(SongAbsoluteName);
            if (song != null)
            {
                try
                {

                    File.Delete(SongAbsoluteName);
                    Songs.Remove(song);
                }
                catch (Exception e)
                {
                    return false;
                }

            }
            return true;
        }
        public PlayList()
        {

        }
        private void LoadSongs()
        {
            Songs = Directory.GetFiles(RelativePathName).Where(s => s.EndsWith(".mp3")).Select(s => new Song(s, RelativePathName)).ToList();
        }
        public PlayList(string playListAbsoluteName)
        {
            this.absoluteName = playListAbsoluteName;
            currentIndex = -1;
            LoadSongs();
        }
    }

}
