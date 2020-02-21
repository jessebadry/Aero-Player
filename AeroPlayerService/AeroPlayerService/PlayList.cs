using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AeroPlayerService
{
    // Playlist of songs defined from folders.
    public class PlayList : PropertyObject
    {
        private int current_index;


        public List<string> Songs { get; set; }


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
        public string PlayListDisplay { get; set; }
        private string name;
        public string AbsoluteName
        {
            get
            {

                return name;
            }
            set
            {
                name = value;
                PlayListDisplay = Path.GetFileName(value);



            }
        }
        public string CurrentSong
        {
            get
            {
                if (!ValidSongIndex(CurrentIndex))
                    return null;
                return Songs[CurrentIndex];
            }
            set
            {
                CurrentSong = value;
            }

        }
        private bool ValidSongIndex(int index) => (index <= Songs.Count - 1 && index >= 0);

        //String version of SetSongIndex
        public void SetCurentSong(string song)
        {
            Console.WriteLine("Song = " + song + " Song COunt = " + Songs.Count);
            int new_index = Songs.IndexOf(song);
            if (ValidSongIndex(new_index))
            {
                CurrentIndex = new_index;
            }
            else
            {
                throw new IndexOutOfRangeException(string.Format("Out of range for Songslist playlist = {0}, index supplied = {1}", AbsoluteName, new_index));
            }
        }

        public void SetSongIndex(int index)
        {
            Console.WriteLine("new index == " + index);
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


    }

}
