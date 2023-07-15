using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrainElevatorCS
{
    // Расходная накладная.
    // ===============================
    // Содержит информацию об отгружаемой продукции продукции.

    public class OutputInvoice : Invoice
    {
        public int category = 0;
        
        public OutputInvoice() { }

        public OutputInvoice(string date, string productTitle, int productWeight, string invNumber, string venicleRegNumber)
            : base(date, productTitle, productWeight, invNumber, venicleRegNumber)
        {}
                       
        public override Invoice RequestInvoiceInfo(Invoice outInv)
        {
            Console.WriteLine("Введите информацию для оформления расходной накладной на отгружаемую Продукцию:\n" + 
                              "--------------------------------------------------------------------------------\n");

            while (true)
            {
                try
                {
                    Console.Write("Дата отгрузки:                                       ");
                    outInv.Date = Console.ReadLine();

                    Console.Write("Номер расходной накладной:                           ");
                    outInv.InvNumber = Console.ReadLine();

                    Console.Write("Регистрационний номер транспортного средства:        ");
                    outInv.VenicleRegNumber = Console.ReadLine();

                    Console.Write("Наименование отгружаемой Продукции:                  ");
                    outInv.ProductTitle = Console.ReadLine();                             

                    Console.Write("Введите тип отгружаемой продукции: 0 - Кондиция\n" +
                                  "                                   1 - Отходы:       ");
                    category = Convert.ToInt32(Console.ReadLine());

                    Console.Write("Вес отгружаемой Продукции (кг):                      ");
                    outInv.ProductWeight = Convert.ToInt32(Console.ReadLine());

                    return outInv;
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка ввода. Введите корректные значения данных.");
                }
            }
        }

        public override string ToString()
        {
            return $"Расходная накладная №{InvNumber}.\n" +
                   $"---------------------------\n" +
                   $"Дата отгрузки:             {Date}\n" +
                   $"Наименование продукции:    {ProductTitle}\n" +
                   $"Категория(0-конд,1-отход): {category}\n" +
                   $"Вес нетто:                 {ProductWeight} кг\n\n" +
                   $"Номер ТС:                  {VenicleRegNumber}\n";
        }
    }
}
