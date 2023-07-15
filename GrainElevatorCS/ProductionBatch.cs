using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

// Производственная партия.
// ========================
// содержит первичную информацию о партии Продукции (Приходная накладная),
// информацию лаборатории: входящие качественные показатели (Карточка анализа),
// запрашивает значения качественных показателей, которые необходимо достигнуть в процессе доработки (Производство),
// рассчитывает результаты доработки (изменение качественно-количественных показателей)


namespace GrainElevatorCS
{
    public class ProductionBatch : LabCard
    { 
        public double WeedinessBase { get; set; } = 0;
        public double MoistureBase { get; set; } = 0;

        public int Waste { get; set; } = 0;
        public int Shrinkage { get; set; } = 0;
        public int AccWeight { get; set; } = 0;

        public ProductionBatch() { }
        public ProductionBatch(string date, string title, int weight, string numInvoice, string vehicleRegNum, double weediness, double moisture, double weedinessBase, double moistureBase):
            base(date, title, weight, numInvoice, vehicleRegNum, weediness, moisture)
        {
            WeedinessBase = weedinessBase;
            MoistureBase = moistureBase;
        }

        public ProductionBatch(LabCard lc)
        {
            Date = lc.Date;
            ProductTitle = lc.ProductTitle;
            ProductWeight = lc.ProductWeight;
            InvNumber = lc.InvNumber;
            VenicleRegNumber = lc.VenicleRegNumber;
            Weediness = lc.Weediness;
            Moisture = lc.Moisture;         
        }

        public ProductionBatch RequestBaseQuilityInfo(ProductionBatch pb)
        {
            Console.WriteLine("\tВведите базовие показатели качества для входящей Продукции: \n");
            while (true)
            {
                try
                {
                    Console.Write("Базовая Сорная примесь (%):    ");
                    pb.WeedinessBase = Convert.ToInt32(Console.ReadLine());

                    Console.Write("Базовая Влажность (%):         ");
                    pb.MoistureBase = Convert.ToInt32(Console.ReadLine());

                    if (pb.Weediness < 0 || pb.WeedinessBase > 100 || pb.MoistureBase < 0 || pb.MoistureBase > 100) // провеока полученних данних на диапазон 0-100%
                        throw new Exception();

                    return pb;
                }
                catch (Exception)
                {
                    Console.WriteLine("Ошибка ввода. Введите корректние значения данних в диапазоне от 0 до 100");
                }
            }
        }

        public void CalcResultProduction()
        {
            if (Weediness <= WeedinessBase)
                Waste = 0;
            else
                Waste = (int)(ProductWeight * (1 - (100 - Weediness) / (100 - WeedinessBase)));

            if (Moisture <= MoistureBase)
                Shrinkage = 0;
            else
                Shrinkage = (int)((ProductWeight - Waste) * (1 - (100 - Moisture) / (100 - MoistureBase)));

            AccWeight = ProductWeight - Waste - Shrinkage;
        }

        public override string ToString()
        {
            return $"Дата прихода:      {Date}\n" +
                   $"Наименование:      {ProductTitle}\n" +
                   $"Вес нетто:         {ProductWeight} кг\n" +
                   $"Номер накладной:   №{InvNumber}\n\n" +
                   
                   $"Сорная примесь:    {Weediness} %\n" +
                   $"Влажность:         {Moisture} %\n\n" +

                   $"Базовая сорность:  {WeedinessBase} %\n" +
                   $"Базовая Влажность: {MoistureBase} %\n\n" +

                   $"Усушка:            {Shrinkage} кг\n" +
                   $"Сорная убыль:      {Waste} кг\n" +
                   $"Зачетный вес:      {AccWeight} кг\n";
        }
        public void PrintProductionBatch()
        {
            if (this != null)
            {
                Console.WriteLine(new string('-', 12 + 10 + 15 + 10 + 10 + 10 + 10 + 15 + 9));
                Console.WriteLine("|{0,12}|{1,10}|{2,15}|{3,10}|{4,10}|{5,10}|{6,10}|{7,15}|", Date, InvNumber, ProductWeight, Moisture, Shrinkage, Weediness, Waste, AccWeight);
                Console.WriteLine(new string('-', 12 + 10 + 15 + 10 + 10 + 10 + 10 + 15 + 9));

            }
        }
    }
}
