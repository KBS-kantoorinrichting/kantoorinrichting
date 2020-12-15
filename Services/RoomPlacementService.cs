using Microsoft.EntityFrameworkCore;
using Models;

namespace Services
{
    public class RoomPlacementService : BasicService<RoomPlacement>
    {
        private static RoomPlacementService _instance;

        public static RoomPlacementService Instance
        {
            get => _instance ??= new RoomPlacementService();
            set => _instance = value;
        }

        protected override DbSet<RoomPlacement> DbSet => RoomDesignContext.Instance.RoomPlacements;
        protected override DbContext DbContext => RoomDesignContext.Instance;
    }
}
