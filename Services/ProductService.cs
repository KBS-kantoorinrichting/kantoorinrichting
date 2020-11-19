using Microsoft.EntityFrameworkCore;
using Models;

namespace Services {
    public class ProductService : BasicService<Product> {
        private static ProductService _instance;

        public static ProductService Instance {
            get => _instance ??= new ProductService();
            set => _instance = value;
        }
        
        protected override DbSet<Product> DbSet => RoomDesignContext.Instance.Products;
        protected override DbContext DbContext => RoomDesignContext.Instance;
    }
}