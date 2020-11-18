using Designer.Model;

namespace Designer.Services {
    public class ProductPlacementService : BasicService<ProductPlacement> {
        private static ProductPlacementService _instance;

        public static ProductPlacementService Instance {
            get => _instance ??= new ProductPlacementService();
            set => _instance = value;
        }
        
        private ProductPlacementService() : base(RoomDesignContext.Instance.ProductPlacements, RoomDesignContext.Instance) { }
    }
}