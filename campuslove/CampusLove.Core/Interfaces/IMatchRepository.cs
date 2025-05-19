 
using System.Collections.Generic;
using System.Threading.Tasks;
using CampusLove.Core.Entities;

namespace CampusLove.Core.Interfaces
{
    public interface IMatchRepository
    {
        Task<int> AddAsync(Match match);
        Task<bool> ExisteMatchAsync(int usuario1Id, int usuario2Id); // Recuerda ordenar los IDs antes de llamar
        Task<IEnumerable<Usuario>> GetMatchesDetalladosParaUsuarioAsync(int usuarioId); // Devuelve los Usuarios con los que hay match
        Task<IEnumerable<Match>> GetMatchesParaUsuarioAsync(int usuarioId); // Devuelve objetos Match
        Task<int> ContarMatchesPorUsuarioAsync(int usuarioId);
    }
}