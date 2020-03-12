using System;
using System.Collections.Generic;
using System.Text;

namespace AeroPlayerService
{
    class TestUnit
    {
        public static void Main(string[] args)
        {
            try
            {

                var player = new MusicPlayer();
                player.SongManager.NextSong(true);
                player.AudioPauseToggleStatus();
                while (true)
                {
                    string line = Console.ReadLine();
                    switch (line)
                    {
                        case "s":
                            player.SongManager.NextSong(true);
                            break;
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("FInished loop");
        }
    }
}
