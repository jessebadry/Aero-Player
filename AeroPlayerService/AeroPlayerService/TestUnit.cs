using System;
using System.Collections.Generic;
using System.Text;

namespace AeroPlayerService
{
    class TestUnit
    {
        public static void Main(string[] args)
        {
            string link = Console.ReadLine();
            YouTubeDownloader.GetSong(link);
        }
    }
}
