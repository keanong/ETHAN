using System;
using System.Security.Cryptography;
using System.Text;

namespace ETHAN.classes
{
    public class Hash
    {
        public static String GetPasswordHash(String AID, String APassword)
        {
            using (HashAlgorithm hasher = SHA1.Create())
            {
                hasher.Initialize();
                byte[] abyte = ASCIIEncoding.Default.GetBytes(AID);
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                abyte = ASCIIEncoding.Default.GetBytes("XDel Singapore Pte Ltd2002");
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                abyte = ASCIIEncoding.Default.GetBytes(APassword);
                hasher.TransformFinalBlock(abyte, 0, abyte.Length);
                //hasher.ComputeHash(ASCIIEncoding.Default.GetBytes(AID + "XDel Singapore Pte Ltd 2007" + APassword));
                return HexEncoding.ToString(hasher.Hash).ToUpper();
            }
        }

        public static String GetHash(String AString)
        {
            using (HashAlgorithm hasher = SHA1.Create())
            {
                hasher.Initialize();
                byte[] abyte = ASCIIEncoding.Default.GetBytes(AString);
                hasher.TransformFinalBlock(abyte, 0, abyte.Length);
                return HexEncoding.ToString(hasher.Hash).ToUpper();
            }
        }

        public static String GetSaltedHash(String AString, String Salt)
        {
            using (HashAlgorithm hasher = SHA1.Create())
            {
                hasher.Initialize();
                byte[] abyte = ASCIIEncoding.Default.GetBytes(AString);
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                abyte = ASCIIEncoding.Default.GetBytes(Salt);
                hasher.TransformFinalBlock(abyte, 0, abyte.Length);
                return HexEncoding.ToString(hasher.Hash).ToUpper();
            }
        }

        public static String GetEncodedHash(String AString)
        {
            using (HashAlgorithm hasher = SHA1.Create())
            {
                hasher.Initialize();
                byte[] abyte = ASCIIEncoding.Default.GetBytes(AString);
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                abyte = ASCIIEncoding.Default.GetBytes("jnksntr*(YjhUB7uygH%Fg8*j(jOIPJuHG^GFBuijniojKJHBYG6FGh^&%^$6598)(");
                hasher.TransformFinalBlock(abyte, 0, abyte.Length);
                return HexEncoding.ToString(hasher.Hash).ToUpper();
            }
        }

        public static String GetSaltedEncodedHash(String AString, String Salt)
        {
            using (HashAlgorithm hasher = SHA1.Create())
            {
                hasher.Initialize();
                byte[] abyte = ASCIIEncoding.Default.GetBytes(AString);
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                abyte = ASCIIEncoding.Default.GetBytes(Salt);
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                abyte =
                    ASCIIEncoding.Default.GetBytes("jnksntr*(YjhUB7uygH%Fg8*j(jOIPJuHG^GFBuijniojKJHBYG6FGh^&%^$6598)(");
                hasher.TransformFinalBlock(abyte, 0, abyte.Length);
                return HexEncoding.ToString(hasher.Hash).ToUpper();
            }
        }

        public static String GetDailyEncodedHash(String AString)
        {
            using (HashAlgorithm hasher = SHA1.Create())
            {
                hasher.Initialize();
                byte[] abyte = ASCIIEncoding.Default.GetBytes(AString);
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                abyte = ASCIIEncoding.Default.GetBytes(DateTime.Now.Date.ToString("ddMMyyyy"));
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                abyte =
                    ASCIIEncoding.Default.GetBytes("jnksntr*(YjhUB7uygH%Fg8*j(jOIPJuHG^GFBuijniojKJHBYG6FGh^&%^$6598)(");
                hasher.TransformFinalBlock(abyte, 0, abyte.Length);
                return HexEncoding.ToString(hasher.Hash).ToUpper();
            }
        }

        public static String GetByteHash(Byte[] abyte)
        {
            using (HashAlgorithm hasher = SHA1.Create())
            {
                hasher.Initialize();
                hasher.TransformFinalBlock(abyte, 0, abyte.Length);
                return HexEncoding.ToString(hasher.Hash).ToUpper();
            }
        }

        public static String GetEncodedByteHash(Byte[] abyte)
        {
            using (HashAlgorithm hasher = SHA1.Create())
            {
                hasher.Initialize();
                hasher.TransformBlock(abyte, 0, abyte.Length, abyte, 0);
                Byte[] saltbyte =
                    ASCIIEncoding.Default.GetBytes("jnksntr*(YjhUB7uygH%Fg8*j(jOIPJuHG^GFBuijniojKJHBYG6FGh^&%^$6598)(");
                hasher.TransformFinalBlock(saltbyte, 0, saltbyte.Length);
                return HexEncoding.ToString(hasher.Hash).ToUpper();
            }
        }
    }
}
