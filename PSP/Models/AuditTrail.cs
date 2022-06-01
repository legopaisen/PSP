using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PSP.Models
{
    public class AuditTrailModel
    {
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
            return list;
        }

        public int Insert(AuditTrailModel model)
        {
            int iReturn = 0;
            string sQuery = string.Empty;
            sQuery = "INSERT INTO tbl_audit_trail (UserName, Details, Date_Time) VALUES ";
            sQuery += $"('{model.UserName}', ";
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
