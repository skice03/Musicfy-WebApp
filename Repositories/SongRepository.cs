using Microsoft.EntityFrameworkCore;
using MusicfyWebApp.Models;
using MusicfyWebApp.Repositories.Interfaces;

namespace MusicfyWebApp.Repositories
{
    public class SongRepository : ISongRepository
    {
        private readonly MusicfyContext _context;

        public SongRepository(MusicfyContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Song>> GetAllAsync()
        {
            return await _context.Songs
                                 .Include(s => s.Album)
                                 .Include(s => s.Artist) // to show the artist finally
                                 .ToListAsync();
        }

        public async Task<Song?> GetByIdAsync(int id)
        {
            return await _context.Songs
                                 .Include(s => s.Album)
                                 .FirstOrDefaultAsync(s => s.SongId == id);
        }

        public async Task AddAsync(Song song)
        {
            await _context.Songs.AddAsync(song);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Song song)
        {
            _context.Songs.Update(song);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var song = await _context.Songs.FindAsync(id);
            if (song != null)
            {
                _context.Songs.Remove(song);
                await _context.SaveChangesAsync();
            }
        }

        // search
        public async Task<IEnumerable<Song>> SearchAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            return await _context.Songs
                .Where(s => s.Title.ToLower().Contains(searchTerm.ToLower()))
                .ToListAsync();
        }
    }
}