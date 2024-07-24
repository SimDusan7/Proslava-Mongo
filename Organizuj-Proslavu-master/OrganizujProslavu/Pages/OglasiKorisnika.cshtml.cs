using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganizujProslavu.Areas.Identity.Data;
using OrganizujProslavu.Models;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDB.Bson;

namespace OrganizujProslavu.Pages
{
    public class OglasiKorisnikaModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
        private MongoClient client;
        [BindProperty]
        public IList<Oglas> Oglasi { get; set; }
        [BindProperty]
        public Termin Termin { get; set; }

        public OglasiKorisnikaModel(UserManager<Korisnik> userManager)
        {
            client = new MongoClient("mongodb://localhost/?safe=true");
            _userManager = userManager;
            Oglasi = new List<Oglas>();
        }
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            var dataBase = client.GetDatabase("OrganizujProslavu");

            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            var oglasi = oglasCollection.AsQueryable().Where(x => x.KorisnikId == user.Id);
            Oglasi = oglasi.ToList();
        }
        public async Task<IActionResult> OnPostObrisiOglasAsync(string Id)
        {
            var dataBase = client.GetDatabase("OrganizujProslavu");

            var id = new ObjectId(Id);
            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            var oglas = await oglasCollection.Find(x => x.Id == id).SingleAsync();

            var terminCollection = dataBase.GetCollection<Termin>("termins");
            await terminCollection.DeleteManyAsync(x => x.Oglas.Id == id);

            var rezervacijaCollection = dataBase.GetCollection<Rezervacija>("rezervacijas");
            await rezervacijaCollection.DeleteManyAsync(x => x.Oglas.Id == id);

            var karakteristikeCollection = dataBase.GetCollection<Karakteristika>("karakteristikas");
            await karakteristikeCollection.DeleteManyAsync(x => x.Oglas.Id == id);

            var clanoviCollection = dataBase.GetCollection<ClanBenda>("clanbendas");
            await clanoviCollection.DeleteManyAsync(x => x.Oglas.Id == id);

            var slikeCollection = dataBase.GetCollection<Slika>("slikas");
            await slikeCollection.DeleteManyAsync(x => x.Oglas.Id == id);

            await oglasCollection.DeleteOneAsync(x => x.Id == id);

            return RedirectToPage();
        }
    }
}