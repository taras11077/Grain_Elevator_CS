using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// Завод.
// ========================		
// Включает в себя:		
//		все суточные Реестры входящей продукции отсортированные по Наименованию,
//		итоговое количество Наименования продукции по категориям: Кондиционная продукция, Отход, Усушка,
//		возможность отгрузки Наименования продукции по категориям,
//		информацию об остатках Наименования продукции по категориям,
//		сохранение состояния Завода(Реестры входящей продукции, Лабораторные карточки, приходные и расходные накладные, остатки по Складу).
//		загрузка сохраненной версии состояния Завода.


namespace GrainElevatorCS
{
    public class Factory
    {
        public Dictionary<string, DepotItem> depot;

        public DataBase<InputInvoice> inInvDB;
        public DataBase<LabCard> labDB;
        public DataBase<OutputInvoice> outInvDB;

        public Factory()
        {
            depot = new Dictionary<string, DepotItem>();
            inInvDB = new DataBase<InputInvoice>("InputInvoice");
            labDB = new DataBase<LabCard>("LabCard");
            outInvDB = new DataBase<OutputInvoice>("OutputInvoice");
        }

        // добавление Продукции на Склад
        public void PushToDepot(Register register)
        {
            if (depot != null && register.ProductTitle != null)
            {
                if (depot.ContainsKey(register.ProductTitle)) // добавление Реестра в существующий список
                    depot[register.ProductTitle].AddRegister(register);
                else
                    depot.Add(register.ProductTitle, new DepotItem(register)); // добавление реестра в новый список
            }
        }
        
        // Печать всех Реестров по Наименованию
        public void PrintByTitle(string productTitle)
        {
            if (depot != null && depot.ContainsKey(productTitle))
            {
                Console.WriteLine($"\nПеречень Реестров по Наименованию {productTitle}:");

                foreach (var depotItem in depot)
                {
                    if (depotItem.Key == productTitle)
                    {
                        foreach (var register in depotItem.Value.registers!)
                            register.PrintReg();
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Продукции с наименованием {productTitle} на складе не обнаружено");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        
        // Печать итоговой количественной информации по Наименованию на текущий момент
        public void PrintInfoByTitle(string productTitle)
        {
            if (depot != null && depot.ContainsKey(productTitle))
            {
                foreach (var depotItem in depot)
                {
                    if (depotItem.Key == productTitle)
                        Console.WriteLine($"Итого по Наименованию: {productTitle}\n" +
                                          $"Зачетный вес:   {depotItem.Value.AccWeightItem} кг\n" +
                                          $"Отход:          {depotItem.Value.WasteItem} кг\n" +
                                          $"Усушка:         {depotItem.Value.ShrinkageItem} кг\n\n");
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Продукции с наименованием {productTitle} на складе не обнаружено");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        // Отгрузка со склада Категорий продукции по Наименованию
        public delegate void ShipCat(OutputInvoice outInv, out bool flag);
        ShipCat shipCat;
        public void ShipByTitle(OutputInvoice outInv, out bool flag)
        {
            if (outInv.ProductTitle != null && depot.ContainsKey(outInv.ProductTitle))
            {
                // !!! организовать динамическое исполнение методов из делегата не получилось. поєтому Свич

                //shipCat.GetInvocationList()[outInv.category].DynamicInvoke();
                //shipCat.DynamicInvoke(new object[0]);

                switch (outInv.category)
                {
                    case 0:
                        shipCat = ShipAccWeight;
                        break;
                    case 1:
                        shipCat = ShipWaste;
                        break;

                    default:
                        break;
                }
                shipCat.Invoke(outInv, out flag);             

            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Продукции с Наименованием {outInv.ProductTitle} на складе не обнаружено");
                Console.ForegroundColor = ConsoleColor.Green;
                flag = false;
            }
        }
        public void ShipAccWeight(OutputInvoice outInv, out bool flag)
        {
            DepotItem? depotItemVal = new();
            depot.TryGetValue(outInv.ProductTitle!, out depotItemVal);

            if (outInv.ProductWeight <= depotItemVal?.AccWeightItem)
            {
                depotItemVal.AccWeightItem -= outInv.ProductWeight;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nОтгружено Кондиционной продукции: {outInv.ProductWeight} кг\n" +
                                  $"Остаток на Складе:                {depotItemVal?.AccWeightItem} кг\n");
                Console.ForegroundColor = ConsoleColor.Green;

                //Console.WriteLine(outInv);

                flag = true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОтгрузка данного количества Кондиционной продукции невозможна.\n" +
                                    $"Остаток на Складе:               {depotItemVal?.AccWeightItem} кг\n" +
                                    $"Введите корректное значение.\n\n");
                Console.ForegroundColor = ConsoleColor.Green;
                flag = false;
            }
        }
        public void ShipWaste(OutputInvoice outInv, out bool flag)
        {
            DepotItem? depotItemVal = new();
            depot.TryGetValue(outInv.ProductTitle!, out depotItemVal);

            if (outInv.ProductWeight <= depotItemVal?.WasteItem)
            {
                depotItemVal.WasteItem -= outInv.ProductWeight;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\nОтгружено Отходов:       {outInv.ProductWeight} кг\n" +
                                  $"Остаток на Складе:      {depotItemVal?.WasteItem} кг\n");
                Console.ForegroundColor = ConsoleColor.Green;

                //Console.WriteLine(outInv);
                flag = true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nОтгрузка данного количества Отходов невозможна.\n" +
                                    $"Остаток на Складе:               {depotItemVal?.WasteItem} кг\n" +
                                    $"Введите корректное значение.\n\n");
                Console.ForegroundColor = ConsoleColor.Green;
                flag = false;
            }
        }
        
        // Удаление всей информации по Наименованию
        public void RemoveByTitle(string productTitle)
        {
            if (depot != null && depot.ContainsKey(productTitle))
            {
                depot.Remove(productTitle);

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Перечень Реестров по Наименованию {productTitle} удален");
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Продукции с наименованием {productTitle} на складе не обнаружено");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        
        // Печать всех Реестров поступивших на Склад
        public void PrintAllReg()
        {
            if (depot != null && depot.Count != 0)
                foreach (var depotItem in depot)
                {
                    Console.WriteLine($"Перечень реестров по Наименованию: {depotItem.Key}\n");

                    foreach (var register in depotItem.Value.registers!)
                        register.PrintReg();

                    Console.WriteLine("\n\n");
                }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Склад пуст");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        
        // Печать итоговой количественной информации по всем Наименованиям на текущий момент
        public void PrintAllInfo()
        {
            if (depot != null && depot.Count != 0)
                foreach (var depotItem in depot)
                {
                    Console.WriteLine($"Наименование продукции: {depotItem.Key}\n");                       
                    Console.WriteLine($"ИТОГО:\n" +
                                      $"Зачетный вес: {depotItem.Value.AccWeightItem} кг\n" +
                                      $"Отход:        {depotItem.Value.WasteItem} кг\n\n");
                }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Склад пуст");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }
        
        // очистка Склада
        public void ClearDepot()
        {
            depot?.Clear();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Информация по складу удалена");
            Console.ForegroundColor = ConsoleColor.Green;
        }



        //Сохранение в файл всех Реестров
        public void SaveReg()
        {
            //Склад(Depot) содержит коллекцию Складских единиц(DepotItem), которые содержат коллекцию Реестров(Register),
            //которые содержат коллекцию Производственных партий(ProductionBatches).

            //В файл сохраняется список(List) Производственных партий одного Реестра.(В цикле по всем DepotItem и по всему Складу(Depot)).
            //Имя файла каждого Реестра добавляется в список fileRegList, которный сохраняется в отдельный файл.

            if (depot != null && depot.Count != 0)
            {
                List<string> fileRegList = new List<string>();

                foreach (var depotItem in depot) // перебор DepotItem в Складе
                {
                    Console.WriteLine($"Перечень реестров по Наименованию: {depotItem.Key}");
                    int i = 0;
                    foreach (var register in depotItem.Value.registers!)// перебор Реестров в DepotItem
                    {
                        using FileStream fs1 = new FileStream($"{register.ProductTitle}{i}.json", FileMode.Create);
                        JsonSerializer.Serialize(fs1, register.prodBatches!); // сохранение списка(List) Производственних партий одного Реестра в файл
                        fileRegList.Add($"{register.ProductTitle}{i}.json"); // добавление имени файла в список файлов
                        i++;
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Сохранено\n");
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                using FileStream fs2 = new FileStream($"fileRegList.json", FileMode.Create);// сохранение в отдельний файл списка файлов
                JsonSerializer.Serialize(fs2, fileRegList);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Склад пуст");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        //Сохранение в файл количественной информации по Складской единице(DepotItem)
        public void SaveInfo()
        {
            if (depot != null && depot.Count != 0)
            {
                List<string> fileInfoList = new List<string>(); // создание списка создаваемых для каждого DepotItem файлов 

                foreach (var depotItem in depot)
                {
                   using FileStream fs1 = new FileStream($"{depotItem.Key}.json", FileMode.Create); // запись в отдельный файл каждого DepotItem Склада
                    JsonSerializer.Serialize<DepotItem>(fs1, depotItem.Value);

                    fileInfoList.Add($"{depotItem.Key}.json"); // имя файла добавлено в список файлов

                    Console.WriteLine($"Количество Продукции по Наименованию: {depotItem.Key}");
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Сохранено\n");
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                using FileStream fs2 = new FileStream($"fileInfoList.json", FileMode.Create); // создание файла для хранения списка созданных файлов DepotItem
                JsonSerializer.Serialize(fs2, fileInfoList);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Склад пуст");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        // Сохранение в файл Базы данных первичной документации(приходные, расходние накладные, лабораторные карточки анализа)
        public void SaveDB(IDataBase idb)
        {
            if (idb != null) 
            {
                if (idb is DataBase<LabCard>)  // вибор типа Бази данних(передается через параметр)
                {
                    List<LabCard>? items = (idb as DataBase<LabCard>)?.items;

                    using FileStream fs = new FileStream($"{idb.TitleDB}.json", FileMode.Create);
                    JsonSerializer.Serialize(fs, items);
                }
                else if (idb is DataBase<InputInvoice>)
                {
                    List<InputInvoice>? items = (idb as DataBase<InputInvoice>)?.items;

                    using FileStream fs = new FileStream($"{idb.TitleDB}.json", FileMode.Create);
                    JsonSerializer.Serialize(fs, items);
                }
                else if (idb is DataBase<OutputInvoice>)
                {
                    List<OutputInvoice>? items = (idb as DataBase<OutputInvoice>)?.items;

                    using FileStream fs = new FileStream($"{idb.TitleDB}.json", FileMode.Create);
                    JsonSerializer.Serialize(fs, items);
                }
                            
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"База данных {idb.TitleDB}DB - Сохранено\n");
                Console.ForegroundColor = ConsoleColor.Green;
                
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Нет данных для сохранения");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        public void ShowDB()
        {
            int choice;
            while (true)
            {
                Console.Clear();
                Console.WriteLine($"\n  Выберите вид первичного документа:\n" +
                                  $"                                      0 -  Приходная накладная\n" +
                                  $"                                      1 -  Лабораторная карточка анализа\n" +
                                  $"                                      2 -  Расходная накладная\n");
                try
                {
                    choice = Convert.ToInt32(Console.ReadLine());
                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Введите цифровое значение из диапазона Меню");
                }
            }
            
            switch(choice)
            {
                case 0:
                    if(inInvDB?.items?.Count > 0)
                        inInvDB.Show();
                    else
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Данная База данных не содержит документов");
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    break;

                 case 1:
                    if (labDB?.items?.Count > 0)
                        labDB.Show();
                    else
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Данная База данных не содержит документов");
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    break;

                case 2:
                    if (outInvDB?.items?.Count > 0)
                        outInvDB.Show();
                    else
                    {
                        Console.Clear();
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Данная База данных не содержит документов");
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    break;
            }
        }

        // Выгрузка Склада из файла
        public void Load()
        {
            //Создается новий пустой склад.
            //В цикле, из файла загружаються список (List) Производственных Партий(ProductionBatch) каждого конкретного Реестра(Register).
            //Создается новий Реестр и в него добавляются загруженные Производственные Партии.
            //Новый Реестр добаляется в новый Склад(Depot), группируясь по Наименованию(Title) в Складские Единицы(DepotItem)
            //Суммарные количественные показатели по Категориям продукции(DepotItem.AccWeightItem и DepotItem.WasteItem)
            //      в штатном режиме рассчитываются по данным всех Реестров Складской единицы при добавлении Реестров в Склад.
            //      При Отгрузке со Склада они изменяются.
            //      Поєтому при архивировании их состояние сохраняется отдельно, и устанавливается для каждого DepotItem  
            //      после загрузки всех Реестров и формировании нового Склада.

            try
            {
                // Загрузка Реестров
                depot = new Dictionary<string, DepotItem>();        //создание пустого Склада
                List<string>? fileRegList = new List<string>();     //создание списка файлов Реестров

                using FileStream fs1 = new FileStream($"fileRegList.json", FileMode.Open);
                fileRegList = JsonSerializer.Deserialize<List<string>>(fs1); // загрузка списка файлов Реестров из файла

                List<ProductionBatch>? productionBatches = new List<ProductionBatch>(); // создание списка Производственных партий

                foreach (var file in fileRegList!) // цикл перебора файлов Реестров
                {
                    using FileStream fs2 = new FileStream(file, FileMode.Open);
                    productionBatches = JsonSerializer.Deserialize<List<ProductionBatch>>(fs2); // загрузка списка списка Производственных Партий из одного файла Реестра 

                    Register reg = new Register(productionBatches); // создание Реестра и заполнение его Производственными Партиями

                    if (depot != null && reg.ProductTitle != null) // добавление Реестра в Склад 
                    {
                        if (depot.ContainsKey(reg.ProductTitle)) // добавление Реестра в существующий DepotItem
                            depot[reg.ProductTitle].AddRegister(reg);
                        else
                            depot.Add(reg.ProductTitle, new DepotItem(reg)); // добавление реестра в новый DepotItem
                    }
                }

               // Загрузка Информации
                List<string>? fileInfoList = new List<string>();     //создание списка файлов DepotItem-сов
                using FileStream fs3 = new FileStream($"fileInfoList.json", FileMode.Open);
                fileInfoList = JsonSerializer.Deserialize<List<string>>(fs3, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true }); // загрузка списка файлов DepotItem-сов из файла

                foreach (var path in fileInfoList!) // цикл перебора файлов DepotItem-сов
                {
                    using FileStream fs4 = new FileStream(path, FileMode.Open);
                    DepotItem depIt = JsonSerializer.Deserialize<DepotItem>(fs4)!; // загрузка отдельного DepotItem из файла
                    
                    foreach (var depotItem in depot)
                    {
                        if (depotItem.Key == depIt.Title) // поиск в Складе DepotItem с соответствующим Наименованием
                        {
                            depotItem.Value.AccWeightItem = depIt.AccWeightItem;   // присвоение найденному DepotItem сохраненного значения Кондиционной продукции
                            depotItem.Value.WasteItem = depIt.WasteItem;           // присвоение найденному DepotItem сохраненного значения Отходов
                        }
                    }
                }
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Последняя сохраненная версия Склада загружена");
                    Console.ForegroundColor = ConsoleColor.Green;
            }

            catch (FileNotFoundException ex)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Информация по складу не загружена. Проверьте наличие сохраненних версий Склада.");
                Console.ForegroundColor = ConsoleColor.Green;
            }
        }

        public IDataBase? LoadDB(IDataBase idb) //по интерфейсной ссылке может передаваться База данных с любым типом документов
        {
            Console.ForegroundColor = ConsoleColor.Yellow;

            try
            {      // Тип переданной в параметре Базы данных определяет Тип создаваемого Списка документов 
                if (idb is DataBase<LabCard>)
                {
                    List<LabCard>? loadItems = new List<LabCard>();
                   
                    using FileStream fs = new FileStream($"{idb.TitleDB}.json", FileMode.Open);
                    loadItems = JsonSerializer.Deserialize<List<LabCard>>(fs); // загрузка списка Лабораторных карточек 

                    DataBase<LabCard> labDB = new DataBase<LabCard>("LabCard");
                    labDB.items = loadItems;
                    Console.WriteLine($"Последняя сохраненная версия Базы данных {idb.TitleDB}DB загружена");
                    Console.ForegroundColor = ConsoleColor.Green;

                    return labDB;
                }
                else if (idb is DataBase<InputInvoice>)
                {
                    List<InputInvoice>? loadItems = new List<InputInvoice>();

                    using FileStream fs = new FileStream($"{idb.TitleDB}.json", FileMode.Open);
                    loadItems = JsonSerializer.Deserialize<List<InputInvoice>>(fs); // загрузка Приходных накладных

                    DataBase<InputInvoice> inInvDB = new DataBase<InputInvoice>("LabCard");
                    inInvDB.items = loadItems;
                    Console.WriteLine($"Последняя сохраненная версия Базы данных {idb.TitleDB}DB загружена");
                    Console.ForegroundColor = ConsoleColor.Green;

                    return inInvDB;
                }
                else if (idb is DataBase<OutputInvoice>)
                {
                    List<OutputInvoice>? loadItems = new List<OutputInvoice>();

                    using FileStream fs = new FileStream($"{idb.TitleDB}.json", FileMode.Open);
                    loadItems = JsonSerializer.Deserialize<List<OutputInvoice>>(fs); // загрузка Расходных накладных

                    DataBase<OutputInvoice> outInvDB = new DataBase<OutputInvoice>("LabCard");
                    outInvDB.items = loadItems;
                    Console.WriteLine($"Последняя сохраненная версия Базы данных {idb.TitleDB}DB загружена");
                    Console.ForegroundColor = ConsoleColor.Green;

                    return outInvDB;
                }
                else
                { 
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"База данных {idb.TitleDB} не обнаружена") ;
                    Console.ForegroundColor = ConsoleColor.Green;
                    return null;
                }
            }

            catch (FileNotFoundException ex)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ex.Message);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Информация по Базе данных {idb.TitleDB}DB не загружена. Проверьте наличие сохраненних версий Баз данных.");
                Console.ForegroundColor = ConsoleColor.Green;
                return null;
            }
        }





        // первоначальное тестовое заполнение Склада
        public Factory Init(DataBase<InputInvoice> inInvDB, DataBase<LabCard> labDB, Factory factory)
        {
            InputInvoice invoice1 = new("01.02.2023", "Atlant", 25350, "205365", "AA1111BH");
            InputInvoice invoice2 = new("02.02.2023", "Halk", 35570, "356984", "AА2222BH");
            InputInvoice invoice3 = new("03.02.2023", "Tisa", 30750, "205365", "AA3333BH");
            InputInvoice invoice4 = new("04.02.2023", "Rigasol", 27250, "356256", "АА4444ВН");
            InputInvoice invoice5 = new("01.02.2023", "Atlant", 28380, "205366", "AA5555BH");
            InputInvoice invoice6 = new("02.02.2023", "Halk", 33570, "356985", "AА6666BH");
            InputInvoice invoice7 = new("03.02.2023", "Tisa", 28750, "205366", "AA7777BH");
            InputInvoice invoice8 = new("04.02.2023", "Rigasol", 25250, "356257", "АА8888ВН");

            inInvDB?.items?.Add(invoice1);
            inInvDB?.items?.Add(invoice2);
            inInvDB?.items?.Add(invoice3);
            inInvDB?.items?.Add(invoice4);
            inInvDB?.items?.Add(invoice5);
            inInvDB?.items?.Add(invoice6);
            inInvDB?.items?.Add(invoice7);
            inInvDB?.items?.Add(invoice8);

            LabCard labCard1 = new("01.02.2023", "Atlant", 25350, "205365", "AA1111BH", 25.2, 15.9);
            LabCard labCard2 = new("02.02.2023", "Halk", 35570, "356984", "AА2222BH", 20.2, 12.9);
            LabCard labCard3 = new("03.02.2023", "Tisa", 30750, "205365", "AA3333BH", 15.2, 10.9);
            LabCard labCard4 = new("04.02.2023", "Rigasol", 27250, "356256", "АА4444ВН", 17.9, 13.1);
            LabCard labCard5 = new("01.02.2023", "Atlant", 28380, "205366", "AA5555BH", 23.2, 13.9);
            LabCard labCard6 = new("02.02.2023", "Halk", 33570, "356985", "AА6666BH", 18.2, 10.9);
            LabCard labCard7 = new("03.02.2023", "Tisa", 28750, "205366", "AA7777BH", 13.2, 8.9);
            LabCard labCard8 = new("04.02.2023", "Rigasol", 23250, "356257", "АА8888ВН", 15.9, 11.1);

            labDB.items?.Add(labCard1);
            labDB.items?.Add(labCard2);
            labDB.items?.Add(labCard3);
            labDB.items?.Add(labCard4);
            labDB.items?.Add(labCard5);
            labDB.items?.Add(labCard6);
            labDB.items?.Add(labCard7);
            labDB.items?.Add(labCard8);

            ProductionBatch pb1 = new("01.02.2023", "Atlant", 25350, "205365", "AA1111BH", 25.2, 15.9, 8, 1);
            pb1.CalcResultProduction();

            ProductionBatch pb2 = new("02.02.2023", "Halk", 35570, "356984", "AА2222BH", 20.2, 12.9, 8, 1);
            pb2.CalcResultProduction();

            ProductionBatch pb3 = new("03.02.2023", "Tisa", 30750, "205365", "AA3333BH", 15.2, 10.9, 8, 1);
            pb3.CalcResultProduction();

            ProductionBatch pb4 = new("04.02.2023", "Rigasol", 27250, "356256", "АА4444ВН", 17.9, 13.1, 8, 1);
            pb4.CalcResultProduction();

            ProductionBatch pb5 = new("01.02.2023", "Atlant", 28380, "205366", "AA5555BH", 23.2, 13.9, 8, 1);
            pb5.CalcResultProduction();

            ProductionBatch pb6 = new("02.02.2023", "Halk", 33570, "356985", "AА6666BH", 18.2, 10.9, 8, 1);
            pb6.CalcResultProduction();

            ProductionBatch pb7 = new("03.02.2023", "Tisa", 28750, "205366", "AA7777BH", 13.2, 8.9, 8, 1);
            pb7.CalcResultProduction();

            ProductionBatch pb8 = new("04.02.2023", "Rigasol", 23250, "356257", "АА8888ВН", 15.9, 11.1, 8, 1);
            pb8.CalcResultProduction();

            Register reg1 = new();
            Register reg2 = new();
            Register reg3 = new();
            Register reg4 = new();

            reg1.AddToRegister(pb1);
            reg2.AddToRegister(pb2);
            reg3.AddToRegister(pb3);
            reg4.AddToRegister(pb4);
            reg1.AddToRegister(pb5);
            reg2.AddToRegister(pb6);
            reg3.AddToRegister(pb7);
            reg4.AddToRegister(pb8);

            factory?.PushToDepot(reg1);
            factory?.PushToDepot(reg2);
            factory?.PushToDepot(reg3);
            factory?.PushToDepot(reg4);

            return factory!;
        }
    }
}
