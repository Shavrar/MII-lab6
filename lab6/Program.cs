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
        //checks if array contain other array
        public static bool EnumerableContainsWithOrder<T>(T[] origin,T[] array)
        {
            var last = 0;
            foreach (var a in array)
            {
                var check = true;
                for (var i = last; i < origin.Length; i++)
                {
                    if (origin[i].Equals(a))
                    {
                        check = false;
                        last = i + 1;
                        break;
                    }
                }
                if (check)
                {
                    return false;
                }
            }
            return true;
        }
        //Check if dictionary of lists contains certain list
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

        //Check if list has equels values to equels in second list
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

        //Check if list of lists contains certain list
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

        //Returns all unique combinations of values of list 
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

        public static List<int[]> GetFinale(List<int[]> data, List<int[]> result, int[] current, int count)
        {
            if (current == null)
            {
                for (var i = 1; i <= count; i++)
                {
                    var cur = new int[] {i};
                    result.Add(cur);
                    result = GetFinale(data, result, cur, count);
                }
            }
            else
            {
                for (int i = 1; i <= count; i++)
                {
                    var cur = current.Concat(new[] {i}).ToArray();
                    double check = 0;
                    foreach (var array in data)
                    {
                        if (EnumerableContainsWithOrder(array, cur))
                        {
                            check++;
                        }
                    }

                    if (check/data.Count>=0.4)
                    {
                        result.Add(cur);
                        result = GetFinale(data, result, cur, count);
                        break;
                    }
                }
            }
            return result;
        }

        public static List<int[]> DeleteEquelities(List<int[]> result)
        {
            foreach (var array in result)
            {
                foreach (var ar in result)
                {
                    if (ar!=array && EnumerableContainsWithOrder(ar,array))
                    {
                        var temp = new List<int[]>(result);
                        temp.Remove(array);
                        return DeleteEquelities(temp);
                    }
                }
            }
            return result;
        }

        static void Main(string[] args)
        {
            var records = LoadRecords();// geting all records

            var recordsSorted = records.GroupBy(a => a.Id).Select(g => g.ToList().OrderBy(a => a.Time)); // devided them by user id and sorted by date

            var allInstancesStorage = new List<List<int>>();// list for all combinations

            //filling this shit with data
            foreach (var group in recordsSorted)
            {
                foreach (var record in group)
                {
                    allInstancesStorage = GetAllInstances(record.ItemId, allInstancesStorage, null);
                }
            }
            var instancesStorage = new Dictionary<int, List<int>>();//this is where we store valid combinations(support >= 0.4)

            var numberOfGroups = (double)recordsSorted.Count();//users count

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
                    instancesStorage.Add(instancesStorage.Count+1,instance);
                }
            }

            Console.WriteLine("------------------------------------------------");
            foreach (var item in instancesStorage)
            {
                Console.Write(item.Key+" |");
                item.Value.ForEach(a => Console.Write(a + " "));
                Console.WriteLine();
            }
            Console.WriteLine("------------------------------------------------");
            var newRecordsStore = new Dictionary<int, List<List<int>>>();// for changed list of records

            foreach (var group in recordsSorted)
            {
                foreach (var record in group)
                {
                   var temp = new List<int>();
                    foreach (var instance in instancesStorage)
                    {
                        if (record.ContainItems(instance.Value))
                        {
                            temp.Add(instance.Key);
                        }
                    }
                    if (temp.Any())
                    {
                        if (newRecordsStore.ContainsKey(record.Id))
                        {
                            newRecordsStore[record.Id].Add(temp);
                        }
                        else
                        {
                            newRecordsStore.Add(record.Id,new List<List<int>> {temp});
                        }
                    }
                }
            }
            foreach (var item in newRecordsStore)
            {
                Console.Write("User "+item.Key + " :");
                foreach (var list in item.Value)
                {
                    Console.Write("{");
                    list.ForEach(a => Console.Write(a+","));
                    Console.Write("}");
                }
                Console.WriteLine();
            }
            Console.WriteLine("------------------------------------------------");
            var data = new List<int[]>();
            foreach (var item in newRecordsStore)
            {
                var list = new List<int>();
                foreach (var a in item.Value)
                {
                    list.AddRange(a);
                }
                data.Add(list.ToArray());
            }
            
            var result = new List<int[]>();

            result = GetFinale(data, result, null, instancesStorage.Count);

            result = DeleteEquelities(result);

            foreach (var array in result)
            {
                Console.Write("<");
                foreach (var item in array)
                {
                    Console.Write(item + " ");
                }
                Console.Write(">");
                Console.WriteLine();
            }

            Console.ReadKey();
        }
        //new records input
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
        //shows records
        public static void Show(List<Record> records)
        {
            foreach (var record in records)
            {
                Console.Write($"Id:{record.Id} \t Date:{record.Time.Date.ToString("yyyy MMMM dd")}\tItems: ");
                record.ItemId.ForEach(item => Console.Write($"{item}, "));
                Console.WriteLine();
            }
        }
        //loads records or initialize creation of new records
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