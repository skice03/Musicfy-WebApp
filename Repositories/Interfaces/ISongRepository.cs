using MusicfyWebApp.Models;

namespace MusicfyWebApp.Repositories.Interfaces
{
    public interface ISongRepository
    {
        Task<IEnumerable<Song>> GetAllAsync();
        Task<Song?> GetByIdAsync(int id);
        Task AddAsync(Song song);
        Task UpdateAsync(Song song);
        Task DeleteAsync(int id);

        // to search for a song
        Task<IEnumerable<Song>> SearchAsync(string searchTerm);
    }
}