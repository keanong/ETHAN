using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using XDelServiceRef;
using System.Runtime.Serialization;

namespace ETHAN.classes
{
    public class common
    {

        public enum eExpressType
        {
            /// <remarks/>
            etNormal,

            /// <remarks/>
            etHalfHour,

            /// <remarks/>
            etOneHour,

            /// <remarks/>
            etTwoHour,

            /// <remarks/>
            etThreeHour,

            /// <remarks/>
            etPriority,

            /// <remarks/>
            etGuaranteed,

            /// <remarks/>
            etAOH,

            /// <remarks/>
            etWeekend,

            /// <remarks/>
            etTwoDays,

            /// <remarks/>
            etThreeDays,

            /// <remarks/>
            etRTwoHour,

            /// <remarks/>
            etRThreeHour,

            /// <remarks/>
            etRFourHour,
        }

        public static async void showAlert(string msg)
        {
            try
            {
                await Shell.Current.DisplayAlert("", msg, "OK");
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
        }

        public static async Task BackToLogin()
        {
            try
            {
                /*SecureStorage.RemoveAll();
                await Shell.Current.GoToAsync("Login");*/

                try
                {
                    //XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
                    XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
        string mode = AppSession.LoginMode;
                    LoginInfo? logininfo = AppSession.logininfo;

                    if (mode == "r" && logininfo?.clientInfo?.Web_UID is string ruid && !string.IsNullOrEmpty(ruid))
                        await xs.XOE_LogOutAsync(ruid);
                    else if (mode == "s" && logininfo?.clientInfo?.Web_UID is string suid && !string.IsNullOrEmpty(suid))
                        await xs.LogOutAsync(suid);
                }
                catch (Exception ex){
                    string ss = ex.Message;
                } // never block logout on server failure


                await AppSession.ClearAsync();
                SecureStorage.RemoveAll(); await AppSession.ClearAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Application.Current.MainPage = new AppShell(); // destroy all cached pages
                });
            } catch (Exception e)
            {
                string s = e.Message;
            }
        }

        public static string getExpTypeStr(string et)
        {
            string ets = "TBA";
            try
            {
                //string et = vm.BSDeliveryTimeopened ? vm.exp1 : vm.exp2; ;

                switch (et)
                {
                    case "etNormal":
                        return "Five Thirty Plus";
                    case "etHalfHour":
                        return "1 Hour";
                    case "etOneHour":
                        return "1.5 Hours";
                    case "etTwoHour":
                        return "2.5 Hours";
                    case "etThreeHour":
                        return "3.5 Hours";
                    case "etPriority":
                        return "Four Thirty";
                    case "etGuaranteed":
                        return "Five Thirty";
                    case "etAOH":
                        return "After Office Hours";
                    case "etWeekend":
                        return "Weekend";
                    case "etTwoDays":
                        return "Two Days";
                    case "etThreeDays":
                        return "Three Days";
                    default:
                        return "TBA";
                }
            }
            catch (Exception ex)
            {
                string s = ex.Message;
            }
            return ets;
        }

        public static XDelServiceRef.eExpressType getEExpType(string et)
        {
            switch (et)
            {
                case "etNormal":
                    return XDelServiceRef.eExpressType.etNormal;
                case "etHalfHour":
                    return XDelServiceRef.eExpressType.etHalfHour;
                case "etOneHour":
                    return XDelServiceRef.eExpressType.etOneHour;
                case "etTwoHour":
                    return XDelServiceRef.eExpressType.etTwoHour;
                case "etThreeHour":
                    return XDelServiceRef.eExpressType.etThreeHour;
                case "etPriority":
                    return XDelServiceRef.eExpressType.etPriority;
                case "etGuaranteed":
                    return XDelServiceRef.eExpressType.etGuaranteed;
                case "etAOH":
                    return XDelServiceRef.eExpressType.etAOH;
                case "etWeekend":
                    return XDelServiceRef.eExpressType.etWeekend;
                case "etTwoDays":
                    return XDelServiceRef.eExpressType.etTwoDays;
                case "etThreeDays":
                    return XDelServiceRef.eExpressType.etThreeDays;
                default:
                    return XDelServiceRef.eExpressType.etGuaranteed;
            }
        }

        public static string convertExpressType(XDelServiceRef.eExpressType ex)
        {
            switch (ex)
            {
                case XDelServiceRef.eExpressType.etNormal:
                    return "Five Thirty Plus";
                case XDelServiceRef.eExpressType.etHalfHour:
                    return "1 Hour";
                case XDelServiceRef.eExpressType.etOneHour:
                    return "1.5 Hours";
                case XDelServiceRef.eExpressType.etTwoHour:
                    return "2.5 Hours";
                case XDelServiceRef.eExpressType.etThreeHour:
                    return "3.5 Hours";
                case XDelServiceRef.eExpressType.etPriority:
                    return "Four Thirty";
                case XDelServiceRef.eExpressType.etGuaranteed:
                    return "Five Thirty";
                case XDelServiceRef.eExpressType.etAOH:
                    return "After Office Hours";
                case XDelServiceRef.eExpressType.etWeekend:
                    return "Weekend";
                case XDelServiceRef.eExpressType.etTwoDays:
                    return "Two Days";
                case XDelServiceRef.eExpressType.etThreeDays:
                    return "Three Days";
                case XDelServiceRef.eExpressType.etRTwoHour:
                    return "2 Hours";
                case XDelServiceRef.eExpressType.etRThreeHour:
                    return "3 Hours";
                case XDelServiceRef.eExpressType.etRFourHour:
                    return "4 Hours";
                default:
                    return "";
            }
        }

        public static string getServiceSelected(XDelServiceRef.eTOSType ex)
        {
            switch (ex)
            {
                case XDelServiceRef.eTOSType.ttEL:
                    return "ONE WAY";
                case XDelServiceRef.eTOSType.ttRT:
                    return "TWO WAY";
                case XDelServiceRef.eTOSType.ttTC:
                    return "COLLECTION";
                default:
                    throw new ArgumentException("UnKnown");
            }
        }

        public static XDelServiceRef.StatusCodeStructure[] getAllStatusCodes(string uid, long[]? selectedidx, string mode)
        {
            //XWSSoapClient xs = new XWSSoapClient(XWSSoapClient.EndpointConfiguration.XWSSoap);
            XOEWSSoapClient xs = new XOEWSSoapClient(XOEWSSoapClient.EndpointConfiguration.XOEWSSoap);
            XDelServiceRef.StatusCodeStructure[] statuscodeStruct = null;
            try
            {
                XDelServiceRef.SettingsInfo status_code = (mode.Equals("r")) ? xs.XOE_GetStatusDefinitionsAsync(uid).Result : xs.GetStatusDefinitionsAsync(uid).Result;
                if (status_code != null && selectedidx == null)
                {
                    statuscodeStruct = status_code.StatusCodes;
                }
                else if (status_code != null && selectedidx != null && selectedidx.Length > 0)
                {
                    XDelServiceRef.StatusCodeStructure[] statuscodeStructfiltered = new XDelServiceRef.StatusCodeStructure[selectedidx.Length + 1];
                    XDelServiceRef.StatusCodeStructure newscs = new XDelServiceRef.StatusCodeStructure();
                    newscs.Description = "[ Please Select ]";
                    newscs.IDX = -1;
                    statuscodeStructfiltered[0] = newscs;
                    for (int i = 0; i <= selectedidx.Length - 1; i++)
                    {
                        for (int x = 0; x <= statuscodeStruct.Length - 1; x++)
                        {
                            if (statuscodeStruct[x].IDX == selectedidx[i])
                            {
                                statuscodeStructfiltered[i + 1] = statuscodeStruct[x];
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

            }
            return statuscodeStruct;
        }

        public static XDelServiceRef.StatusCodeStructure getStatusCode2(XDelServiceRef.StatusCodeStructure[] statuscodeStruct, long? code)
        {
            XDelServiceRef.StatusCodeStructure? scs = null;
            try
            {
                if (statuscodeStruct != null && statuscodeStruct.Length > 0 && code != null)
                {
                    for (int i = 0; i <= statuscodeStruct.Length - 1; i++)
                    {
                        if (statuscodeStruct[i].IDX == code)
                        {
                            scs = statuscodeStruct[i];
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                scs = null;
            }

            return scs;
        }

        public static string FormatAddress(XDelServiceRef.AddressStructure addr)
        {
            if (addr == null) return "";
            var parts = new[] { addr.BLOCK, addr.STREET, addr.UNIT, addr.BUILDING }
                .Where(s => !string.IsNullOrEmpty(s));
            return string.Join(", ", parts);
        }

        public static Int32 GetRandom(Int32 Min, Int32 Max)
        {
            return RandomNumberGenerator.GetInt32(Min, Max);
        }

        public static String JSONSerialize(Object ob, Boolean Formatted = false)
        {
            try
            {
                return JsonConvert.SerializeObject(ob,
                    new JsonSerializerSettings
                    {
                        Formatting = Formatted ? Formatting.Indented : Formatting.None,
                        NullValueHandling = NullValueHandling.Ignore,
                        Converters = { new StringEnumConverter() }
                    });
            }
            catch
            {
                return "";
            }
        }

        public static int getPwdRestrictLvl(String pwd)
        {
            int x = 0;
            Boolean hasDigit = false;
            Boolean hasUpper = false;
            Boolean hasLower = false;
            Boolean hasSpec = false;
            try
            {
                byte[] StringAscII = System.Text.Encoding.ASCII.GetBytes(pwd);
                byte a;
                for (int i = 0; i < StringAscII.Length; i++)
                {
                    a = StringAscII[i];//if i=0 a= 65, if i=1 a=66 and so on
                    if (a >= 48 && a <= 57)
                    {
                        //has digit
                        if (!hasDigit)
                        {
                            x += 1;
                            hasDigit = true;
                        }
                    }
                    else if (a >= 65 && a <= 90)
                    {
                        //has upper case
                        if (!hasUpper)
                        {
                            x += 1;
                            hasUpper = true;
                        }
                    }
                    else if (a >= 97 && a <= 122)
                    {
                        //has lower case
                        if (!hasLower)
                        {
                            x += 1;
                            hasLower = true;
                        }
                    }
                    else
                    {
                        //has special characters
                        if (!hasSpec)
                        {
                            x += 1;
                            hasSpec = true;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                x = 0;
            }
            return x;
        }

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(ms, obj);
                ms.Position = 0;
                return (T)serializer.ReadObject(ms);
            }
        }

        public static string getHash(string pwd)
        {
            HashAlgorithm hasher = SHA1.Create();
            hasher.Initialize();
            byte[] abyte = ASCIIEncoding.Default.GetBytes(pwd);
            hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
            abyte = ASCIIEncoding.Default.GetBytes("jnksntr*(YjhUB7uygH%Fg8*j(jOIPJuHG^GFBuijniojKJHBYG6FGh^&%^$6598)(");
            hasher.TransformFinalBlock(abyte, 0, abyte.Length);
            return HexEncoding.ToString(hasher.Hash);

        }

        public static String GeneratePIN(Int32 Digits)
        {
            ////'RNGCryptoServiceProvider' is obsolete: 'RNGCryptoServiceProvider is obsolete. 
            ////To generate a random number, use one of the RandomNumberGenerator static methods instead.'
            ///
            /*RNGCryptoServiceProvider pr = new RNGCryptoServiceProvider();
            Byte[] rnd = new Byte[4];
            pr.GetBytes(rnd);
            Int32 val = BitConverter.ToInt32(rnd, 0);
            Random r = new Random(val);
            String res = "";
            Digits = Math.Max(2, Math.Min(9, Digits));  // Minimum 2 digits
            String s = String.Format("1{0}", new String('0', Digits));
            Int32 MinDiff = Math.Max(2, Math.Min(6, Digits - 1));
            Int32 AUpperLimit = Convert.ToInt32(s);
            while (res == "")
            {
                s = r.Next(AUpperLimit).ToString();
                while (s.Length < Digits)
                    s = "0" + s;
                if (s.Distinct().ToArray().Length >= MinDiff)
                    res = s;
            }
            return res;*/

            ////Crypto - secure generator(no repeats)
            Digits = Math.Max(2, Math.Min(9, Digits));

            int[] pool = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            // Fisher–Yates shuffle (crypto-secure)
            for (int i = pool.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            // Build PIN from first N digits
            return string.Concat(pool.Take(Digits));
        }
    }

}
