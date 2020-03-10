using HtmlAgilityPack;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AeroPlayer.Services.YoutubeParser
{
    public static class YoutubeParser
    {
        static readonly string output = "YoutubeDownloader/Images";
        public class YoutubeResult
        {
            public string Url { get; set; }
            public string ImgUrl
            {
                get; set;
            }
            public string ImagePath { get; set; }
            public string Title { get; set; }
            public string Views { get; set; }
            public ImageSource ImageOutput { get; set; }


        }
    


        static string GetUrlHtml(string url)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless", "--blink-settings=imagesEnabled=false");
            chromeOptions.AddArguments("--disable-extensions");
            chromeOptions.AddArgument("test-type");
            chromeOptions.AddArgument("--ignore-certificate-errors");
            chromeOptions.AddArgument("no-sandbox");

            string htmlText = null;
            using (var d = new ChromeDriver(chromeOptions))
            {

                d.Url = url;

                d.Navigate();
                htmlText = d.PageSource;
            }
            return htmlText;
        }
        static YoutubeParser()
        {
           

        }
        public static List<YoutubeResult> GetYoutubeQuery(string query)
        {
            Directory.CreateDirectory(output);
            string url = "https://www.youtube.com/results?search_query=" + query;
            var youtubeResults = new List<YoutubeResult>();
   
            
            string htmlText = GetUrlHtml(url);
            var html = new HtmlDocument();
            html.LoadHtml(htmlText);
            var nodes = html.DocumentNode.SelectNodes("//div[@id='dismissable']");

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                var imgNode = node.SelectSingleNode("ytd-thumbnail/a/yt-img-shadow/img");

                if (imgNode == null)
                    continue;

                var imgSrcAttr = imgNode.Attributes["src"];
                if (imgSrcAttr == null)
                    continue;

                var imgSrc = imgSrcAttr.Value;
                var titleATag = node.SelectSingleNode("div/div[@id='meta']/div/h3/a");
                string title = titleATag.Attributes["title"].Value;
                string youtubeUrl = "https://www.youtube.com" + titleATag.Attributes["href"].Value;

                var views = node.SelectSingleNode("div/div[@id='meta']/ytd-video-meta-block/div[@id='metadata']/div[@id='metadata-line']/span[@class='style-scope ytd-video-meta-block']").InnerText;



                var result = new YoutubeResult
                {
                    Url = youtubeUrl,
                    ImgUrl = imgSrc,
                    Title = title,
                    Views = views,
                    ImagePath = Path.Join(output, i + ".jpg"),
                };


                youtubeResults.Add(result);
            }
            DownloadYoutubeThumbnails(youtubeResults);



            return youtubeResults;
        }
        public static void DownloadYoutubeThumbnails(List<YoutubeResult> results)
        {
            using var client = new WebClient();
            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                client.DownloadFile(result.ImgUrl, result.ImagePath);
               
                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new MemoryStream(File.ReadAllBytes(result.ImagePath));
                image.EndInit();
                result.ImageOutput = image;
                
            }
            GC.Collect();
        }
        public static void Main(string[] args)
        {

           

        }
    }
}
