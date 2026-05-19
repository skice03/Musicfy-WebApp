using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicfyWebApp.Models;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Controllers
{
    [Authorize]
    public class SongsController : Controller
    {
        private readonly ISongService _songService;
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IPlaylistService _playlistService;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        public SongsController(ISongService songService, IAlbumService albumService,
            IArtistService artistService, IPlaylistService playlistService, IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager)
        {
            _songService = songService;
            _albumService = albumService;
            _artistService = artistService;
            _playlistService = playlistService;
            _environment = environment;
            _userManager = userManager;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var songs = await _songService.GetAllSongsAsync();
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                ViewBag.CurrentUserId = userId;
                
                var allPlaylists = await _playlistService.GetAllPlaylistsAsync();
                if (User.IsInRole("Admin"))
                {
                    ViewBag.UserPlaylists = allPlaylists.ToList();
                }
                else
                {
                    ViewBag.UserPlaylists = allPlaylists.Where(p => p.UserId == userId).ToList();
                }
            }
            return View(songs);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string searchTerm)
        {
            IEnumerable<Song> songs;
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                songs = await _songService.GetAllSongsAsync();
            }
            else
            {
                songs = await _songService.SearchSongsAsync(searchTerm);
            }

            // Song entities into a lightweight anonymous type to avoid 'System.Text.Json.JsonException' object cycle errors.
            var result = songs.Select(s => new {
                songId = s.SongId,
                title = s.Title,
                length = s.Length,
                audioUrl = s.AudioUrl,
                album = s.Album != null ? new { title = s.Album.Title } : null,
                artist = s.Artist != null ? new { name = s.Artist.Name } : null
            });

            return Json(result);
        }

        // Both Admin and User can create songs
        public async Task<IActionResult> Create()
        {
            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title");
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SongId,Title,Length,AlbumId,ArtistId")] Song song, IFormFile? AudioFile)
        {
            if (AudioFile != null && AudioFile.Length > 0)
            {
                song.AudioUrl = await SaveAudioFile(AudioFile);
            }

            // Set ownership, Admin songs have no UserId (platform songs)
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !User.IsInRole("Admin"))
            {
                song.UserId = user.Id;
            }

            if (ModelState.IsValid)
            {
                await _songService.CreateSongAsync(song);
                return RedirectToAction(nameof(Index));
            }

            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title", song.AlbumId);
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", song.ArtistId);
            return View(song);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var song = await _songService.GetSongByIdAsync(id.Value);
            if (song == null) return NotFound();
            return View(song);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var song = await _songService.GetSongByIdAsync(id.Value);
            if (song == null) return NotFound();

            // Check: Admin can edit platform songs, User can only edit own songs
            if (!CanModifySong(song))
                return RedirectToAction("AccessDenied", "Account");

            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title", song.AlbumId);
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", song.ArtistId);
            return View(song);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SongId,Title,Length,AudioUrl,AlbumId,ArtistId,UserId")] Song song, IFormFile? AudioFile)
        {
            if (id != song.SongId) return NotFound();

            // Re-check ownership
            var existingSong = await _songService.GetSongByIdAsync(id);
            if (existingSong == null || !CanModifySong(existingSong))
                return RedirectToAction("AccessDenied", "Account");

            existingSong.Title = song.Title;
            existingSong.Length = song.Length;
            existingSong.AlbumId = song.AlbumId;
            existingSong.ArtistId = song.ArtistId;

            if (AudioFile != null && AudioFile.Length > 0)
            {
                DeleteAudioFile(existingSong.AudioUrl);
                existingSong.AudioUrl = await SaveAudioFile(AudioFile);
            }

            if (ModelState.IsValid)
            {
                await _songService.UpdateSongAsync(existingSong);
                return RedirectToAction(nameof(Index));
            }

            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title", existingSong.AlbumId);
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", existingSong.ArtistId);
            return View(existingSong);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var song = await _songService.GetSongByIdAsync(id.Value);
            if (song == null) return NotFound();

            if (!CanModifySong(song))
                return RedirectToAction("AccessDenied", "Account");

            return View(song);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _songService.GetSongByIdAsync(id);
            if (song == null || !CanModifySong(song))
                return RedirectToAction("AccessDenied", "Account");

            DeleteAudioFile(song.AudioUrl);
            await _songService.DeleteSongAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Helper Methods

        private bool CanModifySong(Song song)
        {
            if (User.IsInRole("Admin"))
                return true; // Admin can modify all songs

            var userId = _userManager.GetUserId(User);
            return song.UserId != null && song.UserId == userId;
        }

        private async Task<string> SaveAudioFile(IFormFile file)
        {
            var audioFolder = Path.Combine(_environment.WebRootPath, "audio");
            if (!Directory.Exists(audioFolder))
                Directory.CreateDirectory(audioFolder);

            var uniqueFileName = Guid.NewGuid() + "_" + Path.GetFileName(file.FileName);
            var filePath = Path.Combine(audioFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/audio/" + uniqueFileName;
        }

        private void DeleteAudioFile(string? audioUrl)
        {
            if (string.IsNullOrEmpty(audioUrl)) return;
            var filePath = Path.Combine(_environment.WebRootPath, audioUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
}