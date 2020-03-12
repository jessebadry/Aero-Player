using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AeroPlayerService
{

    //Playlist names are from their folder names.

    //Retrieves and organizes next songs,  and facilitates adding and deleting songs from playlists.
    // Non-Instance based, does not need multiple MusicQueues, does not apply.

    public delegate void NewSongEventHandler(object sender, MusicManagerEventArgs e);
    public delegate void AddSongEventHandler(object sender, PlayList playlist);
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

        public PlayLoop LoopType { get; set; } = PlayLoop.PlayListLoop;
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
        public event NewSongEventHandler OnSongChange;
        public event AddSongEventHandler OnAddSong;
        //

        //Storage

        public List<PlayList> PlayLists { get; } = new List<PlayList>();

        public bool LoopPlayList { get; set; } = false;
        //
        private string playingSong;
        public string SongAbsolute
        {
            get
            {
                return playingSong;
            }
            set
            {
                string oldSong = playingSong;
                playingSong = value;
                OnNewSong(new MusicManagerEventArgs(playingSong, currentPlayList.AbsoluteName, oldSong));
            }
        }

        private PlayList currentPlayList;
        public PlayList CurrentPlayList
        {
            get
            {
                if (currentPlayList == null)
                    if (PlayLists.Count > 0)
                    {
                        currentPlayList = PlayLists[0];
                    }
                    else
                    {

                    }

                return currentPlayList;
            }
            set
            {
                currentPlayList = value;
                SongAbsolute = CurrentPlayList.CurrentSong;
            }
        }

        protected virtual void OnNewSong(MusicManagerEventArgs e)
        {
            NewSongEventHandler handler = OnSongChange;
            handler?.Invoke(this, e);
        }

        protected virtual void OnAddSongRun(PlayList playList)
        {
            AddSongEventHandler handler = OnAddSong;
            handler?.Invoke(this, playList);
        }

        public bool ChangeSongName(string playlistName, string oldSongDisplay, string newSong)
        {
            var playlist = FindPlayList(playlistName);
            bool status = playlist.ChangeSongName(oldSongDisplay, newSong);
            return status;
        }

        public PlayList CreateNewPlayList(string name, List<string> newSongs = null)
        {
            var newPlaylist = new PlayList(name)
            {
                Songs = new List<Song>()
            };

            Directory.CreateDirectory(Path.Join(MusicPlayerPath, name));

            if (newSongs != null)
            {
                foreach (string song in newSongs)
                {
                    if (File.Exists(song) && song.EndsWith(".mp3"))
                    {
                        newPlaylist.Songs.Add(new Song(song, newPlaylist.AbsoluteName));
                    }
                }
            }

            PlayLists.Add(newPlaylist);
            return newPlaylist;
        }
        public void SetCurrentlyPlaying(string playList, string songNameAbsolute)
        {

            Console.WriteLine("playlist name = " + playList + " Song = " + songNameAbsolute);
            PlayList playlist = PlayLists.Where(p => p.AbsoluteName == playList).FirstOrDefault();

            if (playlist == null)
                throw new KeyNotFoundException(string.Format("Playlist {0} not found!!", playlist));
            Console.WriteLine(playlist.Songs[0].SongDisplay);
            Song found_song = playlist.Songs.Where(Song => Song.AbsoluteName == songNameAbsolute).FirstOrDefault();

            if (found_song != null)
            {
                //Set song for playlist to sync playlist's queue index.
                playlist.SetCurrentSong(found_song.AbsoluteName);

                CurrentPlayList = playlist; // Set new playlist, which will set currentSong
            }
            else
            {
                throw new KeyNotFoundException(string.Format("Song {0} Not Found!", songNameAbsolute));
            }


        }
        private PlayList FindPlayList(string playlistName) => PlayLists.Where(p => p.DisplayName == playlistName).FirstOrDefault();


        private void AddSong(ref PlayList playlist, string playlistName, string NewSong)
        {
            bool invalid = false;

            string newSongPlace = Path.Join(MusicPlayerPath, playlistName, Path.GetFileName(NewSong));
            invalid = playlist.Songs.Any(s => s.AbsoluteName == newSongPlace);


            invalid = !File.Exists(NewSong);

            if (!invalid)
                try
                {
                    File.Copy(NewSong, newSongPlace, true);

                    playlist.Songs.Add(new Song(newSongPlace, playlist.AbsoluteName));
                }
                catch (Exception e)
                {
                    invalid = true;
                }


        }
        //Returns bool  if successful.
        public bool AddSongs(string playlistName, string[] newSongs)
        {
            if (newSongs == null)
                throw new ArgumentNullException("Null Songs");

            PlayList playlist;


            playlist = FindPlayList(playlistName);

            if (playlist == null && !string.IsNullOrEmpty(playlistName))
                playlist = CreateNewPlayList(playlistName, new List<string>());

            foreach (string NewSong in newSongs)
                AddSong(ref playlist, playlistName, NewSong);

            OnAddSongRun(playlist);
            return true;
        }

        public void RandomInPlayList()
        {
            if (CurrentPlayList == null)
                return;
            Random rand = new Random();
            var songs = CurrentPlayList.Songs;
            int skip = rand.Next(0, songs.Count);
            Song song = songs.Skip(skip).Take(1).First();
            CurrentPlayList.SetCurrentSong(song);
            SongAbsolute = song.AbsoluteName;
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
            var PlayListFolders = Directory.EnumerateDirectories(MusicPlayerPath);
            foreach (var folder in PlayListFolders)
            {
                var newPlayList = new PlayList
                {
                    AbsoluteName = folder,
                    Songs = Directory.GetFiles(folder).Where(s => s.EndsWith(".mp3")).Select(s => new Song(s, folder)).ToList()
                };
                PlayLists.Add(newPlayList);
            }

        }

        MusicManager()
        {
            Directory.CreateDirectory(MusicPlayerPath);

            LoadAllSongs();
        }


    }
}
