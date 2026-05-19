namespace MusicfyWebApp.Models
{
    public class Artist
    {
        public int ArtistId { get; set; } // PK
        public string? Name { get; set; }
        public string? Bio { get; set; }
        public string? ImageUrl { get; set; }
        
        // Indicates whether the artist profile is officially verified
        public bool IsVerified { get; set; }

        // An artist can have multiple albums
        public ICollection<Album>? Albums { get; set; }
        public ICollection<Song>? Songs { get; set; }
    }
}