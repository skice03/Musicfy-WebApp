using Microsoft.AspNetCore.Mvc;
using MusicfyWebApp.Models;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Controllers
{
    public class PlaylistsController : Controller
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistsController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _playlistService.GetAllPlaylistsAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var playlist = await _playlistService.GetPlaylistByIdAsync(id.Value);
            if (playlist == null) return NotFound();
            return View(playlist);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlaylistId,Title,Description,ImageUrl,IsPublic")] Playlist playlist)
        {
            if (ModelState.IsValid)
            {
                // to be added when more users will be created
                await _playlistService.CreatePlaylistAsync(playlist);
                return RedirectToAction(nameof(Index));
            }
            return View(playlist);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var playlist = await _playlistService.GetPlaylistByIdAsync(id.Value);
            if (playlist == null) return NotFound();
            return View(playlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlaylistId,Title,Description,ImageUrl,IsPublic")] Playlist playlist)
        {
            if (id != playlist.PlaylistId) return NotFound();
            if (ModelState.IsValid)
            {
                // same
                await _playlistService.UpdatePlaylistAsync(playlist);
                return RedirectToAction(nameof(Index));
            }
            return View(playlist);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var playlist = await _playlistService.GetPlaylistByIdAsync(id.Value);
            if (playlist == null) return NotFound();
            return View(playlist);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _playlistService.DeletePlaylistAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}