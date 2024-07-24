using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using OrganizujProslavu.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganizujProslavu.Models;

namespace OrganizujProslavu.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
      
        [BindProperty]
        public IList<Oglas> Oglasi { get; set; }

        [BindProperty]
        public Korisnik Korisnik { get; set; }

        public IndexModel(UserManager<Korisnik> userManager)
        {
            Oglasi = new List<Oglas>();
            _userManager = userManager;
        }
        public async Task OnGetAsync(string Id)
        {
            Korisnik = await _userManager.FindByIdAsync(Id);

        }
    }
}
