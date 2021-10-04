using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Threading;
using System.Windows.Forms;
using Core.Interop;
using Core.Models;
using Hook.D3D11.Extensions;
using Hook.Infrastructure;
using SharpDX.Diagnostics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Device = SharpDX.Direct3D11.Device;

namespace Hook.D3D11
{
    public class D3D11Hook : IDirectXDeviceHook
    {
        private readonly ServerInterface _server;

        private Shaders _shaders;
        private Device _device;
        private DrawIndexedHook _drawIndexedHook;

        private readonly object _lock = new object();

        public D3D11Hook(ServerInterface server)
        {
            try
            {
                var serverProxy = new ServerInterfaceProxy();
                _server = server;
                _server.OnModelsReloaded += serverProxy.ReloadModels;
                serverProxy.OnModelsReloaded += OnModelsReloaded;
            }
            catch (Exception e)
            {
                _server.Message(e.ToString());
                throw;
            }
        }

        private void OnModelsReloaded(List<ModelInfo> models)
        {
            lock (_lock)
            {
                try
                {
                    _drawIndexedHook.SavedModels = models;
                    _drawIndexedHook.SavedModels.Sort();
                    _shaders.Generate(models.Select(x => x.Color));
                }
                catch (Exception ex)
                {
                    _server.Message($"Message: {ex.Message}, Stack Trace: {ex.StackTrace}, Exception: {ex}");
                }
            }
        }

        public void Hook()
        {
            try
            {
                DeviceContext deviceContext = null;

                if (!InitializeSwapChain(out var swapChain))
                    _server.Message("Failed to create swap chain");
                else
                    _server.Message("Swap chain created successfully!");

                _server.Message("Hooking Present()...");
                using (var presentHook = new PresentHook(swapChain))
                {
                    presentHook.OnInitialized += (eDevice, eDeviceContext) =>
                    {
                        _device = eDevice;
                        deviceContext = eDeviceContext;
                    };
                    while (!presentHook.Initialized) { }
                }

                lock (_lock)
                {
                    _shaders = new Shaders(_device, _server);
                }
                
                using (_drawIndexedHook = new DrawIndexedHook(deviceContext, _device.CreateDepthStencilState(), _shaders, _server))
                {
                    _server.Message($"DeviceContext: {deviceContext == null}");
                    _server.Message($"Device: {deviceContext.Device == null}");
                    _server.NotifyHookStarted();
                    _server.Message("Ready for input...");

                    while (true)
                    {
                        if (_drawIndexedHook.IsLoggerEnabled)
                        {
                            if (Input.GetKey(Keys.NumPad2))
                                _drawIndexedHook.ChangeModel(-1);
                            else if (Input.GetKey(Keys.NumPad5))
                                _drawIndexedHook.ChangeModel(1);

                            if (Input.GetKey(Keys.NumPad1))
                                _drawIndexedHook.ChangeSelectedByteWidth(-1);
                            else if (Input.GetKey(Keys.NumPad3))
                                _drawIndexedHook.ChangeSelectedByteWidth(1);

                            Thread.Sleep(100);
                        }

                        if (Input.GetKey(Keys.End))
                        {
                            _server.Message($"Logger enabled: {_drawIndexedHook.ToggleLogger()}");
                            Thread.Sleep(1000);
                        }
                        if (Input.GetKey(Keys.Insert))
                        {
                            var selectedModel = _drawIndexedHook.GetSelectedModel();

                            _server.RequestSaveModel(selectedModel);
                            Thread.Sleep(1000);
                        }

                        // Only render saved models - Immediate
                        _drawIndexedHook.RenderSavedModelsOnly(Input.GetKey(Keys.NumPad0));

                        _server.Ping();
                    }
                }
            }
            catch (RemotingException e)
            {
                // Host application has exited
                _server.Message(e.ToString());
            }
            catch (Exception e)
            {
                _server.Message(e.ToString());
            }
        }

        private static bool InitializeSwapChain(out SwapChain swapChain)
        {
            var renderForm = new RenderForm();

            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 1,
                Flags = SwapChainFlags.None,
                IsWindowed = true,
                ModeDescription = new ModeDescription(100, 100, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputHandle = renderForm.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport,
                swapChainDescription,
                out var device,
                out swapChain
            );

            if (swapChain == null)
                return false;

            renderForm.Dispose();
            device?.Dispose(); // We don't need or want this
            return true;
        }
    }
}
