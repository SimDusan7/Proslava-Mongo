using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using MongoDB.Bson;
using OrganizujProslavu.Areas.Identity.Data;
using OrganizujProslavu.Models;
using System.IO;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;

namespace OrganizujProslavu.Pages
{
    public class NapraviOglasModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly IWebHostEnvironment _ihostingEnvironment;
        private MongoClient client { get; set; }
        [BindProperty]
        public Oglas Oglas { get; set; }
        [BindProperty]
        public IList<Karakteristika> Karakteristike { get; set; }
        [BindProperty]
        public IList<ClanBenda> ClanoviBenda { get; set; }
        [BindProperty]
        public bool WifiIsChecked { get; set; }
        [BindProperty]
        public bool KlimaIsChecked { get; set; }
        [BindProperty]
        public bool GrejanjeIsChecked { get; set; }
        [BindProperty]
        public bool PlayStationIsChecked { get; set; }
        [BindProperty]
        public bool DrustveneIgreIsChecked { get; set; }
        [BindProperty]
        public bool PoligoniZaIgranjeIsChecked { get; set; }
        [BindProperty]
        public bool BazenIsChecked { get; set; }
        [BindProperty]
        public bool ParkingIsChecked { get; set; }
        [BindProperty]
        public List<SelectListItem> TipOglasa { get; set; }
        [BindProperty]
        public List<Slika> SlikeOglasa { get; set; }

        public NapraviOglasModel(
        UserManager<Korisnik> userManager,
        IWebHostEnvironment ihostingEnvironment)
        {
            client = new MongoClient("mongodb://localhost/?safe=true");
            _userManager = userManager;
            _ihostingEnvironment = ihostingEnvironment;
        }
        public void OnGet()
        {
            TipOglasa = new List<SelectListItem>
            {
                new SelectListItem { Text="Bend", Value = "Bend"},
                new SelectListItem { Text="Restoran", Value = "Restoran"},
                new SelectListItem { Text="Kafic", Value = "Kafic"},
                new SelectListItem { Text="Kafana", Value = "Kafana"},
                new SelectListItem { Text="Igraonica", Value = "Igraonica"},
                new SelectListItem { Text="Bar", Value = "Bar"}
            };
        }
        public async Task<IActionResult> OnPostAsync(IFormFile[] photos)
        {
            var dataBase = client.GetDatabase("OrganizujProslavu");
            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            await oglasCollection.InsertOneAsync(Oglas);

            foreach (var photo in photos)
            {
                var pathFolder = Path.Combine(this._ihostingEnvironment.WebRootPath, "imagesOglasi"); //,photo.FileName);
                var uniqueName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                string filePath = Path.Combine(pathFolder, uniqueName);
                var stream = new FileStream(filePath, FileMode.Create);
                await photo.CopyToAsync(stream);

                Slika slika = new Slika();
                slika.PhotoPath = uniqueName;
                slika.Oglas = new MongoDBRef("oglass",Oglas.Id);
                SlikeOglasa.Add(slika);
            }


            var slikaCollection = dataBase.GetCollection<Slika>("slikas");
            foreach (var slika in SlikeOglasa)
            {
                if (slika != null)
                {
                    await slikaCollection.InsertOneAsync(slika);
                    Oglas.SlikeOglasa.Add(new MongoDBRef("slikas", slika.Id));
                }
            }
            if (SlikeOglasa.Count > 0)
            {
                Oglas.NaslovnaSlika = SlikeOglasa[0].PhotoPath;
            }
            else
            {
                if (Oglas.TipOglasa == "Bend")
                    Oglas.NaslovnaSlika = "bend.png";
                else
                    Oglas.NaslovnaSlika = "Objekat.png";
            }

            if (WifiIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Wifi"));
            }

            if (KlimaIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Klima"));
            }

            if (GrejanjeIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Grejanje"));
            }

            if (BazenIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Bazen"));
            }

            if (PlayStationIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("PlayStation"));
            }

            if (DrustveneIgreIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("DrustveneIgre"));
            }

            if (PoligoniZaIgranjeIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("PoligoniZaIgranje"));
            }

            if (ParkingIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Parking"));
            }

            var clanoviCollection = dataBase.GetCollection<ClanBenda>("clanbendas");
            if (ClanoviBenda.Count > 0)
            {
                foreach (var clan in ClanoviBenda)
                {
                    clan.Oglas = new MongoDBRef("oglass", Oglas.Id);
                    await clanoviCollection.InsertOneAsync(clan);
                    Oglas.ClanoviBenda.Add(new MongoDBRef("clanovibendas", clan.Id));
                }
            }
            var karakteristikaCollection = dataBase.GetCollection<Karakteristika>("karakteristikas");
            foreach (var k in Karakteristike)
            {
                k.Oglas = new MongoDBRef("oglass", Oglas.Id);
                await karakteristikaCollection.InsertOneAsync(k);
                Oglas.Karakteristike.Add(new MongoDBRef("karakteristikas", k.Id));
            }

            var korisnik = await _userManager.GetUserAsync(User);

            Oglas.KorisnikId = korisnik.Id;
            korisnik.Oglasi.Add(new MongoDBRef("oglass", Oglas.Id));

            var filter = Builders<Oglas>.Filter;
            var f = filter.Eq("Id", Oglas.Id);

            oglasCollection.ReplaceOne(f, Oglas);

            var filter1 = Builders<Korisnik>.Filter;
            var f1 = filter1.Eq("_id", korisnik.Id);

            var korisnikCollection = dataBase.GetCollection<Korisnik>("korisniks");

            korisnikCollection.ReplaceOne(f1, korisnik);

            var rez = await _userManager.IsInRoleAsync(korisnik, "Korisnik");
            if (rez == true)
            {
                var oduzmi = await _userManager.RemoveFromRoleAsync(korisnik, "Korisnik"); //oduzima mu je
                var dodaj = await _userManager.AddToRoleAsync(korisnik, "Oglasivac");
            }

            return RedirectToPage("/ProfilOglasa", new { Id = Oglas.Id.ToString() });
        }
    }
}
