using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

// Итоговый Реестр. 
// ================
//содержит информацию о принятых Производственных партиях одного Наименования продукции за одни сутки, с результатами доработки. 

namespace GrainElevatorCS
{
    public class Register
    {
        public string? Date { get; set; }
        public string? ProductTitle { get; set; }

        public List<ProductionBatch>? prodBatches;

        public int AccWeightsReg { get; set; } = 0;
        public int WastesReg { get; set; } = 0;
        public int ShrinkagesReg { get; set; } = 0;

        public Register()
        {
            prodBatches = new List<ProductionBatch>();
        }    

        public Register(List<ProductionBatch>? prodBatches)
        {
            if (prodBatches != null)
            {
                this.prodBatches = new List<ProductionBatch>(prodBatches);
                
                ProductTitle = prodBatches[0].ProductTitle;

                foreach(var pd in prodBatches)
                {
                    AccWeightsReg += pd.AccWeight;
                    WastesReg += pd.Waste;
                    ShrinkagesReg += pd.Shrinkage;
                }
            }
        }

        public void AddToRegister(ProductionBatch pb)
        {
            if (pb != null)//(ProductTitle == string.Empty || ProductTitle == pb?.ProductTitle && Date == pb?.Date)
            {
                Date = pb.Date;
                ProductTitle = pb.ProductTitle;
                prodBatches?.Add(pb);
                AccWeightsReg += pb.AccWeight;
                WastesReg += pb.Waste;
                ShrinkagesReg += pb.Shrinkage;
            }
        }

        public void PrintReg()
        {
            Console.WriteLine($"Реестр\nНаименование:      {ProductTitle}");
            Console.WriteLine(new string('=', 12 + 10 + 15 + 10 + 10 + 10 + 10 + 15 + 9));
            Console.WriteLine("|{0,8}|{1,10}|{2,15}|{3,10}|{4,10}|{5,10}|{6,10}|{7,15}|","Дата прихода","Номер ТТН", "Физический вес", "Влажность","Усушка","Сорность","Отход","Зачетный вес");
            Console.WriteLine(new string('=', 12 + 10 + 15 + 10 + 10 + 10 + 10 + 15 + 9));

            if (prodBatches != null)
            {
                foreach (var pb in prodBatches)
                    pb.PrintProductionBatch();
            }

            Console.WriteLine(new string('=', 12 + 10 + 15 + 10 + 10 + 10 + 10 + 15 + 9));
            Console.WriteLine("|{0,12}|{1,10}|{2,15}|{3,10}|{4,10}|{5,10}|{6,10}|{7,15}|", "Итого", " ", " ", " ", ShrinkagesReg, " ", WastesReg, AccWeightsReg);
            Console.WriteLine(new string('=', 12 + 10 + 15 + 10 + 10 + 10 + 10 + 15 + 9));
            Console.WriteLine("\n");
        }
    }
}
