using System.Collections.Generic;
using System.Threading.Tasks;
using CampusLove.Core.Entities;

namespace CampusLove.Core.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<int> AddAsync(Usuario usuario);
        Task<Usuario?> GetByIdAsync(int id);
        Task<Usuario?> GetByNombreAsync(string nombre);
        Task<IEnumerable<Usuario>> GetAllAsync();
        Task UpdateAsync(Usuario usuario);
        Task DeleteAsync(int id); // Añadido por si se necesita eliminar un usuario
        Task<IEnumerable<Usuario>> GetPerfilesParaVisualizarAsync(int usuarioActualId, int cantidad, List<int> idsYaInteractuados);
        // Considera si idsYaInteractuados es mejor que yaVistosIds, ya que un "dislike" también es una interacción.
    }
}