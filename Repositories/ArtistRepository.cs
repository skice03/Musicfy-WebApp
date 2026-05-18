using Microsoft.EntityFrameworkCore;
using MusicfyWebApp.Models;
using MusicfyWebApp.Repositories.Interfaces;

namespace MusicfyWebApp.Repositories
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly MusicfyContext _context;

        public ArtistRepository(MusicfyContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Artist>> GetAllAsync() => await _context.Artists.ToListAsync();

        public async Task<Artist?> GetByIdAsync(int id)
        {
            return await _context.Artists
                                 .Include(a => a.Songs!) // we first get the songs and the album afterwards
                                 .ThenInclude(s => s.Album)
                                 .FirstOrDefaultAsync(a => a.ArtistId == id);
        }

        public async Task AddAsync(Artist artist)
        {
            await _context.Artists.AddAsync(artist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Artist artist)
        {
            _context.Artists.Update(artist);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist != null)
            {
                _context.Artists.Remove(artist);
                await _context.SaveChangesAsync();
            }
        }
    }
}