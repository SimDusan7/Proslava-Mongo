using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;


namespace OrganizujProslavu.Models
{
    public class Karakteristika
    {
        public ObjectId Id { get; set; }
        public string Naziv { get; set; }
        public bool IsChecked { get; set; }
        public MongoDBRef Oglas { get; set; }
        public Karakteristika(string naziv)
        {
            Naziv = naziv;
        }

    }
}
    
