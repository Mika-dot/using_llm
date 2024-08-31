using LLama.Common;
using LLM__simple.Model;
using Newtonsoft.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace LLM__simple.WorkingFiles
{
    public static class Files
    {
        /// <summary>
        /// Сохранение истории
        /// </summary>
        /// <param name="storList">История</param>
        public static void SaveToFile(List<Story> storList)
        {
            string json = JsonConvert.SerializeObject(storList, Formatting.Indented);

            File.WriteAllText(Singlet.Json, json);

            Console.WriteLine("Data saved to file.");
        }


        /// <summary>
        /// Чтение истории
        /// </summary>
        /// <returns>История формата List<Story></returns>
        public static List<Story> LoadFromFile()
        {
            string json = File.ReadAllText(Singlet.Json);

            List<Story> storList = JsonConvert.DeserializeObject<List<Story>>(json);
 
            return storList;
        }
    }
}
