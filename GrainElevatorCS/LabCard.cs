using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

// Лабораторная карточка (карточка анализа).
// =========================================
// Содержит первичную информацию о партии Продукции из товарно-транспортной накладной,
// результаты анализа ее качественных показателей.

namespace GrainElevatorCS
{
    public class LabCard : InputInvoice
    {
        public double Weediness { get; set; }
        public double Moisture { get; set; }

        public LabCard() { }
        public LabCard(string date,  string title, int weight, string numInvoice, string vehicleRegNum, double weediness, double moisture) :
            base (date, title, weight, numInvoice, vehicleRegNum)
        {
            Weediness = weediness;
            Moisture = moisture;
        }

        public LabCard(in InputInvoice inv) 
        {
            Date = inv.Date;
            ProductTitle = inv.ProductTitle;
            ProductWeight = inv.ProductWeight;
            InvNumber = inv.InvNumber;
            VenicleRegNumber = inv.VenicleRegNumber;
        }

        public LabCard RequestLabInfo(LabCard lc)
        {
            Console.WriteLine("\tВведите результаты лабораторного анализа входящей Продукции: \n");            
            while (true)
            {
                try
                {
                    NumberFormatInfo numberFormatInfo = new NumberFormatInfo() // установка типа разделителя "." в числах с плавающей запятой
                    {
                        NumberDecimalSeparator = ".",
                    };

                    Console.Write("Сорная Примесь (%):    ");
                    lc.Weediness = double.Parse(Console.ReadLine(), numberFormatInfo);
                    
                    Console.Write("Влажность (%):         ");
                    lc.Moisture = double.Parse(Console.ReadLine(), numberFormatInfo);

                    if (lc.Weediness < 0 || lc.Weediness > 100 || lc.Moisture < 0 || lc.Moisture > 100) // провеока полученних данних на диапазон 0-100%
                        throw new Exception();

                    return lc;
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка ввода. Введите корректние значения данних в диапазоне от 0 до 100");
                }
            }
        }
        public override string ToString()
        {
            return $"Дата прихода:    {Date}\n" +
                   $"Наименование:    {ProductTitle}\n" +
                   $"Вес нетто:       {ProductWeight} кг\n" +
                   $"Номер накладной: №{InvNumber}\n" +
                   $"Номер ТС:        {VenicleRegNumber}\n\n" +
                   $"Сорная примесь:  {Weediness} %\n" +
                   $"Влажность:       {Moisture} %\n";
        }
    };    
}
