using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PSP
{
    public class SqlHelper
    {
        public static string connectionString;
        public static string oracleConnectionString;
        public static SqlConnection GetConnection()
        {
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                return connection;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static OracleConnection GetOracleConnection()
        {
            try
            {
                OracleConnection connection = new OracleConnection(oracleConnectionString);
                return connection;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
