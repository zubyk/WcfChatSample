using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;

namespace WcfChatSample.Client.Wpf.Extensions
{
    internal static class SecureStringExtension
    {
        internal static async Task<T> ProcessPasswordAsync<T>(this SecureString str, Func<string, Task<T>> action)
        {
            var ptr = IntPtr.Zero;
            var pass = String.Empty;
            var handle = GCHandle.Alloc(pass, GCHandleType.Pinned);

            try
            {
                ptr = Marshal.SecureStringToGlobalAllocUnicode(str);
                pass = Marshal.PtrToStringUni(ptr);

                return await action(pass);
            }
            finally
            {
                pass = null;
                handle.Free();
                Marshal.ZeroFreeGlobalAllocUnicode(ptr);
                ptr = IntPtr.Zero;
            }
        }
    }
}
