namespace MusicfyWebApp.Models.ViewModels
{
    public class LibraryViewModel
    {
        public IEnumerable<Album> Albums { get; set; } = new List<Album>();
        public IEnumerable<Playlist> Playlists { get; set; } = new List<Playlist>();
    }
}
