using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using GestionAirPort.Models;
using GestionAirPort.Constants;

namespace GestionAirPort.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly string _currentUser = "Imen-Ht07";
        private readonly DateTime _currentDateTime = DateTime.Parse("2025-05-22 18:20:31");

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.CurrentUser = _currentUser;
            ViewBag.CurrentDateTime = _currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.CurrentUser = _currentUser;
            ViewBag.CurrentDateTime = _currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    if (!user.IsActive)
                    {
                        ModelState.AddModelError(string.Empty, "Ce compte est désactivé.");
                        return View(model);
                    }

                    var result = await _signInManager.PasswordSignInAsync(model.Email,
                        model.Password, model.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"Utilisateur {user.Email} connecté.");
                        return RedirectToLocal(returnUrl);
                    }

                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning($"Compte bloqué : {model.Email}");
                        return View("Lockout");
                    }
                }

                ModelState.AddModelError(string.Empty, "Email ou mot de passe incorrect.");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.CurrentUser = _currentUser;
            ViewBag.CurrentDateTime = _currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            ViewBag.CurrentUser = _currentUser;
            ViewBag.CurrentDateTime = _currentDateTime.ToString("yyyy-MM-dd HH:mm:ss");

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PassportNumber = model.PassportNumber,
                    CreatedAt = _currentDateTime,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Roles.Passenger);

                    // Créer le passager associé
                    var passenger = new Passenger
                    {
                        PassportNumber = model.PassportNumber,
                        FullName = new FullName
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName
                        },
                        EmailAddress = model.Email,
                    };

                    // TODO: Sauvegarder le passager

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}