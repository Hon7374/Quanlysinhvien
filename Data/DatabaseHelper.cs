using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace StudentManagement.Data
{
    public class DatabaseHelper
    {
        private static string connectionString;

        static DatabaseHelper()
        {
            connectionString = ConfigurationManager.ConnectionStrings["StudentManagementDB"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = @"Data Source=.;Initial Catalog=StudentManagementDB;Integrated Security=True;TrustServerCertificate=True";
            }
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static bool TestConnection()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            DataTable dataTable = new DataTable();
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 60;
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing query.", ex);
            }
            return dataTable;
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 60;
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing non-query.", ex);
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandTimeout = 60;
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        return cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing scalar.", ex);
            }
        }

        public static int ExecuteNonQueryStoredProcedure(string spName, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(spName, conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 60; // Increase timeout to 60 seconds
                        if (parameters != null)
                        {
                            cmd.Parameters.AddRange(parameters);
                        }
                        // Prepare to capture stored procedure return value
                        var returnParameter = new SqlParameter("@RETURN_VALUE", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };
                        cmd.Parameters.Add(returnParameter);

                        cmd.ExecuteNonQuery();
                        // Return the stored procedure integer return value if set, otherwise return rows affected
                        try
                        {
                            if (returnParameter.Value != DBNull.Value && returnParameter.Value != null)
                                return Convert.ToInt32(returnParameter.Value);
                        }
                        catch { }
                        return 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Preserve the inner exception for diagnostics but provide context
                throw new Exception("Error executing stored procedure: " + ex.Message, ex);
            }
        }
    }
}
