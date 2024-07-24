using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganizujProslavu.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace OrganizujProslavu.Areas.Identity.Pages.Account
{
    public class IzmeniModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly SignInManager<Korisnik> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public Korisnik Korisnik { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }
        [BindProperty]
        public IFormFile Photo { get; set; }

        [BindProperty]
        public string TrenutnaLozinka { get; set; }
        [BindProperty]
        public string NovaLozinka { get; set; }
        [BindProperty]
        public string PonovoNovaLozinka { get; set; }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Username")]
            public string Username { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

            [Display(Name = "Ime")]
            [DataType(DataType.Text)]
            public string Name { get; set; }

            [Display(Name = "Prezime")]
            [DataType(DataType.Text)]
            public string LastName { get; set; }

            [Display(Name = "Grad")]
            [DataType(DataType.Text)]
            public string City { get; set; }

            public string PhotoPath { get; set; }
        }

        public IzmeniModel(
            UserManager<Korisnik> userManager,
            SignInManager<Korisnik> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task OnGetAsync(string Id)
        {
            Korisnik = await _userManager.FindByIdAsync(Id);
           
            Input = new InputModel
            {
                Username = Korisnik.UserName,
                Email = Korisnik.Email,
                PhoneNumber = Korisnik.PhoneNumber,
                Name = Korisnik.Ime,
                LastName = Korisnik.Prezime,
                City = Korisnik.Grad,
                PhotoPath = Korisnik.Slika
            };
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByIdAsync(Korisnik.Id.ToString());

            if (Photo != null)
            {
                if (Input.PhotoPath != null)
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", Input.PhotoPath);
                    System.IO.File.Delete(filePath);
                }
                Input.PhotoPath = ProcessUploadedFile();
            }

            Input.Username = await _userManager.GetUserNameAsync(user);

            user.Ime = Input.Name;
            user.Prezime = Input.LastName;
            user.Grad = Input.City;
            user.Slika = Input.PhotoPath;

            var mejl = await _userManager.GetEmailAsync(user);
            if (Input.Email != mejl)
            {
                var setEmail = await _userManager.SetEmailAsync(user, Input.Email);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
            }
            if (TrenutnaLozinka != null)
            {
                var tacno = await _userManager.CheckPasswordAsync(user, TrenutnaLozinka);
                if (tacno)
                {
                    if (NovaLozinka == PonovoNovaLozinka)
                    {
                        await _userManager.ChangePasswordAsync(user, TrenutnaLozinka, NovaLozinka);
                    }
                    else
                    {
                        return RedirectToPage("./Manage/Index", new { Id = user.Id });
                    }
                }
                else
                {
                    return RedirectToPage("./Manage/Index", new { Id = user.Id });
                }
            }
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToPage("./Manage/Index", new { Id = user.Id });
            }
            return RedirectToPage("./Manage/Index", new { Id = user.Id });
        }

        private string ProcessUploadedFile()
        {
            string uniqueFileName = null;
            if (Photo != null)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Photo.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    Photo.CopyTo(fileStream);
                }
            }
            return uniqueFileName;
        }
    }
}
