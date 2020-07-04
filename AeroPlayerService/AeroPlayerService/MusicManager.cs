
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AeroPlayerService
{

    //Playlist names are from their folder names.

    //Retrieves and organizes next songs,  and facilitates adding and deleting songs from playlists.
    // Singleton is implemented, multiple instances is not applicable.

    public delegate void NewSongEventHandler(object sender, MusicManagerEventArgs e);
    public delegate void PlayListChangedEventHandler(object sender, PlayList playlist, bool delete);
    public delegate void OnErrorEventHandler(string error);
    public delegate void OnChangeCurrentPlaylistNameEventHandler();
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
        public string SongDisplay
        {
            get
            {

                return Path.GetFileNameWithoutExtension(NewSong);
            }
        }
        public string PlayListDisplay
        {
            get
            {

                return Path.GetFileNameWithoutExtension(PlayList);
            }
        }
    }
    public enum PlayLoop
    {
        SingleLoop,
        PlayListLoop,
        NoLoop
    }
    public class MusicManager : PropertyObject

    {
        public static readonly string MusicPlayerPath = @"MusicPlayer\Songs";

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
        static MusicManager instance = null;
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

        //Events--
        public event OnErrorEventHandler OnErrorEvent;
        public event NewSongEventHandler OnSongChange;
        public event OnChangeCurrentPlaylistNameEventHandler OnChangingCurrentPlaylistName;
        public event PlayListChangedEventHandler OnPlaylistChange;
        //

        //Storage

        public List<PlayList> PlayLists { get; } = new List<PlayList>();

        public bool LoopPlayList { get; set; } = false;
        //
        private string playingSong;

        /// <summary>
        /// Important: Changes currently playing song and activates new song event, if null, event will NOT fire.
        /// </summary>

        public string CurrentPlayingSong
        {
            get
            {
                return CurrentPlayList.CurrentSong.SongDisplay;
            }
            set
            {
                if (CurrentPlayList != null && CurrentPlayList.CurrentSong != null)
                    if (!File.Exists(CurrentPlayList.CurrentSong.FilePath))
                    {
                        CurrentPlayList.CleanInvalidSongs();
                        Save();
                    }
                    else
                    {

                        string oldSong = playingSong;
                        playingSong = value;
                        // If Null, playlist is being reset, thus not being changed.
                        if (currentPlayList != null)
                            OnNewSong(new MusicManagerEventArgs(CurrentPlayList.CurrentSong.FilePath, currentPlayList.RelativePathName, oldSong));
                        onPropertyChanged("CurrentPlayingSong");

                    }

            }
        }

        //Display  returns just the name of the song stripping the path name and file extension
        public string CurrentSongDisplay
        {
            get
            {
                return Path.GetFileNameWithoutExtension(CurrentPlayingSong);
            }
        }
        PlayList currentPlayList;
        public PlayList CurrentPlayList
        {
            get
            {

                if (currentPlayList == null && PlayLists != null && PlayLists.Count > 0)
                {
                    //Set playlist to first in list if current is null
                    currentPlayList = PlayLists[0];
                }

                return currentPlayList;
            }
            set
            {

                currentPlayList = value;
                if (value == null)
                    CurrentPlayingSong = "";
                else
                    CurrentPlayingSong = currentPlayList.CurrentSong.SongDisplay;

                onPropertyChanged("CurrentPlayList");
            }
        }
        void OnError(string error)
        {
            OnErrorEvent?.Invoke(error);
        }
        protected void OnNewSong(MusicManagerEventArgs e)
        {
            OnSongChange?.Invoke(this, e);
        }

        protected void PlayListChanged(PlayList playList, bool delete)
        {

            OnPlaylistChange?.Invoke(this, playList, delete);
        }
        private void OnChangeCurrentPlayListName()
        {
            //Tells other code to stop using the playlist and release its files.
            OnChangingCurrentPlaylistName?.Invoke();
        }

        public void ChangePlaylistName(PlayList playlist, string newName)
        {
            try
            {
                if (playlist == null)
                    throw new ArgumentNullException("PlayList Cannot Be Null!");
                playlist.DisplayName = newName;
                // PlayListChanged(playlist, false);
                Save();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void AddPlaylist(PlayList playlist)
        {
            PlayLists.Add(playlist);
            Save();
        }

        public PlayList CreateNewPlayList(string name, List<string> newSongs = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("PlayList Name Cannot be null.");

            string playlistPath = PlayList.CreateValidPlayListPath(name);
            Directory.CreateDirectory(playlistPath);
            var newPlaylist = new PlayList(playlistPath)
            {
                Songs = new List<Song>()
            };



            if (newSongs != null)
            {
                foreach (string song in newSongs)
                {
                    if (File.Exists(song) && PlayList.IsAudioFile(song))
                    {
                        newPlaylist.Songs.Add(new Song(song));
                    }
                }
            }

            AddPlaylist(newPlaylist);
            return newPlaylist;
        }
        PlayList FindPlayList(string playlistName) => PlayLists.Where(p => p.DisplayName == playlistName).FirstOrDefault();
        PlayList FindPlayList(Song song) => PlayLists.Where(p => p.Songs.Contains(song)).FirstOrDefault();
        public void SetCurrentlyPlaying(string playList, string songRelativePath)
        {
            PlayList playlist = PlayLists.Where(p => p.RelativePathName == playList).FirstOrDefault();

            if (playlist == null)
                throw new KeyNotFoundException(string.Format("Playlist {0}", playlist.DisplayName));

            Song foundSong = playlist.Songs.Where(Song => Song.FilePath == songRelativePath).FirstOrDefault();

            if (foundSong != null)
            {
                //Set song for playlist to sync playlist's queue index.
                playlist.SetCurrentSong(foundSong.FilePath);
                CurrentPlayList = playlist; // Set new playlist, which will set currentSong
            }
            else
            {
                throw new KeyNotFoundException(string.Format("Song {0} Not Found!", songRelativePath));
            }


        }

        public void DeletePlaylist(string playlistDisplayName)
        {
            var playlist = FindPlayList(playlistDisplayName);


            if (playlist != null)
            {
                if (playlist.DisplayName == CurrentPlayList.DisplayName)
                    CurrentPlayList = null;
                PlayLists.Remove(playlist);

                PlayListChanged(playlist, true);
                foreach (var file in Directory.EnumerateFiles(playlist.RelativePathName))
                {
                    File.Delete(file);
                }
                Directory.Delete(playlist.RelativePathName);
            }

        }
        static void CheckNull<T>(T thing, string message)
        {
            if (thing == null)
                throw new ArgumentNullException(message);
        }
        public void DeleteSong(Song song)
        {
            CheckNull(song, "Song Cannot be null!");


            var playlist = FindPlayList(song);

            bool worked = playlist.DeleteSong(song);
            Save();

            if (worked)
                PlayListChanged(playlist, false);
            else OnError(string.Format("Could not delete song {0}", song.SongDisplay));
        }
        void AddSong(ref PlayList playlist, string playlistName, string NewSong, bool copySongs = true)
        {
            //TODO: Implement CopySongs  boolean to enact using songs from another directory.

            bool invalid = false;

            string newSongPlace = Path.Join(playlist.RelativePathName, Path.GetFileName(NewSong));
            invalid = playlist.Songs.Any(s => s.FilePath == newSongPlace) || !File.Exists(NewSong);


            if (!invalid)
                try
                {
                    if (!File.Exists(newSongPlace))
                        File.Copy(NewSong, newSongPlace, true);

                    playlist.Songs.Add(new Song(newSongPlace));

                }
                catch (Exception e)
                {
                    OnError(e.ToString());
                    invalid = true;
                }
            else
                OnError(string.Format("Error could not find file, '{0}'", NewSong));



        }
        //Returns bool  if successful.
        public bool AddSongs(string playlistDisplayName, string[] newSongs)
        {


            if (newSongs == null)
                throw new ArgumentNullException("Null Songs");

            PlayList playlist;


            playlist = FindPlayList(playlistDisplayName);



            if (playlist == null && !string.IsNullOrEmpty(playlistDisplayName))
                playlist = CreateNewPlayList(playlistDisplayName, new List<string>());

            if (playlist != null)
            {

                foreach (string NewSong in newSongs)
                    AddSong(ref playlist, playlistDisplayName, NewSong);

                PlayListChanged(playlist, false);
            }
            Save();
            return true;
        }

        public void RandomInPlayList()
        {
            if (CurrentPlayList == null || CurrentPlayList.Songs.Count <= 0)
                return;
            Random rand = new Random();
            var songs = CurrentPlayList.Songs;
            int skip = rand.Next(0, songs.Count);
            Song song = songs.Skip(skip).Take(1).First();
            CurrentPlayList.SetCurrentSong(song);
            CurrentPlayingSong = song.FilePath;
        }

        PlayList CheckAndChangePlaylist(ref bool next)
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
            var newPlaylist = PlayLists[newPlayListIndex];
            newPlaylist.SetSongIndex(0);
            return newPlaylist;
        }


        public void NextSong(bool next)
        {
            if (CurrentPlayList != null && CurrentPlayList.CurrentSong != null)
            {
                //if (CurrentPlayList.CurrentSong.RelativePlayListPath == null || CurrentPlayList.CurrentSong.FilePath)
                //{
                //    CurrentPlayList.Songs.Remove(CurrentPlayList.CurrentSong);
                //}

                bool PlayListEndOrBeginning = CurrentPlayList.NextSong(next);
                if (!CurrentPlayList.CurrentSongIsValid())
                {
                    CurrentPlayList.RemoveSong(CurrentPlayList.CurrentSong);
                    PlayListChanged(CurrentPlayList, false);
                }

                if (LoopType == PlayLoop.NoLoop && PlayListEndOrBeginning)
                {
                    PlayList newPlaylist = CheckAndChangePlaylist(ref next);
                    CurrentPlayList = newPlaylist;
                }
                else
                {
                    CurrentPlayingSong = CurrentPlayList.CurrentSong.SongDisplay;
                }
            }
        }
        /// <summary>
        /// Removes invalid playlists, and then cleans songs if a valid playlist
        /// </summary>

        static bool CleanSongs(PlayList playlist)
        {
            bool changed = false;
            foreach (var song in playlist.Songs.ToList())
            {
                if (song.RelativePlayListPath == null || !Directory.Exists(song.RelativePlayListPath)
                    || !File.Exists(song.FilePath))
                {
                    playlist.Songs.Remove(song);
                    changed = true;
                }
            }
            return changed;

        }
        void CleanPlayLists()
        {
            bool changed = false;
            foreach (var playlist in PlayLists)
            {
                if (!Directory.Exists(playlist.RelativePathName) || Path.GetFileName(playlist.RelativePathName) == "Songs")
                {
                    changed = true;

                    Instance.DeletePlaylist(playlist.DisplayName);
                }
                else
                    // Now check songs in playlist
                    changed = CleanSongs(playlist);
            }

            // If PlayLists Changed, save the new changes
            if (changed)
                Save();

        }
        /// <summary>
        ///Creates directory for MusicPlayerPath
        /// </summary>
        public static bool EnsureMusicPlayerPath()
        {
            try
            {
                Directory.CreateDirectory(MusicPlayerPath);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        MusicManager()
        {
            var dat = PlayListLoader.DeserializePlayLists();
            var index = dat?.Current.Value;
            if (dat == null)
            {
                Console.WriteLine("Null!");
                OnError(string.Format("Could not load Playlists file {0}", PlayListLoader.SETTINGS_FILE));
            }
            else if (index != null && index >= 0 && index < PlayLists.Count)
            {

                PlayLists = dat?.playLists;
                CurrentPlayList = PlayLists?[dat.Current.Value];
            }

            if (PlayLists == null)
            {
                PlayLists = new List<PlayList>();
                Save();
            }
            else
            {
                CleanPlayLists();
            }

        }
        public void Save()
        {
            int index = PlayLists.FindIndex(pl => pl == CurrentPlayList);
            PlayListLoader.SerializePlayLists(PlayLists, index);
        }


    }
}
