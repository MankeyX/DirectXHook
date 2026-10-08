using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Core.Interop;
using Core.Models;
using Hook.D3D11.Extensions;
using Hook.Infrastructure;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Hook.D3D11
{
    public class D3D11Hook : IDirectXDeviceHook
    {
        private readonly ServerInterface _server;

        private Shaders _shaders;
        private ID3D11Device _device;
        private DrawIndexedHook _drawIndexedHook;

        private readonly object _lock = new object();

        public D3D11Hook(ServerInterface server)
        {
            _server = server;
            _server.OnModelsReloaded += OnModelsReloaded;
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
                ID3D11DeviceContext deviceContext = null;

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
                    while (!presentHook.Initialized)
                        Thread.Sleep(10);
                }

                lock (_lock)
                {
                    _shaders = new Shaders(_device, _server);
                }
                
                using (_drawIndexedHook = new DrawIndexedHook(
                           deviceContext,
                           _device.CreateDepthStencilState(),
                           _device.CreateBlendState(),
                           _shaders,
                           _server
                        ))
                {
                    _server.Message($"DeviceContext: {deviceContext == null}");
                    _server.Message($"Device: {deviceContext?.Device == null}");
                    _server.NotifyHookStarted();
                    _server.Message("Ready for input...");

                    while (true)
                    {
                        if (_drawIndexedHook.IsLoggerEnabled)
                        {
                            if (Input.GetKey(VirtualKey.NumPad2))
                                _drawIndexedHook.ChangeModel(-1);
                            else if (Input.GetKey(VirtualKey.NumPad5))
                                _drawIndexedHook.ChangeModel(1);

                            if (Input.GetKey(VirtualKey.NumPad1))
                                _drawIndexedHook.ChangeSelectedByteWidth(-1);
                            else if (Input.GetKey(VirtualKey.NumPad3))
                                _drawIndexedHook.ChangeSelectedByteWidth(1);

                            Thread.Sleep(100);
                        }

                        if (Input.GetKey(VirtualKey.End))
                        {
                            _server.Message($"Logger enabled: {_drawIndexedHook.ToggleLogger()}");
                            Thread.Sleep(1000);
                        }
                        if (Input.GetKey(VirtualKey.Insert))
                        {
                            var selectedModel = _drawIndexedHook.GetSelectedModel();

                            if (selectedModel != null)
                            {
                                _server.RequestSaveModel(selectedModel);
                                Thread.Sleep(1000);
                            }
                        }

                        // Only render saved models - Immediate
                        _drawIndexedHook.RenderSavedModelsOnly(Input.GetKey(VirtualKey.NumPad0));

                        _server.Ping();
                        Thread.Sleep(10);
                    }
                }
            }
            catch (IOException)
            {
                // Host application has exited
            }
            catch (Exception e)
            {
                _server.Message(e.ToString());
            }
        }

        private static bool InitializeSwapChain(out IDXGISwapChain swapChain)
        {
            var window = NativeWindow.Create();

            var swapChainDescription = new SwapChainDescription
            {
                BufferCount = 1,
                Flags = SwapChainFlags.None,
                Windowed = true,
                BufferDescription = new ModeDescription(100, 100, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                OutputWindow = window,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                BufferUsage = Usage.RenderTargetOutput
            };

            Vortice.Direct3D11.D3D11.D3D11CreateDeviceAndSwapChain(
                null,
                DriverType.Hardware,
                DeviceCreationFlags.BgraSupport,
                null,
                swapChainDescription,
                out swapChain,
                out var device,
                out _,
                out var deviceContext
            );

            NativeWindow.Destroy(window);

            if (swapChain == null)
                return false;

            // We don't need or want these
            deviceContext?.Dispose();
            device?.Dispose();
            return true;
        }
    }
}
