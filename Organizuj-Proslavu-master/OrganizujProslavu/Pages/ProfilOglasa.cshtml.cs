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
    public class ProfilOglasaModel : PageModel
    {
        private readonly UserManager<Korisnik> _userManager;
        private MongoClient client { get; set; }
        [BindProperty]
        public Oglas Oglas { get; set; }
        public Korisnik Korisnik { get; set; }
        [BindProperty]
        public Termin Termin { get; set; }
        [BindProperty]
        public Rezervacija Rezervacija { get; set; }
        [BindProperty]
        public List<SelectListItem> TipProslave { get; set; }
        [BindProperty]
        public List<Slika> SlikeOglasa { get; set; }
        public List<ClanBenda> ClanoviBenda { get; set; }
        public List<Karakteristika> Karakteristike { get; set; }
        [BindProperty]
        public IList<Rezervacija> Rezervacije { get; set; }
        [BindProperty]
        public string Razlog { get; set; }

        public ProfilOglasaModel(UserManager<Korisnik> userManager)
        {
            client = new MongoClient("mongodb://localhost/?safe=true");
            _userManager = userManager;
        }
        public async Task OnGetAsync(string Id)
        {
            var id = new ObjectId(Id);
            var user1 = await _userManager.GetUserAsync(User);
            var dataBase = client.GetDatabase("OrganizujProslavu");
            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            Oglas = await oglasCollection.Find(x => x.Id == id).SingleAsync();

            if (user1 != null)
            {

                if (Oglas.KorisnikId == user1.Id)
                {
                    Oglas.NoveRezervacije = false;
                    oglasCollection.ReplaceOne(x => x.Id == id, Oglas);
                }
            }

            var rezervacijaCollection = dataBase.GetCollection<Rezervacija>("rezervacijas");
            var rezervacije = rezervacijaCollection.AsQueryable().Where(x => x.Oglas.Id == id).OrderBy(x => x.Datum);
          
            if (rezervacije != null)
            {
                Rezervacije = rezervacije.ToList();
                foreach (var r in Rezervacije)
                {
                    if (r.Datum < System.DateTime.Now)
                    {
                        r.Istekla = true;
                        var filter = Builders<Rezervacija>.Filter;
                        var f = filter.Eq("Id", r.Id);
                        rezervacijaCollection.ReplaceOne(f, r);
                    }
                }
            }

            TipProslave = new List<SelectListItem>
            {
                new SelectListItem { Text="Svadba", Value = "Svadba"},
                new SelectListItem { Text="Krstenje", Value = "Krstenje"},
                new SelectListItem { Text="18.rodjendan", Value = "18.rodjendan"},
                new SelectListItem { Text="Matura", Value = "Matura"},
                new SelectListItem { Text="Zurka", Value = "Zurka"},
                new SelectListItem { Text="Deciji rodjendan", Value = "DecijiRodjendan"},
                new SelectListItem { Text="Proslava", Value = "Proslava"}
            };
            var slikaCollection = dataBase.GetCollection<Slika>("slikas");
            var slike = slikaCollection.AsQueryable().Where(x => x.Oglas.Id == id);

            SlikeOglasa = slike.ToList();
            var calanBendaCollection = dataBase.GetCollection<ClanBenda>("clanbendas");
            var clanovi = calanBendaCollection.AsQueryable().Where(x => x.Oglas.Id == id);

            ClanoviBenda = clanovi.ToList();
            var karakteristikaCollection = dataBase.GetCollection<Karakteristika>("karakteristikas");
            var karakteristike = karakteristikaCollection.AsQueryable().Where(x => x.Oglas.Id == id);
         
            Karakteristike = karakteristike.ToList();

            var korisnikCollection = dataBase.GetCollection<Korisnik>("korisniks");
            Korisnik = await korisnikCollection.Find(x => x.Id == Oglas.KorisnikId).SingleAsync();
        }
        public async Task<IActionResult> OnPostDodajTerminAsync(string Id)
        {
            var dataBase = client.GetDatabase("OrganizujProslavu");
           
            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            var id = new ObjectId(Id);
            Oglas =await oglasCollection.Find(x => x.Id == id).SingleAsync();
            var terminCollection = dataBase.GetCollection<Termin>("termins");
            var termini = terminCollection.AsQueryable().Where(x => x.Oglas.Id == Oglas.Id);
            List<Termin> sviTermini = new List<Termin>();
            sviTermini = termini.ToList();

            var user = await _userManager.GetUserAsync(User);
            if (Termin.ZauzetTermin > System.DateTime.Now.AddDays(1)) //minimalno 24h unapred
            {
                foreach (var termin in sviTermini)
                {
                    if (termin.ZauzetTermin.Date == Termin.ZauzetTermin.Date)
                    {
                        if (termin.ZauzetTermin == Termin.ZauzetTermin || (termin.ZauzetTermin.AddHours(termin.Trajanje) >= Termin.ZauzetTermin && Termin.ZauzetTermin.AddHours(Termin.Trajanje) >= termin.ZauzetTermin))// || Termin.ZauzetTermin.AddHours(Termin.Trajanje) > termin.ZauzetTermin.AddHours(-1))
                        {
                            TempData["Rezervacija"] = "GreskaT";
                            return RedirectToPage("ProfilOglasa", new { Id = Id});
                        }
                        if (Termin.ZauzetTermin.Hour >= 0 && Termin.ZauzetTermin.Hour <= Oglas.Do)
                        {
                            if (termin.ZauzetTermin.AddHours(termin.Trajanje) > Termin.ZauzetTermin)
                            {
                                if (termin.ZauzetTermin.AddHours(termin.Trajanje).Date == Termin.ZauzetTermin.Date)
                                {
                                    TempData["Rezervacija"] = "GreskaT";
                                    return RedirectToPage("ProfilOglasa", new { Id =Id });
                                }
                            }
                        }
                    }

                }
                if (Oglas.TipOglasa != "Bend")
                {
                    if (Oglas.Do <= 24 && Oglas.Do >= Oglas.Od) //PRE PONOCI
                    {
                        var KrajPrePonoci = Oglas.Do;
                        int terminVreme = Termin.ZauzetTermin.AddHours(Termin.Trajanje).Hour;

                        if (Termin.ZauzetTermin.Hour <= Oglas.Do && Termin.ZauzetTermin.Hour >= Oglas.Od)
                        {
                            if (Termin.ZauzetTermin.Hour < Oglas.Od || Termin.ZauzetTermin.AddHours(Termin.Trajanje).Hour > Oglas.Do)
                            {
                                TempData["Rezervacija"] = "GreskaRadno";
                                return RedirectToPage("ProfilOglasa", new { Id = Id });
                            }

                            int maxtrajanje = Oglas.Do - Termin.ZauzetTermin.Hour;

                            if (maxtrajanje < Termin.Trajanje)
                            {
                                TempData["Rezervacija"] = "GreskaRadno";
                                return RedirectToPage("ProfilOglasa", new { Id =Id });
                            }
                        }
                        if (Termin.ZauzetTermin.Hour > Oglas.Do)
                        {
                            TempData["Rezervacija"] = "GreskaRadno";
                            return RedirectToPage("ProfilOglasa", new { Id = Id });
                        }
                        if (terminVreme > 0 && terminVreme < Oglas.Do)
                        {
                            if (Termin.ZauzetTermin.Hour < Oglas.Od || Termin.ZauzetTermin.AddHours(Termin.Trajanje).Hour < Oglas.Od)
                            {
                                TempData["Rezervacija"] = "GreskaRadno";
                                return RedirectToPage("ProfilOglasa", new { Id = Id });
                            }
                        }
                    }
                    if (Oglas.Do >= 0 && Oglas.Do < Oglas.Od) //POSLE PONOCI
                    {
                        var KrajPoslePonoci = Oglas.Do;

                        if (Termin.ZauzetTermin.Hour < Oglas.Od && Termin.ZauzetTermin.AddHours(Termin.Trajanje).Hour > Oglas.Do)
                        {
                            TempData["Rezervacija"] = "GreskaRadno";
                            return RedirectToPage("ProfilOglasa", new { Id = Id });
                        }

                        if (Termin.ZauzetTermin.Hour < 24 && Termin.ZauzetTermin.Hour > Oglas.Od)
                        {
                            int mojtermin = 24 - Termin.ZauzetTermin.Hour + Oglas.Do;
                            if (mojtermin <= Termin.Trajanje)
                            {
                                TempData["Rezervacija"] = "GreskaRadno";
                                return RedirectToPage("ProfilOglasa", new { Id = Id });
                            }
                        }
                        if (Termin.ZauzetTermin.Hour > 0 && Termin.ZauzetTermin.Hour < Oglas.Do)
                        {
                            int maxtermin = Oglas.Do - Termin.ZauzetTermin.Hour;
                            if (Termin.Trajanje > maxtermin)
                            {
                                TempData["Rezervacija"] = "GreskaRadno";
                                return RedirectToPage("ProfilOglasa", new { Id = Id });
                            }
                        }
                    }
                }
                Termin.Oglas = new MongoDBRef("oglass",Oglas.Id);
                Termin.KorisnikImePrezime = "Samostalni termin";
                Termin.KorisnikBroj = user.PhoneNumber;

                terminCollection.InsertOne(Termin);
                Oglas.Termini.Add(new MongoDBRef("termins", Termin.Id));

                var filter = Builders<Oglas>.Filter;
                var f = filter.Eq("Id", Oglas.Id);
                oglasCollection.ReplaceOne(f, Oglas);

                TempData["Rezervacija"] = "Success";
                return RedirectToPage("/ProfilOglasa", new { Id = Id });
            }
            else
            {
                TempData["Rezervacija"] = "GreskaProslost";
                return RedirectToPage("/ProfilOglasa", new { Id = Oglas.Id });
            }

        }
        public async Task<IActionResult> OnPostRezervisiAsync(string Id)
        {
            var dataBase = client.GetDatabase("OrganizujProslavu");

            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            var id = new ObjectId(Id);
            Oglas = await oglasCollection.Find(x => x.Id == id).SingleAsync();

            var terminCollection = dataBase.GetCollection<Termin>("termins");
            var termini = terminCollection.AsQueryable().Where(x => x.Oglas.Id == Oglas.Id);
            List<Termin> sviTermini = new List<Termin>();
            sviTermini = termini.ToList();

            var rezervacijaCollection = dataBase.GetCollection<Rezervacija>("rezervacijas");

            var user = await _userManager.GetUserAsync(User);
            if (user.Ime == null || user.Prezime == null || user.PhoneNumber == null) //ne moze da rezervise oglas ako ne unese informacije
            {
                TempData["Rezervacija"] = "GreskaKorisnik";
                return RedirectToPage("ProfilOglasa", new { Id = Id });
            }
            else
            {
                if (Rezervacija.Datum > System.DateTime.Now.AddDays(1))
                {
                    foreach (var termin in sviTermini)
                    {
                        if (termin.ZauzetTermin.Date == Rezervacija.Datum.Date)
                        {
                            if (termin.ZauzetTermin == Rezervacija.Datum || (termin.ZauzetTermin.AddHours(termin.Trajanje) >= Rezervacija.Datum && Rezervacija.Datum.AddHours(Rezervacija.Trajanje) >= termin.ZauzetTermin))
                            {
                                TempData["Rezervacija"] = "GreskaT";
                                return RedirectToPage("ProfilOglasa", new { Id = Id });
                            }
                            if (Rezervacija.Datum.Hour >= 0 && Rezervacija.Datum.Hour <= Oglas.Do)
                            {
                                if (termin.ZauzetTermin.AddHours(termin.Trajanje) > Rezervacija.Datum)
                                {
                                    if (termin.ZauzetTermin.AddHours(termin.Trajanje).Date == Rezervacija.Datum.Date)
                                    {
                                        TempData["Rezervacija"] = "GreskaT";
                                        return RedirectToPage("ProfilOglasa", new { Id = Id });
                                    }
                                }
                            }
                        }
                    }
                    if (Oglas.TipOglasa != "Bend")
                    {
                        if (Oglas.Do <= 24 && Oglas.Do >= Oglas.Od) //PRE PONOCI
                        {
                            var KrajPrePonoci = Oglas.Do;
                            int terminVreme = Rezervacija.Datum.AddHours(Rezervacija.Trajanje).Hour;

                            if (Rezervacija.Datum.Hour <= Oglas.Do && Rezervacija.Datum.Hour >= Oglas.Od)
                            {
                                if (Rezervacija.Datum.Hour < Oglas.Od || Rezervacija.Datum.AddHours(Rezervacija.Trajanje).Hour > Oglas.Do)
                                {
                                    TempData["Rezervacija"] = "GreskaRadno";
                                    return RedirectToPage("ProfilOglasa", new { Id = Id });
                                }

                                int maxtrajanje = Oglas.Do - Rezervacija.Datum.Hour;

                                if (maxtrajanje < Rezervacija.Trajanje)
                                {
                                    TempData["Rezervacija"] = "GreskaRadno";
                                    return RedirectToPage("ProfilOglasa", new { Id = Id });
                                }
                            }
                            if (Rezervacija.Datum.Hour > Oglas.Do)
                            {
                                TempData["Rezervacija"] = "GreskaRadno";
                                return RedirectToPage("ProfilOglasa", new { Id = Id });
                            }
                            if (terminVreme > 0 && terminVreme < Oglas.Do)
                            {
                                if (Rezervacija.Datum.Hour < Oglas.Od || Rezervacija.Datum.AddHours(Rezervacija.Trajanje).Hour < Oglas.Od)
                                {
                                    TempData["Rezervacija"] = "GreskaRadno";
                                    return RedirectToPage("ProfilOglasa", new { Id = Id });
                                }
                            }
                        }
                        if (Oglas.Do >= 0 && Oglas.Do < Oglas.Od) //POSLE PONOCI
                        {
                            var KrajPoslePonoci = Oglas.Do;

                            if (Rezervacija.Datum.Hour < Oglas.Od && Rezervacija.Datum.AddHours(Rezervacija.Trajanje).Hour > Oglas.Do)
                            {
                                TempData["Rezervacija"] = "GreskaRadno";
                                return RedirectToPage("ProfilOglasa", new { Id = Id });
                            }

                            // int maxtrajanje = 24 - Oglas.Od + Oglas.Do;
                            if (Rezervacija.Datum.Hour < 24 && Rezervacija.Datum.Hour > Oglas.Od)
                            {
                                int mojtermin = 24 - Rezervacija.Datum.Hour + Oglas.Do;
                                if (mojtermin <= Rezervacija.Trajanje)
                                {
                                    TempData["Rezervacija"] = "GreskaRadno";
                                    return RedirectToPage("ProfilOglasa", new { Id = Id });
                                }
                            }
                            if (Rezervacija.Datum.Hour > 0 && Rezervacija.Datum.Hour < Oglas.Do)
                            {
                                int maxtermin = Oglas.Do - Rezervacija.Datum.Hour;
                                if (Rezervacija.Trajanje > maxtermin)
                                {
                                    TempData["Rezervacija"] = "GreskaRadno";
                                    return RedirectToPage("ProfilOglasa", new { Id = Id });
                                }

                            }
                        }

                    }

                    Rezervacija.Oglas = new MongoDBRef("oglass", Oglas.Id);
                    Rezervacija.KorisnikId = user.Id;
                    Rezervacija.Otkazana = false;
                    Rezervacija.NazivOglasa = Oglas.Naziv;

                    Termin.Oglas = new MongoDBRef("oglass", Oglas.Id);
                    Termin.ZauzetTermin = Rezervacija.Datum;
                    Termin.Opis = Rezervacija.Opis;
                    Termin.TipProslave = Rezervacija.TipProslave;
                    Termin.BrojGosta = Rezervacija.BrojGosta;
                    Termin.Trajanje = Rezervacija.Trajanje;
                    Termin.KorisnikImePrezime = user.Ime + " " + user.Prezime;
                    Termin.KorisnikBroj = user.PhoneNumber;

                    terminCollection.InsertOne(Termin);
                    rezervacijaCollection.InsertOne(Rezervacija);
                    Oglas.Termini.Add(new MongoDBRef("termins",Termin.Id));
                    Oglas.Rezervacije.Add(new MongoDBRef("rezervacijas", Rezervacija.Id));

                    Oglas.NoveRezervacije = true;
                    

                    var filter = Builders<Oglas>.Filter;
                    var f = filter.Eq("Id", Oglas.Id);
                    oglasCollection.ReplaceOne(f, Oglas);

                    TempData["Rezervacija"] = "Success";
                    return RedirectToPage("/ProfilOglasa", new { Id = Id });
                }
                else
                {
                    TempData["Rezervacija"] = "GreskaProslost";
                    return RedirectToPage("/ProfilOglasa", new { Id = Oglas.Id });
                }
            }
        }
        public async Task<IActionResult> OnPostOtkaziRezervacijuAsync(string IdOglas,string IdRez) //nisu svi termini u rezervacije (ovi privatni)
        { 
            var dataBase = client.GetDatabase("OrganizujProslavu");

            var oglasCollection = dataBase.GetCollection<Oglas>("oglass");
            var id = new ObjectId(IdOglas);
            Oglas = await oglasCollection.Find(x => x.Id == id).SingleAsync();

            var idRez = new ObjectId(IdRez);
            //var idRez = IdRez;
            var terminCollection = dataBase.GetCollection<Termin>("termins");
            var termin = await terminCollection.Find(x=>x.Id==idRez).SingleAsync();
            Termin = termin;
            var rezervacijaCollection = dataBase.GetCollection<Rezervacija>("rezervacijas");
            var ima = rezervacijaCollection.Find(x => x.Datum == termin.ZauzetTermin && x.Oglas.Id == id).Count();

            if (termin != null && termin.ZauzetTermin > System.DateTime.Now)
            {
                if (ima == 1)
                {
                    var rezervacija = await rezervacijaCollection.Find(x => x.Datum == termin.ZauzetTermin && x.Oglas.Id == id).SingleAsync();
                    if (Razlog != null)
                        rezervacija.RazlogOtkaza = Razlog;
                    rezervacija.OtkazanaV = true;
                    rezervacijaCollection.ReplaceOne(x => x.Id == rezervacija.Id, rezervacija);
                }

                MongoDBRef t = Oglas.Termini.Find(x => x.Id == termin.Id);
                Oglas.Termini.Remove(t);

                await terminCollection.DeleteOneAsync(x=>x.Id==termin.Id);

                var filter = Builders<Oglas>.Filter;
                var f = filter.Eq("Id", Oglas.Id);
                oglasCollection.ReplaceOne(f, Oglas);

                TempData["Rezervacija"] = "SuccessO";
                return RedirectToPage("ProfilOglasa", new { Id = IdOglas});
            }
            else
            {
                TempData["Rezervacija"] = "GreskaRez";
                return RedirectToPage("ProfilOglasa", new { Id = IdOglas });
            }
        }

        public IActionResult OnGetNadjiSveTermine(string Id)
        {
            var id = new ObjectId(Id);
            var dataBase = client.GetDatabase("OrganizujProslavu");
            var terminCollection = dataBase.GetCollection<Termin>("termins");
            var termini = terminCollection.AsQueryable().Where(x => x.Oglas.Id == id);
            List<Termin> sviTermini = new List<Termin>();
            sviTermini = termini.ToList();
            var events = sviTermini.Select(e => new
            {
                title = e.ZauzetTermin.Hour + ":" + e.ZauzetTermin.Minute.ToString("D2") + "h" + "-" + e.ZauzetTermin.AddHours(e.Trajanje).Hour + ":" + e.ZauzetTermin.AddHours(e.Trajanje).Minute.ToString("D2") + "h",
                start = e.ZauzetTermin,
                end = e.ZauzetTermin.AddHours(e.Trajanje),
                description = e.Opis,
                brojgosta = e.BrojGosta,
                tipproslave = e.TipProslave,
                vlasnik = e.KorisnikImePrezime,
                broj = e.KorisnikBroj,
                id = e.Id.ToString()
            }).ToList();

            return new JsonResult(events);
        }
    }
}