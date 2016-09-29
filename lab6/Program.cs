using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace lab6
{
    //some comment
    [Serializable]
    class Record
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public List<int> ItemId { get; set; }

        public Record(string[] temp)
        {
            var rnd = new Random();
            Id = int.Parse(temp[0]);
            Time = DateTime.Now.AddDays(rnd.Next(30)).AddHours(rnd.Next(24)).AddSeconds(rnd.Next(60)).AddMonths(rnd.Next(12));
            ItemId = temp[1].Split(',').Select(int.Parse).ToList();
        }

        public bool ContainItems(Record item)
        {
            return item.ItemId.All(id => this.ItemId.Contains(id));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var records = LoadRecords();

            var recordsSort = records.GroupBy(a => a.Id).Select(g => g.ToList().OrderBy(a => a.Time));
            var counter = new Dictionary<int, List<int>>();

            foreach (var item in recordsSort)
            {
                foreach (var record in item)
                {
                    int cot = 1;
                    if (!counter.ContainsValue(record.ItemId))
                    {
                        foreach (var t in recordsSort)
                        {
                            if (t.Any(b => b.ContainItems(record)))
                            {
                                cot++;
                            }
                            
                        }
                    }
                    if ((cot*1.0)/recordsSort.Count() >= 0.4)
                    {
                        counter.Add(counter.Count,record.ItemId);
                    }
                }
                
            }

            counter.ToList().ForEach(a =>
            {
                var q = "";
                a.Value.ForEach(f => q+=$"{f}, ");
                Console.WriteLine($"{a.Key} {q}");
            });


            Console.ReadKey();
        }

        public static List<Record> GetRecords()
        {
            var records = new List<Record>();
            for (var i = 0; i < 10; i++)
            {
                Console.WriteLine("{0}:", i + 1);
                string temp = Console.ReadLine();
                string[] t = temp.Split(' ');
                var record = new Record(t);
                records.Add(record);
            }
            return records;
        }

        public static void Show(List<Record> records)
        {
            foreach (var record in records)
            {
                Console.Write($"Id:{record.Id} \t Date:{record.Time.Date.ToString("yyyy MMMM dd")}\tItems: ");
                record.ItemId.ForEach(item => Console.Write($"{item}, "));
                Console.WriteLine();
            }
        }

        public static List<Record> LoadRecords()
        {
            var records = new List<Record>();
            string path = @"C:\temp\file.txt";
            FileInfo fileInf = new FileInfo(path);
            if (fileInf.Exists)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    records = (List<Record>)formatter.Deserialize(fs);

                    Console.WriteLine("Deserialise object");
                    Show(records);
                }
            }
            else
            {
                Console.WriteLine("No items, enter new:");
                records = GetRecords();
                BinaryFormatter formatter = new BinaryFormatter();

                using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
                {
                    formatter.Serialize(fs, records);

                    Console.WriteLine("Serialize object");
                }
            }
            return records;
        }
    }
}