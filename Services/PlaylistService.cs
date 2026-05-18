using MusicfyWebApp.Models;
using MusicfyWebApp.Repositories.Interfaces;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _repository;

        public PlaylistService(IPlaylistRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Playlist>> GetAllPlaylistsAsync() => await _repository.GetAllAsync();
        public async Task<Playlist?> GetPlaylistByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task CreatePlaylistAsync(Playlist playlist) => await _repository.AddAsync(playlist);
        public async Task UpdatePlaylistAsync(Playlist playlist) => await _repository.UpdateAsync(playlist);
        public async Task DeletePlaylistAsync(int id) => await _repository.DeleteAsync(id);
        public async Task AddSongToPlaylistAsync(int playlistId, int songId) => await _repository.AddSongAsync(playlistId, songId);
        public async Task RemoveSongFromPlaylistAsync(int playlistId, int songId) => await _repository.RemoveSongAsync(playlistId, songId);
    }
}