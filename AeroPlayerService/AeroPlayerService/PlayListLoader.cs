using AeroPlayerService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AeroPlayerService
{
    /*
     * This is an new implementation of the Music Manager, the system will remain the same,
     * but the saving and loading system will be changed, the reason for changing the current system
     * which relies on the windows file system is that changing the names of songs is not as fluent as needed,
     * 
     * Following cases where the current system fails is as follows:
     * - Changing the current song's name due to the file already being  accessed is not allowed, "File is being used error"
     *      Ex: Cannot change the current song being played because NAudio is using it, (Work arounds are not ideal)
     * 
     * - Using multiple Audio types  is inflexible and would have to be statically implemented, when using a dynamic system would be more
     *   flexible and each and every audio type will not need to be implemented seperately.
     * 
     * - Performance of changing the display name quickly instead of the file name would be greatly advantageous.
     * 
     * 
     * This will be achieved by using JSON.
     * 
     * The Structure will remain the same, each playlist will have its own folder, but instead a json file will be managing the display name 
     * of each song.
     * Doing so will also open the possibility for organizing playlists without using folder structure and creating playlists that consist from
     * multiple playlists, called a Compilation Playlist.
     * 
     * The path will be programmatically set by using incrementation to name folders.
     * 
     * Json Structure:
     * 
     * PlayLists:[
     *      "PlaylistDisplayName""[]
     * 
     * ]
     */

    static class PlayListLoader
    {
        private static string SettingsFile = Path.Join(MusicManager.MusicPlayerPath, "AeroPlayer.json");
        public static void SerializePlayLists(List<PlayList> playlists)
        {
            MusicManager.EnsureMusicPlayerPath();
            string output = JsonConvert.SerializeObject(playlists);
            File.WriteAllText(SettingsFile, output);

        }
        public static List<PlayList> DeserializePlayLists()
        {
            MusicManager.EnsureMusicPlayerPath();
            if (!File.Exists(SettingsFile))
                File.Create(SettingsFile).Close();
            try
            {
                return JsonConvert.DeserializeObject<List<PlayList>>(File.ReadAllText(SettingsFile));
            }
            catch (JsonReaderException)
            {
                return null;
            }
        }

        static PlayListLoader()
        {
            MusicManager.EnsureMusicPlayerPath();
        }


    }
}
