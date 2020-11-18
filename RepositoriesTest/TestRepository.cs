using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace RepositoriesTest {
    public class TestRepository {
        public static void Setup(
            List<Room> rooms = null,
            List<Design> designs = null,
            List<Product> products = null,
            List<ProductPlacement> productPlacements = null
        ) {
            RoomDesignContext context = new RoomDesignContext(
                new DbContextOptionsBuilder<RoomDesignContext>()
                   .UseSqlite(CreateInMemoryDatabase())
                   .Options
            );
            
            context.Database.EnsureCreated();
            RoomDesignContext.Instance = context;

            if (rooms != null) context.Rooms.AddRange(rooms);
            if (products != null) context.Products.AddRange(products);
            if (designs != null) context.Designs.AddRange(designs);
            if (productPlacements != null) context.ProductPlacements.AddRange(productPlacements);
            context.SaveChanges();
        }

        private static DbConnection CreateInMemoryDatabase() {
            SqliteConnection connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }
    }
}