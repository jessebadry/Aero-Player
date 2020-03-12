using System;
using System.Collections.Generic;
using System.IO;

namespace AeroPlayerService
{
    public class Song : PropertyObject
    {
        public string AbsoluteName { get; set; }

        public string SongDisplay
        {
            get
            {
                return Path.GetFileNameWithoutExtension(AbsoluteName);

            }
            set
            {
                string playlistName = Path.GetFileNameWithoutExtension(PlayListName);
                bool worked = MusicManager.Instance.ChangeSongName(playlistName, SongDisplay, value);

                if (worked)
                {
                    AbsoluteName = PlayList.CreateValidSongPath(playlistName, value);
                    onPropertyChanged("SongDisplay");
                }
            }
        }
        public string PlayListName { get; }
        public Song(string songName, string playListName)
        {
            this.AbsoluteName = songName;
            PlayListName = playListName;

        }
    }
    public class PlayListDetail
    {
        public string DisplayName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(AbsoluteName);
            }
        }
        public string AbsoluteName { get; set; }
        public List<Song> Songs { get; set; }
        public PlayListDetail(PlayList pl)
        {
            if (pl == null)
                throw new ArgumentNullException("PlayList cannot be null!");

            Songs = pl.Songs;
            AbsoluteName = pl.AbsoluteName;
        }

    }
}
