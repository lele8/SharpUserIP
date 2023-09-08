using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SharpUserIP
{
    internal class Simulation
    {
        [DllImport("ADVAPI32.DLL", SetLastError = true, CharSet = CharSet.Unicode)]
        internal static extern bool LogonUser(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            int dwLogonType,
            int dwLogonProvider,
            out IntPtr phToken);

        [DllImport("KERNEL32.DLL", SetLastError = true)]
        internal static extern bool CloseHandle(IntPtr hObject);

        public static void Run(string domain, string username, string password, Action action)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            const int LOGON32_LOGON_NETONLY = 9;
            IntPtr accessToken;
            bool isLogonSuccessful = LogonUser(username, domain, password, LOGON32_LOGON_NETONLY, LOGON32_PROVIDER_DEFAULT, out accessToken);
            if (!isLogonSuccessful)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            using (var windowsIdentity = new WindowsIdentity(accessToken))
            {
                using (var impersonationContext = windowsIdentity.Impersonate())
                {
                    action.Invoke();
                }
            }

            CloseHandle(accessToken);
        }
    }
}
