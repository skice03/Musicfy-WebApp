using MusicfyWebApp.Models;

namespace MusicfyWebApp.Repositories.Interfaces
{
    public interface IPlaylistRepository
    {
        Task<IEnumerable<Playlist>> GetAllAsync();
        Task<Playlist?> GetByIdAsync(int id);
        Task AddAsync(Playlist playlist);
        Task UpdateAsync(Playlist playlist);
        Task DeleteAsync(int id);
        Task AddSongAsync(int playlistId, int songId);
        Task RemoveSongAsync(int playlistId, int songId);
    }
}