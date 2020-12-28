using Microsoft.EntityFrameworkCore;
using Models;

namespace Services {
    public class DesignService : BasicService<Design> {
        private static DesignService _instance;

        public static DesignService Instance {
            get => _instance ??= new DesignService();
            set => _instance = value;
        }

        protected override DbSet<Design> DbSet => RoomDesignContext.Instance.Designs;
        protected override DbContext DbContext => RoomDesignContext.Instance;
    }
}