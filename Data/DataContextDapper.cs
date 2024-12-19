using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace DotNetApi.Data
{
    class DataContextDapper
    {
        private readonly IConfiguration _configuration;

        public DataContextDapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql);
        }

        public IEnumerable<T> LoadDataWithParams<T>(string sql, object parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Query<T>(sql, parameters);
        }

        public async Task<IEnumerable<T>> LoadDataWithParamsAsync<T>(string sql, object parameters)
        {
            using IDbConnection connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return await connection.QueryAsync<T>(sql, parameters);
        }

        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingle<T>(sql);
        }

        public T? LoadDataSingleWithParams<T>(string sql, object parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.QuerySingleOrDefault<T>(sql, parameters);
        }

        public async Task<T?> LoadDataSingleWithParamsAsync<T>(string sql, object parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return await dbConnection.QuerySingleOrDefaultAsync<T>(sql, parameters);
        }

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql) > 0;
        }

        public bool ExecuteSql(string sql, object parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql, parameters) > 0;
        }

        public async Task<bool> ExecuteSqlAsync(string sql, object parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return await dbConnection.ExecuteAsync(sql, parameters) > 0;
        }

        public async Task<bool> ExecuteSqlWithParamsAsync(string sql, List<SqlParameter> parameters)
        {
            SqlCommand commandWithParams = new SqlCommand(sql);

            foreach (SqlParameter parameter in parameters)
            {
                commandWithParams.Parameters.Add(parameter);
            }

            SqlConnection dbConnection = new(_configuration.GetConnectionString("DefaultConnection"));
            dbConnection.Open();

            commandWithParams.Connection = dbConnection;

            int rowsAffected = await commandWithParams.ExecuteNonQueryAsync();

            dbConnection.Close();

            return rowsAffected > 0;
        }

        public async Task<bool> ExecuteSqlWithParamsAsync(string sql, DynamicParameters parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return await dbConnection.ExecuteAsync(sql, parameters) > 0;
        }

        public bool ExecuteSqlWithParams(string sql, object parameters)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql, parameters) > 0;
        }

        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            return dbConnection.Execute(sql);
        }
    }
}