using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Hook.D3D11;
using Hook.Infrastructure;

namespace Hook
{
    public static unsafe class EntryPoint
    {
        /// <summary>
        /// Called by EasyHook in the target process after the library has been injected.
        /// The user data is the UTF-16 name of the pipe the UI is listening on.
        /// </summary>
        [UnmanagedCallersOnly(EntryPoint = "NativeInjectionEntryPoint", CallConvs = new[] { typeof(CallConvStdcall) })]
        public static void NativeInjectionEntryPoint(EasyHook.RemoteEntryInfo* remoteInfo)
        {
            // The user data is only valid during this call, so copy it before starting the hook thread.
            var channelName = new string((char*)remoteInfo->UserData, 0, remoteInfo->UserDataSize / sizeof(char));

            new Thread(() => Run(channelName)) { IsBackground = true, Name = "DirectXHook" }.Start();
        }

        private static void Run(string channelName)
        {
            // An exception escaping this thread would take down the target process.
            try
            {
                using (var server = new ServerInterface(channelName))
                {
                    var hook = new D3D11Hook(server);
                    hook.Hook();
                }
            }
            catch
            {
                // Nowhere to report this, the UI could not be reached.
            }
        }
    }
}
