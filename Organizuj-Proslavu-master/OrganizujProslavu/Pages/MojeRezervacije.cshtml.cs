using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using MongoDB.Bson;
using OrganizujProslavu.Areas.Identity.Data;
using OrganizujProslavu.Models;
using Microsoft.AspNetCore.Identity;

namespace OrganizujProslavu.Pages
{
    public class MojeRezervacijeModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
        private MongoClient client;
        [BindProperty]
        public IList<Rezervacija> Rezervacije { get; set; }

        [BindProperty]
        public int Ocena { get; set; }

        public MojeRezervacijeModel( UserManager<Korisnik> userManager)
        {
            client = new MongoClient("mongodb://localhost/?safe=true");
            _userManager = userManager;
        }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var database = client.GetDatabase("OrganizujProslavu");
            var rezervacijaCollection = database.GetCollection<Rezervacija>("rezervacijas");
            var rezervacije = rezervacijaCollection.AsQueryable().Where(x => x.KorisnikId == user.Id).OrderBy(x => x.Datum);
            Rezervacije = rezervacije.ToList();

            foreach (var r in Rezervacije)
            {
                if (r.Datum < System.DateTime.Now)
                {
                    r.Istekla = true;
                    rezervacijaCollection.ReplaceOne(x => x.Id == r.Id, r);
                }
            }
        }
        public async Task<IActionResult> OnPostOtkaziRezervacijuAsync(string Id)
        {
            var id = new ObjectId(Id);
            var database = client.GetDatabase("OrganizujProslavu");
            var rezervacijaCollection = database.GetCollection<Rezervacija>("rezervacijas");
            var rezervacija = await rezervacijaCollection.Find(x => x.Id == id).SingleAsync();

            var terminCollection = database.GetCollection<Termin>("termins");
            var termin = await terminCollection.Find(x => x.ZauzetTermin == rezervacija.Datum).SingleAsync();
           
            
            if (rezervacija != null && termin != null)
            {
                rezervacija.Otkazana = true;
                rezervacijaCollection.ReplaceOne(x => x.Id == rezervacija.Id, rezervacija);

                await terminCollection.DeleteOneAsync(x=>x.Id==termin.Id);
                return RedirectToPage();
            }
            else
            {
                return RedirectToPage();
            }
        }
      
    }
}
