using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ServiceLogonMultifactor.Models.UserSessionModel;

namespace ServiceLogonMultifactor.Providers
{
    public class WinApiProvider : IWinApiProvider
    {
        //https://www.codeproject.com/Articles/111430/Grabbing-Information-of-a-Terminal-Services-Sessio

        #region Constants

        public const int WTS_CURRENT_SESSION = -1;

        #endregion

        public IEnumerable<LogonSession> GetSessions()
        {
            var listSession = new List<LogonSession>();
            var pServer = IntPtr.Zero;
            var sUserName = string.Empty;
            var sDomain = string.Empty;
            var sClientApplicationDirectory = string.Empty;
            var sIPAddress = string.Empty;

            var oClientAddres = new WTS_CLIENT_ADDRESS();
            var oClientDisplay = new WTS_CLIENT_DISPLAY();

            var pSessionInfo = IntPtr.Zero;

            var iCount = 0;
            var iReturnValue = WTSEnumerateSessions
                (pServer, 0, 1, ref pSessionInfo, ref iCount);
            var iDataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));

            var iCurrent = (int) pSessionInfo;

            if (iReturnValue != 0)
            {
                //Go to all sessions
                for (var i = 0; i < iCount; i++)
                {
                    var oSessionInfo =
                        (WTS_SESSION_INFO) Marshal.PtrToStructure((IntPtr) iCurrent, typeof(WTS_SESSION_INFO));
                    iCurrent += iDataSize;
                    uint iReturned = 0;
                    //Get the IP address of the Terminal Services User
                    var pAddress = IntPtr.Zero;
                    if (WTSQuerySessionInformation(pServer, oSessionInfo.iSessionID, WTS_INFO_CLASS.WTSClientAddress,
                        out pAddress, out iReturned))
                    {
                        oClientAddres = (WTS_CLIENT_ADDRESS) Marshal.PtrToStructure(pAddress, oClientAddres.GetType());
                        sIPAddress = oClientAddres.bAddress[2] + "." + oClientAddres.bAddress[3] + "." +
                                     oClientAddres.bAddress[4] + "." + oClientAddres.bAddress[5];
                    }

                    //Get the User Name of the Terminal Services User
                    if (WTSQuerySessionInformation(pServer, oSessionInfo.iSessionID, WTS_INFO_CLASS.WTSUserName,
                        out pAddress, out iReturned)) sUserName = Marshal.PtrToStringAnsi(pAddress);
                    //Get the Domain Name of the Terminal Services User
                    if (WTSQuerySessionInformation(pServer, oSessionInfo.iSessionID, WTS_INFO_CLASS.WTSDomainName,
                        out pAddress, out iReturned)) sDomain = Marshal.PtrToStringAnsi(pAddress);
                    //Get the Display Information  of the Terminal Services User
                    if (WTSQuerySessionInformation(pServer, oSessionInfo.iSessionID, WTS_INFO_CLASS.WTSClientDisplay,
                        out pAddress, out iReturned))
                        oClientDisplay =
                            (WTS_CLIENT_DISPLAY) Marshal.PtrToStructure(pAddress, oClientDisplay.GetType());
                    //Get the Application Directory of the Terminal Services User
                    if (WTSQuerySessionInformation(pServer, oSessionInfo.iSessionID, WTS_INFO_CLASS.WTSClientDirectory,
                        out pAddress, out iReturned)) sClientApplicationDirectory = Marshal.PtrToStringAnsi(pAddress);
                    var sessionWinAPI = new LogonSession();
                    sessionWinAPI.SessionID = oSessionInfo.iSessionID;
                    sessionWinAPI.SessionState = oSessionInfo.oState.ToString();
                    sessionWinAPI.WorkstationName = oSessionInfo.sWinsWorkstationName;
                    sessionWinAPI.IP = sIPAddress;
                    sessionWinAPI.UserName = sUserName;
                    sessionWinAPI.Domain = sDomain;
                    sessionWinAPI.DisplayResolution = oClientDisplay.iHorizontalResolution + "x" +
                                                      oClientDisplay.iVerticalResolution + "x" +
                                                      oClientDisplay.iColorDepth;
                    listSession.Add(sessionWinAPI);
                }

                WTSFreeMemory(pSessionInfo);
            }

            return listSession;
        }

        #region Dll Imports

        [DllImport("wtsapi32.dll")]
        private static extern int WTSEnumerateSessions(
            IntPtr pServer,
            [MarshalAs(UnmanagedType.U4)] int iReserved,
            [MarshalAs(UnmanagedType.U4)] int iVersion,
            ref IntPtr pSessionInfo,
            [MarshalAs(UnmanagedType.U4)] ref int iCount);

        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSQuerySessionInformation(
            IntPtr pServer,
            int iSessionID,
            WTS_INFO_CLASS oInfoClass,
            out IntPtr pBuffer,
            out uint iBytesReturned);

        [DllImport("wtsapi32.dll")]
        private static extern void WTSFreeMemory(
            IntPtr pMemory);

        #endregion

        #region Structures

        //Structure for Terminal Service Client IP Address
        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_CLIENT_ADDRESS
        {
            public readonly int iAddressFamily;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public readonly byte[] bAddress;
        }

        //Structure for Terminal Service Session Info
        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_SESSION_INFO
        {
            public readonly int iSessionID;
            [MarshalAs(UnmanagedType.LPStr)] public readonly string sWinsWorkstationName;
            public readonly WTS_CONNECTSTATE_CLASS oState;
        }

        //Structure for Terminal Service Session Client Display
        [StructLayout(LayoutKind.Sequential)]
        private struct WTS_CLIENT_DISPLAY
        {
            public readonly int iHorizontalResolution;

            public readonly int iVerticalResolution;

            //1 = The display uses 4 bits per pixel for a maximum of 16 colors.
            //2 = The display uses 8 bits per pixel for a maximum of 256 colors.
            //4 = The display uses 16 bits per pixel for a maximum of 2^16 colors.
            //8 = The display uses 3-byte RGB values for a maximum of 2^24 colors.
            //16 = The display uses 15 bits per pixel for a maximum of 2^15 colors.
            public readonly int iColorDepth;
        }

        #endregion

        #region Enumurations

        public enum WTS_CONNECTSTATE_CLASS
        {
            WTSActive,
            WTSConnected,
            WTSConnectQuery,
            WTSShadow,
            WTSDisconnected,
            WTSIdle,
            WTSListen,
            WTSReset,
            WTSDown,
            WTSInit
        }

        public enum WTS_INFO_CLASS
        {
            WTSInitialProgram,
            WTSApplicationName,
            WTSWorkingDirectory,
            WTSOEMId,
            WTSSessionId,
            WTSUserName,
            WTSWinStationName,
            WTSDomainName,
            WTSConnectState,
            WTSClientBuildNumber,
            WTSClientName,
            WTSClientDirectory,
            WTSClientProductId,
            WTSClientHardwareId,
            WTSClientAddress,
            WTSClientDisplay,
            WTSClientProtocolType,
            WTSIdleTime,
            WTSLogonTime,
            WTSIncomingBytes,
            WTSOutgoingBytes,
            WTSIncomingFrames,
            WTSOutgoingFrames,
            WTSClientInfo,
            WTSSessionInfo,
            WTSConfigInfo,
            WTSValidationInfo,
            WTSSessionAddressV4,
            WTSIsRemoteSession
        }

        #endregion
    }
}