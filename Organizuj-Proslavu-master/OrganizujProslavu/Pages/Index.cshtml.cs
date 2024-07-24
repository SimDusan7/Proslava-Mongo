using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.AspNetCore.Identity;
using OrganizujProslavu.Models;
using OrganizujProslavu.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrganizujProslavu.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private MongoClient client;

        IQueryable<Oglas> oglas { get; set; }
        [BindProperty]
        public IList<Oglas> Oglasi { get; set; }

        [BindProperty]
        public List<SelectListItem> TipOglasa { get; set; }
        [BindProperty]
        public List<SelectListItem> SortirajPo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TipOglasaFilter { get; set; }
        [BindProperty(SupportsGet = true)]
        public string SortirajPoFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string FilterIme { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
           _logger = logger;
            client = new MongoClient("mongodb://localhost/?safe=true");

            TipOglasa = new List<SelectListItem>
            {
                new SelectListItem { Text="Bend", Value = "Bend"},
                new SelectListItem { Text="Restoran", Value = "Restoran"},
                new SelectListItem { Text="Kafic", Value = "Kafic"},
                new SelectListItem { Text="Kafana", Value = "Kafana"},
                new SelectListItem { Text="Igraonica", Value = "Igraonica"},
                new SelectListItem { Text="Bar", Value = "Bar"}
            };

            SortirajPo = new List<SelectListItem>
            {
                new SelectListItem { Text="Prvo najstariji", Value = "Najstariji"},
                new SelectListItem { Text="Prvo najnoviji", Value = "Najnoviji"},
            };

        }
        public async Task OnGetAsync()
        {
            var dataBase = client.GetDatabase("OrganizujProslavu");
            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");

            oglas = oglasCollection.AsQueryable().OrderByDescending(x => x.Id);

            if (!string.IsNullOrEmpty(SortirajPoFilter))
            {
                if (SortirajPoFilter == "Najstariji")
                {
                    oglas=oglasCollection.AsQueryable().OrderBy(x => x.Id);
                }
                if (SortirajPoFilter == "Najnoviji")
                {
                    oglas = oglasCollection.AsQueryable().OrderByDescending(x => x.Id);
                }
            }
            if (!string.IsNullOrEmpty(TipOglasaFilter))
            {
                oglas = oglasCollection.AsQueryable().Where(x => x.TipOglasa == TipOglasaFilter); 
            }

            if (!string.IsNullOrEmpty(FilterIme))
            {
                oglas = oglasCollection.AsQueryable().Where(x => x.Naziv.Contains(FilterIme));
            }

            if (!string.IsNullOrEmpty(FilterIme) && !string.IsNullOrEmpty(TipOglasaFilter))
            {
                oglas = oglasCollection.AsQueryable().Where(x => x.Naziv.Contains(FilterIme) && x.TipOglasa == TipOglasaFilter);
            }

            if (!string.IsNullOrEmpty(FilterIme) && !string.IsNullOrEmpty(SortirajPoFilter))
            {
                if (SortirajPoFilter == "Uzlazno")
                {
                    oglas = oglasCollection.AsQueryable().Where(x => x.Naziv.Contains(FilterIme)).OrderBy(x => x.SrednjaOcena);
                }
                if (SortirajPoFilter == "Silazno")
                {
                    oglas = oglasCollection.AsQueryable().Where(x => x.Naziv.Contains(FilterIme)).OrderByDescending(x => x.SrednjaOcena);
                }
                if (SortirajPoFilter == "Najstariji")
                {
                    oglas = oglasCollection.AsQueryable().Where(x => x.Naziv.Contains(FilterIme)).OrderBy(x => x.Id);
                }
                if (SortirajPoFilter == "Najnoviji")
                {
                    oglas = oglasCollection.AsQueryable().Where(x => x.Naziv.Contains(FilterIme)).OrderByDescending(x => x.Id);
                }

            }
            if (!string.IsNullOrEmpty(TipOglasaFilter) && !string.IsNullOrEmpty(SortirajPoFilter))
            {
                
                if (SortirajPoFilter == "Najstariji")
                {
                    oglas = oglasCollection.AsQueryable().Where(x => x.TipOglasa == TipOglasaFilter).OrderBy(x => x.Id);
                }
                if (SortirajPoFilter == "Najnoviji")
                {
                    oglas = oglasCollection.AsQueryable().Where(x => x.TipOglasa == TipOglasaFilter).OrderByDescending(x => x.Id);
                }
            }
            if (!string.IsNullOrEmpty(FilterIme) && !string.IsNullOrEmpty(TipOglasaFilter) && !string.IsNullOrEmpty(SortirajPoFilter))
            {
                if (SortirajPoFilter == "Najstariji")
                {
                    oglas = oglasCollection.AsQueryable().Where(x => x.Naziv.Contains(FilterIme) && x.TipOglasa == TipOglasaFilter).OrderBy(x => x.Id);
                }
                if (SortirajPoFilter == "Najnoviji")
                {
                    oglas = oglasCollection.AsQueryable().Where(x => x.Naziv.Contains(FilterIme) && x.TipOglasa == TipOglasaFilter).OrderByDescending(x => x.Id);
                }
            }
            Oglasi = oglas.ToList();
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
