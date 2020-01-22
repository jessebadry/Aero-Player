using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
namespace AeroPlayerService
{



    //Retrieves and organizes next songs,  and facilitates adding and deleting songs from playlists.
    // Non-Instance based, does not need multiple MusicQueues, does not apply.

    public class MusicQueueEventArgs : EventArgs
    {
        public string NewSong { get; set; }
        public string PlayList { get; set; }
        public MusicQueueEventArgs(string newSong, string playlist)
        {
            this.NewSong = newSong;
            this.PlayList = playlist;
        }
    }
    public class MusicManager

    {
        private static string MusicPlayerPath = "MusicPlayer/Songs";


        //Singleton impl...
        private static MusicManager instance = null;
        private static readonly object singletonPadlock = new object();

        public static MusicManager Instance
        {
            get
            {
                lock (singletonPadlock)
                {
                    if (instance == null)
                    {
                        instance = new MusicManager();
                    }
                }
                return instance;
            }
        }
        //

        public delegate void NewSongHandler(object sender, MusicQueueEventArgs e);
        public event NewSongHandler NewSongOutput;




        static List<PlayList> PlayLists = new List<PlayList>();
        protected virtual void OnNewSong(MusicQueueEventArgs e)
        {
            NewSongHandler handler = NewSongOutput;
            handler?.Invoke(this, e);
        }
        private string playing_song;
        public string PlayingSong
        {
            get
            {
                return playing_song;
            }
            set
            {
                playing_song = value;
                OnNewSong(new MusicQueueEventArgs(playing_song, current_playlist.Name));
            }
        }


        private PlayList current_playlist;
        public PlayList CurrentPlayList
        {
            get
            {
                if (current_playlist == null)
                    if (PlayLists.Count > 0)
                    {
                        current_playlist = PlayLists[0];
                    }
                    else
                    {
                        throw new NullReferenceException("There is no playlists avaliable!");
                    }

                return current_playlist;
            }
            set
            {
                current_playlist = value;
            }
        }

        public bool LoopPlayList { get; set; } = false;



        public PlayList CreateNewPlayList(string name, List<string> new_songs = null)
        {

            var new_playlist = new PlayList()
            {
                Name = name,
                Songs = new List<string>()
            };


            if (new_songs != null)
            {
                foreach (string song in new_songs)
                {
                    if (File.Exists(song) && song.EndsWith(".mp3"))
                    {
                        new_playlist.Songs.Add(song);
                    }
                }
            }
            PlayLists.Add(new_playlist);

            return new_playlist;

        }
        public void SetSong(string playList, string songName)
        {


            PlayList playlist = PlayLists.Where(p => p.Name == playList).FirstOrDefault();

            if (playlist == null)
                throw new KeyNotFoundException(string.Format("Playlist {0} not found!!", playlist));

            string found_song = playlist.Songs.Where(Song => Song == songName).FirstOrDefault();

            if (found_song != null)
            {
                //Set song for playlist to sync playlist's queue index.
                playlist.SetCurentSong(found_song);


            }
            else
            {
                throw new KeyNotFoundException(string.Format("Song {0} Not Found!", songName));
            }
        }
        //Copies song to playlist.
        public bool AddSong(string playlist_name, string new_song)
        {
            if (!File.Exists(new_song))
                return false;
            else
            {

                var playlist = PlayLists.Where(p => p.Name == playlist_name).FirstOrDefault();

                if (playlist == null && !string.IsNullOrEmpty(playlist_name))
                    playlist = CreateNewPlayList(playlist_name, new List<string>() { new_song });

                else
                    playlist.Songs.Add(new_song);


                string new_song_place = Path.Join(MusicPlayerPath, playlist_name);

                Directory.CreateDirectory(new_song_place);
                new_song_place = Path.Join(new_song_place, Path.GetFileName(new_song));



                File.Copy(new_song, new_song_place);
            }



            return true;
        }
        public void PreviousSong()
        {

        }
        public void RandomInPlayList()
        {
            Random rand = new Random();
            var songs = CurrentPlayList.Songs;
            int skip = rand.Next(0, songs.Count);
            string song = songs.Skip(skip).Take(1).First();

            PlayingSong = song;
        }
        
        public void NextSong()
        {

            bool next_playlist = CurrentPlayList.NextSong(true);


            if (!LoopPlayList && next_playlist)
            {

                int index = PlayLists.IndexOf(CurrentPlayList);
                bool LastPlaylist = (index == PlayLists.Count - 1);

                if (LastPlaylist)
                {
                    // loop index back to first.
                    index = 0;
                }
                else
                {
                    // increase index  to get next playlist
                    index++;
                }
                // use that index to retreive another playlist to use.
                CurrentPlayList = PlayLists[index];

            }




        }
        static MusicManager()
        {
            var dir = Directory.CreateDirectory(MusicPlayerPath);
            var playlist_folders = Directory.EnumerateDirectories(MusicPlayerPath);
            foreach (var folder in playlist_folders)
            {
                var new_playlist = new PlayList
                {
                    Name = folder,
                    Songs = Directory.GetFiles(folder).Where(s => s.EndsWith(".mp3")).ToList()
                };
                Console.WriteLine(new_playlist.Name);
                new_playlist.PropertyChanged += CurrentSongChanged;
                PlayLists.Add(new_playlist);


            }
        }

        private static void CurrentSongChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PlayList current_playlist = (PlayList)sender;
            
            Instance.PlayingSong = current_playlist.CurrentSong;

            Instance.current_playlist = current_playlist;
        }
    }
}
