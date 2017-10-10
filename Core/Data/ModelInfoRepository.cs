using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Core.Models;

namespace Core.Data
{
    public class ModelInfoRepository : IModelInfoRepository
    {
        private const string Filename = "savedModels";
        private readonly string _filePath;

        public ModelInfoRepository()
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), Filename);
        }

        public List<ModelInfo> Get()
        {
            if (!File.Exists(_filePath))
                return new List<ModelInfo>();

            using (var fileStream = new FileStream(_filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var formatter = new BinaryFormatter();

                try
                {
                    var models = (List<ModelInfo>)formatter.Deserialize(fileStream);
                    return models.ToList();
                }
                catch
                {
                    return new List<ModelInfo>();
                }
            }
        }

        public void Save(ModelInfo modelInfo)
        {
            var savedModels = Get();

            if (savedModels.Contains(modelInfo))
                return;

            savedModels.Add(modelInfo);

            Save(savedModels);
        }

        public void Save(List<ModelInfo> models)
        {
            using (var fileStream = new FileStream(_filePath, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, models);
            }
        }
    }
}
