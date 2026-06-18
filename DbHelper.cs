using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;

namespace PrPractice1
{
    public static class DbHelper
    {
        private static string ConnectionString = "Host=localhost;Database=work_db;Username=admin4ik;Password=1";

        public static async Task<DataTable> CheckUserAsync(string fullName, string password)
        {
            DataTable table = new DataTable();
            string sql = "SELECT id, full_name, role FROM employees WHERE full_name = @name AND passwd = @pass";

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", fullName);
                    cmd.Parameters.AddWithValue("@pass", password);

                    using (NpgsqlDataReader reader = (NpgsqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        table.Load(reader);
                    }
                }
            }
            return table;
        }

        public static async Task<DataTable> GetRequestsAsync(string statusFilter, string role, string userName)
        {
            DataTable table = new DataTable();
            string sql = @"
                SELECT r.id AS request_id, o.address, c.full_name AS client, 
                       e.full_name AS employee, r.status, r.request_date 
                FROM requests r
                JOIN objects o ON r.object_id = o.id
                JOIN clients c ON o.client_id = c.id
                JOIN employees e ON r.employee_id = e.id
                WHERE 1=1";

            if (statusFilter != "Все")
            {
                sql += " AND r.status = @status";
            }

            if (role == "Техник" || role == "Инженер")
            {
                sql += " AND e.full_name = @userName";
            }

            sql += " ORDER BY r.request_date DESC";

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    if (statusFilter != "Все") cmd.Parameters.AddWithValue("@status", statusFilter);
                    if (role == "Техник" || role == "Инженер") cmd.Parameters.AddWithValue("@userName", userName);

                    using (NpgsqlDataReader reader = (NpgsqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        table.Load(reader);
                    }
                }
            }
            return table;
        }

        public static async Task<bool> UpdateRequestStatusAsync(int requestId, string newStatus)
        {
            string sql = "UPDATE requests SET status = @status WHERE id = @id";
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@status", newStatus);
                    cmd.Parameters.AddWithValue("@id", requestId);
                    return await cmd.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public static async Task<DataTable> GetEquipmentAsync()
        {
            DataTable table = new DataTable();
            string sql = @"
                SELECT e.id, e.name AS equipment_name, s.name AS supplier, e.serial_number 
                FROM equipment e
                JOIN suppliers s ON e.supplier_id = s.id
                ORDER BY e.name";

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    using (NpgsqlDataReader reader = (NpgsqlDataReader)await cmd.ExecuteReaderAsync())
                    {
                        table.Load(reader);
                    }
                }
            }
            return table;
        }
        
        public static async Task<DataTable> GetMaterialsDictionaryAsync()
        {
            DataTable table = new DataTable();
            string sql = "SELECT id, name, unit FROM materials ORDER BY name";
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
            {
                await conn.OpenAsync();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    using (NpgsqlDataReader reader = (NpgsqlDataReader)await cmd.ExecuteReaderAsync())
                        table.Load(reader);
                }
            }
            return table;
        }

        public static async Task<bool> AssignMaterialAsync(int reqId, int matId, decimal quantity)
        {
            try
            {
                using (NpgsqlConnection conn = new NpgsqlConnection(ConnectionString))
                {
                    await conn.OpenAsync();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT assign_material_to_request(@r_id, @m_id, @qty)", conn))
                    {
                        cmd.Parameters.AddWithValue("@r_id", reqId);
                        cmd.Parameters.AddWithValue("@m_id", matId);
                        cmd.Parameters.AddWithValue("@qty", quantity);
                        await cmd.ExecuteNonQueryAsync();
                        return true;
                    }
                }
            }
            catch { return false; }
        }
    }
}