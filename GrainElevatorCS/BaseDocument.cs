using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainElevatorCS
{
    public abstract class BaseDocument
    {
        public string? Date { get; set; }
        public string? ProductTitle { get; set; }
        public int ProductWeight { get; set; }

        public BaseDocument() { }
        public BaseDocument(string date, string productTitle, int productWeight)
        {
            Date = date;
            ProductTitle = productTitle;
            ProductWeight = productWeight;
        }
    }
}
