 
using System.Collections.Generic;
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Interfaces; // Para IUsuarioRepository

namespace CampusLove.Application.Strategies
{
    public interface IMatchingStrategy
    {
        // El repositorio de usuarios se pasa para que la estrategia pueda acceder a los datos necesarios.
        // El usuario actual y la cantidad de perfiles a obtener.
        // idsYaInteractuados para no repetir.
        Task<IEnumerable<Usuario>> GetSuggestionsAsync(
            IUsuarioRepository usuarioRepository,
            Usuario usuarioActual,
            int cantidad,
            List<int> idsYaInteractuados);
    }
}