using AeroPlayer.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AeroPlayer.Services.YoutubeParser
{
    public static class YoutubeParser
    {
  
        public delegate void ErrorEvent(string error);
        public static event ErrorEvent ErrorEventHandler;
        private static void OnError(string error)
        {
            ErrorEventHandler?.Invoke(error);

        }
        static readonly string output = "YoutubeDownloader/Images";

        static WebClient webclient;

        public static async Task<string> getContentFromUrl(String Url)
        {
            try
            {
                webclient = new WebClient
                {
                    Encoding = Encoding.UTF8
                };
                webclient.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; " +
                                   "Windows NT 5.2; .NET CLR 1.0.3705;)");
                Task<string> downloadStringTask = webclient.DownloadStringTaskAsync(new Uri(Url));
                var content = await downloadStringTask;

                return content.Replace('\r', ' ').Replace('\n', ' ');
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                OnError(ex.ToString());
            }
            return null;
        }
        private static bool AttributesHasKey(string key, ref HtmlAttributeCollection htmlAttributes)
        {
            return htmlAttributes != null && htmlAttributes[key] != null;
        }
        private static void AssignNonNullAttribute(ref string output, HtmlAttributeCollection attrs, params string[] allowedKeys)
        {
            if (attrs == null)
            {
                OnError("Attributes Collection is null!");
                return;
            }
                

            for (int i = 0; i < allowedKeys.Length; i++)
            {
                if (AttributesHasKey(allowedKeys[i], ref attrs))
                {
                    output = attrs[allowedKeys[i]].Value;
                    return; // LEAVE FUNCTION
                }
            }
        }

        public static async Task<List<YoutubeResult>> GetYoutubeQuery(string query)
        {
       
            var youtubeResults = new List<YoutubeResult>();
            try
            {
                string url = "https://www.youtube.com/results?search_query=" + query;
                string htmlText = await getContentFromUrl(url);
                var html = new HtmlDocument();
                html.LoadHtml(htmlText);
                
                var youtubeResultNodes = html.DocumentNode.SelectNodes("//div[@class='yt-lockup-dismissable yt-uix-tile']");
                if (youtubeResultNodes == null)
                {
                    Console.WriteLine("Could not load query..");
                    OnError("Could not load query");
                  
                    return null;
                }

                //Iterate all youtube results
                for (int i = 0; i < youtubeResultNodes.Count; i++)
                {
                    var node = youtubeResultNodes[i];

                    var thumbNailNode = node.SelectSingleNode("div[@class='yt-lockup-thumbnail contains-addto']/a/div/span/img");
                    if (thumbNailNode == null)
                        continue; // SKIP RESULT

                    var imgAttrs = thumbNailNode.Attributes;

                    // Get Thumbnail Url.
                    string imgSrc = null;

                    //If doesn't start with https, it is an invalid link, and also means that the img url is under 'data-thumb'  instead.
                    if (AttributesHasKey("src", ref imgAttrs) && imgAttrs["src"].Value.StartsWith("https"))
                        imgSrc = imgAttrs["src"].Value;
                    else if (AttributesHasKey("data-thumb", ref imgAttrs))
                        imgSrc = imgAttrs["data-thumb"].Value;
                    else continue;

                    //Get Node of detail content, title, url, views, description ect.
                    var detailsNode = node.SelectSingleNode("div[@class='yt-lockup-content']");

                    //<a> tag contains video title and the end part of the video url, EX: '/watch?v=ID'
                    var videoATag = detailsNode.SelectSingleNode("h3/a");

                    string title = videoATag.InnerText;

                    //Creating FULL youtube link.
                    string youtubeUrl = "https://www.youtube.com" + videoATag.Attributes["href"].Value;

                    //Get Views
                    var viewsNode = detailsNode.SelectSingleNode("(div[@class='yt-lockup-meta ']/ul/li)[2]");


                    string views = viewsNode == null ? "" : viewsNode.InnerText;

                    //Compile strings to Result object.
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
                //END OF LOOP
                await DownloadThumbnails(youtubeResults);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                OnError(e.ToString());
            }
           
            return youtubeResults;
        }
        private static async Task DownloadThumbnails(List<YoutubeResult> results)
        {
            var dir = Directory.CreateDirectory(output);
            await Task.WhenAll(results.Select(result => DownloadYoutubeThumbnail(result)));
            foreach (FileInfo file in dir.EnumerateFiles())
            {
             
                File.Delete(file.FullName);
            }
            //Clear all bitmap memory.
            GC.Collect();
        }
        public static async Task DownloadYoutubeThumbnail(YoutubeResult result)
        {
            try
            {
               
                using var client = new WebClient();
                await client.DownloadFileTaskAsync(new Uri(result.ImgUrl), result.ImagePath);

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new MemoryStream(File.ReadAllBytes(result.ImagePath));
                image.EndInit();
                result.ImageOutput = image;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                OnError(e.ToString());
            }

        }


    }
}
