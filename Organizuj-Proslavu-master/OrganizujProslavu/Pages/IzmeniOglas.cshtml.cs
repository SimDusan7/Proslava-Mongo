using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using OrganizujProslavu.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using OrganizujProslavu.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace OrganizujProslavu.Pages
{
   // [Authorize(Roles = "Admin,Korisnik,Oglasivac")]
    public class IzmeniOglasModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
        private readonly IWebHostEnvironment _ihostingEnvironment;
        private MongoClient client;
        [BindProperty]
        public Oglas Oglas { get; set; }
        [BindProperty]
        public IList<ClanBenda> ClanoviBenda { get; set; }
        [BindProperty]
        public IList<Karakteristika> Karakteristike { get; set; }
        [BindProperty]
        public IList<Slika> SlikeOglasa { get; set; }
        [BindProperty]
        public IList<Slika> NoveSlike { get; set; }
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
        public IList<string> VecPostojece { get; set; }
        [BindProperty]
        public IList<string> ListaImenaClanova { get; set; }
        [BindProperty]
        public IList<string> ListaImenaInstrumenata { get; set; }

        public IzmeniOglasModel(UserManager<Korisnik> userManager, IWebHostEnvironment ihostingEnvironment)
        {
            client = new MongoClient("mongodb://localhost/?safe=true");
            _userManager = userManager;
            _ihostingEnvironment = ihostingEnvironment;
            NoveSlike = new List<Slika>();
            VecPostojece = new List<string>();
            ListaImenaClanova = new List<string>();
            ListaImenaInstrumenata = new List<string>();
        }
        public async Task OnGetAsync(string Id)
        {
            var dataBase = client.GetDatabase("OrganizujProslavu");

            var id = new ObjectId(Id);
            var oglasiCollection = dataBase.GetCollection<Oglas>("oglass");

            Oglas = await oglasiCollection.Find(x => x.Id == id).SingleAsync();

            var karakteristikeCollection = dataBase.GetCollection<Karakteristika>("karakteristikas");
            var karakteristike = karakteristikeCollection.AsQueryable().Where(x => x.Oglas.Id == id);
            Karakteristike = karakteristike.ToList();

            var slikeCollection = dataBase.GetCollection<Slika>("slikas");
            var slike = slikeCollection.AsQueryable().Where(x => x.Oglas.Id == id);
            SlikeOglasa = slike.ToList();

            var clanoviCollection = dataBase.GetCollection<ClanBenda>("clanbendas");
            var clanovi = clanoviCollection.AsQueryable().Where(x => x.Oglas.Id == id);
            ClanoviBenda = clanovi.ToList();
        }

        public async Task<IActionResult> OnPostSacuvajAsync(IFormFile[] photos, string Id)
        { 
            var id = new ObjectId(Id);
            var user1 = await _userManager.GetUserAsync(User);

            var dataBase = client.GetDatabase("OrganizujProslavu");
            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            var karakteristikaCollection = dataBase.GetCollection<Karakteristika>("karakteristikas");
            var clanBendaCollection = dataBase.GetCollection<ClanBenda>("clanbendas");
            var karakteristike = karakteristikaCollection.AsQueryable().Where(x => x.Oglas.Id == id);
            Karakteristike = karakteristike.ToList();
            var clanoviBenda= clanBendaCollection.AsQueryable().Where(x => x.Oglas.Id == id);
            ClanoviBenda = clanoviBenda.ToList();

            foreach (var photo in photos)
            {
                //Upisuje u folder
                var pathFolder = Path.Combine(this._ihostingEnvironment.WebRootPath, "imagesOglasi");
                var uniqueName = Guid.NewGuid().ToString() + "_" + photo.FileName;
                string filePath = Path.Combine(pathFolder, uniqueName);
                var stream = new FileStream(filePath, FileMode.Create);
                await photo.CopyToAsync(stream);

                //Upisuje iz foldera u listu
                Slika slika = new Slika();
                slika.PhotoPath = uniqueName;
                slika.Oglas = new MongoDBRef("oglass", id);
                NoveSlike.Add(slika);
            };

            var slikaCollection = dataBase.GetCollection<Slika>("slikas");

            //Iz liste u bazu
            foreach (var slika in NoveSlike)
            {
                if (slika != null)
                {
                    SlikeOglasa.Add(slika);
                    Oglas.SlikeOglasa.Add(new MongoDBRef("slikas", slika.Id));
                    await slikaCollection.InsertOneAsync(slika);
                }
            }


            foreach (var Karakteristika in Karakteristike)
            {
                MongoDBRef k = Oglas.Karakteristike.Find(x => x.Id == Karakteristika.Id);
                Oglas.Karakteristike.Remove(k);
                await karakteristikaCollection.DeleteOneAsync(x=>x.Id==Karakteristika.Id);
            }
            Karakteristike.Clear();

            if (WifiIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Wifi"));
            }
            if (WifiIsChecked == false)
            {
                await karakteristikaCollection.DeleteOneAsync(x => x.Naziv == "Wifi" && x.Oglas.Id == id);
            }
            if (KlimaIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Klima"));
            }
            if (KlimaIsChecked == false)
            {
                await karakteristikaCollection.DeleteOneAsync(x => x.Naziv == "Klima" && x.Oglas.Id == id);
            }
            if (GrejanjeIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Grejanje"));
            }
            if (GrejanjeIsChecked == false)
            {
                await karakteristikaCollection.DeleteOneAsync(x => x.Naziv == "Grejanje" && x.Oglas.Id == id);
            }
            if (BazenIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Bazen"));
            }
            if (BazenIsChecked == false)
            {
                await karakteristikaCollection.DeleteOneAsync(x => x.Naziv == "Bazen" && x.Oglas.Id == id);
            }
            if (PlayStationIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("PlayStation"));
            }
            if (PlayStationIsChecked == false)
            {
                await karakteristikaCollection.DeleteOneAsync(x => x.Naziv == "PlayStation" && x.Oglas.Id == id);
            }
            if (DrustveneIgreIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("DrustveneIgre"));
            }
            if (DrustveneIgreIsChecked == false)
            {
                await karakteristikaCollection.DeleteOneAsync(x => x.Naziv == "DrustveneIgre" && x.Oglas.Id == id);
            }
            if (PoligoniZaIgranjeIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("PoligoniZaIgranje"));
            }
            if (PoligoniZaIgranjeIsChecked == false)
            {
                await karakteristikaCollection.DeleteOneAsync(x => x.Naziv == "PoligoniZaIgranje" && x.Oglas.Id == id);
            }
            if (ParkingIsChecked == true)
            {
                Karakteristike.Add(new Karakteristika("Parking"));
            }
            if (ParkingIsChecked == false)
            {
                await karakteristikaCollection.DeleteOneAsync(x => x.Naziv == "Parking" && x.Oglas.Id == id);
            }
            foreach (string k in VecPostojece)
            {
                if(k != null)
                    Karakteristike.Add(new Karakteristika(k));
            }

            foreach (var k in Karakteristike)
            {

                k.Oglas = new MongoDBRef("oglass", id);
                karakteristikaCollection.InsertOne(k);
                Oglas.Karakteristike.Add(new MongoDBRef("karakteristikas",k.Id));
            }

            if (ListaImenaClanova.Count > 0)
            {
                for (var clan = 0; clan < ListaImenaClanova.Count; clan++)
                {
                    if (ListaImenaClanova[clan] != "" && ListaImenaInstrumenata[clan] != "")
                    {
                        ClanBenda clanBenda = new ClanBenda();
                        clanBenda.Ime = ListaImenaClanova[clan];
                        clanBenda.Instrument = ListaImenaInstrumenata[clan];
                        clanBenda.Oglas = new MongoDBRef("oglass",id);
                        ClanoviBenda.Add(clanBenda);

                        clanBendaCollection.InsertOne(clanBenda);
                        Oglas.ClanoviBenda.Add(new MongoDBRef("clanbendas",clanBenda.Id));
                       
                    }
                }
               
            }
            if(Oglas.TipOglasa=="Bend")
            {
                Oglas.BrojClanova = ClanoviBenda.Count();
            }
            var filter = Builders<Oglas>.Filter;
            var f = filter.Eq("Id", id);

            Oglas.Id = id;
            Oglas.KorisnikId = user1.Id;
            await oglasCollection.ReplaceOneAsync(f, Oglas);

            return RedirectToPage("IzmeniOglas", new { Id = Oglas.Id });
        }

        public async Task<IActionResult> OnPostObrisiSlikuAsync(string IdSlike, string Id)
        {
            var id = new ObjectId(IdSlike);

            var idO = new ObjectId(Id);
            var dataBase = client.GetDatabase("OrganizujProslavu");
            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            var slikaCollection = dataBase.GetCollection<Slika>("slikas");

            await slikaCollection.DeleteOneAsync(x => x.Id == id);

            return RedirectToPage("IzmeniOglas", new { Id = Id });
        }

        public async Task<IActionResult> OnPostObrisiClanaAsync(string IdClan,string Id)
        {
            var id = new ObjectId(IdClan);

            var idO = new ObjectId(Id);
            var dataBase = client.GetDatabase("OrganizujProslavu");
            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            var clanBendaCollection = dataBase.GetCollection<ClanBenda>("clanbendas");
            var clan = await clanBendaCollection.Find(x => x.Id == id).SingleAsync();

            await clanBendaCollection.DeleteOneAsync(x => x.Id == id);

            ClanoviBenda = clanBendaCollection.Find(x => x.Oglas.Id == idO).ToList();

            var filter = Builders<Oglas>.Filter;
            var f = filter.Eq("Id", idO);
            var update = Builders<Oglas>.Update.Set(x => x.BrojClanova, ClanoviBenda.Count);
            oglasCollection.UpdateOne(f, update);

            return RedirectToPage("IzmeniOglas", new { Id = Id });
            
        }
    }
}
