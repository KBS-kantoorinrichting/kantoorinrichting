using Models;
using Repositories;

namespace Services {
    public class RoomService : BasicService<Room> {
        private static RoomService _instance;

        public static RoomService Instance {
            get => _instance ??= new RoomService();
            set => _instance = value;
        }
        
        private RoomService() : base(RoomDesignContext.Instance.Rooms, RoomDesignContext.Instance) { }
    }
}