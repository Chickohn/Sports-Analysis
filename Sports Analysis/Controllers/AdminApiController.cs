using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Sports_Analysis.Data;
using System.Data;
using CsvHelper;
using System.Globalization;

namespace Sports_Analysis.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminApiController : ControllerBase
    {
        private readonly FootballDbContext _context;
        private readonly string _connectionString;

        public AdminApiController(FootballDbContext context)
        {
            _context = context;
            _connectionString = context.Database.GetConnectionString();
        }

        [HttpPost("UploadCsvTable")]
        public async Task<IActionResult> UploadCsvTable([FromForm] IFormFile file, [FromForm] string tableName)
        {
            if (file == null || file.Length == 0 || string.IsNullOrWhiteSpace(tableName))
                return BadRequest("File or table name missing.");

            // Sanitize table name
            tableName = tableName.Replace(" ", "_").Replace("-", "_");

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            using var dr = new CsvDataReader(csv);
            var dt = new DataTable();
            dt.Load(dr);

            // Build CREATE TABLE SQL
            var columns = dt.Columns.Cast<DataColumn>().Select(col => $"[{col.ColumnName}] NVARCHAR(MAX)");
            var createTableSql = $"CREATE TABLE [{tableName}] ({string.Join(", ", columns)})";

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new SqlCommand(createTableSql, conn);
            try { await cmd.ExecuteNonQueryAsync(); } catch { return BadRequest("Table already exists or invalid name."); }

            // Insert data
            foreach (DataRow row in dt.Rows)
            {
                var colNames = string.Join(", ", dt.Columns.Cast<DataColumn>().Select(c => $"[{c.ColumnName}]").ToArray());
                var vals = string.Join(", ", dt.Columns.Cast<DataColumn>().Select(c => $"@{c.ColumnName}").ToArray());
                var insertSql = $"INSERT INTO [{tableName}] ({colNames}) VALUES ({vals})";
                using var insertCmd = new SqlCommand(insertSql, conn);
                foreach (DataColumn col in dt.Columns)
                    insertCmd.Parameters.AddWithValue($"@{col.ColumnName}", row[col] ?? DBNull.Value);
                await insertCmd.ExecuteNonQueryAsync();
            }
            return Ok("Table created and data inserted.");
        }

        [HttpGet("Tables")]
        public async Task<IActionResult> GetTables()
        {
            var tables = new List<string>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                tables.Add(reader.GetString(0));
            return Ok(tables);
        }

        [HttpGet("TableData/{tableName}")]
        public async Task<IActionResult> GetTableData(string tableName)
        {
            var data = new List<Dictionary<string, object>>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var cmd = new SqlCommand($"SELECT * FROM [{tableName}]", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.GetValue(i);
                data.Add(row);
            }
            return Ok(data);
        }

        [HttpPost("InsertRow/{tableName}")]
        public async Task<IActionResult> InsertRow(string tableName, [FromBody] Dictionary<string, object> row)
        {
            if (string.IsNullOrWhiteSpace(tableName) || row == null || row.Count == 0)
                return BadRequest("Missing table name or row data.");
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var colNames = string.Join(", ", row.Keys.Select(k => $"[{k}]").ToArray());
            var paramNames = string.Join(", ", row.Keys.Select(k => $"@{k}").ToArray());
            var insertSql = $"INSERT INTO [{tableName}] ({colNames}) VALUES ({paramNames})";
            using var cmd = new SqlCommand(insertSql, conn);
            foreach (var kv in row)
                cmd.Parameters.AddWithValue($"@{kv.Key}", kv.Value ?? DBNull.Value);
            try {
                await cmd.ExecuteNonQueryAsync();
                return Ok("Row inserted.");
            } catch (Exception ex) {
                return BadRequest($"Insert failed: {ex.Message}");
            }
        }
    }
} 