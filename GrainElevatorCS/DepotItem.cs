using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

//Складская учетная единица.
//==========================
//Содержит информацию о всех Реестрах одного Наименования
//Суммарные количественные показатели Наименования: Зачетный вес, Отход, Усушка.

namespace GrainElevatorCS
{
    public class DepotItem
    {
        //[JsonIgnore]
        public List<Register>? registers;
        public string? Title { get; set; } = null;
        public int AccWeightItem { get; set; }  //общий Зачетный вес(AccWeight) 
        public int WasteItem { get; set; }      //общий Отход(Waste)			
        public int ShrinkageItem { get; set; }	//общая Усушка(Shrinkage)		

        public DepotItem(){}

        public DepotItem(Register register)
        {
            registers = new List<Register>() { register };

            Title = register.ProductTitle;
            AccWeightItem += register.AccWeightsReg;
            WasteItem += register.WastesReg;
            ShrinkageItem += register.ShrinkagesReg;
        }

        public DepotItem(string? title, int accWeightItem, int wasteItem, int shrinkageItem)
        {
            registers = new List<Register>();
            Title = title;
            AccWeightItem = accWeightItem;
            WasteItem = wasteItem;
            ShrinkageItem = shrinkageItem;

        }

        public void AddRegister(Register register)
        {
            if (registers != null)
            {
                registers.Add(register);

                AccWeightItem += register.AccWeightsReg;
                WasteItem += register.WastesReg;
                ShrinkageItem += register.ShrinkagesReg;
            }
        }
    }
}
