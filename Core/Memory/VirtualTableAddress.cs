using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Core.Memory
{
    public class VirtualTableAddress
    {
        public static IntPtr GetVtableAddress(IntPtr pointer, int index)
        {
            var vTable = Marshal.ReadIntPtr(pointer);
            return Marshal.ReadIntPtr(vTable, index * IntPtr.Size);
        }

        public static List<IntPtr> GetVtableAddresses(IntPtr pointer, int startIndex, int numberOfMethods)
        {
            var vtableAdds = new List<IntPtr>();
            var vTable = Marshal.ReadIntPtr(pointer);

            for (var index = startIndex; index < numberOfMethods; index++)
                vtableAdds.Add(Marshal.ReadIntPtr(vTable, index * IntPtr.Size));

            return vtableAdds;
        }
    }
}
