using System.IO;
using EvidenceApi.V1.Infrastructure.Interfaces;
using Newtonsoft.Json;

namespace EvidenceApi.V1.Infrastructure
{
    public class FileReader<T> : IFileReader<T>
    {
        public FileReader(string path)
        {
            _data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        private readonly T _data;

        public T GetData()
        {
            return _data;
        }
    }
}
