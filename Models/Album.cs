namespace MusicfyWebApp.Models
{
    public class Album
    {
        public int AlbumId { get; set; } // PK
        public string? Title { get; set; }
        public int ReleaseYear { get; set; }
        public string? CoverImageUrl { get; set; }

        // Artist reference (Foreign Key & Reference Navigation Property)
        public int ArtistId { get; set; }
        public Artist? Artist { get; set; }

        // An album has multiple songs
        public ICollection<Song>? Songs { get; set; }
    }
}