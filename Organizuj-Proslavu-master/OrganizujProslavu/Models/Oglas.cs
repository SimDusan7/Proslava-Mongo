using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OrganizujProslavu.Models
{
    public class Oglas
    {
        public ObjectId Id { get; set; }
        public string Naziv { get; set; }
        public int? BrojClanova { get; set; }
        public int? BrojGosta { get; set; }
        public int? BrojDece { get; set; }
        public string Opis { get; set; }
        public string TipOglasa { get; set; }
        public string VrstaMuzike { get; set; }
        public decimal SrednjaOcena { get; set; }
        public string Adresa { get; set; }
        public string Grad { get; set; }
        public string BrojTelefona { get; set; }
        public string NaslovnaSlika { get; set; }
        public bool NoveRezervacije { get; set; }
        public int Od { get; set; }
        public int Do { get; set; }
        public Guid KorisnikId { get; set; }
        public List<MongoDBRef> Karakteristike { get; set; }
        public List<MongoDBRef> Termini { get; set; }
        public List<MongoDBRef> Rezervacije { get; set; }
        public List<MongoDBRef> ClanoviBenda { get; set; }
        public List<MongoDBRef> SlikeOglasa { get; set; }

        public Oglas()
        {
            Karakteristike = new List<MongoDBRef>();
            Termini = new List<MongoDBRef>();
            Rezervacije = new List<MongoDBRef>();
            ClanoviBenda = new List<MongoDBRef>();
            SlikeOglasa = new List<MongoDBRef>();
        }
    }
}
