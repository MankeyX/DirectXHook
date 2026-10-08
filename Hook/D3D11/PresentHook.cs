using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Core.Memory;
using Hook.Infrastructure;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Hook.D3D11
{
    // TODO: Create a base class that wants T as a ComObject
    public unsafe class PresentHook : IDisposable
    {
        // The hook handler has to be static, so only one PresentHook can be active at a time.
        private static PresentHook _instance;

        private readonly FunctionHook _presentHook;
        private volatile bool _initialized;

        public bool Initialized
        {
            get => _initialized;
            set => _initialized = value;
        }

        public delegate void OnInitializedEvent(ID3D11Device device, ID3D11DeviceContext deviceContext);
        public event OnInitializedEvent OnInitialized;

        public PresentHook(IDXGISwapChain swapChain)
        {
            _instance = this;
            _presentHook = new FunctionHook(
                VirtualTableAddress.GetVtableAddress(swapChain.NativePointer, (int)VirtualTableIndices.DxgiSwapChain.Present),
                (IntPtr)(delegate* unmanaged[Stdcall]<IntPtr, uint, uint, int>)&OnPresent);

            // This is not the SwapChain we're looking for...
            swapChain.Dispose();

            _presentHook.Activate();
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvStdcall) })]
        private static int OnPresent(IntPtr swapChainPointer, uint syncInterval, uint flags)
        {
            var instance = _instance;

            if (!instance.Initialized)
            {
                try
                {
                    // The wrapper releases its reference when disposed, so take one for it.
                    Marshal.AddRef(swapChainPointer);
                    using (var swapChain = new IDXGISwapChain(swapChainPointer))
                    {
                        var device = swapChain.GetDevice<ID3D11Device>();
                        instance.OnInitialized?.Invoke(device, device.ImmediateContext);
                    }

                    instance.Initialized = true;
                }
                catch
                {
                    // Try again on the next frame rather than crashing the target.
                }
            }

            return CallOriginal(instance, swapChainPointer, syncInterval, flags);
        }

        private static int CallOriginal(PresentHook instance, IntPtr swapChainPointer, uint syncInterval, uint flags)
        {
            var original = (delegate* unmanaged[Stdcall]<IntPtr, uint, uint, int>)instance._presentHook.FunctionToHook;
            return original(swapChainPointer, syncInterval, flags);
        }

        public void Dispose()
        {
            _presentHook.Deactivate();
            _presentHook?.Dispose();
        }
    }
}
