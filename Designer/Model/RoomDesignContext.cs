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
        public static RoomDesignContext Instance {
            get => _instance ??= new RoomDesignContext();
            set => _instance = value;
        }

        private bool _ignore = false;

        protected RoomDesignContext() {
        }

        public RoomDesignContext(DbContextOptions options) : base(options) => _ignore = true;

        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<Design> Designs { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductPlacement> ProductPlacements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            if (_ignore) return;
            Console.WriteLine("[RoomDesignContext] Currently running in: " + Environment.CurrentDirectory);
            //Load the .env file from the project root
            DotEnv.Config(true, Environment.CurrentDirectory + @"\..\..\..\..\.env");
            var envReader = new EnvReader();
            //Use the CONNECTION_STRING from the .env file
            options.UseSqlServer(envReader.GetStringValue("CONNECTION_STRING"));
        }
    }
}
