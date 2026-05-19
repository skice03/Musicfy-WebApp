using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicfyWebApp.Models;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Controllers
{
    [Authorize]
    public class ArtistsController : Controller
    {
        private readonly IArtistService _artistService;
        private readonly IWebHostEnvironment _environment;

        public ArtistsController(IArtistService artistService, IWebHostEnvironment environment)
        {
            _artistService = artistService;
            _environment = environment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            return View(await _artistService.GetAllArtistsAsync());
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var artist = await _artistService.GetArtistByIdAsync(id.Value);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ArtistId,Name,Bio,IsVerified")] Artist artist, IFormFile? ImageFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                artist.ImageUrl = await SaveImageFile(ImageFile, "artists");
            }

            if (ModelState.IsValid)
            {
                await _artistService.CreateArtistAsync(artist);
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var artist = await _artistService.GetArtistByIdAsync(id.Value);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ArtistId,Name,Bio,ImageUrl,IsVerified")] Artist artist, IFormFile? ImageFile)
        {
            if (id != artist.ArtistId) return NotFound();

            if (ImageFile != null && ImageFile.Length > 0)
            {
                DeleteImageFile(artist.ImageUrl);
                artist.ImageUrl = await SaveImageFile(ImageFile, "artists");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _artistService.UpdateArtistAsync(artist);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ArtistExists(artist.ArtistId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var artist = await _artistService.GetArtistByIdAsync(id.Value);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            if (artist != null)
                DeleteImageFile(artist.ImageUrl);

            await _artistService.DeleteArtistAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // --- Helper Methods ---

        private async Task<bool> ArtistExists(int id)
        {
            var artist = await _artistService.GetArtistByIdAsync(id);
            return artist != null;
        }

        private async Task<string> SaveImageFile(IFormFile file, string subfolder)
        {
            var folder = Path.Combine(_environment.WebRootPath, "images", subfolder);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/{subfolder}/{uniqueFileName}";
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
