using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using czatSerwerTIN.Startup;
using MongoDB.Driver;
using czatSerwerTIN.DBmanager;

namespace czatSerwerTIN
{
    class Program
    {
        static void Main(string[] args)
        {

            //MongoConnect mongo = new MongoConnect();
            //mongo.InsertUser("Marcin");
           /* mongo.GetUserList().ForEach(element => {
                Console.WriteLine(element);
            });*/
           
            //var client = new MongoClient("mongodb://darekddd:Password1!@localhost:2717");
            //var database = client.GetDatabase("ChatDB");
            //Console.WriteLine(database.GetCollection<string>("users").Find( some =>true).ToList<string>());
            CreateWebHostBUilder(args).Build().Run();
            
        }

        private static IWebHostBuilder CreateWebHostBUilder(string[] args) =>
        
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup.Startup>();

           
        
    }
}

