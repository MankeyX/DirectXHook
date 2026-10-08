using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using Core.Ipc;
using Core.Models;

namespace UI.Remoting
{
    public delegate void MessageRecievedEvent(string message);
    public delegate void HookInitializedEvent();
    public delegate void SaveModelRequestRecievedEvent(ModelInfo model);

    /// <summary>
    /// Injects the hook into a process and talks to it over a named pipe.
    /// </summary>
    public partial class HookManager
    {
        private MessageChannel _channel;

        public event MessageRecievedEvent MessageRecieved;
        public event HookInitializedEvent HookStarted;
        public event SaveModelRequestRecievedEvent SaveModelRequestRecieved;

        public void Inject(int processId)
        {
            try
            {
                var channelName = $"DirectXHook_{Guid.NewGuid():N}";
                var pipe = new NamedPipeServerStream(channelName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                _ = ListenAsync(pipe);

                var injectionLibrary = Path.Combine(AppContext.BaseDirectory, "Hook.dll");
                var channelNameBytes = MemoryMarshal.AsBytes(channelName.AsSpan()).ToArray();

                var status = RhInjectLibrary(
                    processId,
                    0,
                    InjectDefault,
                    injectionLibrary,
                    injectionLibrary,
                    channelNameBytes,
                    channelNameBytes.Length
                    );

                if (status < 0)
                {
                    pipe.Dispose();
                    throw new InvalidOperationException($"Injection failed with status 0x{status:X8}: {Marshal.PtrToStringUni(RtlGetLastErrorString())}");
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private async Task ListenAsync(NamedPipeServerStream pipe)
        {
            try
            {
                await pipe.WaitForConnectionAsync();
            }
            catch (Exception e) when (e is ObjectDisposedException || e is IOException || e is OperationCanceledException)
            {
                // Injection failed and the pipe was closed.
                return;
            }

            _channel = new MessageChannel(pipe);
            _channel.MessageReceived += OnMessageReceived;

            try
            {
                await Task.Run(_channel.ReadMessages);
            }
            catch (IOException)
            {
                // The pipe broke
            }

            MessageRecieved?.Invoke("The hook has disconnected.");
        }

        private void OnMessageReceived(PipeMessage message)
        {
            switch (message.Type)
            {
                case PipeMessageType.Log:
                    MessageRecieved?.Invoke(message.Text);
                    break;
                case PipeMessageType.HookStarted:
                    HookStarted?.Invoke();
                    break;
                case PipeMessageType.SaveModelRequest:
                    SaveModelRequestRecieved?.Invoke(message.Model);
                    break;
            }
        }

        public void ReloadModels(IEnumerable<ModelInfo> models)
        {
            try
            {
                _channel?.Send(new PipeMessage
                {
                    Type = PipeMessageType.ReloadModels,
                    Models = models.Where(x => x.Enabled).ToList()
                });
            }
            catch (IOException)
            {
                // The hook is gone, nothing to reload.
            }
        }

        private const int InjectDefault = 0;

        [LibraryImport("EasyHook64.dll", StringMarshalling = StringMarshalling.Utf16)]
        private static partial int RhInjectLibrary(int targetPid, int wakeUpTid, int injectionOptions,
            string libraryPathX86, string libraryPathX64, byte[] passThruBuffer, int passThruSize);

        [LibraryImport("EasyHook64.dll")]
        private static partial IntPtr RtlGetLastErrorString();
    }
}
