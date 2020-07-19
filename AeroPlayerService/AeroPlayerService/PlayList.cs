
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
                if (value > Songs?.Count - 1)
                    currentIndex = 0;
                else if (value < 0)
                {
                    if (Songs == null) currentIndex = -1;
                    else currentIndex = Songs.Count - 1;
                }

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
        public void RemoveSong(Song song)
        {
            Songs.Remove(song);
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
        private string RelativePathName_; 	


        public string RelativePathName
        {
            get
            {

                return RelativePathName_;
            }
            set
            {
                RelativePathName_ = value;
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
            if (song == null) return;
            int index = Songs.IndexOf(song);
            if (index >= -1) return;

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

        public void PickRandomSong()
        {
            int SongLength = Songs.Count;
            if (SongLength >= 1)
            {
                Random r = new Random();
                CurrentIndex = r.Next(0, SongLength);
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
            return false;
        }
        /// <summary>
        /// Checks if the song exists and if path is not null
        /// </summary>
        /// <returns></returns>
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
     
        public static bool IsAudioFile(string song)
        {
            if (song == null)
                throw new ArgumentNullException("Song cannot be null");

            bool isValid = false;

            string songLower = song.ToLower();

            for (int i = 0; i < Types.AudioTypes.Length; i++)
                isValid = songLower.EndsWith(Types.AudioTypes[i]);

            return isValid;
        }
        /// <summary>
        /// REQUIRED FOR SERIALIZATION
        /// </summary>
        public PlayList()
        {

        }
        public PlayList(string playlistPath, List<Song> songs) : this(playlistPath)
        {
            this.Songs = songs;
        }
        public PlayList(string playListPath)
        {
            this.RelativePathName_ = playListPath;
            currentIndex = -1;
        }
    }

}
