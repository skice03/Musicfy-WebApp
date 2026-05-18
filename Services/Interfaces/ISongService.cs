using MusicfyWebApp.Models;

namespace MusicfyWebApp.Services.Interfaces
{
    public interface ISongService
    {
        Task<IEnumerable<Song>> GetAllSongsAsync();
        Task<Song?> GetSongByIdAsync(int id);
        Task CreateSongAsync(Song song);
        Task UpdateSongAsync(Song song);
        Task DeleteSongAsync(int id);

        // search
        Task<IEnumerable<Song>> SearchSongsAsync(string searchTerm);
    }
}