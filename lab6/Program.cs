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

        public bool ContainRecordItems(Record record)
        {
            return record.ItemId.All(id => this.ItemId.Contains(id));
        }
        public bool ContainItems(List<int> items)
        {
            return items.All(id => this.ItemId.Contains(id));
        }
    }

   
    class Program
    {
        public static bool DictionaryContainsList<T>(Dictionary<int, List<T>> dictionary, List<T> value)
        {           
            foreach (var list in dictionary)
            {
                if (ListRawEquel(list.Value, value))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ListRawEquel<T>(List<T> list1, List<T> list2)
        {
            if (list1.Count != list2.Count) return false;
            foreach (var item in list1)
            {
                if (!list2.Contains(item))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool ListContainsList<T>(List<List<T>> lisy, List<T> value)
        {
            var result = false;
            foreach (var list in lisy)
            {
                result = true;
                result = ListRawEquel(list, value);
                if (result) break;
            }
            return result;
        }

        public static List<List<T>> GetAllInstances<T>(List<T> list, List<List<T>> result, List<T> currentNode)
        {
            if (currentNode == null)
            {
                foreach (var item in list)
                {
                    var node = new List<T> {item};
                    var temp = new List<T>(list);
                    temp.Remove(item);
                    if (!ListContainsList(result, node))
                    {
                        result.Add(node);
                    }
                    result = GetAllInstances(temp, result, node);
                }
            }
            else
            {
                foreach (var item in list)
                {
                    var node = new List<T>(currentNode);
                    node.Add(item);
                    var temp = new List<T>(list);
                    temp.Remove(item);
                    if (!ListContainsList(result, node))
                    {
                        result.Add(node);
                    }
                    result = GetAllInstances(temp, result, node);
                }
            }
            return result;
        }

        static void Main(string[] args)
        {
            var records = LoadRecords();

            var recordsSorted = records.GroupBy(a => a.Id).Select(g => g.ToList().OrderBy(a => a.Time));
            var allInstancesStorage = new List<List<int>>();
            foreach (var group in recordsSorted)
            {
                foreach (var record in group)
                {
                    allInstancesStorage = GetAllInstances(record.ItemId, allInstancesStorage, null);
                }
            }
            var instancesStorage = new Dictionary<int, List<int>>();
            var numberOfGroups = (double)recordsSorted.Count();
            foreach (var instance in allInstancesStorage)
            {
                int i = 0;
                foreach (var group in recordsSorted)
                {
                    foreach (var record in group)
                    {
                        if (record.ContainItems(instance))
                        {
                            i++;
                            break;
                        }
                    }
                }
                if (i/numberOfGroups >= 0.4)
                {
                    instancesStorage.Add(instancesStorage.Count,instance);
                }
            }


            instancesStorage.ToList().ForEach(a =>
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
            string path = @"D:\file.txt";
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