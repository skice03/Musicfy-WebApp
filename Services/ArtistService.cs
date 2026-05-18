using MusicfyWebApp.Models;
using MusicfyWebApp.Repositories.Interfaces;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _repository;

        public ArtistService(IArtistRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Artist>> GetAllArtistsAsync() => await _repository.GetAllAsync();
        public async Task<Artist?> GetArtistByIdAsync(int id) => await _repository.GetByIdAsync(id);
        public async Task CreateArtistAsync(Artist artist) => await _repository.AddAsync(artist);
        public async Task UpdateArtistAsync(Artist artist) => await _repository.UpdateAsync(artist);
        public async Task DeleteArtistAsync(int id) => await _repository.DeleteAsync(id);
    }
}