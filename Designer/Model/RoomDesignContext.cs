using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using dotenv.net;
using dotenv.net.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Designer.Model
{
    public class RoomDesignContext : DbContext {
        private static RoomDesignContext _instance;
        public static RoomDesignContext Instance => _instance ??= new RoomDesignContext();

        public DbSet<Room> Rooms { get; set; }
        public DbSet<Design> Designs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPlacement> ProductPlacements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            Console.WriteLine("[RoomDesignContext] Currently running in: " + Environment.CurrentDirectory);
            //Load the .env file from the project root
            DotEnv.Config(true, Environment.CurrentDirectory + @"\..\..\..\..\.env");
            var envReader = new EnvReader();
            //Use the CONNECTION_STRING from the .env file
            options.UseSqlServer(envReader.GetStringValue("CONNECTION_STRING"));
        }
    }
}
