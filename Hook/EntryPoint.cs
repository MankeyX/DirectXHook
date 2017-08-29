using System;
using System.Collections;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using EasyHook;
using Hook.D3D11;

namespace Hook
{
    public class EntryPoint : IEntryPoint
    {
        private readonly ServerInterface _server;

        public EntryPoint(RemoteHooking.IContext context, string channelName)
        {
            _server = RemoteHooking.IpcConnectClient<ServerInterface>(channelName);
            _server.Ping();

            IDictionary properties = new Hashtable
            {
                ["name"] = channelName,
                ["portName"] = channelName + Guid.NewGuid().ToString("N")
            };
            var binaryProvider = new BinaryServerFormatterSinkProvider
            {
                TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full
            };

            var clientServerChannel = new IpcServerChannel(properties, binaryProvider);
            ChannelServices.RegisterChannel(clientServerChannel, false);
        }

        public void Run(RemoteHooking.IContext context, string channelName)
        {
            var hook = new D3D11Hook(_server);
            hook.Hook();
        }
    }
}
