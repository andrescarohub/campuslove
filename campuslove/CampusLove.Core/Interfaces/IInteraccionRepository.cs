using System.Collections.Generic;
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Enums;

namespace CampusLove.Core.Interfaces
{
    public interface IInteraccionRepository
    {
        Task<int> AddAsync(Interaccion interaccion);
        Task<bool> ExisteInteraccionAsync(int usuarioOrigenId, int usuarioDestinoId);
        Task<Interaccion?> GetInteraccionAsync(int usuarioOrigenId, int usuarioDestinoId); // Devuelve cualquier tipo de interacción
        Task<Interaccion?> GetLikeEspecificoAsync(int usuarioOrigenId, int usuarioDestinoId); // Específico para buscar un Like
        Task<IEnumerable<Interaccion>> GetLikesRecibidosPorUsuarioAsync(int usuarioId);
        Task<int> ContarLikesDadosHoyPorUsuarioAsync(int usuarioOrigenId);
        Task<IEnumerable<int>> GetIdsUsuariosInteractuadosPorAsync(int usuarioOrigenId); // Para no volver a mostrar perfiles ya interactuados
    }
}