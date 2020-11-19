using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using NUnit.Framework;
using Repositories;

namespace RepositoriesTest {
    public class DatabaseTest {
        protected virtual List<Room> Rooms { get; set; }
        protected virtual List<Design> Designs { get; set; }
        protected virtual List<Product> Products  { get; set; }
        protected virtual List<ProductPlacement> ProductPlacements  { get; set; }
        
        [SetUp]
        public void SetupDatabase() {
            RoomDesignContext context = new RoomDesignContext(
                new DbContextOptionsBuilder<RoomDesignContext>()
                   .UseSqlite(CreateInMemoryDatabase())
                   .EnableSensitiveDataLogging()
                   .Options
            );
            
            context.Database.EnsureCreated();
            RoomDesignContext.Instance = context;

            if (Rooms != null) context.Rooms.AddRange(Rooms);
            if (Products != null) context.Products.AddRange(Products);
            if (Designs != null) context.Designs.AddRange(Designs);
            if (ProductPlacements != null) context.ProductPlacements.AddRange(ProductPlacements);
            context.SaveChanges();
        }

        private static DbConnection CreateInMemoryDatabase() {
            DbConnection connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        [TearDown]
        public void TearDownDatabase() {
            RoomDesignContext.Instance.Database.CloseConnection();
            RoomDesignContext.Instance = null;
        }
    }
}