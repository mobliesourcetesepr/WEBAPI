using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WEBAPI.Models;
namespace WEBAPI.Helpers
{
public  class AesEncryption
{
    private static readonly byte[] Key = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes key (AES-128)
    private static readonly byte[] IV = Encoding.UTF8.GetBytes("abcdefghijklmnop");  // 16 bytes IV


    public static string GenerateToken(string username)
    {
        var session = new SessionToken
        {
            Username = username,
            IssuedAt = DateTime.UtcNow,
        };

        string json = JsonSerializer.Serialize(session);
        string token = Encrypt(json);
        SessionStore.UpdateLastAccess(token); 
        return token;
    }


    public static string Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        using StreamWriter sw = new(cs);
        sw.Write(plainText);
        sw.Flush();
        cs.FlushFinalBlock();

        return Convert.ToBase64String(ms.ToArray());
    }

    public static string Decrypt(string cipherText)
    {
        byte[] buffer = Convert.FromBase64String(cipherText);
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using MemoryStream ms = new(buffer);
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        return sr.ReadToEnd();
    }
        public static bool IsTokenValid(string token, out SessionToken session)
        {
            session = null;

            try
            {
                if (!SessionStore.IsTokenAlive(token))
                    return false;

                string decryptedJson = Decrypt(token);
                session = JsonSerializer.Deserialize<SessionToken>(decryptedJson);

                if (session == null)
                    return false;

                SessionStore.UpdateLastAccess(token); // Keep it alive
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}





