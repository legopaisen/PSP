using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PSP.Models
{
    public static class Decryptor
    {
        public static void Decrypt(string sFilePath)
        {
            byte[] btKey;
            btKey = Encoding.ASCII.GetBytes("5D019DABC2701C5810FD98087A7FD6640B20756B");
            string sInputFile = sFilePath;
            string sDecryptPath = "C:\\Payroll Files\\temp";
            if (!Directory.Exists(sDecryptPath))
            {
                _ = Directory.CreateDirectory(sDecryptPath);

            }
            string sOutput = "C:\\Payroll Files\\temp\\decrypted.txt";
            FileStream fsInput = null;
            FileStream fsOutput = null;

            //Let's derive some salt key    
            Rfc2898DeriveBytes deriveKey = new Rfc2898DeriveBytes("5D019DABC2701C5810FD98087A7FD6640B20756B", btKey); ;
            //Let's create the algorithm and specify the key and IV
            RijndaelManaged rmManaged = new RijndaelManaged();
            rmManaged.Padding = PaddingMode.PKCS7;
            rmManaged.Key = deriveKey.GetBytes(Convert.ToInt32(rmManaged.KeySize / 8));
            rmManaged.IV = deriveKey.GetBytes(Convert.ToInt32(rmManaged.BlockSize / 8));

            ICryptoTransform decryptor;
            decryptor = rmManaged.CreateDecryptor();

            //Let's read the file from input file.
            fsInput = new FileStream(sInputFile, FileMode.Open, FileAccess.Read);
            fsOutput = new FileStream(sOutput, FileMode.Create, FileAccess.Write);

            byte[] btInputFileData = new byte[Convert.ToInt32(fsInput.Length)];
            fsInput.Read(btInputFileData, 0, Convert.ToInt32(Convert.ToInt32(fsInput.Length)));

            //Create a CryptoStream object using the Stream and ICryptoTransform objects.
            using(CryptoStream csStream = new CryptoStream(fsOutput, decryptor, CryptoStreamMode.Write))
            {
                csStream.Write(btInputFileData, 0, Convert.ToInt32(fsInput.Length));
                csStream.FlushFinalBlock();
                csStream.Flush();
            }
            fsInput.Flush();
        }

    }
}
