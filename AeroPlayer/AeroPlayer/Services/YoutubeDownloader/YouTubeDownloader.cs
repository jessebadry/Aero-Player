using AeroPlayer.Services.AeroPlayerErrorHandler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace AeroPlayer.Services
{
    public class YouTubeDownloader
    {
        static readonly string YoutubeDownloaderPath = "MusicPlayer/DownloaderTemp/";
        static readonly string SongPath = Path.Join(YoutubeDownloaderPath, "Songs");
        public static string[] DownloadSongs(string[] urls)
        {
            Directory.CreateDirectory(SongPath);
            ClearTemp();
            string path = Path.Join(SongPath, "%(title)s.mp3");
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "youtube-dl.exe",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            foreach (string url in urls)
            {
                Process downloadYoutubeVids = new Process();

                startInfo.Arguments = string.Format("-f bestaudio {0} -o \"{1}\"", url, path);
                downloadYoutubeVids.StartInfo = startInfo;

                downloadYoutubeVids.Start();
                downloadYoutubeVids.WaitForExit();

            }
            var files = Directory.GetFiles(SongPath);


            return files;
        }
        public static void ClearTemp()
        {

            foreach (var file in Directory.GetFiles(SongPath))
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    AeroError.EmitError(e.ToString());
                }
            }
        }


    }
}