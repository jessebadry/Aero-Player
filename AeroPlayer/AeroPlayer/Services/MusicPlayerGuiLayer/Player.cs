using AeroPlayerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AeroPlayer.Services.MusicPlayerGuiLayer
{

    //AeroPlayer version of the AeroPlayer Service.
    public class Player : PropertyObject
    {
        public readonly MusicPlayer player = new MusicPlayer();
        private ObservableCollection<PlayList> playlists = new ObservableCollection<PlayList>();
        public ObservableCollection<PlayList> PlayLists { get { return playlists; } set { playlists = value; onPropertyChanged("PlayLists"); } }

        private void LoadPlayLists()
        {
            PlayLists.Clear();
           for(int i =0; i < player.SongManager.PlayLists.Count;i++)
            {
                PlayLists.Add(player.SongManager.PlayLists[i]);
            }
        }
        public Player()
        {
            player.SongManager.OnPlaylistChange += delegate (object sender, PlayList playlist, bool delete)
            {
                Console.WriteLine("deleting..");
                if (delete)
                {
                    PlayLists.Remove(playlist);
                    return; /////////////////  LEAVE FUNCTION
                }

                int index = PlayLists.Select((Playlist, index) => new { Playlist, index })
                            .First(s => s.Playlist.DisplayName == playlist.DisplayName).index;
                //for (int i = 0; i < PlayLists.Count; i++)
                //{
                //    if (PlayLists[i].DisplayName == playlist.DisplayName)
                //    {
                //        index = i;
                //        break;
                //    }

                //}
                Console.WriteLine("index = " + index);

                if (index > PlayLists.Count - 1)
                    PlayLists.Add(playlist);
                else
                    PlayLists[index] = playlist;
                onPropertyChanged("PlayLists");


            };
            LoadPlayLists();

        }
        



    }
}
