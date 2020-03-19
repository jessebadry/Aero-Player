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
    }
    public enum PlayLoop
    {
        SingleLoop,
        PlayListLoop,
        NoLoop
    }
    public class MusicManager

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
            Console.WriteLine("loop = "+ LoopType.ToString());
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
                return playingSong;
            }
            set
            {
                string oldSong = playingSong;
                playingSong = value;
                // If Null playlist is being reset thus not being changed.
                if (currentPlayList != null)
                    OnNewSong(new MusicManagerEventArgs(playingSong, currentPlayList.RelativePathName, oldSong));
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
                if (value == null)
                {
                    CurrentPlayingSong = "";
                }
                else
                {

                    CurrentPlayingSong = currentPlayList.CurrentSong;
                }
            }
        }
        private void OnError(string error)
        {
            OnErrorEvent?.Invoke(error);
        }
        protected virtual void OnNewSong(MusicManagerEventArgs e)
        {
            OnSongChange?.Invoke(this, e);
        }

        protected virtual void PlayListChanged(PlayList playList, bool delete)
        {
            PlayListChangedEventHandler handler = OnPlaylistChange;
            handler?.Invoke(this, playList, delete);
        }
        private void OnChangeCurrentPlayListName()
        {
            //Tells other code to stop using the playlist and release its files.
            OnChangingCurrentPlaylistName?.Invoke();
        }
        public bool ChangeSongName(string playlistName, string oldSongDisplay, string newSong)
        {
            var playlist = FindPlayList(playlistName);
            bool status = playlist.ChangeSongName(oldSongDisplay, newSong);
            return status;
        }
        public void ChangePlaylistName(string playlistName, string newName)
        {
            var playlist = FindPlayList(playlistName);
            if (playlist == CurrentPlayList)
                OnChangeCurrentPlayListName();

            string newPlaylistName = PlayList.CreateValidPlayListPath(newName);
            try
            {
                Directory.Move(PlayList.CreateValidPlayListPath(playlistName), newPlaylistName);
                playlist.RelativePathName = newPlaylistName;
                PlayListChanged(playlist, false);
            }catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public PlayList CreateNewPlayList(string name, List<string> newSongs = null)
        {
            Console.WriteLine("Creating playlist " + name);
            string playlistPath = Path.Join(MusicPlayerPath, name);
            Directory.CreateDirectory(playlistPath);
            var newPlaylist = new PlayList(playlistPath)
            {
                Songs = new List<Song>()
            };



            if (newSongs != null)
            {
                foreach (string song in newSongs)
                {
                    if (File.Exists(song) && song.EndsWith(".mp3"))
                    {
                        newPlaylist.Songs.Add(new Song(song, newPlaylist.RelativePathName));
                    }
                }
            }

            PlayLists.Add(newPlaylist);
            return newPlaylist;
        }
        public void SetCurrentlyPlaying(string playList, string songRelativePath)
        {

            Console.WriteLine(playList);
            PlayList playlist = PlayLists.Where(p => p.RelativePathName == playList).FirstOrDefault();

            if (playlist == null)
                throw new KeyNotFoundException(string.Format("Playlist "+  playlist));
       
            Song foundSong = playlist.Songs.Where(Song => Song.RelativePath == songRelativePath).FirstOrDefault();

            if (foundSong != null)
            {
                //Set song for playlist to sync playlist's queue index.
                playlist.SetCurrentSong(foundSong.RelativePath);
                CurrentPlayList = playlist; // Set new playlist, which will set currentSong
            }
            else
            {
                throw new KeyNotFoundException(string.Format("Song {0} Not Found!", songRelativePath));
            }


        }
        private PlayList FindPlayList(string playlistName) => PlayLists.Where(p => p.DisplayName == playlistName).FirstOrDefault();
        public void DeletePlaylist(string playlistName)
        {
            var playlist = FindPlayList(playlistName);
          

            if (playlist != null)
            {
                if (playlist.DisplayName == CurrentPlayList.DisplayName)
                    CurrentPlayList = null;
                PlayLists.Remove(playlist);
                
                PlayListChanged(playlist, true);
                foreach(var file in Directory.EnumerateFiles(playlist.RelativePathName))
                {
                    File.Delete(file);
                }
                Directory.Delete(playlist.RelativePathName);
            }

        }

        public void DeleteSong(string playlistAbsoluteName, string song)
        {
            string playlistDisplayName = Path.GetFileNameWithoutExtension(playlistAbsoluteName);
            var playlist = FindPlayList(playlistDisplayName);


            bool worked = playlist.DeleteSong(song);

            if (worked)
                PlayListChanged(playlist, false);
        }
        private void AddSong(ref PlayList playlist, string playlistName, string NewSong)
        {
            bool invalid = false;

            string newSongPlace = Path.Join(MusicPlayerPath, playlistName, Path.GetFileName(NewSong));
            invalid = playlist.Songs.Any(s => s.RelativePath == newSongPlace);


            invalid = !File.Exists(NewSong);

            if (!invalid)
                try
                {
                    if (!File.Exists(newSongPlace))
                        File.Copy(NewSong, newSongPlace, true);

                    playlist.Songs.Add(new Song(newSongPlace, playlist.RelativePathName));

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
            Console.WriteLine("Adding songs to " + playlistDisplayName);

            if (newSongs == null)
                throw new ArgumentNullException("Null Songs");

            PlayList playlist;


            playlist = FindPlayList(playlistDisplayName);

            if (playlist == null && !string.IsNullOrEmpty(playlistDisplayName))
                playlist = CreateNewPlayList(playlistDisplayName, new List<string>());

            foreach (string NewSong in newSongs)
                AddSong(ref playlist, playlistDisplayName, NewSong);

            PlayListChanged(playlist, false);
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
            CurrentPlayingSong = song.RelativePath;
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
                CurrentPlayingSong = CurrentPlayList.CurrentSong;
            }
        }

        public void LoadAllSongs()
        {
            var PlayListFolders = Directory.EnumerateDirectories(MusicPlayerPath);
            foreach (var folder in PlayListFolders)
            {
                var newPlayList = new PlayList(folder);
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
