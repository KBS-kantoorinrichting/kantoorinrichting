using Models;
using Repositories;

namespace Services {
    public class ProductService : BasicService<Product> {
        private static ProductService _instance;

        public static ProductService Instance {
            get => _instance ??= new ProductService();
            set => _instance = value;
        }
        
        private ProductService() : base(RoomDesignContext.Instance.Products, RoomDesignContext.Instance) { }
    }
}