using Bison.Core;
using Bison.Infrastructure.Data;
using Bison.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Web.Pages;

[Authorize]
public class AboutMeModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPostService _posts;

    public AboutMeModel(UserManager<ApplicationUser> userManager, IPostService posts)
    {
        _userManager = userManager;
        _posts = posts;
    }

    public string UserName { get; private set; } = "";
    public string Email { get; private set; } = "";
    public bool EmailConfirmed { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? FullName { get; private set; }
    public List<string> ExternalLogins { get; private set; } = new();
    public UserPostsDTO Posts { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
            return Challenge();

        UserName = user.UserName ?? "";
        Email = user.Email ?? "";
        EmailConfirmed = user.EmailConfirmed;
        PhoneNumber = user.PhoneNumber;

        var claims = await _userManager.GetClaimsAsync(user);
        FullName = claims.FirstOrDefault(c => c.Type == "FullName")?.Value;

        var logins = await _userManager.GetLoginsAsync(user);
        ExternalLogins = logins.Select(l => l.ProviderDisplayName ?? l.LoginProvider).ToList();

        Posts = _posts.GetPostsByAuthorEmail(Email);

        return Page();
    }
}