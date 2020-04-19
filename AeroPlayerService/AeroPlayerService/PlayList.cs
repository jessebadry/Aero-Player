
using AeroPlayerService.Constants;
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
                    currentIndex = value;

                onPropertyChanged("CurrentSong");
            }
        }
        private string displayName;
        //When ran, deletes any songs that do not exist in the given path.
        public void CleanInvalidSongs()
        {
            for (int i = 0; i < Songs.Count; i++)
            {
                if (!File.Exists(Songs[i].FilePath))
                    Songs.Remove(Songs[i]);
            }
       
        }
        public string DisplayName
        {
            get
            {
                if (displayName == null)
                {
                    displayName = Path.GetFileNameWithoutExtension(RelativePathName);
                }
                return displayName;
            }
            set
            {
                displayName = value;
                onPropertyChanged("DisplayName");
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
                Console.WriteLine("playlist name set to = " + value);
                absoluteName = value;
            }

        }
        public Song GetCurrentSongObject()
        {
            return Songs[CurrentIndex];
        }
        public Song CurrentSong
        {
            get
            {
                if (!ValidSongIndex(CurrentIndex))
                    return null;
                return Songs[CurrentIndex];
            }

        }
        private bool ValidSongIndex(int index) => (index <= Songs.Count - 1 && index >= 0);
        public static string CreateValidPlayListPath(string playlistName)
        {
            return Path.Join(MusicManager.MusicPlayerPath, playlistName);
        }
        public static string CreateValidSongPath(string playlistName, string SongRelativeName)
        {
            return Path.Join(MusicManager.MusicPlayerPath, playlistName, SongRelativeName);
        }
        //String version of SetSongIndex
        public void SetCurrentSong(Song song)
        {
            if (song == null)
                return;
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
        private int GetIndexOfSong(string song) => Songs.FindIndex(s => s.FilePath == song);
        public void SetCurrentSong(string song)
        {
            int newIndex = GetIndexOfSong(song);
            SetSongByIndex(newIndex);
        }
        public Song FindSong(string songDisplay)
        {
            return Songs.Where(s => s.SongDisplay == songDisplay).FirstOrDefault();
        }
        public bool ChangeSongName(string oldSongDisplay, string newSong)
        {
            string oldSongAbsolute = CreateValidSongPath(DisplayName, oldSongDisplay);
            Console.WriteLine("Generated Path = " + oldSongAbsolute);
            int index = GetIndexOfSong(oldSongAbsolute);
            if (ValidSongIndex(index))
            {
                Songs[index]?.ChangeSongName(newSong);
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
        public bool CurrentSongIsValid()
        {
            return (File.Exists(CurrentSong.FilePath) && CurrentSong.RelativePlayListPath != null);
        }
        public bool DeleteSong(Song song)
        {
            if (song != null)
            {
                try
                {
                    File.Delete(song.FilePath);
                    Songs.Remove(song);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            }
            return true;
        }
        public PlayList()
        {

        }
        public static bool IsAudioFile(string song)
        {
            bool isValid = false;

            string songLower = song.ToLower();

            for (int i = 0; i < Types.AudioTypes.Count; i++)
                isValid = songLower.EndsWith(Types.AudioTypes[i]);

            return isValid;
        }

        public PlayList(string playListAbsoluteName)
        {
            this.absoluteName = playListAbsoluteName;
            currentIndex = -1;
        }
    }

}
