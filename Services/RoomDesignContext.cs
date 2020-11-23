﻿using Microsoft.EntityFrameworkCore;
using Models;

namespace Services {
    class RoomDesignContext : DbContext {
        //Zorgt er voor dat er maar 1 instance van de context bestaat.
        private static RoomDesignContext _instance;

        public static RoomDesignContext Instance {
            get => _instance ??= _create();
            set => _instance = value;
        }

        private static RoomDesignContext _create() {
            RoomDesignContext context = new RoomDesignContext(RoomDesignOptions.Options);
            if (RoomDesignOptions.EnsureCreated) context.Database.EnsureCreated();
            else context.Database.Migrate();
            return context;
        }

        public RoomDesignContext() : base(RoomDesignOptions.Options) { }

        //Alternative constructor zodat er de db getest kan worden.
        public RoomDesignContext(DbContextOptions options) : base(options) { }

        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<Design> Designs { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductPlacement> ProductPlacements { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options) {
            // todo options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }
}