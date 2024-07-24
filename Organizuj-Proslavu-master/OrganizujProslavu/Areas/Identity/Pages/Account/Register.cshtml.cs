using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using OrganizujProslavu.Areas.Identity.Data;
using AspNetCore.Identity.MongoDbCore.Models;

namespace OrganizujProslavu.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Korisnik> _signInManager;
        private readonly UserManager<Korisnik> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<MongoIdentityRole> _roleManager;

        public RegisterModel(
            UserManager<Korisnik> userManager,
            SignInManager<Korisnik> signInManager,
            ILogger<RegisterModel> logger,
            RoleManager<MongoIdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Korisnicko ime")]
            public string Name { get; set; }
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} mora sadržati najmanje {2} i najviše {1} karaktera.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Lozinka")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Potvrdite lozinku")]
            [Compare("Password", ErrorMessage = "Lozinke se ne poklapaju, pokušajte ponovo.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new Korisnik { UserName = Input.Name, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);

                IdentityResult roleResult;

                bool adminRoleExists = await _roleManager.RoleExistsAsync("Admin");
                if (!adminRoleExists)
                {
                    roleResult = await _roleManager.CreateAsync(new MongoIdentityRole("Admin"));
                    Korisnik admin = new Korisnik { UserName = "Admin", Email = "admin@gmail.com" };
                    await _userManager.CreateAsync(admin, "Admin13@");
                    await _userManager.AddToRoleAsync(admin, "Admin");
                }

                bool OglasivacRoleExists = await _roleManager.RoleExistsAsync("Oglasivac");
                if (!OglasivacRoleExists)
                {
                    roleResult = await _roleManager.CreateAsync(new MongoIdentityRole("Oglasivac"));
                }

                bool KorisnikRoleExists = await _roleManager.RoleExistsAsync("Korisnik");
                if (!KorisnikRoleExists)
                {
                    roleResult = await _roleManager.CreateAsync(new  MongoIdentityRole("Korisnik"));
                }



                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    await _userManager.AddToRoleAsync(user, "Korisnik");

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

        
            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
