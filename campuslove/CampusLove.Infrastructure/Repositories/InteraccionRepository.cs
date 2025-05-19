 
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Enums;
using CampusLove.Core.Interfaces;
using Dapper;

namespace CampusLove.Infrastructure.Repositories
{
    public class InteraccionRepository : IInteraccionRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public InteraccionRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<int> AddAsync(Interaccion interaccion)
        {
            const string sql = @"
                INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion)
                VALUES (@UsuarioOrigenID, @UsuarioDestinoID, @Tipo, @FechaInteraccion);
                SELECT LAST_INSERT_ID();";
            
            using var connection = _dbConnectionFactory.CreateConnection();
            // Dapper mapea enums a string si la columna de la DB es ENUM o VARCHAR
            return await connection.ExecuteScalarAsync<int>(sql, new 
            { 
                interaccion.UsuarioOrigenID,
                interaccion.UsuarioDestinoID,
                Tipo = interaccion.Tipo.ToString(), // Convertir Enum a string para MySQL ENUM
                interaccion.FechaInteraccion
            });
        }

        public async Task<bool> ExisteInteraccionAsync(int usuarioOrigenId, int usuarioDestinoId)
        {
            const string sql = "SELECT COUNT(1) FROM Interacciones WHERE UsuarioOrigenID = @UsuarioOrigenId AND UsuarioDestinoID = @UsuarioDestinoId;";
            using var connection = _dbConnectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { UsuarioOrigenId = usuarioOrigenId, UsuarioDestinoId = usuarioDestinoId });
            return count > 0;
        }

        public async Task<Interaccion?> GetInteraccionAsync(int usuarioOrigenId, int usuarioDestinoId)
        {
            const string sql = "SELECT InteraccionID, UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion FROM Interacciones WHERE UsuarioOrigenID = @UsuarioOrigenId AND UsuarioDestinoID = @UsuarioDestinoId;";
            using var connection = _dbConnectionFactory.CreateConnection();
            var interaccionData = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { UsuarioOrigenId = usuarioOrigenId, UsuarioDestinoId = usuarioDestinoId });
            
            if (interaccionData == null) return null;
            return MapRowToInteraccion(interaccionData);
        }
        
        public async Task<Interaccion?> GetLikeEspecificoAsync(int usuarioOrigenId, int usuarioDestinoId)
        {
            const string sql = "SELECT InteraccionID, UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion FROM Interacciones WHERE UsuarioOrigenID = @UsuarioOrigenId AND UsuarioDestinoID = @UsuarioDestinoId AND TipoInteraccion = 'Like';";
            using var connection = _dbConnectionFactory.CreateConnection();
            var interaccionData = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { UsuarioOrigenId = usuarioOrigenId, UsuarioDestinoId = usuarioDestinoId });
            
            if (interaccionData == null) return null;
            return MapRowToInteraccion(interaccionData);
        }

        public async Task<IEnumerable<Interaccion>> GetLikesRecibidosPorUsuarioAsync(int usuarioId)
        {
            const string sql = "SELECT InteraccionID, UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion FROM Interacciones WHERE UsuarioDestinoID = @UsuarioId AND TipoInteraccion = 'Like';";
            using var connection = _dbConnectionFactory.CreateConnection();
            var interaccionesData = await connection.QueryAsync<dynamic>(sql, new { UsuarioId = usuarioId });
            return (IEnumerable<Interaccion>)interaccionesData.Select(row => MapRowToInteraccion(row)).ToList();
        }

        public async Task<int> ContarLikesDadosHoyPorUsuarioAsync(int usuarioOrigenId)
        {
            // CURDATE() es específico de MySQL para obtener la fecha actual sin la hora.
            // Para otras BBDD, podría ser DATE('now') en SQLite, CAST(GETDATE() AS DATE) en SQL Server.
            const string sql = "SELECT COUNT(*) FROM Interacciones WHERE UsuarioOrigenID = @UsuarioOrigenId AND TipoInteraccion = 'Like' AND DATE(FechaInteraccion) = CURDATE();";
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { UsuarioOrigenId = usuarioOrigenId });
        }

        public async Task<IEnumerable<int>> GetIdsUsuariosInteractuadosPorAsync(int usuarioOrigenId)
        {
            const string sql = "SELECT DISTINCT UsuarioDestinoID FROM Interacciones WHERE UsuarioOrigenID = @UsuarioOrigenId;";
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryAsync<int>(sql, new { UsuarioOrigenId = usuarioOrigenId });
        }

        private Interaccion MapRowToInteraccion(dynamic row)
        {
            return new Interaccion
            {
                InteraccionID = (int)row.InteraccionID,
                UsuarioOrigenID = (int)row.UsuarioOrigenID,
                UsuarioDestinoID = (int)row.UsuarioDestinoID,
                // Dapper puede necesitar ayuda para mapear string a Enum si no coincide exactamente
                // o si el tipo de columna en DB no es ENUM. Si es ENUM, a veces funciona directo.
                // Si es VARCHAR, necesitas convertirlo.
                Tipo = Enum.Parse<TipoInteraccion>((string)row.TipoInteraccion, true), // true para ignorar case
                FechaInteraccion = (DateTime)row.FechaInteraccion
            };
        }
    }
}