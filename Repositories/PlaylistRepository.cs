using Microsoft.EntityFrameworkCore;
using MusicfyWebApp.Models;
using MusicfyWebApp.Repositories.Interfaces;

namespace MusicfyWebApp.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly MusicfyContext _context;

        public PlaylistRepository(MusicfyContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Playlist>> GetAllAsync() => await _context.Playlists.ToListAsync();

        public async Task<Playlist?> GetByIdAsync(int id) => await _context.Playlists.FindAsync(id);

        public async Task AddAsync(Playlist playlist)
        {
            await _context.Playlists.AddAsync(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Playlist playlist)
        {
            _context.Playlists.Update(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist != null)
            {
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();
            }
        }
    }
}