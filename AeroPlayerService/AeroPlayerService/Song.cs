using Newtonsoft.Json;
using System;
using System.IO;

namespace AeroPlayerService
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Song : PropertyObject
    {

        [JsonProperty]
        private string relPath;
        public string FilePath
        {
            get
            {
                return Path.Join(MusicManager.MusicPlayerPath, relativePlayListPath, relPath);
            }
            set
            {
                relPath = Path.GetFileName(value);

                //Uses the folder name of the song.
                RelativePlayListPath = Directory.GetParent(value).Name;
            }
        }
        public bool ChangeSongName(string songName)
        {
            SongDisplay = songName;
            MusicManager.Instance.Save();

            return true;
        }

        [JsonProperty]
        private string songDisplay;

        public string SongDisplay
        {
            get
            {
                if (songDisplay == null)
                    songDisplay = Path.GetFileNameWithoutExtension(relPath);
                return songDisplay;
            }
            set
            {
                songDisplay = value;
                MusicManager.Instance.Save();
                onPropertyChanged("SongDisplay");
            }
        }
        [JsonProperty]
        private string relativePlayListPath;
       
        public string RelativePlayListPath
        {
            get
            {
                return PlayList.CreateValidPlayListPath(relativePlayListPath);
            }
            set { relativePlayListPath = value; }
        }
    
        public Song()
        {
        }
        public Song(string songName) : base()
        {
            FilePath = songName;

        }
    }

}
