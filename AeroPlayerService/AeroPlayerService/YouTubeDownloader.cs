using MediaToolkit;
using MediaToolkit.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VideoLibrary;

namespace AeroPlayerService
{
    class YouTubeDownloader
    {

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
