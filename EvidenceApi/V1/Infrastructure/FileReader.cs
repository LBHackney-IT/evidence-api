using System.IO;
using Newtonsoft.Json;

namespace EvidenceApi.V1.Infrastructure
{
    public class FileReader<T> : IFileReader<T>
    {
        public FileReader(string path)
        {
            _data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        private T _data { get; set; }

        public T GetData()
        {
            return _data;
        }
    }
}
