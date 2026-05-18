using System.ComponentModel.DataAnnotations;
namespace MusicfyWebApp.Models

{
    public class Song
    {
        public int SongId { get; set; } // PK
        [Required(ErrorMessage = "Title is required")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Artist is required")]
        public int ArtistId { get; set; }
        public Artist? Artist { get; set; }
        [Required(ErrorMessage = "Length is required")]
        public string? Length { get; set; } // ex: "1:23"
        public string? AudioUrl { get; set; } // path to uploaded audio file

        // Album reference (Foreign Key & Reference Navigation Property)
        public int? AlbumId { get; set; }
        public Album? Album { get; set; }

        // User who uploaded this song (null = platform/admin song)
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // A song belongs to one or more playlists
        public ICollection<PlaylistSong>? PlaylistSongs { get; set; }
    }
}