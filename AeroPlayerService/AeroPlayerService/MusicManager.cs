using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using AeroPlayerService.ExtensionMethods;
using System.Timers;

namespace AeroPlayerService
{

    //Playlist names are from their folder names.

    //Retrieves and organizes next songs,  and facilitates adding and deleting songs from playlists.
    // Non-Instance based, does not need multiple MusicQueues, does not apply.

    public delegate void NewSongHandler(object sender, MusicManagerEventArgs e);
    public delegate void AddSongHandler(object sender, PlayListDetail playlist);
    public class MusicManagerEventArgs : EventArgs
    {
        public string NewSong { get; }
        public string PlayList { get; }
        public string OldSong { get; }
        public MusicManagerEventArgs(string newSong, string playlist, string oldSong)
        {
            this.NewSong = newSong;
            this.PlayList = playlist;
            this.OldSong = oldSong;
        }
    }
    public enum PlayLoop
    {
        SingleLoop,
        PlayListLoop,
        NoLoop
    }
    public class MusicManager

    {
        public static readonly string MusicPlayerPath = "MusicPlayer\\Songs";

        public PlayLoop LoopType = PlayLoop.PlayListLoop;
        public void ToggleLooping()
        {

            if (LoopType == PlayLoop.PlayListLoop)
            {

                LoopType = PlayLoop.SingleLoop;
            }
            else if (LoopType == PlayLoop.SingleLoop)
            {
                LoopPlayList = false;
                LoopType = PlayLoop.NoLoop;
            }
            else if (LoopType == PlayLoop.NoLoop)
            {

                LoopPlayList = true;
                LoopType = PlayLoop.PlayListLoop;
            }


        }



        //Singleton impl...
        private static MusicManager instance = null;
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

        private static readonly object singletonPadlock = new object();

        //

        //Events
        public event NewSongHandler OnSongChange;
        public event AddSongHandler OnAddSong;
        //

        //Storage

        private readonly List<PlayList> PlayLists = new List<PlayList>();
        public List<PlayListDetail> GetPlayListDetails()
        {

            var playListDetails = new List<PlayListDetail>();
            for (int i = 0; i < PlayLists.Count; i++)
            {
                var pl = PlayLists[i];


                playListDetails.Add(new PlayListDetail(pl));
            }

            return playListDetails;
        }
        public bool LoopPlayList { get; set; } = false;


        //


        private string playing_song;

        public string SongAbsolute
        {
            get
            {
                return playing_song;
            }
            set
            {
                string old_song = playing_song;
                playing_song = value;
                OnNewSong(new MusicManagerEventArgs(playing_song, current_playlist.AbsoluteName, old_song));
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

                    }

                return current_playlist;
            }
            set
            {
                current_playlist = value;
                SongAbsolute = CurrentPlayList.CurrentSong;
            }
        }

        protected virtual void OnNewSong(MusicManagerEventArgs e)
        {
            NewSongHandler handler = OnSongChange;
            handler?.Invoke(this, e);
        }
        protected virtual void OnAddSong_Run(PlayListDetail playList)
        {
            AddSongHandler handler = OnAddSong;
            handler?.Invoke(this, playList);
        }
        public PlayList CreateNewPlayList(string name, List<string> new_songs = null)
        {

            var new_playlist = new PlayList()
            {
                AbsoluteName = name,
                Songs = new List<string>()
            };

            Directory.CreateDirectory(Path.Join(MusicPlayerPath, name));

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

            Console.WriteLine("playlist name = " + playList + " Song = " + songName);
            PlayList playlist = PlayLists.Where(p => p.AbsoluteName == playList).FirstOrDefault();

            if (playlist == null)
                throw new KeyNotFoundException(string.Format("Playlist {0} not found!!", playlist));

            string found_song = playlist.Songs.Where(Song => Song == songName).FirstOrDefault();

            if (found_song != null)
            {
                //Set song for playlist to sync playlist's queue index.
                playlist.SetCurentSong(found_song);

                CurrentPlayList = playlist; // Set new playlist, which will set currentSong
            }
            else
            {
                throw new KeyNotFoundException(string.Format("Song {0} Not Found!", songName));
            }


        }
        private PlayList findPlayList(string playlistName) => PlayLists.Where(p => p.PlayListDisplay == playlistName).FirstOrDefault();

        //Returns bool  if successful.
        public bool AddSongs(string playlistName, string[] NewSongs)
        {

            PlayList playlist;


            playlist = findPlayList(playlistName);

            if (playlist == null && !string.IsNullOrEmpty(playlistName))
                playlist = CreateNewPlayList(playlistName, new List<string>());

          
            foreach (string NewSong in NewSongs)
            {

                string newSongPlace = Path.Join(MusicPlayerPath, playlistName, Path.GetFileName(NewSong));
                if (playlist.Songs.Contains(newSongPlace))
                    continue;
                
                if (!File.Exists(NewSong))
                    return false;

                try
                {
                    File.Copy(NewSong, newSongPlace, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }

                playlist.Songs.Add(newSongPlace);
            }





            OnAddSong_Run(new PlayListDetail(playlist));
            return true;
        }

        public void RandomInPlayList()
        {
            if (CurrentPlayList == null)
                return;
            Random rand = new Random();
            var songs = CurrentPlayList.Songs;
            int skip = rand.Next(0, songs.Count);
            string song = songs.Skip(skip).Take(1).First();
            CurrentPlayList.SetCurentSong(song);
            SongAbsolute = song;
        }
        private void CheckAndChangePlaylist(ref bool next, ref bool PlayListChange)
        {
            if (!LoopPlayList && PlayListChange)
            {
                int newPlayListIndex = PlayLists.IndexOf(CurrentPlayList);
                bool EndOfOrBeginning = next ? (newPlayListIndex == PlayLists.Count - 1) : (newPlayListIndex <= 0);

                if (EndOfOrBeginning && next)
                {

                    // loop index back to first.
                    newPlayListIndex = 0;
                }
                else if (EndOfOrBeginning && !next)
                {

                    newPlayListIndex = PlayLists.Count - 1;
                }
                else if (next)
                {
                    newPlayListIndex++;
                }
                else
                {
                    // increase index  to get next playlist
                    newPlayListIndex--;
                }
                // use that index to retreive another playlist to use.
                CurrentPlayList = PlayLists[newPlayListIndex];
                CurrentPlayList.SetSongIndex(0);


            }
        }
        public void NextSong(bool next)
        {
            if (CurrentPlayList != null)
            {

                bool PotentialPlayListChange = CurrentPlayList.NextSong(next);
                if (LoopType == PlayLoop.NoLoop)
                {
                    CheckAndChangePlaylist(ref next, ref PotentialPlayListChange);
                }
                this.SongAbsolute = CurrentPlayList.CurrentSong;
            }
        }

        public void LoadAllSongs()
        {

            var playlist_folders = Directory.EnumerateDirectories(MusicPlayerPath);
            foreach (var folder in playlist_folders)
            {
                var new_playlist = new PlayList
                {
                    AbsoluteName = folder,
                    Songs = Directory.GetFiles(folder).Where(s => s.EndsWith(".mp3")).ToList()
                };


                PlayLists.Add(new_playlist);


            }
            Console.WriteLine("PlayList Count = " + PlayLists.Count);
        }

        MusicManager()
        {
            var dir = Directory.CreateDirectory(MusicPlayerPath);

            LoadAllSongs();
        }


    }
}
