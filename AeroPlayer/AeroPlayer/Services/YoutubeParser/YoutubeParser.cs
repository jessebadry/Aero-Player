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
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ScrapySharp.Extensions;
using OpenQA.Selenium.Support.UI;
using System.Threading;
using AeroPlayer.Services.AeroPlayerErrorHandler;

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

        public static async Task<string> GetContentFromUrl(string Url)
        {
            try
            {
                string content = null;
                await Task.Run(() =>
                {

                    ChromeDriverService service = ChromeDriverService.CreateDefaultService();
                    service.HideCommandPromptWindow = true;
                    ChromeOptions options = new ChromeOptions();
                    options.AddArgument("headless");
                    var driver = new ChromeDriver(service, options);

                    driver.Url = Url;
                    driver.Navigate();
                    content = driver.FindElement(By.TagName("body")).GetAttribute("innerHTML");
                    driver.Quit();
                });
                //  File.WriteAllText("test.html", content);
                return content.Replace('\r', ' ').Replace('\n', ' ');
            }
            catch (Exception ex)
            {
                OnError(ex.ToString());
            }
            return null;
        }
        private static string GetKey(this HtmlAttributeCollection collection, string key)
        {
            var attr = collection[key];

            return attr != null ? attr.Value : "";

        }
        //private static bool AttributesHasKey(string key, ref HtmlAttributeCollection htmlAttributes)
        //{
        //    return htmlAttributes != null && htmlAttributes[key] != null;
        //}
        //private static void AssignNonNullAttribute(ref string output, HtmlAttributeCollection attrs, params string[] allowedKeys)
        //{
        //    if (attrs == null)
        //    {
        //        OnError("Attributes Collection is null!");
        //        return;
        //    }


        //    for (int i = 0; i < allowedKeys.Length; i++)
        //    {
        //        if (AttributesHasKey(allowedKeys[i], ref attrs))
        //        {
        //            output = attrs[allowedKeys[i]].Value;
        //            return; // LEAVE FUNCTION
        //        }
        //    }
        //}
        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder(str.Length);
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
        public static async Task<List<YoutubeResult>> GetYoutubeQuery(string query)
        {

            var youtubeResults = new List<YoutubeResult>();
            try
            {
                string url = string.Format("https://www.youtube.com/results?search_query='{0}'", query);
                string htmlText = await GetContentFromUrl(url);
                var html = new HtmlDocument();
                html.LoadHtml(htmlText);
                //File.WriteAllText("test.html", htmlText);

                var youtubeResultNodes = html.DocumentNode.CssSelect("#dismissable");
                if (youtubeResultNodes == null || youtubeResultNodes.Count() == 0)
                {
                    AeroError.EmitError("No Results found");

                    return null;
                }
                int youtubeResultCount = youtubeResultNodes.Count();
                for (int i = 0; i < youtubeResultCount; i++)
                {
                    HtmlNode node = youtubeResultNodes.ElementAt(i);
                    var titleNode = node.CssSelect("#video-title");
                    string YoutubeUrl = titleNode.Count() == 0 ? null : titleNode.First()?.Attributes?.GetKey("href");

                    string Title = node.CssSelect("#video-title > yt-formatted-string").First().InnerText.Trim();


                    if (string.IsNullOrEmpty(YoutubeUrl))
                        continue;

                    YoutubeUrl = string.Format("https://www.youtube.com{0}", YoutubeUrl);

                    string ThumbnailImage = node.CssSelect("#img")?.First().Attributes.GetKey("src");


                    var result = new YoutubeResult
                    {
                        Url = YoutubeUrl,
                        ImgUrl = ThumbnailImage,
                        Title = Title,
                        ImagePath = Path.Join(output, i.ToString() + ".jpg"),

                    };
                    youtubeResults.Add(result);

                }

                var playlistResults = html.DocumentNode.CssSelect("#contents > ytd-playlist-renderer");
                Console.WriteLine("playlists count = {0}", playlistResults.Count());

                for (int i = 0; i < playlistResults.Count(); i++)
                {
                    var playlistVideoHeader = playlistResults.ElementAt(i);
                    string YoutubeUrl = playlistVideoHeader.CssSelect("#content > a")?.First()?.Attributes?.GetKey("href");
                    YoutubeUrl = string.Format("https://www.youtube.com{0}", YoutubeUrl);
                    string thumbnailUrl = playlistVideoHeader.CssSelect("#img")?.First().Attributes.GetKey("src");
                    if (string.IsNullOrEmpty(YoutubeUrl))
                        continue;
                    string playlistCount = playlistVideoHeader.CssSelect("#overlays > ytd-thumbnail-overlay-side-panel-renderer > yt-formatted-string")?.First().InnerText;
               
                    var youtubeResult = new YoutubeResult
                    {
                        Url = YoutubeUrl,
                        ImgUrl = thumbnailUrl,
                        Title = playlistVideoHeader.CssSelect("#video-title")?.First()?.Attributes?.GetKey("title") + " PLAYLIST",
                        ImagePath = Path.Join(output, (i + youtubeResultCount).ToString() + ".jpg"),
                        IsPlayList = true,
                        PlayListCount = playlistCount,

                    };

                    youtubeResults.Add(youtubeResult);
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
            foreach (string file in Directory.EnumerateFiles(output))
                File.Delete(file);

            //Clear all bitmap memory.
            GC.Collect();
        }
        public static async Task DownloadYoutubeThumbnail(YoutubeResult result)
        {
            if (string.IsNullOrEmpty(result.ImgUrl)) return;
            using var client = new WebClient();
            try
            {


                await client.DownloadFileTaskAsync(new Uri(result.ImgUrl), result.ImagePath);

                var image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = new MemoryStream(File.ReadAllBytes(result.ImagePath));
                image.EndInit();
                result.ImageBitMap = image;
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not download image, {0}", e);
                OnError(e.ToString());
            }

        }


    }
}
