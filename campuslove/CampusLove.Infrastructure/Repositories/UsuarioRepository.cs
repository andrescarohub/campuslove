using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Interfaces;
using Dapper; // De Dapper NuGet package
using MySql.Data.MySqlClient; // Necesario si usas tipos específicos de MySQL, aunque Dapper abstrae mucho

namespace CampusLove.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public UsuarioRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<int> AddAsync(Usuario usuario)
        {
            const string sql = @"
                INSERT INTO Usuarios (Nombre, Edad, Genero, Intereses, Carrera, FrasePerfil, CreditosLikesDiarios, UltimoReinicioCreditos, FechaRegistro)
                VALUES (@Nombre, @Edad, @Genero, @InteresesCsv, @Carrera, @FrasePerfil, @CreditosLikesDiarios, @UltimoReinicioCreditos, @FechaRegistro);
                SELECT LAST_INSERT_ID();";
            
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(sql, new
            {
                usuario.Nombre,
                usuario.Edad,
                usuario.Genero,
                InteresesCsv = string.Join(",", usuario.Intereses),
                usuario.Carrera,
                usuario.FrasePerfil,
                usuario.CreditosLikesDiarios,
                usuario.UltimoReinicioCreditos,
                usuario.FechaRegistro
            });
        }

        public async Task<Usuario?> GetByIdAsync(int id)
        {
            const string sql = "SELECT UsuarioID, Nombre, Edad, Genero, Intereses, Carrera, FrasePerfil, CreditosLikesDiarios, UltimoReinicioCreditos, FechaRegistro FROM Usuarios WHERE UsuarioID = @Id;";
            using var connection = _dbConnectionFactory.CreateConnection();
            var usuarioData = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });
            
            if (usuarioData == null) return null;
            return MapRowToUsuario(usuarioData);
        }

        public async Task<Usuario?> GetByNombreAsync(string nombre)
        {
            const string sql = "SELECT UsuarioID, Nombre, Edad, Genero, Intereses, Carrera, FrasePerfil, CreditosLikesDiarios, UltimoReinicioCreditos, FechaRegistro FROM Usuarios WHERE Nombre = @Nombre;";
            using var connection = _dbConnectionFactory.CreateConnection();
            var usuarioData = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Nombre = nombre });

            if (usuarioData == null) return null;
            return MapRowToUsuario(usuarioData);
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync()
        {
            const string sql = "SELECT UsuarioID, Nombre, Edad, Genero, Intereses, Carrera, FrasePerfil, CreditosLikesDiarios, UltimoReinicioCreditos, FechaRegistro FROM Usuarios;";
            using var connection = _dbConnectionFactory.CreateConnection();
            var usuariosData = await connection.QueryAsync<dynamic>(sql);
            return (IEnumerable<Usuario>)usuariosData.Select(row => MapRowToUsuario(row)).ToList();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            const string sql = @"
                UPDATE Usuarios SET
                    Nombre = @Nombre,
                    Edad = @Edad,
                    Genero = @Genero,
                    Intereses = @InteresesCsv,
                    Carrera = @Carrera,
                    FrasePerfil = @FrasePerfil,
                    CreditosLikesDiarios = @CreditosLikesDiarios,
                    UltimoReinicioCreditos = @UltimoReinicioCreditos
                WHERE UsuarioID = @UsuarioID;";
            
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new {
                usuario.Nombre,
                usuario.Edad,
                usuario.Genero,
                InteresesCsv = string.Join(",", usuario.Intereses),
                usuario.Carrera,
                usuario.FrasePerfil,
                usuario.CreditosLikesDiarios,
                usuario.UltimoReinicioCreditos,
                usuario.UsuarioID
            });
        }

        public async Task DeleteAsync(int id)
        {
            // Considera las implicaciones de ON DELETE CASCADE en la BBDD
            const string sql = "DELETE FROM Usuarios WHERE UsuarioID = @Id;";
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Usuario>> GetPerfilesParaVisualizarAsync(int usuarioActualId, int cantidad, List<int> idsYaInteractuados)
        {
            // Construir la parte de `NOT IN` para idsYaInteractuados dinámicamente si no está vacía
            string interactuadosInClause = "";
            var parameters = new DynamicParameters();
            parameters.Add("UsuarioActualId", usuarioActualId);
            parameters.Add("Cantidad", cantidad);

            if (idsYaInteractuados != null && idsYaInteractuados.Any())
            {
                // Dapper maneja bien pasar una lista como parámetro para una cláusula IN
                parameters.Add("IdsYaInteractuados", idsYaInteractuados);
                interactuadosInClause = "AND u.UsuarioID NOT IN @IdsYaInteractuados";
            }
            
            // Nota: ORDER BY RAND() puede ser lento en tablas grandes.
            // Para producción se usarían otras estrategias de recomendación.
            // También, nos aseguramos de que no se muestren perfiles con los que ya hubo cualquier tipo de interacción.
            string sql = $@"
                SELECT u.UsuarioID, u.Nombre, u.Edad, u.Genero, u.Intereses, u.Carrera, u.FrasePerfil, u.CreditosLikesDiarios, u.UltimoReinicioCreditos, u.FechaRegistro
                FROM Usuarios u
                WHERE u.UsuarioID != @UsuarioActualId 
                {interactuadosInClause}
                ORDER BY RAND() 
                LIMIT @Cantidad;";
            // Modificación para no mostrar perfiles ya interactuados (like o dislike) por el usuario actual:
            // La cláusula `idsYaInteractuados` ya cubre esto si la llenamos correctamente desde el servicio.
            // Si quisiéramos hacerlo solo en SQL sin pasar la lista:
            // AND u.UsuarioID NOT IN (SELECT Interaccion.UsuarioDestinoID FROM Interacciones WHERE Interaccion.UsuarioOrigenID = @UsuarioActualId)


            using var connection = _dbConnectionFactory.CreateConnection();
            var perfilesData = await connection.QueryAsync<dynamic>(sql, parameters);
            return (IEnumerable<Usuario>)perfilesData.Select(row => MapRowToUsuario(row)).ToList();
        }

        private Usuario MapRowToUsuario(dynamic row)
        {
            string? interesesString = row.Intereses as string; // Casteo seguro
            DateTime? ultimoReinicio = row.UltimoReinicioCreditos; // Dapper maneja bien DateTime?

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
    }
}