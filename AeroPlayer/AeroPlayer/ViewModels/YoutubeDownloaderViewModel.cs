using AeroPlayer.Models;
using AeroPlayer.Views.Dialogs;
using AeroPlayerService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using static AeroPlayer.Services.YoutubeParser.YoutubeParser;

namespace AeroPlayer.ViewModels
{
    class YoutubeDownloaderViewModel : PropertyObject
    {
        private bool isSearchingYoutube = false;
        Tuple<string, string> selectedSong;
        public Tuple<string, string> SelectedSong
        {
            get
            {
                return selectedSong;
            }
            set
            {
                selectedSong = value;
                onPropertyChanged("SelectedSong");

            }
        }
        string url;
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                url = value;
                onPropertyChanged("Url");
            }
        }
        private PlayList RunSelectPlayListDialog()
        {
            var dialog = new SongDownloadDialog();
            if (dialog.ShowDialog() == true)
            {
                return ((SongDownloadDialogViewModel)dialog.DataContext).Selected;
            }
            else
            {
                return null;
            }

        }
        public ObservableCollection<YoutubeResult> YoutubeResults { get; set; } = new ObservableCollection<YoutubeResult>();

        public ObservableCollection<Tuple<string, string>> Urls { get; set; } = new ObservableCollection<Tuple<string, string>>();
        public DelegateCommand DownloadUrls { get; }
        public DelegateCommand SearchUrl { get; }
        public DelegateCommand<YoutubeResult> AddSelection { get; }
        public DelegateCommand DeleteSong { get; }

        public async void DownloadUrlsDelegate()
        {

            PlayList playlist = RunSelectPlayListDialog();
            if (playlist != null)
            {
                Console.WriteLine("downloading...");
                string[] downloadUrls = new string[Urls.Count];
                for (int i = 0; i < Urls.Count; i++)
                {
                    downloadUrls[i] = Urls[i].Item1;
                }
                List<string> AddedSongs = null;
                await Task.Run(() =>
                {
                    AddedSongs = YouTubeDownloader.DownloadSongs(downloadUrls, playlist.RelativePathName);
                });

                bool worked = MusicManager.Instance.AddSongs(playlist.DisplayName, AddedSongs.ToArray());
                Console.WriteLine(worked);

            }
        }
        public void DeleteSongDelegate()
        {
            Console.WriteLine("deleting..");
            Urls.Remove(SelectedSong);
        }
        public async void SearchUrlDelegate()
        {
            if (isSearchingYoutube)
                return;

            isSearchingYoutube = true;
            YoutubeResults.Clear();

            var results = await GetYoutubeQuery(Url);
            if (results != null)
                for (int i = 0; i < results.Count; i++)
                {
                    YoutubeResults.Add(results[i]);
                }

            isSearchingYoutube = false;
        }
        public void AddSelectionDelegate(object sender)
        {
            var youtubeResult = (YoutubeResult)sender;

            if (Urls.Any(url => url.Item1 == youtubeResult.Url))
                return;
            Urls.Add(Tuple.Create(youtubeResult.Url, youtubeResult.Title));

        }
        public YoutubeDownloaderViewModel()
        {

            SearchUrl = new DelegateCommand(SearchUrlDelegate);
            AddSelection = new DelegateCommand<YoutubeResult>(AddSelectionDelegate);
            DeleteSong = new DelegateCommand(DeleteSongDelegate);
            DownloadUrls = new DelegateCommand(DownloadUrlsDelegate);

        }

    }
}
