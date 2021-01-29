using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Temperature.Data;
namespace TemperatureControl.UI
{
    class Program
    {
        static void Main(string[] args)
        {

            InsertIntoDatabase();
        }
        private static void InsertIntoDatabase()  // tar all data från cvs fil Namn och lägger den i databasen.
                                                                // FilNamn är cvs filen därifrån vi hämtar all data.
            {
            var temperatures = ProcessFile(@"C:\Users\Dilan\Pictures\ProjectTemperature\ControlingTemperature\Temperature.Data\TemperatureData.csv");
            var db = new TemperatureContext();
            db.Database.EnsureCreated();
            if (!db.AllTemperatureData.Any())
            {
                foreach (var temp in temperatures)
                {
                    db.AllTemperatureData.Add(temp);
                }
            }
            db.SaveChanges();
            




           
            QueryDataOutside(); 
            QueryDataInside();
        }
        private static void QueryDataOutside() // datum ute
        {
            var db = new TemperatureContext();

            Console.WriteLine("Medeltemperatur för valt datum Utomhus: Ange ett datum från 2016 :"); // läser in data och väljer sedan vilket datum för att få fram temp på konsolen
            DateTime DT = DateTime.Parse(Console.ReadLine());
            var query1 = db.AllTemperatureData.Where(d => d.DateTime.Date == DT.Date && d.Place == "Ute") // Linq metod datum, och plats 
                                              .GroupBy(d => d.DateTime.Date) // sorterar datum
                                              .Select(t => new { Date = t.Key, Average = t.Average(h => h.Temperature) }).ToList(); // average genom knappval

            if (query1.Count != 0)
            {
                foreach (var item in query1) 
                {
                    Console.WriteLine($"Datum : {item.Date.ToShortDateString()} Medeltemperatur : {item.Average}"); 

                }
            }
            else if (query1.Count == 0)
            {
                Console.WriteLine("Ingen temperatur hittades det datumet."); // felmeddelande
            }



            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine($"Sortering av den varmaste till kallaste dagen enligt genomsnittstemperaturen per dag: Utomhus:"); 


            var query2 = db.AllTemperatureData.Where(p => p.Place == "Ute")
                                               .GroupBy(d => d.DateTime.Date)  // sorterar efter datum och tid
                                               .Select(t => new { Date = t.Key, Average = t.Average(h => h.Temperature) }) // väljer datum o kollar average
                                               .OrderByDescending(t => t.Average).ToList(); 

            
            foreach (var temp in query2.Take(10))
            {
                Console.WriteLine($" Datum : {temp.Date.ToShortDateString()},  Medeltemperatur : {temp.Average}"); // Sorterar torraste till regnigaste dagen genom att läsa in från databasen
            }
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine($"Sortera den torraste till den regnigaste dagen efter genomsnittlig luftfuktighet per dag: Utomhus:");

            var query3 = db.AllTemperatureData.Where(p => p.Place == "Ute") 
                                               .GroupBy(d => d.DateTime.Date)
                                               .Select(t => new { Date = t.Key, Average = t.Average(h => h.Humidity) })
                                               .OrderBy(t => t.Average).ToList();

           
            foreach (var temp in query3.Take(10))
            {
                Console.WriteLine($" Datum : {temp.Date.ToShortDateString()},  Genomsnittlig luftfuktighet : {temp.Average}");
            }

            // sortering av mögel genom databas
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Sortering av den minsta till största risken för mögel: Utomhus:");
            var query4 = db.AllTemperatureData.Where(p => p.Place == "Ute")
                                               .GroupBy(d => d.DateTime.Date)
                                               .Select(t => new { Date = t.Key, AvereTemp = t.Average(h => h.Temperature), AvereHumid = t.Average(h => h.Humidity) })
                                               .ToList();

            var query67 = query4
                                 .Select(r => new { r.Date, RM = RiskOfMold(r.AvereHumid, r.AvereTemp) })
                                 .OrderByDescending(m => m.RM)
                                 .ThenByDescending(d => d.Date)
                                 .ToList();
            foreach (var item in query67.Take(10))
            {
                Console.WriteLine($" Datum : {item.Date.ToShortDateString()},  Risk för Mögel: Utomhus: : {item.RM}");

            }
            // Ska räkna ut meteoroligisk höst
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Datum för meteorologisk höst:");
            DateTime AutumStartDate = new DateTime(2016, 09, 01);
            DateTime AutumEndDate = new DateTime(2016, 11, 30);

            var query5 = db.AllTemperatureData.Any(d => d.DateTime.Date == AutumStartDate.Date);
            var query6 = db.AllTemperatureData.Any(d => d.DateTime.Date == AutumEndDate.Date);
            if (query5 == true && query6 == true)
            {
                Console.WriteLine($"Höstens start datum: {AutumStartDate}  Höstens slut datum:{AutumEndDate}");
            }
            else if (query5 == false && query6 == false)
            {
                for (int i = 1; i <= 90; i++)
                {
                    AutumStartDate = AutumStartDate.Date.AddDays(i);
                    var query7 = db.AllTemperatureData.Where(d => d.DateTime.Date == AutumStartDate.Date)
                                                    .FirstOrDefault();
                    AutumEndDate = AutumEndDate.Date.AddDays(-i);
                    var query12 = db.AllTemperatureData.Where(d => d.DateTime.Date == AutumEndDate.Date)
                                                      .FirstOrDefault();


                    if (query7 is not null && query12 != null)
                    {

                        Console.WriteLine($"Höstens start datum: {AutumStartDate.ToShortDateString()} , Höstens slut datum:{AutumEndDate.ToShortDateString()}");
                        break;
                    }
                    AutumStartDate = new DateTime(2016, 09, 01);
                    AutumEndDate = new DateTime(2016, 11, 30);
                }

            }
            else if (query5 == false && query6 == true)
            {

                for (int i = 1; i <= 90; i++)
                {
                    AutumStartDate = AutumStartDate.Date.AddDays(i);
                    var query7 = db.AllTemperatureData.Where(d => d.DateTime.Date == AutumStartDate.Date)
                                                       .FirstOrDefault();
                    if (query7 is not null)
                    {

                        Console.WriteLine($"Höstens start datum: {AutumStartDate.ToShortDateString()} , Höstens slut datum:{AutumEndDate.ToShortDateString()}");
                        break;
                    }
                    AutumStartDate = new DateTime(2016, 09, 01);
                }


            }
            else if (query5 == true && query6 == false)
            {

                for (int i = 1; i <= 90; i++)
                {
                    AutumEndDate = AutumEndDate.Date.AddDays(-i);
                    var query7 = db.AllTemperatureData.Where(d => d.DateTime.Date == AutumEndDate.Date)
                                                       .FirstOrDefault();
                    if (query7 is not null)
                    {

                        Console.WriteLine($"Höstens start datum: {AutumStartDate.ToShortDateString()} , Höstens slut datum:{AutumEndDate.ToShortDateString()}");
                        break;
                    }
                    AutumEndDate = new DateTime(2016, 11, 30);
                }

            }
            // räkna ut meteroligisk vinter
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Datum för meteorologisk vinter :");
            DateTime WinterStartDate = new DateTime(2016, 12, 21);  
            DateTime WinterEndDate = new DateTime(2017, 03, 20); 

            var query8 = db.AllTemperatureData.Any(d => d.DateTime.Date == WinterStartDate.Date);
            var query9 = db.AllTemperatureData.Any(d => d.DateTime.Date == WinterEndDate.Date);
            if (query8 == true && query9 == true)
            {
                Console.WriteLine($"Vinterns start datum: {WinterStartDate.ToShortDateString()} , Vinterns slut datum:{WinterEndDate.ToShortDateString()}");

            }
            else if (query8 == false && query9 == false)
            {
                for (int i = 1; i <= 90; i++)
                {
                    WinterStartDate = WinterStartDate.Date.AddDays(i);
                    var query10 = db.AllTemperatureData.Where(d => d.DateTime.Date == WinterStartDate.Date)
                                                       .FirstOrDefault();
                    WinterEndDate = WinterEndDate.Date.AddDays(-i);
                    var query11 = db.AllTemperatureData.Where(d => d.DateTime.Date == WinterEndDate.Date)
                                                      .FirstOrDefault();

                    if (query10 != null && query11 != null)
                    {

                        Console.WriteLine($"Vinterns start datum: {WinterStartDate.ToShortDateString()} , Vinter slut datum:{WinterEndDate.ToShortDateString()}");
                        break;
                    }
                    WinterStartDate = new DateTime(2016, 12, 21);
                    WinterEndDate = new DateTime(2017, 03, 20);
                }

            }
            else if (query8 == false && query9 == true)
            {

                for (int i = 1; i <= 90; i++)
                {
                    WinterStartDate = WinterStartDate.Date.AddDays(i);
                    var query10 = db.AllTemperatureData.Where(d => d.DateTime.Date == WinterStartDate.Date)
                                                       .FirstOrDefault();
                    if (query10 is not null)
                    {

                        Console.WriteLine($"Vinterns start datum: {WinterStartDate.ToShortDateString()} , Vinterns slut datum:{WinterEndDate.ToShortDateString()}");
                        break;
                    }
                    WinterStartDate = new DateTime(2016, 12, 21);
                }


            }
            else if (query8 == true && query9 == false)
            {

                for (int i = 1; i <= 90; i++)
                {
                    WinterEndDate = WinterEndDate.Date.AddDays(-i);
                    var query10 = db.AllTemperatureData.Where(d => d.DateTime.Date == WinterEndDate.Date)
                                                       .FirstOrDefault();
                    if (query10 is not null)
                    {

                        Console.WriteLine($"Vinterns start datum: {WinterStartDate.ToShortDateString()} , Vinterns slut datum:{WinterEndDate.ToShortDateString()}");
                        break;
                    }
                    WinterEndDate = new DateTime(2017, 03, 20);
                }

            }

        }

        private static double RiskOfMold(double h, double t)
        {


            return ((h - 78) * (t / 15)) / 0.22;


        }



        private static void QueryDataInside()
        {
            //Datum ute
            var db = new TemperatureContext();
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Medeltemperatur för valt datum: Inomhus: Ange ett datum från 2016 :");
            DateTime DT = DateTime.Parse(Console.ReadLine());
            var query1 = db.AllTemperatureData.Where(d => d.DateTime.Date == DT.Date && d.Place == "Inne")
                                              .GroupBy(d => d.DateTime.Date)
                                              .Select(t => new { Date = t.Key, Average = t.Average(h => h.Temperature) }).ToList();


            if (query1.Count != 0)
            {
                foreach (var item in query1)
                {
                    Console.WriteLine($"Datum : {item.Date.ToShortDateString()} Medeltemperatur : {item.Average}");

                }
            }
            else if (query1.Count == 0)
            {
                Console.WriteLine("Ingen temperatur hittades det datumet."); // felmeddelande
            }

            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine($"Sortering av den varmaste till kallaste dagen enligt genomsnittstemperaturen per dag: Inomhus:");

            //sortering
            var query2 = db.AllTemperatureData.Where(p => p.Place == "Inne")
                                               .GroupBy(d => d.DateTime.Date)
                                               .Select(t => new { Date = t.Key, Average = t.Average(h => h.Temperature) })
                                               .OrderByDescending(t => t.Average).ToList();

            
            foreach (var temp in query2.Take(10))
            {
                Console.WriteLine($" Datum : {temp.Date.ToShortDateString()},  Medeltemperatur : {temp.Average}");
            }

            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine($"Sortera den torraste till den regnigaste dagen efter genomsnittlig luftfuktighet per dag: Inomhus:");

            var query3 = db.AllTemperatureData.Where(p => p.Place == "Inne")
                                               .GroupBy(d => d.DateTime.Date)
                                               .Select(t => new { Date = t.Key, Average = t.Average(h => h.Humidity) })
                                               .OrderBy(t => t.Average).ToList();

            
            foreach (var temp in query3.Take(10))
            {
                Console.WriteLine($" Datum : {temp.Date.ToShortDateString()}, Genomsnittlig luftfuktighet: {temp.Average}");
            }

            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Sortering av den minsta till största risken för mögel: Inomhus::");
            var query4 = db.AllTemperatureData.Where(p => p.Place == "Inne")
                                               .GroupBy(d => d.DateTime.Date)
                                               .Select(t => new { Date = t.Key, AveTemp = t.Average(h => h.Temperature), AveHum = t.Average(h => h.Humidity) })
                                               .ToList();

            var query67 = query4
                                 .Select(r => new { r.Date, RM = RiskOfMold(r.AveHum, r.AveTemp) })
                                 .OrderByDescending(m => m.RM)
                                 .ThenByDescending(d => d.Date)
                                 .ToList();
            foreach (var item in query67.Take(10))
            {
                Console.WriteLine($" Datum : {item.Date.ToShortDateString()},  Risk för mögel Inomhus: : {item.RM}");

            }

        }
   
        private static List<TemperatureList> ProcessFile(string path) 
        {
            return
            File.ReadAllLines(path)
                 .Where(l => l.Length > 1)
                 .Select(TemperatureList.ParseFromCSV)
                 .ToList(); 
        }
    }
}
