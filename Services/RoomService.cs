using Microsoft.EntityFrameworkCore;
using Models;
using System.Linq;

namespace Services {
    public class RoomService : BasicService<Room> {
        private static RoomService _instance;

        public RoomService()
        {
            RoomDesignContext.Instance.RoomPlacements.ToList();
        }

        public static RoomService Instance {
            get => _instance ??= new RoomService();
            set => _instance = value;
        }

        protected override DbSet<Room> DbSet => RoomDesignContext.Instance.Rooms;
        protected override DbContext DbContext => RoomDesignContext.Instance;
    }
}