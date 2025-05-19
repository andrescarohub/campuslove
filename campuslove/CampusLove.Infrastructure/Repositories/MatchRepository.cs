 
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Interfaces;
using Dapper;

namespace CampusLove.Infrastructure.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public MatchRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<int> AddAsync(Match match)
        {
            // Asegurarse que los IDs están ordenados antes de insertar,
            // la entidad Match ya lo hace en su constructor.
            // El constraint CK_MatchOrder en la DB también ayuda.
            const string sql = @"
                INSERT INTO Matches (Usuario1ID, Usuario2ID, FechaMatch)
                VALUES (@Usuario1ID, @Usuario2ID, @FechaMatch);
                SELECT LAST_INSERT_ID();";
            
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, match); // Dapper mapeará las propiedades de Match
        }

        public async Task<bool> ExisteMatchAsync(int usuario1Id, int usuario2Id)
        {
            // Siempre consultar con el ID menor primero para coincidir con el constraint y la inserción
            int u1 = Math.Min(usuario1Id, usuario2Id);
            int u2 = Math.Max(usuario1Id, usuario2Id);

            const string sql = "SELECT COUNT(1) FROM Matches WHERE Usuario1ID = @U1 AND Usuario2ID = @U2;";
            using var connection = _dbConnectionFactory.CreateConnection();
            var count = await connection.ExecuteScalarAsync<int>(sql, new { U1 = u1, U2 = u2 });
            return count > 0;
        }
        
        public async Task<IEnumerable<Match>> GetMatchesParaUsuarioAsync(int usuarioId)
        {
            const string sql = "SELECT MatchID, Usuario1ID, Usuario2ID, FechaMatch FROM Matches WHERE Usuario1ID = @UsuarioId OR Usuario2ID = @UsuarioId;";
            using var connection = _dbConnectionFactory.CreateConnection();
            // Dapper puede mapear directamente a la entidad Match si los nombres de columnas coinciden
            var matches = await connection.QueryAsync<Match>(sql, new { UsuarioId = usuarioId });
            return matches;
        }

        public async Task<IEnumerable<Usuario>> GetMatchesDetalladosParaUsuarioAsync(int usuarioId)
        {
            // Esta query es un poco más compleja: une Matches con Usuarios para obtener los perfiles completos de los matches.
            const string sql = @"
                SELECT u.UsuarioID, u.Nombre, u.Edad, u.Genero, u.Intereses, u.Carrera, u.FrasePerfil, u.CreditosLikesDiarios, u.UltimoReinicioCreditos, u.FechaRegistro
                FROM Usuarios u
                JOIN Matches m ON (u.UsuarioID = m.Usuario1ID OR u.UsuarioID = m.Usuario2ID)
                WHERE (m.Usuario1ID = @UsuarioId OR m.Usuario2ID = @UsuarioId) -- El match involucra al usuario actual
                  AND u.UsuarioID != @UsuarioId; -- Y queremos el perfil del OTRO usuario del match
            ";
            using var connection = _dbConnectionFactory.CreateConnection();
            var usuariosData = await connection.QueryAsync<dynamic>(sql, new { UsuarioId = usuarioId });
            // Reutilizamos el mapper del UsuarioRepository. Podríamos hacerlo más genérico o duplicarlo aquí.
            // Por simplicidad, lo duplicaremos con ligeras adaptaciones o lo haríamos estático/compartido.
            // Aquí, asumimos que `MapRowToUsuario` está disponible o reimplementado.
            // Para evitar duplicación, es mejor tener el mapper en un lugar accesible o Dapper mapea a Usuario directamente.
            // Si las columnas de la query coinciden con las propiedades de Usuario, Dapper lo hará.

            // Para que Dapper mapee directamente a `Usuario` sin un `MapRowToUsuario` explícito aquí:
            var usuarios = await connection.QueryAsync<Usuario>(sql, new { UsuarioId = usuarioId });
            // Si Dapper no maneja el string de Intereses a List<string> automáticamente,
            // necesitarás un TypeHandler personalizado para Dapper o mapeo manual post-query.
            // En nuestro `MapRowToUsuario` ya manejamos la conversión de `Intereses` (string CSV) a `List<string>`.
            // Si usamos `QueryAsync<Usuario>`, Dapper intentará asignar `Intereses` (string) a `List<string>` y podría fallar.
            // Es más seguro usar QueryAsync<dynamic> y luego el mapper manual:
            return (IEnumerable<Usuario>)usuariosData.Select(row => MapDinamicToUsuario(row)).ToList();
        }
        
        // Este es el mapper que usaría GetMatchesDetalladosParaUsuarioAsync
        private Usuario MapDinamicToUsuario(dynamic row)
        {
            string? interesesString = row.Intereses as string;
            DateTime? ultimoReinicio = row.UltimoReinicioCreditos;

            return new Usuario
            {
                UsuarioID = (int)row.UsuarioID,
                Nombre = row.Nombre,
                Edad = (int)row.Edad,
                Genero = row.Genero,
                Intereses = !string.IsNullOrEmpty(interesesString) ? interesesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>(),
                Carrera = row.Carrera,
                FrasePerfil = row.FrasePerfil,
                CreditosLikesDiarios = (int)row.CreditosLikesDiarios,
                UltimoReinicioCreditos = ultimoReinicio,
                FechaRegistro = (DateTime)row.FechaRegistro
            };
        }


        public async Task<int> ContarMatchesPorUsuarioAsync(int usuarioId)
        {
            const string sql = "SELECT COUNT(*) FROM Matches WHERE Usuario1ID = @UsuarioId OR Usuario2ID = @UsuarioId;";
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new { UsuarioId = usuarioId });
        }
    }
}