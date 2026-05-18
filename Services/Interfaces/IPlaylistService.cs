using MusicfyWebApp.Models;

namespace MusicfyWebApp.Services.Interfaces
{
    public interface IPlaylistService
    {
        Task<IEnumerable<Playlist>> GetAllPlaylistsAsync();
        Task<Playlist?> GetPlaylistByIdAsync(int id);
        Task CreatePlaylistAsync(Playlist playlist);
        Task UpdatePlaylistAsync(Playlist playlist);
        Task DeletePlaylistAsync(int id);
        Task AddSongToPlaylistAsync(int playlistId, int songId);
        Task RemoveSongFromPlaylistAsync(int playlistId, int songId);
    }
}