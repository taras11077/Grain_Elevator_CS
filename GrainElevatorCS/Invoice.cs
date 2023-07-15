using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainElevatorCS
{
    public abstract class Invoice : BaseDocument
    {
        public string? InvNumber { get; set; }
        public string? VenicleRegNumber { get; set; }

        public Invoice() { }
        public Invoice(string date, string productTitle, int productWeight, string invNumber, string venicleRegNumber)
            : base(date, productTitle, productWeight)
        {
            InvNumber = invNumber;
            VenicleRegNumber = venicleRegNumber;
        }

        public abstract Invoice RequestInvoiceInfo(Invoice inInv);
    }
}
