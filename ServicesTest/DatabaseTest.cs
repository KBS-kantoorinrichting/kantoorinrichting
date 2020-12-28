using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using NUnit.Framework;
using Services;

namespace ServicesTest {
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

            Rooms?.ForEach(m => RoomService.Instance.Add(m));
            Products?.ForEach(m => ProductService.Instance.Add(m));
            Designs?.ForEach(m => DesignService.Instance.Add(m));
            ProductPlacements?.ForEach(m => ProductPlacementService.Instance.Add(m));

            //Just calls context save changes
            RoomService.Instance.SaveChanges();
        }

        private static DbConnection CreateInMemoryDatabase() {
            DbConnection connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        [TearDown]
        public void TearDownDatabase() { RoomDesignOptions.Reset(); }
    }
}