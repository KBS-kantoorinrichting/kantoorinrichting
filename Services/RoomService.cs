using Microsoft.EntityFrameworkCore;
using Models;

namespace Services {
    public class RoomService : BasicService<Room> {
        private static RoomService _instance;

        public static RoomService Instance {
            get => _instance ??= new RoomService();
            set => _instance = value;
        }

        protected override DbSet<Room> DbSet => RoomDesignContext.Instance.Rooms;
        protected override DbContext DbContext => RoomDesignContext.Instance;
    }
}