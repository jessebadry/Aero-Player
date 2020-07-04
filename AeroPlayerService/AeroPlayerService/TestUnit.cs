using NAudio.Wave.Compression;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace AeroPlayerService
{
    class TestUnit
    {

        private static void ErrorHandler(string errorMsg)
        {
            Console.WriteLine(errorMsg);
        }
        private static void PrintSongs(ref MusicPlayer player)
        {
            foreach (var song in player.SongManager.CurrentPlayList.Songs)
            {
                Console.WriteLine(song.SongDisplay);
            }
        }
        public static void Main(string[] args)
        {

            string urlAddress = "http://google.com";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(urlAddress);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = null;

                if (String.IsNullOrWhiteSpace(response.CharacterSet))
                    readStream = new StreamReader(receiveStream);
                else
                    readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));

                string data = readStream.ReadToEnd();
                File.WriteAllText("test.html", data);
                response.Close();
                readStream.Close();
            }

            //try
            //{
            //    var player = new MusicPlayer();
            //    player.SongManager.RandomInPlayList();
            //    player.AudioPauseToggleStatus();
            //    player.SongManager.OnErrorEvent += ErrorHandler;
            //    while (true)
            //    {
            //        Console.WriteLine("Currently playing == " + player.SongDisplay);
            //        string line = Console.ReadLine();
            //        switch (line)
            //        {
            //            case "s":
            //                player.SongManager.NextSong(true);
            //                break;
            //            //Change Song Name
            //            case "vol":
            //                float amount = float.Parse(Console.ReadLine()) / 100;
            //                player.Volume = amount;
            //                break;
            //            case "csn":
            //                Console.WriteLine("Enter Song to change, Songs:");
            //                PrintSongs(ref player);
            //                string oldSongString = Console.ReadLine();
            //                Console.WriteLine("Enter new name: ");
            //                string newSong = Console.ReadLine();
            //                player.SongManager.CurrentPlayList.FindSong(oldSongString).ChangeSongName(newSong);
            //                PrintSongs(ref player);
            //                break;
            //            case "ds":
            //                Console.WriteLine("Enter Song to delete");
            //                PrintSongs(ref player);
            //                string songName = Console.ReadLine();
            //                Song song = player.SongManager.CurrentPlayList.FindSong(songName);

            //                player.SongManager.DeleteSong(song);
            //                PrintSongs(ref player);
            //                break;
            //            default:
            //                Console.WriteLine("Not a command!");
            //                break;


            //        }
            //    }
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e);
            //}
            //Console.WriteLine("FInished loop");
        }
    }
}
