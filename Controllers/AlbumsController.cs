using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicfyWebApp.Models;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Controllers
{
    [Authorize]
    public class AlbumsController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IWebHostEnvironment _environment;

        public AlbumsController(IAlbumService albumService, IArtistService artistService, IWebHostEnvironment environment)
        {
            _albumService = albumService;
            _artistService = artistService;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _albumService.GetAllAlbumsAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var album = await _albumService.GetAlbumByIdAsync(id.Value);
            if (album == null) return NotFound();
            return View(album);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("AlbumId,Title,ReleaseYear,ArtistId")] Album album, IFormFile? CoverImage)
        {
            if (CoverImage != null && CoverImage.Length > 0)
            {
                album.CoverImageUrl = await SaveImageFile(CoverImage);
            }

            if (ModelState.IsValid)
            {
                await _albumService.CreateAlbumAsync(album);
                return RedirectToAction(nameof(Index));
            }

            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var album = await _albumService.GetAlbumByIdAsync(id.Value);
            if (album == null) return NotFound();

            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("AlbumId,Title,ReleaseYear,ArtistId,CoverImageUrl")] Album album, IFormFile? CoverImage)
        {
            if (id != album.AlbumId) return NotFound();

            if (CoverImage != null && CoverImage.Length > 0)
            {
                DeleteImageFile(album.CoverImageUrl);
                album.CoverImageUrl = await SaveImageFile(CoverImage);
            }

            if (ModelState.IsValid)
            {
                await _albumService.UpdateAlbumAsync(album);
                return RedirectToAction(nameof(Index));
            }

            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var album = await _albumService.GetAlbumByIdAsync(id.Value);
            if (album == null) return NotFound();
            return View(album);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var album = await _albumService.GetAlbumByIdAsync(id);
            if (album != null)
                DeleteImageFile(album.CoverImageUrl);

            await _albumService.DeleteAlbumAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- Helper Methods ---

        private async Task<string> SaveImageFile(IFormFile file)
        {
            var folder = Path.Combine(_environment.WebRootPath, "images", "albums");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/albums/{uniqueFileName}";
        }

        private void DeleteImageFile(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl.StartsWith("http")) return;
            var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
        }
    }
}