
//using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XDelServiceRef;

namespace ETHAN.classes
{
    public static class AppSession
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);

        private static string _FORGOT_MOTP_VERIFIED;
        public static string FORGOT_MOTP_VERIFIED => _FORGOT_MOTP_VERIFIED;

        private static string _FORGOT_EOTP_VERIFIED;
        public static string FORGOT_EOTP_VERIFIED => _FORGOT_EOTP_VERIFIED;

        private static string _LOGIN_MOTP_VERIFIED;
        public static string LOGIN_MOTP_VERIFIED => _LOGIN_MOTP_VERIFIED;

        private static string _LOGIN_EOTP_VERIFIED;
        public static string LOGIN_EOTP_VERIFIED => _LOGIN_EOTP_VERIFIED;

        private static string _MOTP_VERIFIED;
        public static string MOTP_VERIFIED => _MOTP_VERIFIED;

        private static string _EOTP_VERIFIED;

        public static string EOTP_VERIFIED => _EOTP_VERIFIED;

        private static string _PENDING_EUIDX;
        public static string PENDING_EUIDX => _PENDING_EUIDX;

        private static string _LOGIN_SUIDX;
        public static string LOGIN_SUIDX => _LOGIN_SUIDX;

        private static string _LOGIN_EUIDX;
        public static string LOGIN_EUIDX => _LOGIN_EUIDX;

        private static string _FORGOT_EUIDX;
        public static string FORGOT_EUIDX => _FORGOT_EUIDX;

        private static string _APP_UID;
        public static string APP_UID => _APP_UID;

        private static string _TEMP_UID;
        public static string TEMP_UID => _TEMP_UID;

        private static string _LOGIN_SOTP_SESSIONID;
        public static string LOGIN_SOTP_SESSIONID => _LOGIN_SOTP_SESSIONID;

        private static string _LOGIN_MOTP_SESSIONID;
        public static string LOGIN_MOTP_SESSIONID => _LOGIN_MOTP_SESSIONID;

        private static string _LOGIN_EOTP_SESSIONID;
        public static string LOGIN_EOTP_SESSIONID => _LOGIN_EOTP_SESSIONID;

        private static string _FORGOT_EOTP_SESSIONID;
        public static string FORGOT_EOTP_SESSIONID => _FORGOT_EOTP_SESSIONID;

        private static string _FORGOT_MOTP_SESSIONID;
        public static string FORGOT_MOTP_SESSIONID => _FORGOT_MOTP_SESSIONID;

        private static string _REG_MOTP_SESSIONID;
        public static string REG_MOTP_SESSIONID => _REG_MOTP_SESSIONID;

        private static string _REG_EOTP_SESSIONID;
        public static string REG_EOTP_SESSIONID => _REG_EOTP_SESSIONID;

        public static async Task SetFORGOT_MOTP_VERIFIED(string sess)
        {
            _FORGOT_MOTP_VERIFIED = sess;
            await SecureStorage.SetAsync("FORGOT_MOTP_VERIFIED", sess);
        }

        public static async Task SetFORGOT_EOTP_VERIFIED(string sess)
        {
            _FORGOT_EOTP_VERIFIED = sess;
            await SecureStorage.SetAsync("FORGOT_EOTP_VERIFIED", sess);
        }

        public static async Task SetLOGIN_MOTP_VERIFIED(string sess)
        {
            _LOGIN_MOTP_VERIFIED = sess;
            await SecureStorage.SetAsync("LOGIN_MOTP_VERIFIED", sess);
        }

        public static async Task SetLOGIN_EOTP_VERIFIED(string sess)
        {
            _LOGIN_EOTP_VERIFIED = sess;
            await SecureStorage.SetAsync("LOGIN_EOTP_VERIFIED", sess);
        }

        private static string _loginMode;
        public static string LoginMode => _loginMode;

        private static LoginInfo? _loginInfo;
        public static LoginInfo? logininfo => _loginInfo;

        public static async Task InitializeAsync()
        {
            if (_loginMode != null)
                return;

            await _lock.WaitAsync();
            try
            {
                if (_loginMode == null)
                    _loginMode = await SecureStorage.GetAsync("LOGINMODE");
            }
            finally
            {
                _lock.Release();
            }
        }

        public static async Task SetPENDING_EUIDXAsync(string aPENDING_EUIDX)
        {
            _PENDING_EUIDX = aPENDING_EUIDX;
            await SecureStorage.SetAsync("PENDING_EUIDX", aPENDING_EUIDX);
        }

        public static async Task SetLOGIN_SUIDX(string aLOGIN_SUIDX)
        {
            _LOGIN_SUIDX = aLOGIN_SUIDX;
            await SecureStorage.SetAsync("LOGIN_SUIDX", aLOGIN_SUIDX);
        }

        public static async Task SetLOGIN_EUIDX(string aLOGIN_EUIDX)
        {
            _LOGIN_EUIDX = aLOGIN_EUIDX;
            await SecureStorage.SetAsync("LOGIN_EUIDX", aLOGIN_EUIDX);
        }

        public static async Task SetFORGOT_EUIDX(string aFORGOT_EUIDX)
        {
            _FORGOT_EUIDX = aFORGOT_EUIDX;
            await SecureStorage.SetAsync("FORGOT_EUIDX", aFORGOT_EUIDX);
        }

        public static async Task SetAPP_UID(string auid)
        {
            _APP_UID = auid;
            await SecureStorage.SetAsync("APP_UID", auid);
        }

        public static async Task SetTEMP_UID(string tuid)
        {
            _TEMP_UID = tuid;
            await SecureStorage.SetAsync("TEMP_UID", tuid);
        }

        public static async Task SetLOGIN_SOTP_SESSIONIDAsync(string sess)
        {
            _LOGIN_SOTP_SESSIONID = sess;
            await SecureStorage.SetAsync("LOGIN_SOTP_SESSIONID", sess);
        }

        public static async Task SetLOGIN_MOTP_SESSIONIDAsync(string sess)
        {
            _LOGIN_MOTP_SESSIONID = sess;
            await SecureStorage.SetAsync("LOGIN_MOTP_SESSIONID", sess);
        }

        public static async Task SetLOGIN_EOTP_SESSIONIDAsync(string sess)
        {
            _LOGIN_EOTP_SESSIONID = sess;
            await SecureStorage.SetAsync("LOGIN_EOTP_SESSIONID", sess);
        }

        public static async Task SetFORGOT_EOTP_SESSIONIDAsync(string sess)
        {
            _FORGOT_EOTP_SESSIONID = sess;
            await SecureStorage.SetAsync("FORGOT_EOTP_SESSIONID", sess);
        }

        public static async Task SetFORGOT_MOTP_SESSIONIDAsync(string sess)
        {
            _FORGOT_MOTP_SESSIONID = sess;
            await SecureStorage.SetAsync("FORGOT_MOTP_SESSIONID", sess);
        }

        public static async Task SetREG_MOTP_SESSIONIDAsync(string sess)
        {
            _REG_MOTP_SESSIONID = sess;
            await SecureStorage.SetAsync("REG_MOTP_SESSIONID", sess);
        }

        public static async Task SetREG_EOTP_SESSIONIDAsync(string sess)
        {
            _REG_EOTP_SESSIONID = sess;
            await SecureStorage.SetAsync("REG_EOTP_SESSIONID", sess);
        }

        public static async Task SetMOTP_VERIFIEDAsync(string sess)
        {
            _MOTP_VERIFIED = sess;
            await SecureStorage.SetAsync("MOTP_VERIFIED", sess);
        }

        public static async Task SetEOTP_VERIFIEDAsync(string sess)
        {
            _EOTP_VERIFIED = sess;
            await SecureStorage.SetAsync("EOTP_VERIFIED", sess);
        }

        public static async Task SetLoginModeAsync(string mode)
        {
            _loginMode = mode;
            await SecureStorage.SetAsync("LOGINMODE", mode);
        }

        // MEMORY ONLY
        public static void SetLoginInfo(LoginInfo info)
        {
            _loginInfo = info;
        }

        //LOGOUT CLEANUP
        public static async Task ClearAsync()
        {
            await _lock.WaitAsync();
            try
            {
                _loginMode = null;
                _loginInfo = null;
                SecureStorage.Remove("LOGINMODE"); // DO NOT await (Android bug)
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
