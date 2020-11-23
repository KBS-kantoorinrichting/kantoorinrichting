﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Models {
    public class Room : Data, IEntity {
        [Column("RoomId")] public int Id { get; set; }

        public string Name { get; set; }

        public string Positions { get; set; }

        public Room(string name = default, int width = default, int length = default, int id = default, string positions = default) {
            Id = id;
            Name = name;
            Positions = positions ?? FromDimensions(width, length);
        }

        protected override ITuple Variables => (Id, Name, Positions);

        public Room(string name, string positions) {
            Name = name;
            Positions = positions;
        }

        public static List<Position> ToList(string positions) {
            return positions switch {
                null => null,
                "" => new List<Position>(),
                _ => positions.Split("|").Select(p => new Position(p)).ToList()
            };
        }

        public static string FromList(IEnumerable<Position> positions) {
            if (positions == null) return null;
            IEnumerable<Position> enumerable = positions.ToList();

            if (!enumerable.Any()) return "";
            return enumerable.Select(p => p.ToString())
                .Aggregate((s1, s2) => $"{s1}|{s2}");
        }

        public static Room FromDimensions(string name, int width, int length) {
            return new Room(name, FromDimensions(width, length));
        }

        public static string FromDimensions(int width, int length) {
            // maakt een lijst posities aan van de gegeven breedte en lengte (vierhoek kamer)
            return FromList(
                new[] {
                    new Position(0, 0),
                    new Position(width, 0),
                    new Position(width, length),
                    new Position(0, length),
                }
            );
        }

        public static Room FromTemplate(string name, int width, int length, int template) {
            // returnt nieuwe kamer
            return new Room(name, FromTemplate(width, length, template));
        }

        public static string FromTemplate(int width, int length, int template) {
            if (template == 1) {
                // maakt een lijst posities aan van de gegeven breedte en lengte (Hoek kamer)
                return FromList(
                    new[] {
                        new Position(0, 0),
                        new Position((width / 2), 0),
                        new Position((width / 2), (length / 2)),
                        new Position(width, (length / 2)),
                        new Position(width, length),
                        new Position(0, length),
                    }
                );
            } else {
                // maakt een lijst posities aan van de gegeven breedte en lengte (vierhoek kamer)
                return FromList(
                    new[] {
                        new Position(0, 0),
                        new Position(width, 0),
                        new Position(width, length),
                        new Position(0, length),
                    }
                );
            }
        }
    }
}