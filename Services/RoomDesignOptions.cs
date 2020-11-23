using System;
using dotenv.net;
using dotenv.net.Utilities;
using Microsoft.EntityFrameworkCore;

namespace Services {
    public static class RoomDesignOptions {
        private static DbContextOptions _options;

        public static DbContextOptions Options {
            get => _options ??= _default();
            set => _options = value;
        }

        //Wordt alleen gebruikt bij testen, normaal gesproken migrate de database
        public static bool EnsureCreated { get; set; }

        private static DbContextOptions _default() {
            Console.WriteLine("[RoomDesignContext] Currently running in: " + Environment.CurrentDirectory);
            //Load the .env file from the project root
            DotEnv.Config(true, Environment.CurrentDirectory + @"\.env");
            var envReader = new EnvReader();

            //Use the CONNECTION_STRING from the .env file
            return new DbContextOptionsBuilder()
                .UseSqlServer(envReader.GetStringValue("CONNECTION_STRING"))
                .Options;
        }

        public static void Reset() {
            RoomDesignContext.Instance.Database.CloseConnection();
            RoomDesignContext.Instance = null;
        }
    }
}