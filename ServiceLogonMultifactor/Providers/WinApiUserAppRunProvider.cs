using System;
using System.Text;
using System.Security;
using System.Management;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using ServiceLogonMultifactor.Logging;
//https://stackoverflow.com/questions/2313553/process-start-with-different-credentials-with-uac-on
//https://stackoverflow.com/questions/28674344/windows-service-fails-to-start-interactive-process-on-user-log-on-with-wtsqueryu

namespace ServiceLogonMultifactor.Providers
{
    class WinApiUserAppRunProvider: IWinApiUserAppRunProvider
    {
        private readonly ITracing tracing;
        private readonly IFileTrastChekingProvider fileTrastChekingProvider;
        public WinApiUserAppRunProvider (ITracing tracing)
        {
            this.tracing = tracing;
            this.fileTrastChekingProvider = new FileTrastChekingProvider();
        }
        #region Win32 API routines

        [StructLayout(LayoutKind.Sequential)]
        struct SECURITY_ATTRIBUTES
        {
            public Int32 Length;
            public IntPtr lpSecurityDescriptor;
            public Boolean bInheritHandle;
        }

        enum TOKEN_TYPE
        {
            TokenPrimary = 1,
            TokenImpersonation = 2
        }

        enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,
            TokenGroups,
            TokenPrivileges,
            TokenOwner,
            TokenPrimaryGroup,
            TokenDefaultDacl,
            TokenSource,
            TokenType,
            TokenImpersonationLevel,
            TokenStatistics,
            TokenRestrictedSids,
            TokenSessionId,
            TokenGroupsAndPrivileges,
            TokenSessionReference,
            TokenSandBoxInert,
            TokenAuditPolicy,
            TokenOrigin,
            MaxTokenInfoClass  // MaxTokenInfoClass should always be the last enum
        }

        [StructLayout(LayoutKind.Sequential)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public String lpReserved;
            public String lpDesktop;
            public String lpTitle;
            public UInt32 dwX;
            public UInt32 dwY;
            public UInt32 dwXSize;
            public UInt32 dwYSize;
            public UInt32 dwXCountChars;
            public UInt32 dwYCountChars;
            public UInt32 dwFillAttribute;
            public UInt32 dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public UInt32 dwProcessId;
            public UInt32 dwThreadId;
        }

        enum SECURITY_IMPERSONATION_LEVEL
        {
            SecurityAnonymous = 0,
            SecurityIdentification = 1,
            SecurityImpersonation = 2,
            SecurityDelegation = 3,
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LUID
        {
            public Int32 LowPart;
            public Int32 HighPart;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct LUID_AND_ATRIBUTES
        {
            LUID Luid;
            Int32 Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct TOKEN_PRIVILEGES
        {
            public Int32 PrivilegeCount;
            //LUID_AND_ATRIBUTES
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public Int32[] Privileges;
        }

        const Int32 READ_CONTROL = 0x00020000;

        const Int32 STANDARD_RIGHTS_REQUIRED = 0x000F0000;

        const Int32 STANDARD_RIGHTS_READ = READ_CONTROL;
        const Int32 STANDARD_RIGHTS_WRITE = READ_CONTROL;
        const Int32 STANDARD_RIGHTS_EXECUTE = READ_CONTROL;

        const Int32 STANDARD_RIGHTS_ALL = 0x001F0000;

        const Int32 SPECIFIC_RIGHTS_ALL = 0x0000FFFF;

        const Int32 TOKEN_ASSIGN_PRIMARY = 0x0001;
        const Int32 TOKEN_DUPLICATE = 0x0002;
        const Int32 TOKEN_IMPERSONATE = 0x0004;
        const Int32 TOKEN_QUERY = 0x0008;
        const Int32 TOKEN_QUERY_SOURCE = 0x0010;
        const Int32 TOKEN_ADJUST_PRIVILEGES = 0x0020;
        const Int32 TOKEN_ADJUST_GROUPS = 0x0040;
        const Int32 TOKEN_ADJUST_DEFAULT = 0x0080;
        const Int32 TOKEN_ADJUST_SESSIONID = 0x0100;

        const Int32 TOKEN_ALL_ACCESS_P = (
            STANDARD_RIGHTS_REQUIRED |
            TOKEN_ASSIGN_PRIMARY |
            TOKEN_DUPLICATE |
            TOKEN_IMPERSONATE |
            TOKEN_QUERY |
            TOKEN_QUERY_SOURCE |
            TOKEN_ADJUST_PRIVILEGES |
            TOKEN_ADJUST_GROUPS |
            TOKEN_ADJUST_DEFAULT);

        const Int32 TOKEN_ALL_ACCESS = TOKEN_ALL_ACCESS_P | TOKEN_ADJUST_SESSIONID;

        const Int32 TOKEN_READ = STANDARD_RIGHTS_READ | TOKEN_QUERY;


        const Int32 TOKEN_WRITE = STANDARD_RIGHTS_WRITE |
                                      TOKEN_ADJUST_PRIVILEGES |
                                      TOKEN_ADJUST_GROUPS |
                                      TOKEN_ADJUST_DEFAULT;

        const Int32 TOKEN_EXECUTE = STANDARD_RIGHTS_EXECUTE;

        const UInt32 MAXIMUM_ALLOWED = 0x2000000;

        const Int32 CREATE_NEW_PROCESS_GROUP = 0x00000200;
        const Int32 CREATE_UNICODE_ENVIRONMENT = 0x00000400;

        const Int32 IDLE_PRIORITY_CLASS = 0x40;
        const Int32 NORMAL_PRIORITY_CLASS = 0x20;
        const Int32 HIGH_PRIORITY_CLASS = 0x80;
        const Int32 REALTIME_PRIORITY_CLASS = 0x100;

        const Int32 CREATE_NEW_CONSOLE = 0x00000010;

        const string SE_DEBUG_NAME = "SeDebugPrivilege";
        const string SE_RESTORE_NAME = "SeRestorePrivilege";
        const string SE_BACKUP_NAME = "SeBackupPrivilege";

        const Int32 SE_PRIVILEGE_ENABLED = 0x0002;

        const Int32 ERROR_NOT_ALL_ASSIGNED = 1300;

        [StructLayout(LayoutKind.Sequential)]
        struct PROCESSENTRY32
        {
            UInt32 dwSize;
            UInt32 cntUsage;
            UInt32 th32ProcessID;
            IntPtr th32DefaultHeapID;
            UInt32 th32ModuleID;
            UInt32 cntThreads;
            UInt32 th32ParentProcessID;
            Int32 pcPriClassBase;
            UInt32 dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            string szExeFile;
        }

        const UInt32 TH32CS_SNAPPROCESS = 0x00000002;

        const Int32 INVALID_HANDLE_VALUE = -1;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern Boolean CloseHandle(IntPtr hSnapshot);



        [DllImport("Wtsapi32.dll")]
        static extern UInt32 WTSQueryUserToken(int SessionId, ref IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern Boolean LookupPrivilegeValue(IntPtr lpSystemName, string lpname, [MarshalAs(UnmanagedType.Struct)] ref LUID lpLuid);

        [DllImport("advapi32.dll", EntryPoint = "CreateProcessAsUser", SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        extern static Boolean CreateProcessAsUser(IntPtr hToken, String lpApplicationName, String lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, Boolean bInheritHandle, Int32 dwCreationFlags, IntPtr lpEnvironment,
            String lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("advapi32.dll", EntryPoint = "DuplicateTokenEx")]
        extern static Boolean DuplicateTokenEx(IntPtr ExistingTokenHandle, UInt32 dwDesiredAccess,
            ref SECURITY_ATTRIBUTES lpThreadAttributes, Int32 TokenType,
            Int32 ImpersonationLevel, ref IntPtr DuplicateTokenHandle);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Boolean bInheritHandle, UInt32 dwProcessId);

        [DllImport("advapi32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        static extern Boolean OpenProcessToken(IntPtr ProcessHandle, // handle to process
                                            Int32 DesiredAccess, // desired access to process
                                            ref IntPtr TokenHandle); // handle to open access token

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern Boolean AdjustTokenPrivileges(IntPtr TokenHandle, Boolean DisableAllPrivileges, ref TOKEN_PRIVILEGES NewState, Int32 BufferLength, IntPtr PreviousState, IntPtr ReturnLength);

        [DllImport("advapi32.dll", SetLastError = true)]
        static extern Boolean SetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, ref UInt32 TokenInformation, UInt32 TokenInformationLength);

        [DllImport("userenv.dll", SetLastError = true)]
        static extern Boolean CreateEnvironmentBlock(ref IntPtr lpEnvironment, IntPtr hToken, Boolean bInherit);

        #endregion

        #region Methods

       // const string tmp_log = @"c:\!\!logWinApi.txt";

        /// <summary>
        /// Creates the process in the interactive desktop with credentials of the logged in user.
        /// </summary>
        public  UInt32 CreateProcessAsUser(string fileExe,string param, string workingDirectory, int dwSessionId)
        {
            UInt32 processIDcreated = 0;
            string commandLine = fileExe + " " + param;
            if (!fileTrastChekingProvider.FileBlockValid(fileExe))
            {
                tracing.WriteError($"CreateProcessAsUser wrong file cert or md5");
                return processIDcreated;
            }

            try
            {
                //UInt32 dwSessionId = WTSGetActiveConsoleSessionId();
                //File.AppendAllText(tmp_log, $"preparing to start process in session Id: { dwSessionId }" + "\n");

                IntPtr hUserToken = IntPtr.Zero;
                WTSQueryUserToken(dwSessionId, ref hUserToken);

                if (hUserToken != IntPtr.Zero)
                {
                    //File.AppendAllText(tmp_log, "WTSQueryUserToken() OK (hUserToken:{0})" + hUserToken + "\n");

                    Process[] processes = Process.GetProcessesByName("winlogon");

                    if (processes.Length == 0)
                    {
                        //File.AppendAllText(tmp_log, "Application can not be found in the running processes" + "\n"); ;
                        return processIDcreated;
                    }

                    Int32 userAppProcessId = -1;

                    for (Int32 k = 0; k < processes.Length; k++)
                    {
                        //File.AppendAllText(tmp_log, $"Process: '{processes[k].ProcessName}', PID: {processes[k].Id}, Handle: {processes[k].Handle}, " +                            $"Session: {processes[k].SessionId}" + "\n");


                        if ((UInt32)processes[k].SessionId == dwSessionId)
                        {
                            userAppProcessId = processes[k].Id;
                        }
                    }

                    if (userAppProcessId == -1)
                    {
                        //File.AppendAllText(tmp_log, "Application '{0}' is not found in the processes of the current session" + dwSessionId + "\n");
                        return processIDcreated;
                    }

                    IntPtr hProcess = OpenProcess((Int32)MAXIMUM_ALLOWED, false, (UInt32)userAppProcessId);

                    IntPtr hPToken = IntPtr.Zero;

                    OpenProcessToken(hProcess,
                        TOKEN_ADJUST_PRIVILEGES
                        | TOKEN_QUERY
                        | TOKEN_DUPLICATE
                        | TOKEN_ASSIGN_PRIMARY
                        | TOKEN_ADJUST_SESSIONID
                        | TOKEN_READ
                        | TOKEN_WRITE,
                        ref hPToken);

                    if (hPToken != IntPtr.Zero)
                    {
                        //File.AppendAllText(tmp_log, "OpenProcessToken() OK (Token: {0})" + hPToken + "\n");

                        LUID luid = new LUID();

                        if (LookupPrivilegeValue(IntPtr.Zero, SE_DEBUG_NAME, ref luid))
                        {
                            //File.AppendAllText(tmp_log, $"LookupPrivilegeValue() OK (High: {luid.HighPart}, Low: {luid.LowPart})" + "\n");

                            SECURITY_ATTRIBUTES sa = new SECURITY_ATTRIBUTES();
                            sa.Length = Marshal.SizeOf(sa);

                            IntPtr hUserTokenDup = IntPtr.Zero;
                            DuplicateTokenEx(hPToken,
                                (Int32)MAXIMUM_ALLOWED,
                                ref sa,
                                (Int32)SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                                (Int32)TOKEN_TYPE.TokenPrimary,
                                ref hUserTokenDup);

                            if (hUserTokenDup != IntPtr.Zero)
                            {
                                //File.AppendAllText(tmp_log, "DuplicateTokenEx() OK (hToken: {0})" + hUserTokenDup + "\n");

                                TOKEN_PRIVILEGES tp = new TOKEN_PRIVILEGES
                                {
                                    PrivilegeCount = 1,
                                    Privileges = new Int32[3]
                                };

                                tp.Privileges[1] = luid.HighPart;
                                tp.Privileges[0] = luid.LowPart;
                                tp.Privileges[2] = SE_PRIVILEGE_ENABLED;


                                //File.AppendAllText(tmp_log, "SetTokenInformation() OK" + "\n");

                                if (AdjustTokenPrivileges(hUserTokenDup,
                                    false, ref tp, Marshal.SizeOf(tp),
                                    IntPtr.Zero, IntPtr.Zero))
                                {
                                    //File.AppendAllText(tmp_log, "AdjustTokenPrivileges() OK" + "\n");

                                    Int32 dwCreationFlags = NORMAL_PRIORITY_CLASS | CREATE_NEW_CONSOLE;

                                    IntPtr pEnv = IntPtr.Zero;
                                    if (CreateEnvironmentBlock(ref pEnv, hUserTokenDup, true))
                                    {
                                        dwCreationFlags |= CREATE_UNICODE_ENVIRONMENT;
                                    }
                                    else
                                    {
                                        //File.AppendAllText(tmp_log, "CreateEnvironmentBlock() FAILED (Last Error: {0})" + Marshal.GetLastWin32Error() + "\n");
                                        pEnv = IntPtr.Zero;
                                    }

                                    // Launch the process in the client's logon session.
                                    PROCESS_INFORMATION pi;

                                    STARTUPINFO si = new STARTUPINFO();
                                    si.cb = Marshal.SizeOf(si);
                                    si.lpDesktop = "winsta0\\default";

                                    //File.AppendAllText(tmp_log, $"CreateProcess (Path:{commandLine}, CurrDir:{workingDirectory})" + "\n");

                                    if (CreateProcessAsUser(hUserTokenDup,    // client's access token
                                            null,                // file to execute
                                            commandLine,        // command line
                                            ref sa,                // pointer to process SECURITY_ATTRIBUTES
                                            ref sa,                // pointer to thread SECURITY_ATTRIBUTES
                                            false,                // handles are not inheritable
                                            dwCreationFlags,    // creation flags
                                            pEnv,                // pointer to new environment block 
                                            workingDirectory,    // name of current directory 
                                            ref si,                // pointer to STARTUPINFO structure
                                            out pi                // receives information about new process
                                        ))
                                    {

                                        //File.AppendAllText(tmp_log, "CreateProcessAsUser() OK (PID: {0})" + pi.dwProcessId + "\n");
                                        processIDcreated = pi.dwProcessId;
                                    }
                                    else
                                    {
                                        //File.AppendAllText(tmp_log, $"CreateProcessAsUser() failed (Last Error: {Marshal.GetLastWin32Error()})" + "\n");
                                    }
                                }
                                else
                                {
                                    //File.AppendAllText(tmp_log, $"AdjustTokenPrivileges() failed (Last Error: {Marshal.GetLastWin32Error()})" + "\n");
                                }


                                CloseHandle(hUserTokenDup);
                            }
                            else
                            {
                                //File.AppendAllText(tmp_log, $"DuplicateTokenEx() failed (Last Error: {Marshal.GetLastWin32Error()})" + "\n");
                            }
                        }
                        else
                        {
                            //File.AppendAllText(tmp_log, $"LookupPrivilegeValue() failed (Last Error: {Marshal.GetLastWin32Error()})" + "\n");
                        }

                        CloseHandle(hPToken);
                    }
                    else
                    {
                        //File.AppendAllText(tmp_log, $"OpenProcessToken() failed (Last Error: { Marshal.GetLastWin32Error()})" + "\n");
                    }

                    CloseHandle(hUserToken);
                }
                else
                {
                    //File.AppendAllText(tmp_log, $"WTSQueryUserToken failed: {Marshal.GetLastWin32Error()}" + "\n");
                }

            }
            catch (Exception ex)
            {
                //File.AppendAllText(tmp_log, "Exception occurred: " + ex.Message + "\n");
            }

            return processIDcreated;
        }

        #endregion




    }
}
