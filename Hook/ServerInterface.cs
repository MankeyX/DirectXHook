using System;
using System.Collections.Generic;
using Core.Models;

namespace Hook
{
    public delegate void MessageRecievedEvent(string message);
    public delegate void SaveFileLocationRecievedEvent(string filename);
    public delegate void HookInitializedEvent();
    public delegate void SaveModelRequestRecievedEvent(ModelInfo model);
    public delegate void OnModelsReloadedEvent(List<ModelInfo> models);

    public class ServerInterface : MarshalByRefObject
    {
        public event MessageRecievedEvent MessageRecieved;
        public event SaveFileLocationRecievedEvent SaveFileLocationRecieved;
        public event HookInitializedEvent HookStarted;
        public event SaveModelRequestRecievedEvent SaveModelRequestRecieved;
        public event OnModelsReloadedEvent OnModelsReloaded;
        
        public void Message(string message)
        {
            MessageRecieved?.Invoke(message);
        }

        public void ChangeSaveFileLocation(string filename)
        {
            SaveFileLocationRecieved?.Invoke(filename);
        }

        public void NotifyHookStarted()
        {
            HookStarted?.Invoke();
        }

        public void ReloadModels(List<ModelInfo> models)
        {
            OnModelsReloaded?.Invoke(models);
        }

        public void RequestSaveModel(ModelInfo model)
        {
            SaveModelRequestRecieved?.Invoke(model);
        }
        
        public void Ping()
        {
            // This will fail when the UI has been closed.
        }
    }
}
