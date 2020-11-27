using Designer.Other;
using Designer.View;
using Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Designer.ViewModel
{
    public class RoomEditorViewModel
    {
        public string Name { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public BasicCommand Submit { get; set; }


        public void SubmitRoom()
        {
            /*//if (SaveRoom(Name) != null)
            {
                //opent successvol dialoog
                GeneralPopup popup = new GeneralPopup("De kamer is opgeslagen!");
                popup.ShowDialog();
                return;
            }
            //opent onsuccesvol dialoog
            GeneralPopup popuperror = new GeneralPopup("Er is iets misgegaan! probeer opnieuw.");
            popuperror.ShowDialog();*/
        }

       
    }


        
}

