using System;
using System.Runtime.InteropServices;
using Core.Memory;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Hook.D3D11
{
    // TODO: Create a base class that wants T as a CppObject
    public class PresentHook : IDisposable
    {
        private readonly FunctionHook<Present> _presentHook;

        public bool Initialized { get; set; }
        
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int Present(IntPtr swapChainPointer, int syncInterval, int flags);

        public delegate void OnInitializedEvent(Device device, DeviceContext deviceContext);
        public event OnInitializedEvent OnInitialized;

        public PresentHook(CppObject swapChain)
        {
            _presentHook = new FunctionHook<Present>(
                VirtualTableAddress.GetVtableAddress(swapChain.NativePointer, (int)VirtualTableIndices.DxgiSwapChain.Present),
                new Present(OnPresent),
                this);

            // This is not the SwapChain we're looking for...
            swapChain.Dispose();

            _presentHook.Activate();
        }

        private int OnPresent(IntPtr swapChainPointer, int syncInterval, int flags)
        {
            if (Initialized)
                return _presentHook.OriginalFunction(swapChainPointer, syncInterval, flags);
            
            var device = new SwapChain(swapChainPointer).GetDevice<Device>();
            OnInitialized?.Invoke(device, device.ImmediateContext);

            Initialized = true;

            return _presentHook.OriginalFunction(swapChainPointer, syncInterval, flags);
        }

        public void Dispose()
        {
            _presentHook.Deactivate();
            _presentHook?.Dispose();
        }
    }
}
