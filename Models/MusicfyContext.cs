using Microsoft.EntityFrameworkCore;

namespace MusicfyWebApp.Models
{
    public class MusicfyContext : DbContext
    {
        public MusicfyContext(DbContextOptions<MusicfyContext> options)
            : base(options)
        {
        }

        public DbSet<User>? Users { get; set; }
        public DbSet<Artist>? Artists { get; set; }
        public DbSet<Album>? Albums { get; set; }
        public DbSet<Song>? Songs { get; set; }
        public DbSet<Playlist>? Playlists { get; set; }
        public DbSet<PlaylistSong>? PlaylistSongs { get; set; }
    }
}