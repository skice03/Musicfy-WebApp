namespace MusicfyWebApp.Models
{
    public class Playlist
    {
        public int PlaylistId { get; set; } // PK
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }

        // User Reference (whoever made the playlist) using Identity string IDs
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }

        // A playlist has one or more songs 
        public ICollection<PlaylistSong>? PlaylistSongs { get; set; }
    }
}