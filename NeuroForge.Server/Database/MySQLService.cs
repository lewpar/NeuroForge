using MySqlConnector;

namespace NeuroForge.Server.Database
{
    public class MySQLService
    {
        public MySqlConnection Connection { get => _connection; }
        private MySqlConnection _connection;

        public async Task ConnectAsync(string server, string username, string password, string database)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = server,
                UserID = username,
                Password = password,
                Database = database
            };

            _connection = new MySqlConnection(builder.ConnectionString);
            await _connection.OpenAsync();
        }
    }
}
