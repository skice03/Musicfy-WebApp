using Microsoft.AspNetCore.Identity;
using MusicfyWebApp.Models;
using MusicfyWebApp.Services.Interfaces;
using System.Security.Claims;

namespace MusicfyWebApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Succeeded, string[] Errors)> RegisterAsync(string username, string email, string password)
        {
            var user = new ApplicationUser
            {
                UserName = username,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Assign "User" role by default
                await _userManager.AddToRoleAsync(user, "User");
                return (true, Array.Empty<string>());
            }

            var errors = result.Errors.Select(e => e.Description).ToArray();
            return (false, errors);
        }

        public async Task<bool> LoginAsync(string email, string password, bool rememberMe)
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return false;

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                password,
                isPersistent: rememberMe,
                lockoutOnFailure: false
            );

            return result.Succeeded;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<ApplicationUser?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            return await _userManager.GetUserAsync(principal);
        }
    }
}
