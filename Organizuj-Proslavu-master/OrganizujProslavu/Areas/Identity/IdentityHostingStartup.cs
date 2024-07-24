using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrganizujProslavu.Areas.Identity.Data;
using OrganizujProslavu.Data;
using AspNetCore.Identity.MongoDbCore.Models;

[assembly: HostingStartup(typeof(OrganizujProslavu.Areas.Identity.IdentityHostingStartup))]
namespace OrganizujProslavu.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            //builder.ConfigureServices((context, services) => {
            //    services.AddIdentity<Korisnik, MongoIdentityRole>()
            //        .AddMongoDbStores<Korisnik, MongoIdentityRole, Guid>("mongodb://localhost:27017", "MongoDbTests")
            //        .AddSignInManager()
            //        .AddDefaultTokenProviders();
            //});
        }
    }
}