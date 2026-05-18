namespace MusicfyWebApp.Models
{
    public class PlaylistSong
    {
        public int PlaylistSongId { get; set; } // PK

        // FK -> Playlist
        public int PlaylistId { get; set; }
        public Playlist? Playlist { get; set; }

        // FK -> Song
        public int SongId { get; set; }
        public Song? Song { get; set; }
    }
}