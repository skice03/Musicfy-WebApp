using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // for select list
using Microsoft.EntityFrameworkCore;
using MusicfyWebApp.Models;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;

        public AlbumsController(IAlbumService albumService, IArtistService artistService)
        {
            _albumService = albumService;
            _artistService = artistService;
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

        // send artist list to view
        public async Task<IActionResult> Create()
        {
            var artists = await _artistService.GetAllArtistsAsync();
            // create drop-down
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AlbumId,Title,ReleaseYear,ArtistId")] Album album)
        {
            if (ModelState.IsValid)
            {
                await _albumService.CreateAlbumAsync(album);
                return RedirectToAction(nameof(Index));
            }

            // if error => reload for dropdown
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        // modify get to update list with the selected artist
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
        public async Task<IActionResult> Edit(int id, [Bind("AlbumId,Title,ReleaseYear,ArtistId")] Album album)
        {
            if (id != album.AlbumId) return NotFound();

            if (ModelState.IsValid)
            {
                await _albumService.UpdateAlbumAsync(album);
                return RedirectToAction(nameof(Index));
            }

            // reload drop-down in case of validation error
            var artists = await _artistService.GetAllArtistsAsync();
            ViewBag.ArtistId = new SelectList(artists, "ArtistId", "Name", album.ArtistId);
            return View(album);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var album = await _albumService.GetAlbumByIdAsync(id.Value);
            if (album == null) return NotFound();
            return View(album);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _albumService.DeleteAlbumAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}