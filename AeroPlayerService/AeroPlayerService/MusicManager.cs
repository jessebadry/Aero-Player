
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
        public PlayList PlayList { get; }
        public MusicManagerEventArgs(PlayList playlist)
        {
            this.PlayList = playlist;
        }

    }
    public enum PlayLoop
    {
        SingleLoop,
        PlayListLoop,
        NoLoop,
        Shuffle
    }
    public class MusicManager : PropertyObject

    {
        public static readonly string MusicPlayerPath = @"MusicPlayer\Songs";

        public PlayLoop LoopType { get; private set; } = PlayLoop.PlayListLoop;
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
                LoopType = PlayLoop.Shuffle;
            }
            else if (LoopType == PlayLoop.Shuffle)
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

        public bool LoopPlayList { get; private set; } = false;
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
                            OnNewSong(new MusicManagerEventArgs(CurrentPlayList));
                        onPropertyChanged("CurrentPlayingSong");

                    }

            }
        }
        private void UpdateCurrentSong()
        {
            if (currentPlayList != null)
                OnNewSong(new MusicManagerEventArgs(CurrentPlayList));
            onPropertyChanged("CurrentPlayingSong");
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
                    UpdateCurrentSong();

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
            var newPlaylist = new PlayList(playlistPath, new List<Song>());


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
        static void CheckNull<T>(T thing)
        {
            if (thing == null)
                throw new ArgumentNullException(string.Format("{0} cannot be null", typeof(T).Name));
        }
        public void DeleteSong(Song song)
        {
            CheckNull(song);


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

            PlayList playlist = FindPlayList(playlistDisplayName);



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
                // Logic to determine next song, based on PlayLoop type.
                if (LoopType == PlayLoop.NoLoop && PlayListEndOrBeginning)
                {
                    PlayList newPlaylist = CheckAndChangePlaylist(ref next);
                    CurrentPlayList = newPlaylist;
                }
                else
                {
                    UpdateCurrentSong();
                }
            }
        }
        public void RandomSongInPlayList()
        {
            CurrentPlayList.PickRandomSong();
            UpdateCurrentSong();
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
        void CleanPlayLists(List<PlayList> PlayLists)
        {
            bool changed = false;
            for (int i = 0; i < PlayLists.Count; i++)
            {
                PlayList playlist = PlayLists[i];
                if (!Directory.Exists(playlist.RelativePathName) || Path.GetFileName(playlist.RelativePathName) == "Songs")
                {
                    changed = true;

                    PlayLists.Remove(playlist);
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
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        MusicManager()
        {
            if (instance != null)
                throw new Exception("MusicManager is singleton! Cannot create new object");
            //Load Playlists
            EnsureMusicPlayerPath();
            var dat = PlayListLoader.DeserializePlayLists();
            var index = dat?.Current;

            List<PlayList> playlists = dat?.playLists;
            PlayList Current = null;

            if (dat == null)
            {
                OnError(string.Format("Could not load Playlists file {0}", PlayListLoader.SETTINGS_FILE));
            }

            if (index != null && index.Value >= 0 && index.Value < playlists?.Count)
            {

                Current = playlists?[dat.Current.Value];
            }

            if (playlists == null)
            {
                playlists = new List<PlayList>();
                Save();
            }
            else
            {
                CleanPlayLists(playlists);
            }
            PlayLists = playlists;
            CurrentPlayList = Current;

        }
        public void Save()
        {
            Console.WriteLine("Saving Playlists, count ==  {0}", PlayLists.Count);
            int index = PlayLists.FindIndex(pl => pl == CurrentPlayList);
            PlayListLoader.SerializePlayLists(PlayLists, index);
        }


    }
}
