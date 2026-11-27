using System.Security.Cryptography;
using System.Text;

namespace Tool_Utility.Encryption;

public static class Encryption
{
    private static readonly string publicKey =
        "<RSAKeyValue><Modulus>q+CpBMGiwyFuWpWSNP9Gs0+sy6ip5Gqy4bESx/u60vJT+ZkT7UPGhZq+BdudCzIfW3NP342YhU6s0+6gpFSzjvaEY0O8ejMAjOjDpu3n4j3IzVd/Y76dNBxszLUaCD2LNSnkCesEErltBrnNOWSPOWqjyRP16gID0t6Uc0lWqp8=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

    private static readonly string privateKey =
        "<RSAKeyValue><Modulus>q+CpBMGiwyFuWpWSNP9Gs0+sy6ip5Gqy4bESx/u60vJT+ZkT7UPGhZq+BdudCzIfW3NP342YhU6s0+6gpFSzjvaEY0O8ejMAjOjDpu3n4j3IzVd/Y76dNBxszLUaCD2LNSnkCesEErltBrnNOWSPOWqjyRP16gID0t6Uc0lWqp8=</Modulus><Exponent>AQAB</Exponent><P>1WZfnL71PV9TOFuFTKEvHOtRgTxc6loxL1aEfsSwZxGwti+M3o06T3p4hQfjqWZH1MSh2hTGamKFD9eW+Gb1/Q==</P><Q>zjBSowlT5DemxxbOrsgE8XCUwQg3toIVttrfqjKiJEjfci+f1niXTSO/1ojKPiev/Msr1/D24pjTE7hxA413yw==</Q><DP>xVf8x3mYVP8YeJ+6L9lHNlkvcHIgAuERZxN9R7Yg48/qOTDT9uNnryQ02+El2fwqJUsJ2iEoNC2Tm8jlZiXhCQ==</DP><DQ>uwQjgBNuzxoXII80Sl3L9f4qPmeouNaGwYMGycYBQS4D1fsSXCKnAWXCk+Egp32x53WUv9mk5OS+1y08/H7tPw==</DQ><InverseQ>JElzBQi1WXqDK5iwvO/cq5hrpr5y+cu9Kr6cJyePQ+mceKlDIKqhGa7uQ33HFWKfNyCP/L1931jle3QVW5iu6A==</InverseQ><D>U9eOe0x9uhP3weZOYN3ZviRylRIcddj6k4+OKtGH69jbz9i1WLsD/y5h3Xeonsp/c1vHrbRjnzumoOc5o4/R2nGGkx/roXft+QUpb33s/8kQpZS8kEXiU5Rt3GznjZxzkDoydroc4eRMmWxsulprH3WtwqWexCA3F69c0373Gdk=</D></RSAKeyValue>";


    public static string base64Encrypt(this string strText)
    {
        var testData = Encoding.UTF8.GetBytes(strText);

        using (var rsa = new RSACryptoServiceProvider(1024))
        {
            try
            {
                // client encrypting data with public key issued by server                    
                rsa.FromXmlString(publicKey);

                var encryptedData = rsa.Encrypt(testData, true);

                var base64Encrypted = Convert.ToBase64String(encryptedData);

                return base64Encrypted;
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }

    public static byte[] Encrypt(this string strText)
    {
        var testData = Encoding.UTF8.GetBytes(strText);

        using (var rsa = new RSACryptoServiceProvider(4096))
        {
            try
            {
                // client encrypting data with public key issued by server                    
                rsa.FromXmlString(publicKey);

                var encryptedData = rsa.Encrypt(testData, true);

                return encryptedData;
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }

    public static string DecryptionFromBase64String(this string strText)
    {
        using (var rsa = new RSACryptoServiceProvider(1024))
        {
            try
            {
                var base64Encrypted = strText;

                // server decrypting data with private key                    
                rsa.FromXmlString(privateKey);

                var resultBytes = Convert.FromBase64String(base64Encrypted);
                var decryptedBytes = rsa.Decrypt(resultBytes, true);
                var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedData;
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }

    public static string Decryption(this byte[] Data)
    {
        using (var rsa = new RSACryptoServiceProvider(4096))
        {
            try
            {
                // server decrypting data with private key                    
                rsa.FromXmlString(privateKey);

                var decryptedBytes = rsa.Decrypt(Data, true);
                var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedData;
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }
    }

    public static string SignData(this string message)
    {
        //// The array to store the signed message in bytes
        byte[] signedBytes;
        using (var rsa = new RSACryptoServiceProvider())
        {
            //// Write the message to a byte array using UTF8 as the encoding.
            var encoder = new UTF8Encoding();
            var originalData = encoder.GetBytes(message);

            try
            {
                //// Import the private key used for signing the message
                rsa.FromXmlString(privateKey);

                //// Sign the data, using SHA512 as the hashing algorithm 
                signedBytes = rsa.SignData(originalData, CryptoConfig.MapNameToOID("SHA512"));
            }
            catch (CryptographicException e)
            {
                return null;
            }
            finally
            {
                //// Set the keycontainer to be cleared when rsa is garbage collected.
                rsa.PersistKeyInCsp = false;
            }
        }

        //// Convert the a base64 string before returning
        return Convert.ToBase64String(signedBytes);
    }

    public static bool VerifyData(string originalMessage, string signedMessage)
    {
        var success = false;
        using (var rsa = new RSACryptoServiceProvider())
        {
            //Don't do this, do the same as you did in SignData:
            //byte[] bytesToVerify = Convert.FromBase64String(originalMessage);
            var encoder = new UTF8Encoding();
            var bytesToVerify = encoder.GetBytes(originalMessage);

            var signedBytes = Convert.FromBase64String(signedMessage);
            try
            {
                rsa.FromXmlString(publicKey);

                var Hash = new SHA512Managed();

                var hashedData = Hash.ComputeHash(signedBytes);

                success = rsa.VerifyData(bytesToVerify, CryptoConfig.MapNameToOID("SHA512"), signedBytes);
            }
            catch (CryptographicException e)
            {
            }
            finally
            {
                rsa.PersistKeyInCsp = false;
            }
        }

        return success;
    }

    private static byte[] PerformCryptography(ICryptoTransform cryptoTransform, byte[] data)
    {
        using (var memoryStream = new MemoryStream())
        {
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                cryptoStream.Write(data, 0, data.Length);
                cryptoStream.FlushFinalBlock();
                return memoryStream.ToArray();
            }
        }
    }
    //public static byte[] AesEncryptString(this string plainText, string Salt = "")
    //{
    //    if (string.IsNullOrWhiteSpace(Salt))
    //    {
    //        Salt = "";
    //    }

    //    byte[] encrypted;
    //    // Create a new AesManaged.
    //    using (AesManaged aes = new AesManaged())
    //    {
    //        // Create encryptor
    //        ICryptoTransform encryptor = aes.CreateEncryptor(Encoding.ASCII.GetBytes((Container.GetlicenceKey() + Salt).MD5Hash()), Encoding.ASCII.GetBytes(Container.GetlicenceIV()));
    //        // Create MemoryStream
    //        using (MemoryStream ms = new MemoryStream())
    //        {
    //            // Create crypto stream using the CryptoStream class. This class is the key to encryption
    //            // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream
    //            // to encrypt
    //            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
    //            {
    //                // Create StreamWriter and write data to a stream
    //                using (StreamWriter sw = new StreamWriter(cs))
    //                    sw.Write(plainText);

    //                encrypted = ms.ToArray();
    //            }
    //        }
    //    }
    //    // Return encrypted data
    //    return encrypted;
    //}
    //public static string AesDecryptString(this byte[] cipherText, string Salt = "")
    //{
    //    try
    //    {
    //        if (string.IsNullOrWhiteSpace(Salt))
    //        {
    //            Salt = "";
    //        }
    //        string plaintext = null;
    //        // Create AesManaged
    //        using (AesManaged aes = new AesManaged())
    //        {
    //            ICryptoTransform decryptor = aes.CreateDecryptor(Encoding.ASCII.GetBytes((Container.GetlicenceKey() + Salt).MD5Hash()), Encoding.ASCII.GetBytes(Container.GetlicenceIV()));
    //            // Create a decryptor

    //            // Create the streams used for decryption.
    //            using (MemoryStream ms = new MemoryStream(cipherText))
    //            {
    //                // Create crypto stream
    //                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
    //                {
    //                    // Read crypto stream
    //                    using (StreamReader reader = new StreamReader(cs))
    //                        plaintext = reader.ReadToEnd();
    //                }
    //            }
    //        }
    //        return plaintext;
    //    }
    //    catch (Exception e)
    //    {
    //        throw new ServiceExeption("رمز گشایی نامه با خطا مواجه شد. لطفا رمز نامه را بررسی فرمایید.");

    //    }
    //}
    //public static byte[] AesEncryptByte(this byte[] clearBytes, string Salt = "")
    //{

    //    if (string.IsNullOrWhiteSpace(Salt))
    //    {
    //        Salt = "";
    //    }
    //    byte[] encrypted;
    //    // Create a new AesManaged.
    //    using (AesManaged aes = new AesManaged())
    //    {

    //        // Create encryptor
    //        ICryptoTransform encryptor = aes.CreateEncryptor(Encoding.ASCII.GetBytes((Container.GetlicenceKey() + Salt).MD5Hash()), Encoding.ASCII.GetBytes(Container.GetlicenceIV()));
    //        // Create MemoryStream
    //        using (MemoryStream ms = new MemoryStream())
    //        {
    //            // Create crypto stream using the CryptoStream class. This class is the key to encryption
    //            // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream
    //            // to encrypt
    //            using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
    //            {
    //                // Create StreamWriter and write data to a stream
    //                cs.Write(clearBytes, 0, clearBytes.Length);
    //                cs.FlushFinalBlock();
    //                encrypted = ms.ToArray();
    //            }
    //        }
    //    }
    //    // Return encrypted data
    //    return encrypted;
    //}
    //public static byte[] AesDecryptByte(this byte[] cipherBytes, string Salt)
    //{
    //    if (string.IsNullOrWhiteSpace(Salt))
    //    {
    //        Salt = "";
    //    }
    //    // Create AesManaged
    //    try
    //    {
    //        using (AesManaged aes = new AesManaged())
    //        {
    //            ICryptoTransform decryptor = aes.CreateDecryptor(Encoding.ASCII.GetBytes((Container.GetlicenceKey() + Salt).MD5Hash()), Encoding.ASCII.GetBytes(Container.GetlicenceIV()));
    //            // Create a decryptor
    //            // Create the streams used for decryption.
    //            using (MemoryStream ms = new MemoryStream())
    //            {
    //                // Create crypto stream
    //                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
    //                {
    //                    cs.Write(cipherBytes, 0, cipherBytes.Length);
    //                    cs.FlushFinalBlock();
    //                    return ms.ToArray();
    //                }
    //            }
    //        }
    //    }
    //    catch (Exception e)
    //    {
    //        throw new ServiceExeption("اطلاعات وارد شده در سامانه دستکاری شده است و به دلیل مسائل امنیتی امکان نمایش اطلاعات وجود ندارد.");
    //    }

    //}

    public static string SHA256Hash(this string Text)
    {
        using (var sha256Hash = SHA256.Create())
        {
            // ComputeHash - returns byte array  
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(Text));

            // Convert byte array to a string   
            var builder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
            return builder.ToString();
        }
    }

    public static string MD5Hash(this string Text)
    {
        using (var MD5Hash = MD5.Create())
        {
            // ComputeHash - returns byte array  
            var bytes = MD5Hash.ComputeHash(Encoding.UTF8.GetBytes(Text));

            // Convert byte array to a string   
            var builder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i++) builder.Append(bytes[i].ToString("x2"));
            return builder.ToString();
        }
    }
}