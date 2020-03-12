using AeroPlayer.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using static AeroPlayer.Services.YoutubeParser.YoutubeParser;

namespace AeroPlayer.ViewModels
{
    class YoutubeDownloaderViewModel : PropertyObject
    {
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

        public ObservableCollection<YoutubeResult> YoutubeResults { get; set; } = new ObservableCollection<YoutubeResult>();

        public ObservableCollection<Tuple<string, string>> Urls { get; set; } = new ObservableCollection<Tuple<string, string>>();
        public DelegateCommand AddUrl { get; }
        public DelegateCommand SearchUrl { get; }
        public DelegateCommand<YoutubeResult> Download { get; }

        public void AddUrlDelegate()
        {

        }
        public async void SearchUrlDelegate()
        {
            YoutubeResults.Clear();
            var results = await GetYoutubeQuery(Url);
            for (int i = 0; i < results.Count; i++)
            {
                YoutubeResults.Add(results[i]);
            }

        }
        public void DownloadDelegate(object sender)
        {
            var youtubeResult = (YoutubeResult)sender;

            if (Urls.Any(url => url.Item1 == youtubeResult.Url))
                return;
            Urls.Add(Tuple.Create(youtubeResult.Url, youtubeResult.Title));

        }
        public YoutubeDownloaderViewModel()
        {
            AddUrl = new DelegateCommand(AddUrlDelegate);
            SearchUrl = new DelegateCommand(SearchUrlDelegate);
            Download = new DelegateCommand<YoutubeResult>(DownloadDelegate);

        }

    }
}
