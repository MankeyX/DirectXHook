using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting.Channels.Ipc;
using Core.Models;
using Hook;
using UI.Windows;

namespace UI.Models
{
    public class MainWindowModel
    {
        public string Log { get; set; }
        public List<ModelInfo> SavedModels { get; set; }
        public Process SelectedProcess { get; set; }
        public IpcServerChannel Server { get; set; } // Communications
        public ServerInterface ServerInterface { get; set; } // Communications
        public IntPtr WindowHandle { get; set; }
        public SaveModelWindow SaveModelWindow { get; set; }
    }
}
