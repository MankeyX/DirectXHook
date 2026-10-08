using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using Core.Ipc;
using Core.Models;

namespace Hook
{
    public delegate void OnModelsReloadedEvent(List<ModelInfo> models);

    /// <summary>
    /// The hook's connection to the UI, over the named pipe created by the UI.
    /// </summary>
    public class ServerInterface : IDisposable
    {
        private const int ConnectTimeoutMilliseconds = 10000;

        private readonly MessageChannel _channel;
        private volatile bool _isConnected = true;

        public event OnModelsReloadedEvent OnModelsReloaded;

        public ServerInterface(string pipeName)
        {
            var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut);
            pipe.Connect(ConnectTimeoutMilliseconds);

            _channel = new MessageChannel(pipe);
            _channel.MessageReceived += OnMessageReceived;

            new Thread(ReadMessages) { IsBackground = true, Name = "DirectXHook pipe reader" }.Start();
        }

        private void ReadMessages()
        {
            try
            {
                _channel.ReadMessages();
            }
            catch
            {
                // The pipe broke, which means the UI has been closed.
            }

            _isConnected = false;
        }

        private void OnMessageReceived(PipeMessage message)
        {
            if (message.Type == PipeMessageType.ReloadModels)
                OnModelsReloaded?.Invoke(message.Models ?? new List<ModelInfo>());
        }

        public void Message(string message)
        {
            Send(new PipeMessage { Type = PipeMessageType.Log, Text = message });
        }

        public void NotifyHookStarted()
        {
            Send(new PipeMessage { Type = PipeMessageType.HookStarted });
        }

        public void RequestSaveModel(ModelInfo model)
        {
            Send(new PipeMessage { Type = PipeMessageType.SaveModelRequest, Model = model });
        }

        /// <summary>
        /// Throws when the UI has been closed.
        /// </summary>
        public void Ping()
        {
            if (!_isConnected)
                throw new IOException("The UI has disconnected.");
        }

        private void Send(PipeMessage message)
        {
            try
            {
                _channel.Send(message);
            }
            catch (Exception e) when (e is IOException || e is ObjectDisposedException)
            {
                _isConnected = false;
            }
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
