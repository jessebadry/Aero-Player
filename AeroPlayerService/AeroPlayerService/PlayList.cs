using System;
using System.Collections.Generic;
using System.IO;

namespace AeroPlayerService
{
    // Playlist of songs defined from folders.
    public class PlayList : PropertyObject
    {
        private int current_index;
        public List<Song> Songs { get; set; }
        public int CurrentIndex
        {
            get
            {
                return current_index;
            }
            set
            {
                //Index change of current index changes CurrentSong, so must update currentsong...
                current_index = value;
                onPropertyChanged("CurrentSong");
            }
        }
        public string DisplayName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(AbsoluteName);
            }
        }
        private string absoluteName;
        public string AbsoluteName
        {
            get
            {
                return absoluteName;
            }
            set
            {
                absoluteName = value;
            }
           
        }
        public string CurrentSong
        {
            get
            {
                if (!ValidSongIndex(CurrentIndex))
                    return null;
                return Songs[CurrentIndex].AbsoluteName;
            }
            set
            {
                CurrentSong = value;
            }

        }
        private bool ValidSongIndex(int index) => (index <= Songs.Count - 1 && index >= 0);
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
                throw new IndexOutOfRangeException(string.Format("Out of range for Songslist playlist = {0}, index supplied = {1}", AbsoluteName, newIndex));
            }
        }
        private int GetIndexOfSong(string song) => Songs.FindIndex(s => s.AbsoluteName == song);
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
                string oldSongName = Songs[index].AbsoluteName;
                try
                {
                    string newSongName = CreateValidSongPath(DisplayName, newSong); ;
                    File.Move(oldSongName, newSongName);
                    Songs[index] = new Song(newSongName, AbsoluteName);
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

                current_index = index;
            }
            else
            {
                throw new IndexOutOfRangeException(string.Format("Out of range for Songslist playlist = {0}, index supplied = {1}", AbsoluteName, index));
            }
        }
        public bool NextSong(bool next)
        {
            if (CurrentIndex == Songs.Count - 1 && next)
            {
                CurrentIndex = 0;
                return true;
            }
            else if (CurrentIndex == 0 && !next)
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
            return false;
        }
        public PlayList()
        {
            current_index = -1;
        }
        public PlayList(string playListAbsoluteName) : this()
        {
            this.absoluteName = playListAbsoluteName;
        }
    }

}
