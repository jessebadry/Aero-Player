using AeroPlayerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AeroPlayer.ViewModels
{
    class SongDownloadDialogViewModel : PropertyObject
    {


        public ObservableCollection<PlayList> PlayLists { get; set; } = new ObservableCollection<PlayList>();
        PlayList selected;
        public PlayList Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
                onPropertyChanged("Selected");
            }


        }
        public void LoadPlaylists()
        {
            PlayLists.Clear();
            foreach (var playlist in MusicManager.Instance.PlayLists)
            {
                PlayLists.Add(playlist);
            }
        }
      
        public SongDownloadDialogViewModel()
        {
         
        }

        

    }
}
