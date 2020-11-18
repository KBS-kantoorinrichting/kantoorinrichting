using Models;
using Repositories;

namespace Services {
    public class DesignService : BasicService<Design> {
        private static DesignService _instance;

        public static DesignService Instance {
            get => _instance ??= new DesignService();
            set => _instance = value;
        }
        
        private DesignService() : base(RoomDesignContext.Instance.Designs, RoomDesignContext.Instance) { }
    }
}