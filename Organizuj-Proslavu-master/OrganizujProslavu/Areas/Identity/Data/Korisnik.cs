using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using AspNetCore.Identity.MongoDbCore.Models;
using AspNetCore.Identity.MongoDbCore;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OrganizujProslavu.Areas.Identity.Data
{
    public class Korisnik : MongoIdentityUser
    {
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public string Grad { get; set; }
        public string Slika { get; set; }
        public virtual List<MongoDBRef> Oglasi { get; set; }
        public virtual List<MongoDBRef> Rezervacije { get; set; }

        public Korisnik()
        {
            Oglasi = new List<MongoDBRef>();
            Rezervacije = new List<MongoDBRef>();
        }
    }
}
