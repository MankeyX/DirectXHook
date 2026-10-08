using System;
using System.Runtime.InteropServices;

namespace Hook.Infrastructure
{
    /// <summary>
    /// Bindings for the native EasyHook64.dll exports used by the hook.
    /// EasyHook64.dll is already loaded into the target process by the injector, so it is resolved by name.
    /// </summary>
    public static unsafe partial class EasyHook
    {
        private const string Library = "EasyHook64.dll";

        /// <summary>
        /// Layout of EasyHook's REMOTE_ENTRY_INFO, passed to <c>NativeInjectionEntryPoint</c>.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RemoteEntryInfo
        {
            public int HostPid;
            public byte* UserData;
            public int UserDataSize;
        }

        [LibraryImport(Library)]
        public static partial int LhInstallHook(IntPtr entryPoint, IntPtr hookProc, IntPtr callback, IntPtr outHandle);

        [LibraryImport(Library)]
        public static partial int LhUninstallHook(IntPtr handle);

        [LibraryImport(Library)]
        public static partial int LhWaitForPendingRemovals();

        [LibraryImport(Library)]
        public static partial int LhSetInclusiveACL(int* threadIdList, int threadCount, IntPtr handle);

        [LibraryImport(Library)]
        public static partial int LhSetExclusiveACL(int* threadIdList, int threadCount, IntPtr handle);

        [LibraryImport(Library)]
        private static partial IntPtr RtlGetLastErrorString();

        public static void ThrowIfFailed(int status)
        {
            if (status >= 0)
                return;

            var message = Marshal.PtrToStringUni(RtlGetLastErrorString());
            throw new InvalidOperationException($"EasyHook call failed with status 0x{status:X8}: {message}");
        }
    }
}
