using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Core.Models;

namespace Core.IO
{
    public class SaveLoad
    {
        public static void Save(string filename, ModelInfo model)
        {
            var savedModels = Load(filename);

            if (savedModels.Contains(model))
                return;

            savedModels.Add(model);

            Save(filename, savedModels);
        }

        public static void Save(string filename, List<ModelInfo> models)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(fileStream, models);
            }
        }

        // Should loadDisabled even be here? That should probably be in the view-model or whatever calls this method
        public static List<ModelInfo> Load(string filename)
        {
            if (!File.Exists(filename))
                return new List<ModelInfo>();

            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
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
    }
}
