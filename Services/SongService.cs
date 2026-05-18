using MusicfyWebApp.Models;
using MusicfyWebApp.Repositories.Interfaces;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Services
{
    public class SongService : ISongService
    {
        private readonly ISongRepository _repository;

        public SongService(ISongRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Song>> GetAllSongsAsync() => await _repository.GetAllAsync();
        public async Task<Song?> GetSongByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task CreateSongAsync(Song song) => await _repository.AddAsync(song);
        public async Task UpdateSongAsync(Song song) => await _repository.UpdateAsync(song);
        public async Task DeleteSongAsync(int id) => await _repository.DeleteAsync(id);

        // call search method from repository
        public async Task<IEnumerable<Song>> SearchSongsAsync(string searchTerm)
            => await _repository.SearchAsync(searchTerm);
    }
}