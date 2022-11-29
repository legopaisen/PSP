using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace PSP.Models
{
    public static class Encryptor
    {
        public static void EncryptAes(string sRawFile)
        {
            string password = "5D019DABC2701C5810FD98087A7FD6640B20756B"; //set password
            byte[] salt;
            salt = Encoding.ASCII.GetBytes(password); //add salt
            string sInputFile = sRawFile;

            string sDecryptPath = "C:\\Payroll Files\\EncryptedAES";
            if (!Directory.Exists(sDecryptPath))
            {
                _ = Directory.CreateDirectory(sDecryptPath);

            }
            string sOutput = "C:\\Payroll Files\\EncryptedAES\\encrypted.txt";

            FileStream fsInput = new FileStream(sInputFile, FileMode.Open, FileAccess.Read);
            FileStream fsOutput = new FileStream(sOutput, FileMode.Create, FileAccess.Write);

            byte[] btInputFileData = new byte[Convert.ToInt32(fsInput.Length)];
            fsInput.Read(btInputFileData, 0, Convert.ToInt32(Convert.ToInt32(fsInput.Length)));

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                var key = new Rfc2898DeriveBytes(password, salt, 1000, HashAlgorithmName.SHA256); //derive key from pass and salt
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (CryptoStream csStream = new CryptoStream(fsOutput, encryptor, CryptoStreamMode.Write))
                {
                    csStream.Write(btInputFileData, 0, Convert.ToInt32(fsInput.Length));
                    csStream.FlushFinalBlock();
                    csStream.Flush();
                }
            }
            fsInput.Flush();

        }
    }
}
