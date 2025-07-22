using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ProjetFinale.Utils
{
    public class ExportService : IExportService
    {
        public void ExportJson<T>(IEnumerable<T> data, string filePath)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public void ExportXml<T>(IEnumerable<T> data, string filePath)
        {
            var serializer = new XmlSerializer(typeof(List<T>));
            using var stream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(stream, data.ToList());
        }

        public void ExportCsv<T>(IEnumerable<T> data, string filePath)
        {
            var props = typeof(T).GetProperties();
            using var writer = new StreamWriter(filePath);
            writer.WriteLine(string.Join(",", props.Select(p => p.Name)));

            foreach (var item in data)
            {
                writer.WriteLine(string.Join(",", props.Select(p => p.GetValue(item)?.ToString())));
            }
        }

        public void ExportTxt<T>(IEnumerable<T> data, string filePath)
        {
            using var writer = new StreamWriter(filePath);
            foreach (var item in data)
            {
                writer.WriteLine(item.ToString());
            }
        }


        public void ExportJson<T>(T data, string filePath)
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        public void ExportXml<T>(T data, string filePath)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var stream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(stream, data);
        }

    }
}
