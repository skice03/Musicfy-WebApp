using MusicfyWebApp.Models;

namespace MusicfyWebApp.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Succeeded, string[] Errors)> RegisterAsync(string username, string email, string password);
        Task<bool> LoginAsync(string email, string password, bool rememberMe);
        Task LogoutAsync();
        Task<ApplicationUser?> GetCurrentUserAsync(System.Security.Claims.ClaimsPrincipal principal);
    }
}
