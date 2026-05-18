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

        public async Task<IEnumerable<Playlist>> GetAllAsync() =>
            await _context.Playlists.Include(p => p.User).ToListAsync();

        public async Task<Playlist?> GetByIdAsync(int id) =>
            await _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistSongs!)
                    .ThenInclude(ps => ps.Song!)
                    .ThenInclude(s => s.Artist)
                .Include(p => p.PlaylistSongs!)
                    .ThenInclude(ps => ps.Song!)
                    .ThenInclude(s => s.Album)
                .FirstOrDefaultAsync(p => p.PlaylistId == id);

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

        public async Task AddSongAsync(int playlistId, int songId)
        {
            // Check if already exists
            var exists = await _context.PlaylistSongs
                .AnyAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (!exists)
            {
                _context.PlaylistSongs.Add(new PlaylistSong
                {
                    PlaylistId = playlistId,
                    SongId = songId
                });
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveSongAsync(int playlistId, int songId)
        {
            var entry = await _context.PlaylistSongs
                .FirstOrDefaultAsync(ps => ps.PlaylistId == playlistId && ps.SongId == songId);
            if (entry != null)
            {
                _context.PlaylistSongs.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }
    }
}