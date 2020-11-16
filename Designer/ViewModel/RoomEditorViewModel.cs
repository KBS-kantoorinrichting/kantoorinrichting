using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Designer.Model;

namespace Designer.ViewModel
{
    public class RoomEditorViewModel
    {

        public void SaveRoom(string name, int width, int length)
        {
            Room room = new Room()
            {
                Name = name,
                Width = width,
                Length = length,
            };


            using (var context = RoomDesignContext.Instance)
            {
                var post = context.Rooms.Add(room);

                /* post.Name = RoomNameTextBox.Text;
                 post.Width = 300;
                 post.Length = 300;*/
                try
                {
                    context.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

    }
}
