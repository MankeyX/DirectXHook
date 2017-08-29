using System;
using System.Collections.Generic;
using Hook.D3D11;

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

        public void ReloadModels(List<ModelParameters> models)
        {
            OnModelsReloaded?.Invoke(models);
        }
    }
}
