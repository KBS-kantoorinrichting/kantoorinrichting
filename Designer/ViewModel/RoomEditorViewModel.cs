using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using Designer.Model;

namespace Designer.ViewModel
{
    public class RoomEditorViewModel
    {

        public Room room = new Room();

        // gebruik regex om te kijken of je text letters bevat (voor lengte en breedte)
        private readonly Regex _regex = new Regex("[^0-9.-]+");
        public bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        // methode om de kamer op te slaan
        public Boolean SaveRoom(string name, int width, int length)
        {

            // voegt de specificaties van de kamer aan het object room toe
            room.Name = name;
            room.Width = width;
            room.Length = length;
            
            // kamer opslaan
            using (var context = RoomDesignContext.Instance)
            {
                var post = context.Rooms.Add(room);

                try
                {
                    context.SaveChanges();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                    throw;
                }
            }
        }

    }
}
