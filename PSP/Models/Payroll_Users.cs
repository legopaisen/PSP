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
        public string UserName { get; set; }
        public string Name { get; set; }
        public string AccLevel { get; set; }
        public DateTime CreatedOn { get; set; }
        public string ModifiedBy { get; set; }
        public string ModifiedOn { get; set; }
        public string Email { get; set; }
        public int Active { get; set; }

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
                                CreatedOn = dr.GetDateTime(3),
                                ModifiedBy = dr.IsDBNull(4) ? null : dr.GetString(4),
                                ModifiedOn = dr.IsDBNull(5) ? null : dr.GetDateTime(5).ToString(),
                                Email = dr.IsDBNull(6) ? null : dr.GetString(6)
                            });
                        }
                }
            }
            return list;
        }

        public int InsertUser(Payroll_Users_MODEL model)
        {
            int iReturn = 0;
            string sQuery = string.Empty;
            sQuery = "INSERT INTO Payroll_Users (UserName, Name, AccLevel, CreatedOn, ModifiedBy, ModifiedOn, Email) VALUES";
            sQuery += $" ('{model.UserName}', ";
            sQuery += $" '{model.Name}', ";
            sQuery += $" '{model.AccLevel}', ";
            sQuery += $" '{model.CreatedOn}', ";
            sQuery += $" '{model.ModifiedBy}', ";
            sQuery += $" '{model.ModifiedOn}', ";
            sQuery += $" '{model.Email}')";

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
                                CreatedOn = dr.GetDateTime(3),
                                ModifiedBy = dr.GetString(4) == null ? "" : "",
                                ModifiedOn = dr.GetDateTime(5).ToString() == null ? "" : "",
                                Email = dr.GetString(6).ToString() == null ? "" : ""
                            });
                        }
                }
            }
            return list;
        }

        public int ActivateUser(string UserName)
        {
            string sQuery = string.Empty;
            int iCurrActive = 0;
            int iSetActive = 0;

            using(SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString))
            {
                using(SqlCommand cmd = con.CreateCommand())
                {
                    con.Open();
                    cmd.CommandText = $"select Active from payroll_users WHERE UserName = '{UserName}'";
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            iCurrActive = reader.GetInt32(0);
                        }
                        if (iCurrActive == 1)
                            iSetActive = 0;
                        else
                            iSetActive = 1;
                    }

                    cmd.CommandText = $"UPDATE Payroll_Users SET Active = '{iSetActive}' WHERE UserName = '{UserName}'";
                    if (cmd.ExecuteNonQuery() == 0)
                    { }
                }
            }
            return iSetActive;
        }

        public int EditUser(Payroll_Users_MODEL model)
        {
            string sQuery = string.Empty;
            int iReturn = 0;
            sQuery = "UPDATE Payroll_Users SET ";
            sQuery += $"AccLevel = '{model.AccLevel}', ModifiedBy = '{model.ModifiedBy}', ModifiedOn = '{model.ModifiedOn}' WHERE UserName = '{model.UserName}'";

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

        //public int ChangePassword(Payroll_Users_MODEL model)
        //{
        //    int iReturn = 0;
        //    string sQuery = string.Empty;
        //    sQuery = $"UPDATE Payroll_Users SET Email = '{model.Email}' WHERE UserName = '{model.UserName}'";
        //    using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString))
        //    {
        //        using (SqlCommand cmd = con.CreateCommand())
        //        {
        //            cmd.CommandText = sQuery;
        //            con.Open();
        //            if (cmd.ExecuteNonQuery() == 0)
        //            { }
        //            else
        //                iReturn++;
        //        }
        //    }
        //    return iReturn;
        //}
    }
}
