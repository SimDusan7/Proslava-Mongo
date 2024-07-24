using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OrganizujProslavu.Models
{
    public class Termin
    {
        public ObjectId Id { get; set; }
        public MongoDBRef Oglas { get; set; }
        public Guid KorisnikId { get; set; }
        public DateTime ZauzetTermin { get; set; }
        public int Trajanje { get; set; }
        public string TipProslave { get; set; }
        public string KorisnikImePrezime { get; set; }
        public string KorisnikBroj{ get; set; }
        public int BrojGosta { get; set; }
        public string Opis { get; set; }
    }
}
