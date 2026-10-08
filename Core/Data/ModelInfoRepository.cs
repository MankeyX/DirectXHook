using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Core.Models;
using Core.Serialization;

namespace Core.Data
{
    public class ModelInfoRepository : IModelInfoRepository
    {
        private readonly string _filePath;

        public ModelInfoRepository(string filename)
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
        }

        public List<ModelInfo> Get()
        {
            if (!File.Exists(_filePath))
                return new List<ModelInfo>();

            using (var fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                try
                {
                    var models = JsonSerializer.Deserialize(fileStream, CoreJsonContext.Default.ListModelInfo);
                    return models?.ToList() ?? new List<ModelInfo>();
                }
                catch
                {
                    return new List<ModelInfo>();
                }
            }
        }

        public void Save(List<ModelInfo> models)
        {
            using (var fileStream = new FileStream(_filePath, FileMode.Create))
            {
                JsonSerializer.Serialize(fileStream, models, CoreJsonContext.Default.ListModelInfo);
            }
        }
    }
}
