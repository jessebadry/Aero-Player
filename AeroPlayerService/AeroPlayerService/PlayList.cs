using System;
using System.Collections.Generic;
using System.Text;

namespace AeroPlayerService
{
    // Playlist of songs defined from folders.
    public class PlayList : PropertyObject
    {

        public List<string> Songs { get; set; }

        private int current_index;
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


        public string Name { get; set; }
        public string CurrentSong
        {
            get
            {
                
                return Songs[CurrentIndex];
            }
            set
            {
                CurrentSong = value;
            }

        }
        public void SetCurentSong(string song)
        {
            CurrentIndex = Songs.IndexOf(song);
        }
        public void SetSongIndex(int index)
        {
            if (Songs.Count - 1 >= index && index <= 0 )
            {
                //Change Low level index of songs so next time current song is retrieved, songs will start from the beginning of the playlist.
                current_index = index;
            }
            else
            {
                throw new IndexOutOfRangeException(string.Format("Out of range for Songslist playlist = {0}, index supplied = {1}", Name, index));
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


    }

}
