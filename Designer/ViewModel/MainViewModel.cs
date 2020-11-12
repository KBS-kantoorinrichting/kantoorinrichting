using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;

namespace Designer.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public Timer Timer = new Timer(100);
        public int counter = 0;
        public string Test { get; set; }
        public MainViewModel()
        {
            Timer.Start();
            Timer.Elapsed += TimerOnElapsed;
            Test = "test";
            Console.WriteLine("Yeet");
            Debug.WriteLine("Yeet");
            // Do your thing
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            counter++;
            Test = counter.ToString();
            OnPropertyChanged("Test");
        }


        private void OnPropertyChanged(string propertyName)  
        {  
            if (PropertyChanged != null)  
            {  
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));  
            }  
        }  
    }
}
