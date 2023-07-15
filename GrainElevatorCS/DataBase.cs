using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainElevatorCS
{
    public class DataBase<T> : IDataBase
    {
        public string? TitleDB { get; set; }
        public List<T>? items { get; set; }

        public DataBase(string? title)
        {
            TitleDB = title;
            items = new List<T>();
        }

        public void Show()
        { 
            if(items != null)
                foreach(var item in items)
                     Console.WriteLine($"{item}\n\n");
        }
    }
}
