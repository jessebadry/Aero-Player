using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AeroPlayerService
{
    public class SongDetail
    {
        public string SongName { get; }
        public string SongDisplay { get; set; }
        public string PlayListName { get; }
        public SongDetail(string songName, string songDisplay, string playListName)
        {
            this.SongName = songName;
            this.SongDisplay = songDisplay;
            PlayListName = playListName;

        }
    }
    public class PlayListDetail
    {
        public string DisplayName
        {
            get;
        }
        public string AbsoluteName { get; set; }
        public List<SongDetail> Songs { get; set; }
        public PlayListDetail(PlayList pl)
        {
            if(pl == null)
                throw new ArgumentNullException("PlayList cannot be null!");

            List<string> songs = pl.Songs;


            
            if (songs == null)
                throw new ArgumentNullException("Songs cannot be null!");


            AbsoluteName = pl.AbsoluteName;
            this.DisplayName = pl.PlayListDisplay;

            var ProcessedSongs = new List<SongDetail>();

            for (int i = 0; i < songs.Count; i++)
            {
                ProcessedSongs.Add(new SongDetail(songs[i], Path.GetFileNameWithoutExtension(songs[i]),  AbsoluteName));
            }
            this.Songs = ProcessedSongs;
        }

    }
}
