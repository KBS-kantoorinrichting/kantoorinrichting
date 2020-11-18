using System;
using System.Collections.Generic;
using Models;
using NUnit.Framework;
using RepositoriesTest;
using Services;

namespace ServicesRest {
    public class Tests {
        private static readonly Room Room1 = new Room("TestRoom1", 1, 1);
        private static readonly Room Room2 = new Room("TestRoom2", 2, 4);
        private static readonly Room Room3 = new Room("TestRoom3", 2, 5);

        [SetUp]
        public void Setup() {
            TestRepository.Setup(
                new List<Room> {Room1, Room2, Room3}
            );
        }

        [Test]
        public void Test1() {
            RoomService.Instance.Save(new Room("test", 10, 10));
            RoomService.Instance.Save(new Room("test", 10, 10));
            RoomService.Instance.Save(new Room("test", 10, 10));
            Console.WriteLine(RoomService.Instance.Count());
            foreach (var room in RoomService.Instance.GetAll()) {
                Console.WriteLine(room.Id);
            }
        }
    }
}