using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PSP.Models
{
    public class Payroll_Users_MODEL
    {
        public string Control_No { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string AccLevel { get; set; }
        public string Password { get; set; }
        public DateTime ExpDate { get; set; }
        public int ErrorCtrl { get; set; }
        public int Locked { get; set; }
    }
    public class Payroll_Users : IDisposable
    {
        //private readonly IConfiguration _configuration;
        //public string connStr = string.Empty;
        public void Dispose() { GC.SuppressFinalize(this); }
        
        //private Payroll_Users(IConfiguration configuration)
        //{
        //    _configuration = configuration;
        //    connStr = _configuration.GetConnectionString("DefaultConnection");
        //}
       public List<Payroll_Users_MODEL> GetList()
        {
            List<Payroll_Users_MODEL> list = new List<Payroll_Users_MODEL>();
            string sQuery = string.Empty;
            sQuery = "select * from payroll_users ";
            sQuery += "order by UserName";

            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = sQuery;
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                        while (dr.Read())
                        {
                            list.Add(new Payroll_Users_MODEL()
                            {
                                UserName = dr.GetString(0).ToString(),
                                Name = dr.GetString(1).ToString(),
                                AccLevel = dr.GetString(2).ToString(),
                                Password = dr.GetString(3).ToString(),
                                ExpDate = dr.GetDateTime(4),
                                ErrorCtrl = dr.GetInt32(5),
                                Locked = dr.GetInt32(6)
                            });
                        }
                }
            }
            return list;
        }

        public int InsertUser(Payroll_Users_MODEL model)
        {
            int iReturn = 0;
            DateTime dtExp = model.ExpDate.AddMonths(1);
            string sQuery = string.Empty;
            sQuery = "INSERT INTO Payroll_Users (UserName, Name, AccLevel, Password, ExpDate, ErrorCtrl, Locked) VALUES";
            sQuery += $" ('{model.UserName}', ";
            sQuery += $" '{model.Name}', ";
            sQuery += $" '{model.AccLevel}', ";
            sQuery += $" '{model.Password}', ";
            sQuery += $" '{dtExp.ToString("yyyy-MM-dd")}', ";
            sQuery += $" '{model.ErrorCtrl}', ";
            sQuery += $" '{model.Locked}')";

            using(SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using(SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = sQuery;
                    con.Open();
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                    else
                        iReturn++;
                }
            }
            return iReturn;
        }

        public List<Payroll_Users_MODEL> GetUser(string UserName)
        {
            List<Payroll_Users_MODEL> list = new List<Payroll_Users_MODEL>();
            string sQuery = string.Empty;
            sQuery = "select * from payroll_users ";
            sQuery += $"where UserName = '{UserName}'";

            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString.ToString()))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = sQuery;
                    con.Open();
                    using (SqlDataReader dr = cmd.ExecuteReader())
                        while (dr.Read())
                        {
                            list.Add(new Payroll_Users_MODEL()
                            {
                                UserName = dr.GetString(0).ToString(),
                                Name = dr.GetString(1).ToString(),
                                AccLevel = dr.GetString(2).ToString(),
                                Password = dr.GetString(3).ToString(),
                                ExpDate = dr.GetDateTime(4),
                                ErrorCtrl = dr.GetInt32(5),
                                Locked = dr.GetInt32(6)
                            });
                        }
                }
            }
            return list;
        }

        public int DeleteUser(string UserName)
        {
            string sQuery = string.Empty;
            int iReturn = 0;
            sQuery = $"DELETE FROM Payroll_Users WHERE UserName = '{UserName}'";

            using(SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString))
            {
                using(SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = sQuery;
                    con.Open();
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                    else
                        iReturn++;
                }
            }
            return iReturn;
        }

        public int EditUser(Payroll_Users_MODEL model)
        {
            string sQuery = string.Empty;
            int iReturn = 0;
            sQuery = "UPDATE Payroll_Users SET ";
            if (string.IsNullOrEmpty(model.Name))
                sQuery += $"AccLevel = '{model.AccLevel}' WHERE UserName = '{model.UserName}'";
            else
                sQuery += $"Name = '{model.Name}', AccLevel = '{model.AccLevel}' WHERE UserName = '{model.UserName}'";

            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = sQuery;
                    con.Open();
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                    else
                        iReturn++;
                }
            }
            return iReturn;
        }

        public int ChangePassword(Payroll_Users_MODEL model)
        {
            int iReturn = 0;
            string sQuery = string.Empty;
            sQuery = $"UPDATE Payroll_Users SET Password = '{model.Password}' WHERE UserName = '{model.UserName}'";
            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = sQuery;
                    con.Open();
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                    else
                        iReturn++;
                }
            }
            return iReturn;
        }
    }
}
