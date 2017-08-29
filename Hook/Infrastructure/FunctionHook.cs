using System;
using System.Runtime.InteropServices;
using EasyHook;

namespace Hook
{
    public class FunctionHook<T> : IDisposable where T : class
    {
        public IntPtr FunctionToHook { get; }
        public Delegate NewFunction { get; }
        public object Owner { get; }
        public T OriginalFunction { get; private set; }
        public bool IsActive { get; private set; }

        private LocalHook Hook { get; set; }

        public FunctionHook(IntPtr functionToHook, Delegate newFunction, object owner)
        {
            FunctionToHook = functionToHook;
            NewFunction = newFunction;
            Owner = owner;
            OriginalFunction = Marshal.GetDelegateForFunctionPointer<T>(functionToHook);
        }

        ~FunctionHook()
        {
            Dispose(false);
        }

        private void CreateHook()
        {
            if (Hook != null)
                return;

            Hook = LocalHook.Create(FunctionToHook, NewFunction, Owner);
        }

        private void UnHook()
        {
            if (!IsActive)
                return;

            Hook?.Dispose();
        }

        public void Activate()
        {
            if (Hook == null)
                CreateHook();

            if (IsActive)
                return;

            IsActive = true;
            Hook.ThreadACL.SetExclusiveACL(new [] { 0 });
        }

        public void Deactivate()
        {
            if (!IsActive) return;

            IsActive = false;
            Hook.ThreadACL.SetInclusiveACL(new [] { 0 });
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
