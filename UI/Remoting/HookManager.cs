using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows;
using Core.Models;
using EasyHook;
using Hook;

namespace UI.Remoting
{
    public class HookManager
    {
        public ServerInterface ServerInterface { get; }

        public HookManager()
        {
            ServerInterface = new ServerInterface();
        }

        public void Inject(int processId)
        {
            try
            {
                string channelName = null;

                RemoteHooking.IpcCreateServer(ref channelName, WellKnownObjectMode.Singleton, ServerInterface);

                var injectionLibrary = Path.Combine(Directory.GetCurrentDirectory(), "Hook.dll");
                
                RemoteHooking.Inject(
                    processId,
                    injectionLibrary,
                    injectionLibrary,
                    channelName
                    );

                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void ReloadModels(IEnumerable<ModelInfo> models)
        {
            ServerInterface.ReloadModels(models.Where(x => x.Enabled).ToList());
        }
    }
}
