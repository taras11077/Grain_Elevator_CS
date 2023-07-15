using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainElevatorCS
{
    // Приходная накладная.
    // ===============================
    // Содержит первичную информацию о входящей партии продукции.
    public class InputInvoice : Invoice
    {      
        public InputInvoice() { }
        public InputInvoice(string date, string productTitle, int productWeight, string invNumber, string venicleRegNumber)
            : base(date, productTitle, productWeight, invNumber, venicleRegNumber)
        { }
                
        public override InputInvoice RequestInvoiceInfo(Invoice inInv)
        {
            Console.WriteLine("\tВведите информацию для оформления приходной накладной на входящую Продукцию: \n");

            while (true)
            {
                try
                {
                    Console.Write("Дата поступления:                             ");
                    inInv.Date = Console.ReadLine();

                    Console.Write("Номер приходной накладной:                    ");
                    inInv.InvNumber = Console.ReadLine();

                    Console.Write("Регистрационний номер транспортного средства: ");
                    inInv.VenicleRegNumber = Console.ReadLine();

                    Console.Write("Наименование поступившей Продукции:           ");
                    inInv.ProductTitle = Console.ReadLine();

                    Console.Write("Физический вес поступившей Продукции (кг):    ");
                    inInv.ProductWeight = Convert.ToInt32(Console.ReadLine());

                    return (InputInvoice)inInv;
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка ввода. Введите корректные значения данных.");
                }
            }
        }

        public override string ToString()
        {
            return $"Приходная накладная №{InvNumber}\n" +
                   $"Дата прихода:    {Date}\n" +
                   $"Наименование:    {ProductTitle}\n" +
                   $"Вес нетто:       {ProductWeight} кг\n" +
                   $"Номер ТС:        {VenicleRegNumber}\n";
        }
    }
}
