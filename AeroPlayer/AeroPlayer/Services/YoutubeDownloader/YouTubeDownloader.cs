using MediaToolkit;
using MediaToolkit.Model;
using System.Collections.Generic;
using System.IO;
using VideoLibrary;

namespace AeroPlayerService
{
    public class YouTubeDownloader
    {
        static readonly string YoutubeDownloaderPath = "MusicPlayer/Downloader";
        public static List<string> DownloadSongs(string[] urls, string output)
        {


            var youtube = YouTube.Default;
            var results = new List<string>();
            foreach (string url in urls)
            {
                var vid = youtube.GetVideo(url);
                File.WriteAllBytes(Path.Join(YoutubeDownloaderPath, vid.FullName), vid.GetBytes());

                var inputFile = new MediaFile { Filename = YoutubeDownloaderPath + vid.FullName };

                string outputName = Path.Join(output, Path.GetFileNameWithoutExtension(vid.FullName));

                results.Add(outputName);

                var outputFile = new MediaFile { Filename = $"{outputName}.mp3" };

                using (var engine = new Engine())
                {
                    engine.GetMetadata(inputFile);

                    engine.Convert(inputFile, outputFile);
                }
            }
            return results;
        }
        public static void GetSong(string url)
        {
            var source = @"MusicPlayer\";
            var youtube = YouTube.Default;
            var vid = youtube.GetVideo(url);
            File.WriteAllBytes(source + vid.FullName, vid.GetBytes());

            var inputFile = new MediaFile { Filename = source + vid.FullName };
            var outputFile = new MediaFile { Filename = $"{source + Path.GetFileNameWithoutExtension(vid.FullName)}.mp3" };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                engine.Convert(inputFile, outputFile);
            }


        }

    }
}
