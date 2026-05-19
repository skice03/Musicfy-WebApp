using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MusicfyWebApp.Models;
using MusicfyWebApp.Models.ViewModels;
using MusicfyWebApp.Services.Interfaces;

namespace MusicfyWebApp.Controllers;

public class HomeController : Controller
{
    private readonly IAlbumService _albumService;
    private readonly IPlaylistService _playlistService;
    private readonly ISongService _songService;
    private readonly IArtistService _artistService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public HomeController(IAlbumService albumService, IPlaylistService playlistService,
        ISongService songService, IArtistService artistService,
        UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
    {
        _albumService = albumService;
        _playlistService = playlistService;
        _songService = songService;
        _artistService = artistService;
        _userManager = userManager;
        _environment = environment;
    }

    public async Task<IActionResult> Index()
    {
        var songs = await _songService.GetAllSongsAsync();
        ViewBag.TrackCount = songs.Count();

        var artists = await _artistService.GetAllArtistsAsync();
        ViewBag.VerifiedCount = artists.Count(a => a.IsVerified);

        return View();
    }

    public async Task<IActionResult> Library()
    {
        var allPlaylists = await _playlistService.GetAllPlaylistsAsync();
        var allAlbums = await _albumService.GetAllAlbumsAsync();
        var user = await _userManager.GetUserAsync(User);

        var visiblePlaylists = allPlaylists
            .Where(p => p.IsPublic || (user != null && p.UserId == user.Id))
            .ToList();

        var viewModel = new LibraryViewModel
        {
            Albums = allAlbums,
            Playlists = visiblePlaylists
        };
        return View(viewModel);
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return RedirectToAction("Login", "Account");

        var roles = await _userManager.GetRolesAsync(user);
        ViewBag.UserRole = roles.FirstOrDefault() ?? "User";

        // Get all playlists and filter by user
        var allPlaylists = await _playlistService.GetAllPlaylistsAsync();
        var userPlaylists = allPlaylists.Where(p => p.UserId == user.Id).ToList();
        ViewBag.UserPlaylists = userPlaylists;

        return View(user);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (newPassword != confirmNewPassword)
        {
            TempData["Error"] = "New passwords do not match.";
            return RedirectToAction(nameof(Profile));
        }

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (result.Succeeded)
        {
            TempData["Success"] = "Password changed successfully!";
        }
        else
        {
            TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
        }

        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadProfilePicture(IFormFile ProfileImage)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        if (ProfileImage != null && ProfileImage.Length > 0)
        {
            // Delete old profile picture
            if (!string.IsNullOrEmpty(user.ProfileImageUrl) && !user.ProfileImageUrl.StartsWith("http"))
            {
                var oldPath = Path.Combine(_environment.WebRootPath, user.ProfileImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var folder = Path.Combine(_environment.WebRootPath, "images", "profiles");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var uniqueFileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
            var filePath = Path.Combine(folder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProfileImage.CopyToAsync(stream);
            }

            user.ProfileImageUrl = $"/images/profiles/{uniqueFileName}";
            await _userManager.UpdateAsync(user);
            TempData["Success"] = "Profile picture updated!";
        }

        return RedirectToAction(nameof(Profile));
    }
}