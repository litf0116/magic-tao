using System;
using System.Data;
using MySqlConnector;

namespace TtWork.Abp.Dapper {
    public class SqlConnectionFactory : ISqlConnectionFactory {
        private readonly string _connectionString;
        private IDbConnection _connection;
        private readonly Guid _key = Guid.NewGuid();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        public SqlConnectionFactory(string connectionString) {
            this._connectionString = connectionString;
        }

        public IDbConnection GetOpenConnection() {
            if (this._connection == null || this._connection.State != ConnectionState.Open) {
                Console.WriteLine($"db GetOpenConnection  {_key}");

                this._connection = new MySqlConnection(_connectionString);
                this._connection.Open();
            }

            return this._connection;
        }

        public void Dispose() {
            if (this._connection != null && this._connection.State == ConnectionState.Open) {
                Console.WriteLine($"db Dispose {_key}");

                this._connection.Dispose();
            }
        }
    }
}