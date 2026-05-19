using Microsoft.AspNetCore.Identity;

namespace MusicfyWebApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Profile picture path
        public string? ProfileImageUrl { get; set; }

        // User can have multiple playlists
        public ICollection<Playlist>? Playlists { get; set; }
    }
}
