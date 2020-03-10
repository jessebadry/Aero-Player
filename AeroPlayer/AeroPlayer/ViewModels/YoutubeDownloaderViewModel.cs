using AeroPlayer.Services.YoutubeParser;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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

        public ObservableCollection<string> Urls { get; set; } = new ObservableCollection<string>();
        public DelegateCommand AddUrl { get; }
        public DelegateCommand SearchUrl { get; }
        public void AddUrlDelegate()
        {
            Urls.Add("");
        }
        public void SearchUrlDelegate()
        {
            YoutubeResults.Clear();
            var results = GetYoutubeQuery(Url);
            for(int  i= 0; i < results.Count; i++)
            {
                Console.WriteLine(results[i].ImageOutput);
                YoutubeResults.Add(results[i]);
            }
        }
        public YoutubeDownloaderViewModel()
        {
            AddUrl = new DelegateCommand(AddUrlDelegate);
            SearchUrl = new DelegateCommand(SearchUrlDelegate);

        }

    }
}
