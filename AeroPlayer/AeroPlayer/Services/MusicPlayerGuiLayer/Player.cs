using AeroPlayerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AeroPlayer.Services.MusicPlayerGuiLayer
{

    //AeroPlayer version of the AeroPlayer Service.
    public class Player : MusicPlayer
    {
        private ObservableCollection<PlayList> playlists = new ObservableCollection<PlayList>();
        public ObservableCollection<PlayList> PlayLists { get { return playlists; } set { playlists = value; onPropertyChanged("PlayLists"); } }

        private void LoadPlayLists()
        {
            PlayLists.Clear();
            for (int i = 0; i < SongManager.PlayLists.Count; i++)
            {
                PlayLists.Add(SongManager.PlayLists[i]);
            }
        }

        public Player() : base()
        {
            SongManager.OnPlaylistChange += delegate (object sender, PlayList playlist, bool delete)
            {

                if (delete)
                {
                    PlayLists.Remove(playlist);
                    return; /////////////////  LEAVE FUNCTION
                }
                //If not deleting this playlist, we are editing the existing one, or adding a new one if not found.

                int index = PlayLists.Select((Playlist, index) => new { Playlist, index })
                            .First(s => s.Playlist.DisplayName == playlist.DisplayName).index;

                Console.WriteLine("index = " + index);

                if (index >= 0 && index > PlayLists.Count - 1)
                    PlayLists.Add(playlist);
                else
                    PlayLists[index] = playlist;
                onPropertyChanged("PlayLists");


            };
            LoadPlayLists();

        }




    }
}
