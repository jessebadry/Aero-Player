using AeroPlayer.Models;
using AeroPlayer.Services;
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
        (string, string) selectedSong;
        public (string, string) SelectedSong
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


        /// <summary>
        /// Tuple(url:string, title:string);
        /// </summary>
        public ObservableCollection<(string, string)> Urls { get; set; } = new ObservableCollection<(string, string)>();
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
                string[] downloadUrls = Urls.Select(url => url.Item1).ToArray();
                List<string> AddedSongs = null;
                await Task.Run(() =>
                {
                    try
                    {
                        AddedSongs = YouTubeDownloader.DownloadSongs(downloadUrls, playlist.RelativePathName);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error Downloading songs! Err : {0} ", e);
                    }
                });
                bool? worked = null;
                if (AddedSongs != null)
                    worked = MusicManager.Instance.AddSongs(playlist.DisplayName, AddedSongs.ToArray());
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
            var tup = (url: youtubeResult.Url, title: youtubeResult.Title);
            
            Urls.Add(tup);

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
