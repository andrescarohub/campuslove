 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Interfaces; // Para IUsuarioRepository

namespace CampusLove.Application.Strategies
{
    // Estrategia simple: Aleatoria, sin considerar intereses, edad, etc.
    // Usa el método GetPerfilesParaVisualizarAsync del repositorio que ya implementamos.
    public class RandomMatchingStrategy : IMatchingStrategy
    {
        public async Task<IEnumerable<Usuario>> GetSuggestionsAsync(
            IUsuarioRepository usuarioRepository,
            Usuario usuarioActual,
            int cantidad,
            List<int> idsYaInteractuados)
        {
            // La lógica de no mostrar el propio usuario ni los ya interactuados
            // la delegamos al método del repositorio.
            return await usuarioRepository.GetPerfilesParaVisualizarAsync(usuarioActual.UsuarioID, cantidad, idsYaInteractuados);
        }
    }

    // Podrías añadir más estrategias aquí:
    // Por ejemplo, una que priorice por intereses comunes, cercanía de edad, misma carrera, etc.

    public class InterestBasedMatchingStrategy : IMatchingStrategy
    {
        public async Task<IEnumerable<Usuario>> GetSuggestionsAsync(
            IUsuarioRepository usuarioRepository,
            Usuario usuarioActual,
            int cantidad,
            List<int> idsYaInteractuados)
        {
            // 1. Obtener todos los perfiles disponibles (que no sean el actual y no estén en idsYaInteractuados)
            // Esto podría ser ineficiente si hay muchos usuarios. GetPerfilesParaVisualizarAsync ya hace un buen filtrado inicial.
            // Vamos a usar GetPerfilesParaVisualizarAsync para obtener un conjunto inicial y luego reordenar o filtrar más.
            
            var candidatosIniciales = (await usuarioRepository.GetPerfilesParaVisualizarAsync(usuarioActual.UsuarioID, cantidad * 5, idsYaInteractuados)).ToList(); // Pedir más para tener de dónde elegir
            
            if (!candidatosIniciales.Any())
            {
                return Enumerable.Empty<Usuario>();
            }

            // 2. Calcular "puntuación" basada en intereses comunes
            var candidatosPuntuados = candidatosIniciales.Select(candidato =>
            {
                int interesesComunes = usuarioActual.Intereses.Intersect(candidato.Intereses, StringComparer.OrdinalIgnoreCase).Count();
                // Podrías añadir otros factores a la puntuación: edad, carrera, etc.
                return new { Usuario = candidato, Puntuacion = interesesComunes };
            })
            .OrderByDescending(x => x.Puntuacion) // Ordenar por más intereses comunes
            .ThenBy(x => Guid.NewGuid()) // Añadir un poco de aleatoriedad para mismos puntajes (o usar RAND() en DB si es posible)
            .Take(cantidad)
            .Select(x => x.Usuario);

            return candidatosPuntuados;
        }
    }
}