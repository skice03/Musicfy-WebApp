using Microsoft.EntityFrameworkCore;
using MusicfyWebApp.Models;
using MusicfyWebApp.Repositories.Interfaces;

namespace MusicfyWebApp.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly MusicfyContext _context;

        public AlbumRepository(MusicfyContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Album>> GetAllAsync() =>
            await _context.Albums.Include(a => a.Artist).ToListAsync();

        public async Task<Album?> GetByIdAsync(int id) =>
            await _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Songs!)
                    .ThenInclude(s => s.Artist)
                .FirstOrDefaultAsync(a => a.AlbumId == id);

        public async Task AddAsync(Album album)
        {
            await _context.Albums.AddAsync(album);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Album album)
        {
            _context.Albums.Update(album);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var album = await _context.Albums.FindAsync(id);
            if (album != null)
            {
                _context.Albums.Remove(album);
                await _context.SaveChangesAsync();
            }
        }
    }
}