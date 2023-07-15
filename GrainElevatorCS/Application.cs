using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GrainElevatorCS
{
    public class Application
    {
        public static void Execute()
        {
            Factory factory = new();

            // тестовое заполнение пустого Склада 

            if(factory.depot.Count == 0)
            factory.Init(factory.inInvDB, factory.labDB, factory); 

            bool exit = true;
            while (exit)
            {
                switch (Menu.ChoiceOperation())
                {
                    case 0: // загрузка последней сохраненной версии Склада из файла
                        Console.Clear();
                        factory?.Load();
                        break;

                    case 1: //Приемка, доработка и передача на склад входящей партии продукции

                        // создание Реестра
                        Register reg = new();

                        while (true)
                        {
                            Console.Clear();
                            //создание приходной накладной
                            InputInvoice inInv = new();
                            inInv.RequestInvoiceInfo(inInv); // запрос пользовательской информации по приемке

                            if (reg.prodBatches?.Count != 0 && (reg.ProductTitle != inInv.ProductTitle || reg.Date != inInv.Date))
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine($"\nДата поступления или Наименование входящей продукции не соответствуют данному Реестру.\n" +
                                   $"Можете ввести их в новый Реестр после закрытия текущего.\n");
                                Console.ForegroundColor = ConsoleColor.Green;
                            }
                            else
                            {
                                Console.Clear();
                                Console.WriteLine($"\nСоздана и добавлена в базу данных\n{inInv}\n");
                                factory?.inInvDB?.items?.Add(inInv);

                             //создание карточки анализа
                                LabCard lc = new(inInv);
                                lc.RequestLabInfo(lc); // запрос пользовательской информации по лабораторному анализу

                                Console.Clear();
                                Console.WriteLine($"\nСоздана и добавлена в базу данных\nЛабораторная карточка анализа:\n{lc}\n");
                                factory?.labDB.items?.Add(lc);

                            // создание производственной партии
                                ProductionBatch pb = new(lc);
                                pb.RequestBaseQuilityInfo(pb); // запрос пользовательской информации по базовим показателям качества для данного типа продукции
                                pb.CalcResultProduction();     // рассчет результатов доработки продукции

                                Console.Clear();
                                Console.WriteLine("\nПроизводственная партия сформирована и доработана до базовых показателей качества:\n\n");
                                pb.PrintProductionBatch();

                            //добавление Производственной партии в Реестр
                            
                                reg.AddToRegister(pb);
                                Console.WriteLine($"\nВходящие данные и результаты доработки внесены в Реестр" +
                                                  $" Продукции:  {reg.ProductTitle}  за  {reg.Date}\n\n");
                           
                            }
                            Console.WriteLine($"Для продолжения работы введите:\n" +
                                              $"                                  0 - Продолжить внесение информации в Реестр по данной Дате и Наименованию.\n" +
                                              $"                                  1 - Закрыть Реестр.\n");

                            string? stop = Console.ReadLine();
                            if (stop == "0")
                                continue;
                            else
                            {
                                Console.Clear();
                                reg.PrintReg();
                                // добавление Реестра на Склад
                                factory?.PushToDepot(reg);

                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"\nРеестр Продукции:{reg.ProductTitle} за {reg.Date} добавлен на Склад.\n");
                                Console.ForegroundColor = ConsoleColor.Green;
                                break;
                            }
                        }
                                           
                        break;
                        
                    case 2: // Печать всех Реестров по Наименованию
                        Console.Clear();
                        Console.Write($"Введите Наименование продукции: ");
                        string? titlePrint = Console.ReadLine();
                       
                        factory?.PrintByTitle(titlePrint!);
                        break;

                    case 3: // Печать итоговой количественной информации по Наименованию на текущий момент
                        Console.Clear();
                        Console.Write($"Введите Наименование продукции: ");
                        string? titleInfo = Console.ReadLine();
                        
                        factory?.PrintInfoByTitle(titleInfo!);
                        break;

                    case 4: // Отгрузка со склада категорий продукции по Наименованию

                        Console.Clear();
                        if (factory?.depot.Count() != 0)
                        {
                            OutputInvoice outInv = new(); // создание расходной накладной
                            outInv.RequestInvoiceInfo(outInv); // запрос информации по отгрузке
                            Console.Clear();

                            bool flag = false;
                            factory?.ShipByTitle(outInv, out flag);

                            if (flag)
                            {
                                Console.WriteLine($"\nСоздана и добавлена в базу данных\n{outInv}");
                                factory?.outInvDB?.items?.Add(outInv);
                            }
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("Склад пуст");
                            Console.ForegroundColor = ConsoleColor.Green;
                        }
                        break;

                    case 5: // Удаление всей информации по Наименованию
                        Console.Clear();
                        Console.Write($"Введите Наименование продукции: ");
                        string? titleDel = Console.ReadLine();

                        factory?.RemoveByTitle(titleDel!);
                        break;

                    case 6: // Печать всех Реестров поступивших на Склад
                        Console.Clear();
                        factory?.PrintAllReg();
                        break;

                    case 7: // Печать итоговой количественной информации по всем Наименованиям на текущий момент
                        Console.Clear();
                        factory?.PrintAllInfo();
                        break;

                    case 8: // Очистка склада
                        Console.Clear();
                        factory?.ClearDepot();
                        break;

                    case 9: // загрузка последнних сохраненних версий Баз данных первичных документов из файлов
                        Console.Clear();
                        if (factory?.LoadDB(factory.labDB!) != null)
                            factory.labDB = (DataBase<LabCard>)factory.LoadDB(factory.labDB);

                        if (factory?.LoadDB(factory.inInvDB!) != null)
                            factory.inInvDB = (DataBase<InputInvoice>)factory.LoadDB(factory.inInvDB);

                        if (factory?.LoadDB(factory.outInvDB!) != null)
                            factory.outInvDB = (DataBase<OutputInvoice>)factory.LoadDB(factory.outInvDB);


                        // factory?.LoadDB(factory.outInvDB);

                        break;

                    case 10:
                        Console.Clear();
                        factory?.ShowDB();
                        break;

                    case 11: // Сохранение информацию по Базам данных первичных документов в файл
                        Console.Clear();
                        if(factory?.labDB != null)
                            factory?.SaveDB(factory.labDB);
                        if(factory?.inInvDB != null)
                            factory?.SaveDB(factory.inInvDB);
                        if (factory?.outInvDB != null)
                            factory?.SaveDB(factory.outInvDB);
                        break;

                    case 12: // Сохранение состояния Склада в файл
                        Console.Clear();
                        factory?.SaveReg();
                        factory?.SaveInfo();
                        break;

                    case 13: //Завершение работы
                        Console.Clear();
                        exit = false;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Работа завершена");
                        Console.SetCursorPosition(0, 25);
                        break;
                }
            }
        }
    }
}
