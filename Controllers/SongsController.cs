using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicfyWebApp.Models;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Controllers
{
    public class SongsController : Controller
    {
        private readonly ISongService _songService;
        private readonly IAlbumService _albumService; // album service
        private readonly IArtistService _artistService;

        public SongsController(ISongService songService, IAlbumService albumService, IArtistService artistService)
        {
            _songService = songService;
            _albumService = albumService;
            _artistService = artistService;
        }

        public async Task<IActionResult> Index()
        {
            var songs = await _songService.GetAllSongsAsync();
            return View(songs);
        }

        [HttpGet]
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

        public async Task<IActionResult> Create()
        {
            // drop-down albums
            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title");
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SongId,Title,Length,AudioUrl,AlbumId,ArtistId")] Song song)
        {
            if (ModelState.IsValid)
            {
                await _songService.CreateSongAsync(song);
                return RedirectToAction(nameof(Index)); // go to ajax list after saving
            }

            // reload drop-down if error
            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title", song.AlbumId);
            return View(song);
        }

        // --- GET: Details ---
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var song = await _songService.GetSongByIdAsync(id.Value);
            if (song == null) return NotFound();
            return View(song);
        }

        // --- GET & POST: Edit ---
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var song = await _songService.GetSongByIdAsync(id.Value);
            if (song == null) return NotFound();

            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title", song.AlbumId);
            return View(song);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SongId,Title,Length,AudioUrl,AlbumId")] Song song)
        {
            if (id != song.SongId) return NotFound();

            if (ModelState.IsValid)
            {
                await _songService.UpdateSongAsync(song);
                return RedirectToAction(nameof(Index));
            }
            var albums = await _albumService.GetAllAlbumsAsync();
            ViewBag.AlbumId = new SelectList(albums, "AlbumId", "Title", song.AlbumId);
            return View(song);
        }

        // --- GET & POST: Delete ---
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var song = await _songService.GetSongByIdAsync(id.Value);
            if (song == null) return NotFound();
            return View(song);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _songService.DeleteSongAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}