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
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IPostService _posts;

    public AboutMeModel(UserManager<ApplicationUser> userManager, IPostService posts, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _posts = posts;
    }

    [BindProperty]
    public bool KeepObservations{get; set;}
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

    public async Task<IActionResult> OnPostForgetMeAsync([FromServices] IPictureStorage pictures)
    {
        var user = await _userManager.GetUserAsync(User);
        if(user is null)
            return Challenge();
        
        var authorEmail = user.Email ?? user.UserName;
        if(!string.IsNullOrWhiteSpace(authorEmail))
        {
            var posts = _posts.GetPostsByAuthorEmail(authorEmail);
            _posts.ForgetUserByAuthorEmail(authorEmail, KeepObservations);

            if(!KeepObservations)
            {
                foreach(var observation in posts.Observations)
                await pictures.DeleteAsync(observation.PicturePath);
            }
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("Woops something went wrong, could not delete the user: " + string.Join("; ", result.Errors.Select(error => error.Description)));
        }
        await _signInManager.SignOutAsync();
        return Redirect("~/");
    }
}