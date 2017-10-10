using System;
using System.Collections.Generic;
using Core.Models;

namespace Hook
{
    public class ServerInterfaceProxy : MarshalByRefObject
    {
        public event OnModelsReloadedEvent OnModelsReloaded;

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public void ReloadModels(List<ModelInfo> models)
        {
            OnModelsReloaded?.Invoke(models);
        }
    }
}
