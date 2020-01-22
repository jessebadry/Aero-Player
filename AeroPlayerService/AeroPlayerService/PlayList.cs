using System;
using System.Collections.Generic;
using System.Text;

namespace AeroPlayerService
{
    // Playlist of songs defined from folders.
    public class PlayList : PropertyObject
    {

        public List<string> Songs { get; set; }
        private int current_index = 0;
        private int CurrentIndex
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
                onPropertyChanged("CurrentSong");
            }

        }
        public void SetCurentSong(string song)
        {
            CurrentIndex = Songs.IndexOf(song);
        }
        public bool NextSong(bool next)
        {
            if (CurrentIndex == Songs.Count - 1 && next)
            {
                Console.WriteLine("End  of playlist!!!");
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
            
            current_index = 0;

        }

  
    }

}
