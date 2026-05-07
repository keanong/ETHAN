using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETHAN.XDelSys
{
    public static class ReadRegistry
    {
        private static String ProductKey = "XDel Singapore Pte Ltd";

        public static RegistryKey GetCurrentUserKey(String ASubKey)
        {
            RegistryKey RKey = Registry.CurrentUser;
            RKey = RKey.CreateSubKey(ASubKey);
            return RKey;
        }

        public static RegistryKey GetLocalMachineKey(String ASubKey)
        {
            RegistryKey RKey = Registry.LocalMachine;
            RKey = RKey.CreateSubKey(ASubKey);
            return RKey;
        }

        public static void SetObject(RegistryKey RKey, String AKey, String AValueKey, Object AValue)
        {
            RegistryValueKind rvk = RegistryValueKind.Unknown;
            if (AValue is Int16 || AValue is Int32 || AValue is Boolean)
                rvk = RegistryValueKind.DWord;
            else if (AValue is Int64)
                rvk = RegistryValueKind.QWord;
            else if (AValue is String)
                rvk = RegistryValueKind.String;
            RKey.SetValue(AValueKey, AValue, rvk);
        }

        public static void SetObject(String AKey, String AValueKey, Object AValue)
        {
            RegistryKey RKey = Registry.CurrentUser;
            RKey = RKey.CreateSubKey(String.Format("Software\\{0}\\{1}", ProductKey, AKey));
            SetObject(RKey, AKey, AValueKey, AValue);
        }

        public static Object GetObject(RegistryKey RKey, String AValueKey)
        {
            if (RKey != null)
            {
                foreach (String AString in RKey.GetValueNames())
                {
                    if (AString == AValueKey)
                        return RKey.GetValue(AValueKey);
                }
            }
            return null;
        }

        public static Object GetObject(String AKey, String AValueKey)
        {
            RegistryKey RKey = Registry.CurrentUser;
            RKey = RKey.CreateSubKey(String.Format("Software\\{0}\\{1}", ProductKey, AKey));
            return GetObject(RKey, AValueKey);
        }

        public static Boolean GetBoolean(String AKey, String AValueKey, Boolean ADefaultValue)
        {
            try
            {
                Object AValue = GetObject(AKey, AValueKey);
                if (AValue != null)
                    return Convert.ToBoolean(AValue);
                SetObject(AKey, AValueKey, ADefaultValue);
                return ADefaultValue;
            }
            catch
            {
                return ADefaultValue;
            }
        }

        public static Boolean GetBoolean(String AKey, String AValueKey)
        {
            return GetBoolean(AKey, AValueKey, false);
        }

        public static String GetString(String AKey, String AValueKey, String ADefaultValue)
        {
            try
            {
                Object AValue = GetObject(AKey, AValueKey);
                if (AValue != null)
                    return Convert.ToString(AValue);
                SetObject(AKey, AValueKey, ADefaultValue);
                return ADefaultValue;
            }
            catch
            {
                return ADefaultValue;
            }
        }

        public static String GetString(String AKey, String AValueKey)
        {
            return GetString(AKey, AValueKey, "");
        }

        public static Int64 GetInt64(String AKey, String AValueKey, Int64 ADefaultValue)
        {
            try
            {
                Object AValue = GetObject(AKey, AValueKey);
                if (AValue != null)
                    return Convert.ToInt64(AValue);
                SetObject(AKey, AValueKey, ADefaultValue);
                return ADefaultValue;
            }
            catch
            {
                return ADefaultValue;
            }
        }

        public static Int64 GetInt64(String AKey, String AValueKey)
        {
            return GetInt64(AKey, AValueKey, 0);
        }

        public static Int32 GetInt32(String AKey, String AValueKey, Int32 ADefaultValue)
        {
            try
            {
                Object AValue = GetObject(AKey, AValueKey);
                if (AValue != null)
                    return Convert.ToInt32(AValue);
                SetObject(AKey, AValueKey, ADefaultValue);
                return ADefaultValue;
            }
            catch
            {
                return ADefaultValue;
            }
        }

        public static Int32 GetInt32(String AKey, String AValueKey)
        {
            return GetInt32(AKey, AValueKey, 0);
        }

        public static Int16 GetInt16(String AKey, String AValueKey, Int16 ADefaultValue)
        {
            try
            {
                Object AValue = GetObject(AKey, AValueKey);
                if (AValue != null)
                    return Convert.ToInt16(AValue);
                SetObject(AKey, AValueKey, ADefaultValue);
                return ADefaultValue;
            }
            catch
            {
                return ADefaultValue;
            }
        }

        public static Int16 GetInt16(String AKey, String AValueKey)
        {
            return GetInt16(AKey, AValueKey, 0);
        }
    }
}
