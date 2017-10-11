using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Models;

namespace UI.Models
{
    public class MainWindowModel
    {
        public string Log { get; set; }
        public List<ModelInfo> SavedModels { get; set; }
        public Process SelectedProcess { get; set; }
        public IntPtr WindowHandle { get; set; }
    }
}
