using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace OrganizujProslavu.Models
{
    public class Rezervacija
    {
        public ObjectId Id { get; set; }
        public string NazivOglasa { get; set; }
        public Guid KorisnikId { get; set; }
        public MongoDBRef Oglas { get; set; }
        public DateTime Datum { get; set; }
        public int Trajanje { get; set; }
        public bool Otkazana { get; set; }
        public bool OtkazanaV { get; set; }
        public bool Istekla { get; set; }
        public string RazlogOtkaza { get; set; }
        public string TipProslave { get; set; }
        public int BrojGosta { get; set; }
        public string Opis { get; set; }
    }
}
