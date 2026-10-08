using System.Collections.Generic;
using Core.Models;

namespace Core.Ipc
{
    public enum PipeMessageType
    {
        /// <summary>Hook to UI: a line for the log.</summary>
        Log,
        /// <summary>Hook to UI: the hook is installed and ready for models.</summary>
        HookStarted,
        /// <summary>Hook to UI: the user asked to save the selected model.</summary>
        SaveModelRequest,
        /// <summary>UI to hook: the list of models to highlight.</summary>
        ReloadModels
    }

    public class PipeMessage
    {
        public PipeMessageType Type { get; set; }
        public string Text { get; set; }
        public ModelInfo Model { get; set; }
        public List<ModelInfo> Models { get; set; }
    }
}
