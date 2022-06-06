using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PSP.Models
{
    public class AuditTrailModel
    {
        public int Control_No{ get; set; }
        public string UserName { get; set; }
        public string Details { get; set; }
        public DateTime Date_Time { get; set; }
    }
    public class AuditTrail : IDisposable
    {
        public void Dispose() { GC.SuppressFinalize(this); }

        public List<AuditTrailModel> GetList()
        {
            List<AuditTrailModel> list = new List<AuditTrailModel>();
            string sQuery = string.Empty;
            sQuery = @"select AT.*, CD.Description from tbl_audit_trail AT, tbl_control_desc CD
                    where CD.Control_No = AT.Control_No";

            using (SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString))
            {
                using (SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = sQuery;
                    con.Open();
                    using(SqlDataReader reader = cmd.ExecuteReader())
                        while(reader.Read())
                        {
                            list.Add(new AuditTrailModel()
                            {
                                Control_No = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                Details = reader.GetString(4) + " - " + reader.GetString(2),
                                Date_Time = reader.GetDateTime(3)
                            });
                        }
                }
            }
            return list;
        }

        public int Insert(AuditTrailModel model)
        {
            int iReturn = 0;
            string sQuery = string.Empty;
            sQuery = "INSERT INTO tbl_audit_trail (Control_No, UserName, Details, Date_Time) VALUES ";
            sQuery += $"('{model.Control_No}', ";
            sQuery += $"'{model.UserName}', ";
            sQuery += $"'{model.Details}', ";
            sQuery += $"'{model.Date_Time}') ";

            using(SqlConnection con = new SqlConnection(SqlHelper.GetConnection().ConnectionString))
            {
                using(SqlCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = sQuery;
                    con.Open();
                    iReturn = cmd.ExecuteNonQuery();
                }
            }
            return iReturn;
        }
    }
}
