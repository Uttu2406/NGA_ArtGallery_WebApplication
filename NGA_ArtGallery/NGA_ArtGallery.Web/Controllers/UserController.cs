using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class UserController : Controller
{
    private readonly UserManager<IdentityUser<int>> _userManager;

    public UserController(UserManager<IdentityUser<int>> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Get the logged-in user's data
        var user = await _userManager.GetUserAsync(User);

        if (user == null) return NotFound();

        return View(user);
    }
}