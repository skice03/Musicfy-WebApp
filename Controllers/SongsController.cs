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
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> _userManager;

        public SongsController(ISongService songService, IAlbumService albumService,
            IArtistService artistService, IWebHostEnvironment environment,
            UserManager<ApplicationUser> userManager)
        {
            _songService = songService;
            _albumService = albumService;
            _artistService = artistService;
            _environment = environment;
            _userManager = userManager;
        }

        // Everyone can view all songs
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var songs = await _songService.GetAllSongsAsync();
            if (User.Identity?.IsAuthenticated == true)
                ViewBag.CurrentUserId = _userManager.GetUserId(User);
            return View(songs);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                var allSongs = await _songService.GetAllSongsAsync();
                return Json(allSongs);
            }

            var filteredSongs = await _songService.SearchSongsAsync(searchTerm);
            return Json(filteredSongs);
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

            // Set ownership — Admin songs have no UserId (platform songs)
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

            if (AudioFile != null && AudioFile.Length > 0)
            {
                DeleteAudioFile(existingSong.AudioUrl);
                song.AudioUrl = await SaveAudioFile(AudioFile);
            }

            // Preserve original ownership
            song.UserId = existingSong.UserId;

            if (ModelState.IsValid)
            {
                await _songService.UpdateSongAsync(song);
                return RedirectToAction(nameof(Index));
            }

            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title", song.AlbumId);
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", song.ArtistId);
            return View(song);
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

        // --- Helper Methods ---

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