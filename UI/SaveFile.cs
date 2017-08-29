using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hook.D3D11;
using Newtonsoft.Json;

namespace Hook
{
    public class SaveFile
    {
        public static bool Save(string filename, ModelParameters model)
        {
            var savedModels = Load(filename, true);

            if (savedModels.Contains(model))
                return false;

            savedModels.Add(model);

            return Save(filename, savedModels);
        }

        public static bool Save(string filename, List<ModelParameters> models)
        {
            var json = JsonConvert.SerializeObject(models, Formatting.Indented);
            File.WriteAllText(filename, json);
            return true;
        }

        public static List<ModelParameters> Load(string filename, bool loadDisabled)
        {
            if (!File.Exists(filename))
                return new List<ModelParameters>();

            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var reader = new StreamReader(fileStream))
                {
                    var json = reader.ReadToEnd();

                    if (string.IsNullOrEmpty(json))
                        return new List<ModelParameters>();

                    var savedModels = JsonConvert.DeserializeObject<List<ModelParameters>>(json);

                    return loadDisabled 
                        ? savedModels.ToList() 
                        : savedModels.Where(x => x.Enabled).ToList();
                }
            }
        }
    }
}
