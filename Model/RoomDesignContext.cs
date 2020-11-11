﻿using System;
using System.Collections.Generic;
using System.Text;
using dotenv.net;
using dotenv.net.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Model
{
    public class RoomDesignContext : DbContext
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Design> Designs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductPlacement> ProductPlacements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            //Load the .env file from the project root
            DotEnv.Config(true, Environment.CurrentDirectory + @"\..\.env");
            var envReader = new EnvReader();
            //Use the CONNECTION_STRING from the .env file
            options.UseSqlServer(envReader.GetStringValue("CONNECTION_STRING"));
        }
    }
}
