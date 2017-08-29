using System;
using System.Collections.Generic;
using Hook.D3D11;

namespace Hook
{
    public delegate void MessageRecievedEvent(string message, byte[] color);
    public delegate void SaveFileLocationRecievedEvent(string filename);
    public delegate void HookInitializedEvent();
    public delegate void SaveModelRequestRecievedEvent(ModelParameters model);
    public delegate void OnModelsReloadedEvent(List<ModelParameters> models);

    public class ServerInterface : MarshalByRefObject
    {
        public event MessageRecievedEvent MessageRecieved;
        public event SaveFileLocationRecievedEvent SaveFileLocationRecieved;
        public event HookInitializedEvent HookStarted;
        public event SaveModelRequestRecievedEvent SaveModelRequestRecieved;
        public event OnModelsReloadedEvent OnModelsReloaded;
        
        public void Message(string message, byte[] color)
        {
            MessageRecieved?.Invoke(message, color);
        }

        public void ChangeSaveFileLocation(string filename)
        {
            SaveFileLocationRecieved?.Invoke(filename);
        }

        public void NotifyHookStarted()
        {
            HookStarted?.Invoke();
        }

        public void ReloadModels(List<ModelParameters> models)
        {
            OnModelsReloaded?.Invoke(models);
        }

        public void RequestSaveModel(ModelParameters model)
        {
            SaveModelRequestRecieved?.Invoke(model);
        }
        
        public void Ping()
        {

        }
    }
}
