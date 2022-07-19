using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;

public class SystemCore
{
    public static string Connectionstring { get; set; }
    public static string ApplicationURL { get; set; }
    public static string SecurityKey { get; set; }

    public struct SystemResponse
    {
        public ResponseStatus ResponseStatus { get; set; }
        public string Description { get; set; }
    }

    //TODO DAGDAG - Dhell 20161110
    public class SearchResponse
    {
        public long total { get; set; }
        public List<object> rows { get; set; }
    }

    public enum ResponseStatus
    {
        SUCCESS = 1
      , FAILED = 0
    }

    public string DecryptStringAES(string cipherText, string pHashCode)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(pHashCode);
        byte[] numArray = Encoding.UTF8.GetBytes(pHashCode.Substring(0, 16));
        return this.DecryptStringFromBytes_Aes(Convert.FromBase64String(cipherText), bytes, numArray);
    }

    private string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        if (cipherText == null || (int)cipherText.Length <= 0)
        {
            throw new ArgumentNullException("cipherText");
        }
        if (Key == null || (int)Key.Length <= 0)
        {
            throw new ArgumentNullException("Key");
        }
        if (IV == null || (int)IV.Length <= 0)
        {
            throw new ArgumentNullException("IV");
        }
        string end = null;
        using (AesCryptoServiceProvider aesCryptoServiceProvider = new AesCryptoServiceProvider())
        {
            aesCryptoServiceProvider.Key = Key;
            aesCryptoServiceProvider.IV = IV;
            ICryptoTransform cryptoTransform = aesCryptoServiceProvider.CreateDecryptor(aesCryptoServiceProvider.Key, aesCryptoServiceProvider.IV);
            using (MemoryStream memoryStream = new MemoryStream(cipherText))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        end = streamReader.ReadToEnd();
                    }
                }
            }
        }
        return end;
    }


    //public static string GetIPAddress()
    //{
    //    string strReturn = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
    //    string strNetworkID = HttpContext


    //    if (String.IsNullOrEmpty(strReturn))
    //        strReturn = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

    //    if (string.IsNullOrEmpty(strReturn))
    //        strReturn = HttpContext.Current.Request.UserHostAddress;

    //    if (string.IsNullOrEmpty(strReturn) || strReturn.Trim() == "::1")
    //    {
    //        strReturn = "127.0.0.1";
    //    }

    //    try
    //    {
    //        strReturn += "/" + System.Net.Dns.GetHostEntry(strReturn).HostName.Split(new char[] { '.' })[0].ToString();
    //    }
    //    catch (Exception e)
    //    {
    //        CTBC.Logs.Write("GetIPAddress", e.Message, "SystemCore");

    //    }
    //    return strReturn;
    //}

    public struct Differences
    {
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }

    public static string ToStringDifferences(List<Differences> pData)
    {
        string strReturn = "";
        string strOldValue = "";
        string strNewValue = "";
        foreach (Differences diff in pData)
        {
            if (diff.NewValue == " ")
            {
                strNewValue += diff.NewValue;
            }
            else
            {
                strOldValue += diff.FieldName + ":" + diff.OldValue + ",";
                strNewValue += diff.FieldName + ":" + diff.NewValue + ",";
            }
        }
        strReturn = strOldValue + "|" + strNewValue;
        return strReturn;
    }

    public List<Differences> GetDiffirence(object pOriginalModel, object pUpdatedModel)
    {
        List<Differences> list = new List<Differences>();

        foreach (var Model2 in pUpdatedModel.GetType().GetProperties().Where(x => pOriginalModel.GetType().GetProperties().Where(y => y.Name.Equals(x.Name)).ToList().Count > 0))
        {
            string strNewValue = Model2.GetValue(pUpdatedModel, null) == null ? " " : Model2.GetValue(pUpdatedModel, null).ToString();

            foreach (var Model1 in pOriginalModel.GetType().GetProperties().Where(xx => pUpdatedModel.GetType().GetProperties().Where(y => y.Name.Equals(xx.Name)).ToList().Count > 0))
            {
                string strOldValue = Model1.GetValue(pOriginalModel, null) == null ? " " : Model1.GetValue(pOriginalModel, null).ToString();

                if (Model2.Name.Equals(Model1.Name))
                {
                    if (!strNewValue.Equals(strOldValue))
                    {
                        list.Add(new Differences()
                        {
                            FieldName = Model2.Name,
                            OldValue = strOldValue,
                            NewValue = strNewValue
                        });
                    }
                }
            }
        }
        return list;
    }

    public string CreateSessionHash()
    {
        string strReturn = "";
        Random random = new Random();
        int intRandomNumber = random.Next(Convert.ToInt32(DateTime.Now.ToString("yyyyddMM")));
        strReturn = CTBC.Cryptography.AES.CreateMD5(DateTime.Now.ToString("yyyyddMMhhmmss") + intRandomNumber.ToString()).Substring(0, 32);
        return strReturn;
    }

    public string GetCrossDomainResponse(string pUrl)
    {
        string strReturn = "";
        HttpWebRequest request = null;//
        HttpWebResponse response = null;//
        StreamReader reader = null;// 
        try
        {
            request = (HttpWebRequest)WebRequest.Create(pUrl);
            response = (HttpWebResponse)request.GetResponse();
            reader = new StreamReader(response.GetResponseStream());
            strReturn = reader.ReadToEnd();
        }
        catch (Exception e)
        {
            CTBC.Logs.Write("GetCrossDomainResponse", e.Message, "SystemCore");

        }
        finally
        {
            request = null;
            if (response != null)
            {
                response.Close();
                response = null;
            }
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }
        }
        return strReturn;
    }

    public class TableSearchResponse
    {
        public int searchtotal { get; set; }
        public List<object> searchrows { get; set; }
    }
    /// <summary>
    /// For Boostrap-Table by wenzhixin only
    /// </summary>
    public class Search
    {
        public string OrderBy { get; set; }
        public string SearchString { get; set; }
        public int? Limit { get; set; }
        public int PageNumber { get; set; }
        public Search()
        {
            this.OrderBy = "ASC";
            this.SearchString = "";
            this.Limit = this.Limit == null ? 10 : this.Limit;
        }

    }

}
//}