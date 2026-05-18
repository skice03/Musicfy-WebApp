namespace MusicfyWebApp.Models
{
    public class User
    {
        public int UserId { get; set; } // PK
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }

        // User can have muliple playlists (Collection Navigation Property)
        public ICollection<Playlist>? Playlists { get; set; }
    }
}