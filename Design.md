Technologies: C#, Wpf, NAudio.

MusicPlayerService{
  MusicPlayerService is the backend for the music player, the music player will queue songs and have them group in playlists by folder.
  
  ex. RockSongsFolder = PlayList for RockSongs
  
  To Store the queues each playlist/directory will be given its own IEnumerable<string> and the queue will be stored 
  in a List<IEnumerable<string>>.
  
  

}
