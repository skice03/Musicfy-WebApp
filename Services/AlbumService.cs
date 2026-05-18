using MusicfyWebApp.Models;
using MusicfyWebApp.Repositories.Interfaces;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _repository;

        public AlbumService(IAlbumRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Album>> GetAllAlbumsAsync() => await _repository.GetAllAsync();
        public async Task<Album?> GetAlbumByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task CreateAlbumAsync(Album album) => await _repository.AddAsync(album);
        public async Task UpdateAlbumAsync(Album album) => await _repository.UpdateAsync(album);
        public async Task DeleteAlbumAsync(int id) => await _repository.DeleteAsync(id);
    }
}