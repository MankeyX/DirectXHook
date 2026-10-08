using System;
using System.Runtime.InteropServices;

namespace Hook.Infrastructure
{
    /// <summary>
    /// An EasyHook detour from <see cref="FunctionToHook"/> to a native function pointer.
    /// While a thread is inside the hook handler, calling <see cref="FunctionToHook"/> again
    /// goes straight to the original function (EasyHook's re-entrance barrier).
    /// </summary>
    public unsafe class FunctionHook : IDisposable
    {
        public IntPtr FunctionToHook { get; }
        public IntPtr NewFunction { get; }
        public bool IsActive { get; private set; }

        // Points to EasyHook's HOOK_TRACE_INFO, which must stay alive for the lifetime of the hook.
        private IntPtr _handle;

        public FunctionHook(IntPtr functionToHook, IntPtr newFunction)
        {
            FunctionToHook = functionToHook;
            NewFunction = newFunction;
        }

        ~FunctionHook()
        {
            Dispose(false);
        }

        private void CreateHook()
        {
            if (_handle != IntPtr.Zero)
                return;

            _handle = (IntPtr)NativeMemory.AllocZeroed((nuint)IntPtr.Size);

            var status = EasyHook.LhInstallHook(FunctionToHook, NewFunction, IntPtr.Zero, _handle);
            if (status < 0)
            {
                NativeMemory.Free((void*)_handle);
                _handle = IntPtr.Zero;
            }
            EasyHook.ThrowIfFailed(status);
        }

        private void UnHook()
        {
            if (_handle == IntPtr.Zero)
                return;

            EasyHook.LhUninstallHook(_handle);
            EasyHook.LhWaitForPendingRemovals();
            NativeMemory.Free((void*)_handle);
            _handle = IntPtr.Zero;
        }

        public void Activate()
        {
            if (_handle == IntPtr.Zero)
                CreateHook();

            if (IsActive)
                return;

            IsActive = true;

            // Thread ID 0 is the current thread: intercept every thread except this one.
            var threadIds = stackalloc int[] { 0 };
            EasyHook.ThrowIfFailed(EasyHook.LhSetExclusiveACL(threadIds, 1, _handle));
        }

        public void Deactivate()
        {
            if (!IsActive) return;

            IsActive = false;

            // Only intercept the current thread, which never calls the hooked function.
            var threadIds = stackalloc int[] { 0 };
            EasyHook.ThrowIfFailed(EasyHook.LhSetInclusiveACL(threadIds, 1, _handle));
        }

        #region IDisposable
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnHook();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
