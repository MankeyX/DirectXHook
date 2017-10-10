using System;
using System.Collections.Generic;
using Core.Models;

namespace Hook
{
    public class ServerInterfaceProxy : MarshalByRefObject
    {
        public event SaveFileLocationRecievedEvent SaveFileLocationChanged;
        public event OnModelsReloadedEvent OnModelsReloaded;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void ChangeSaveFileLocation(string filename)
        {
            SaveFileLocationChanged?.Invoke(filename);
        }

        public void ReloadModels(List<ModelInfo> models)
        {
            OnModelsReloaded?.Invoke(models);
        }
    }
}
