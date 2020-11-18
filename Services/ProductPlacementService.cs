﻿using Models;
using Repositories;

namespace Services {
    public class ProductPlacementService : BasicService<ProductPlacement> {
        private static ProductPlacementService _instance;

        public static ProductPlacementService Instance {
            get => _instance ??= new ProductPlacementService();
            set => _instance = value;
        }
        
        private ProductPlacementService() : base(RoomDesignContext.Instance.ProductPlacements, RoomDesignContext.Instance) { }
    }
}