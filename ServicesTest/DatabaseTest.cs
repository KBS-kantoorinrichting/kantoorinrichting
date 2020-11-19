using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using NUnit.Framework;
using Services;

namespace ServicesRest {
    public class DatabaseTest {
        protected virtual List<Room> Rooms { get; set; }
        protected virtual List<Design> Designs { get; set; }
        protected virtual List<Product> Products { get; set; }
        protected virtual List<ProductPlacement> ProductPlacements { get; set; }

        [SetUp]
        public void SetupDatabase() {
            RoomDesignOptions.EnsureCreated = true;
            RoomDesignOptions.Options = new DbContextOptionsBuilder()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableSensitiveDataLogging()
                .Options;

            if (Rooms != null) RoomService.Instance.SaveAll(Rooms);
            if (Products != null) ProductService.Instance.SaveAll(Products);
            if (Designs != null) DesignService.Instance.SaveAll(Designs);
            if (ProductPlacements != null) ProductPlacementService.Instance.SaveAll(ProductPlacements);
        }

        private static DbConnection CreateInMemoryDatabase() {
            DbConnection connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        [TearDown]
        public void TearDownDatabase() {
            RoomDesignOptions.Reset();
        }
    }
}