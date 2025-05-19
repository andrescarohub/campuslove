using System.Data;
using CampusLove.Core.Interfaces; // Asegúrate que este using es correcto
using MySql.Data.MySqlClient; // De MySql.Data NuGet package

namespace CampusLove.Infrastructure.Data
{
    public class MySqlDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        // La cadena de conexión se inyectará o se leerá de config.
        // Por ahora, la pasaremos al crear Program.cs, pero idealmente iría en un appsettings.json
        public MySqlDbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public IDbConnection CreateConnection()
        {
            var connection = new MySqlConnection(_connectionString);
            // No es necesario abrir la conexión aquí. Dapper la maneja.
            // Si usaras ADO.NET puro sin Dapper, podrías abrirla antes de retornarla,
            // pero es mejor que quien usa la conexión controle su ciclo de vida (open/close).
            return connection;
        }
    }
}