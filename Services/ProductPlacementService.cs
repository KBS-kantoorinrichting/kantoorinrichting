using Microsoft.EntityFrameworkCore;
using Models;
using Repositories;

namespace Services {
    public class ProductPlacementService : BasicService<ProductPlacement> {
        private static ProductPlacementService _instance;

        public static ProductPlacementService Instance {
            get => _instance ??= new ProductPlacementService();
            set => _instance = value;
        }
        
        protected override DbSet<ProductPlacement> DbSet => RoomDesignContext.Instance.ProductPlacements;
        protected override DbContext DbContext => RoomDesignContext.Instance;
    }
}