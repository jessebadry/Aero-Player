using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.IO;
using VideoLibrary;

namespace AeroPlayerService
{
    public class YouTubeDownloader
    {
        static readonly string YoutubeDownloaderPath = "MusicPlayer/DownloaderTemp/";
        public static List<string> DownloadSongs(string[] urls, string output)
        {


            var youtube = YouTube.Default;
            var results = new List<string>();
            Directory.CreateDirectory(YoutubeDownloaderPath);

            foreach (string url in urls)
            {

                var vid = youtube.GetVideo(url);
                string mp4Path = Path.Join(YoutubeDownloaderPath, vid.FullName);
                File.WriteAllBytes(mp4Path, vid.GetBytes());

                var inputFile = new MediaFile { Filename = YoutubeDownloaderPath + vid.FullName };

                string outputName = Path.Join(output, Path.GetFileNameWithoutExtension(vid.FullName))+".mp3";

                bool invalid = false;
                try
                {
                    var outputFile = new MediaFile { Filename = outputName };

                    using (var engine = new Engine())
                    {
                        engine.GetMetadata(inputFile);

                        engine.Convert(inputFile, outputFile);
                    }
                    
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    invalid = true; 
                }


                if (!invalid)
                    results.Add(outputName);

            }
            GC.Collect();

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
