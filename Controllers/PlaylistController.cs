using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicfyWebApp.Models;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Controllers
{
    [Authorize]
    public class PlaylistsController : Controller
    {
        private readonly IPlaylistService _playlistService;
        private readonly ISongService _songService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public PlaylistsController(IPlaylistService playlistService, ISongService songService,
            UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _playlistService = playlistService;
            _songService = songService;
            _userManager = userManager;
            _environment = environment;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var allPlaylists = await _playlistService.GetAllPlaylistsAsync();

            List<Playlist> visiblePlaylists;

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                // Admin sees all, users see own + public
                visiblePlaylists = allPlaylists.Where(p =>
                    p.UserId == userId || p.IsPublic || User.IsInRole("Admin")
                ).ToList();
                ViewBag.CurrentUserId = userId;
            }
            else
            {
                // Guests only see public playlists
                visiblePlaylists = allPlaylists.Where(p => p.IsPublic).ToList();
                ViewBag.CurrentUserId = null;
            }

            return View(visiblePlaylists);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var playlist = await _playlistService.GetPlaylistByIdAsync(id.Value);
            if (playlist == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var isOwner = playlist.UserId == userId;

            // Private playlists are only visible to owner and admin
            if (!isOwner && !User.IsInRole("Admin") && !playlist.IsPublic)
                return RedirectToAction("AccessDenied", "Account");

            ViewBag.CanModify = isOwner || User.IsInRole("Admin");

            // For Add Song dropdown, get all songs (platform + user's own)
            if (ViewBag.CanModify == true)
            {
                var allSongs = await _songService.GetAllSongsAsync();
                var existingSongIds = playlist.PlaylistSongs?.Select(ps => ps.SongId).ToHashSet() ?? new HashSet<int>();

                // Get user's playlists for the "Add to Playlist" feature
                ViewBag.AvailableSongs = allSongs
                    .Where(s => !existingSongIds.Contains(s.SongId))
                    .ToList();
            }

            return View(playlist);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlaylistId,Title,Description,IsPublic")] Playlist playlist, IFormFile? CoverImage)
        {
            if (CoverImage != null && CoverImage.Length > 0)
            {
                playlist.ImageUrl = await SaveImageFile(CoverImage);
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                    playlist.UserId = user.Id;

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

            if (!CanModifyPlaylist(playlist))
                return RedirectToAction("AccessDenied", "Account");

            return View(playlist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlaylistId,Title,Description,ImageUrl,IsPublic")] Playlist playlist, IFormFile? CoverImage)
        {
            if (id != playlist.PlaylistId) return NotFound();

            var existingPlaylist = await _playlistService.GetPlaylistByIdAsync(id);
            if (existingPlaylist == null || !CanModifyPlaylist(existingPlaylist))
                return RedirectToAction("AccessDenied", "Account");

            if (CoverImage != null && CoverImage.Length > 0)
            {
                DeleteImageFile(existingPlaylist.ImageUrl);
                existingPlaylist.ImageUrl = await SaveImageFile(CoverImage);
            }

            if (ModelState.IsValid)
            {
                // Update the tracked entity's properties directly to avoid tracking conflicts
                existingPlaylist.Title = playlist.Title;
                existingPlaylist.Description = playlist.Description;
                existingPlaylist.IsPublic = playlist.IsPublic;

                await _playlistService.UpdatePlaylistAsync(existingPlaylist);
                return RedirectToAction(nameof(Index));
            }
            return View(playlist);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var playlist = await _playlistService.GetPlaylistByIdAsync(id.Value);
            if (playlist == null) return NotFound();

            if (!CanModifyPlaylist(playlist))
                return RedirectToAction("AccessDenied", "Account");

            return View(playlist);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(id);
            if (playlist == null || !CanModifyPlaylist(playlist))
                return RedirectToAction("AccessDenied", "Account");

            DeleteImageFile(playlist.ImageUrl);
            await _playlistService.DeletePlaylistAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Add / Remove songs from playlist 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSong(int playlistId, int songId)
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
            if (playlist == null || !CanModifyPlaylist(playlist))
                return RedirectToAction("AccessDenied", "Account");

            var songExists = playlist.PlaylistSongs?.Any(ps => ps.SongId == songId) == true;

            if (songExists)
            {
                TempData["Error"] = $"This song is already in '{playlist.Title}'.";
            }
            else
            {
                await _playlistService.AddSongToPlaylistAsync(playlistId, songId);
                TempData["Success"] = $"Song successfully added to '{playlist.Title}'!";
            }

            // Redirect back to the page the user came from (e.g., Songs Index or Playlist Details)
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSong(int playlistId, int songId)
        {
            var playlist = await _playlistService.GetPlaylistByIdAsync(playlistId);
            if (playlist == null || !CanModifyPlaylist(playlist))
                return RedirectToAction("AccessDenied", "Account");

            await _playlistService.RemoveSongFromPlaylistAsync(playlistId, songId);
            return RedirectToAction(nameof(Details), new { id = playlistId });
        }

        // --- Helper Methods ---

        private bool CanModifyPlaylist(Playlist playlist)
        {
            if (User.IsInRole("Admin")) return true;
            var userId = _userManager.GetUserId(User);
            return playlist.UserId == userId;
        }

        private async Task<string> SaveImageFile(IFormFile file)
        {
            var folder = Path.Combine(_environment.WebRootPath, "images", "playlists");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/playlists/{uniqueFileName}";
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