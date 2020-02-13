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
        public SongDetail(string songName, string songDisplay, ref string playListName)
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
        public PlayListDetail(string PlayListDisplay, List<string> songs,  string absoluteName)
        {
            if (songs == null)
                throw new ArgumentNullException("Songs cannot be null!");


            AbsoluteName = absoluteName;
            this.DisplayName = PlayListDisplay;

            var ProcessedSongs = new List<SongDetail>();

            for (int i = 0; i < songs.Count; i++)
            {
                ProcessedSongs.Add(new SongDetail(songs[i], Path.GetFileNameWithoutExtension(songs[i]), ref absoluteName));
            }
            this.Songs = ProcessedSongs;
        }

    }
}
