using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
namespace OrganizujProslavu.Models
{
    public class Slika
    {
        public ObjectId Id { get; set; }
        public string PhotoPath { get; set; }

        public MongoDBRef Oglas { get; set; }
    }
}
