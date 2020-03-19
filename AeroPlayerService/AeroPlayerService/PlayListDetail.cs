using System;
using System.Collections.Generic;
using System.IO;

namespace AeroPlayerService
{
    public class Song : PropertyObject
    {
        public string RelativePath { get; set; }

        public string SongDisplay
        {
            get
            {
                return Path.GetFileNameWithoutExtension(RelativePath);

            }
            set
            {
                string playlistName = Path.GetFileNameWithoutExtension(RelativePlayListPath);
                bool worked = MusicManager.Instance.ChangeSongName(playlistName, SongDisplay, value);

                if (worked)
                {
                    RelativePath = PlayList.CreateValidSongPath(playlistName, value);
                    onPropertyChanged("SongDisplay");
                }
            }
        }
        public string RelativePlayListPath { get; }
        public Song(string songName, string playListName)
        {
            RelativePath = songName;
            RelativePlayListPath = playListName;

        }
    }

}
