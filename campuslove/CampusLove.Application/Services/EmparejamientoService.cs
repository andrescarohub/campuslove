using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Enums;
using CampusLove.Core.Interfaces;
using CampusLove.Application.Strategies; // Para IMatchingStrategy

namespace CampusLove.Application.Services
{
    public class EmparejamientoService
    {
        // Para los warnings CS8618, si persisten después de asegurar la asignación en el constructor:
        private readonly IUsuarioRepository _usuarioRepository = null!;
        private readonly IInteraccionRepository _interaccionRepository = null!;
        private readonly IMatchRepository _matchRepository = null!;
        private readonly IMatchingStrategy _matchingStrategy = null!;
        // Si no necesitas el null!, y solo con la asignación en el constructor es suficiente para tu compilador, omítelo.

        public EmparejamientoService(
            IUsuarioRepository usuarioRepository,
            IInteraccionRepository interaccionRepository,
            IMatchRepository matchRepository,
            IMatchingStrategy matchingStrategy)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _interaccionRepository = interaccionRepository ?? throw new ArgumentNullException(nameof(interaccionRepository));
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
            _matchingStrategy = matchingStrategy ?? throw new ArgumentNullException(nameof(matchingStrategy));
        }

        public async Task<(bool exito, string mensaje, bool esMatch)> RegistrarInteraccionAsync(
            int idUsuarioOrigen,
            int idUsuarioDestino,
            TipoInteraccion tipoInteraccion)
        {
            var usuarioOrigen = await _usuarioRepository.GetByIdAsync(idUsuarioOrigen);
            if (usuarioOrigen == null) return (false, "Usuario origen no encontrado.", false);

            var usuarioDestino = await _usuarioRepository.GetByIdAsync(idUsuarioDestino);
            if (usuarioDestino == null) return (false, "Usuario destino no encontrado.", false);

            if (idUsuarioOrigen == idUsuarioDestino) return (false, "No puedes interactuar contigo mismo.", false);

            if (await _interaccionRepository.ExisteInteraccionAsync(idUsuarioOrigen, idUsuarioDestino))
            {
                return (false, "Ya has interactuado con este perfil anteriormente.", false);
            }

            bool esNuevoMatch = false;

            if (tipoInteraccion == TipoInteraccion.Like)
            {
                // Lógica de créditos de like
                // Asumimos que los créditos se verifican/actualizan al inicio de sesión o antes de llamar a esta función.
                // Si no, se podría añadir una llamada a UsuarioService.AsegurarCreditosDiariosAsync aquí,
                // pero implicaría inyectar UsuarioService o duplicar lógica.
                // Por ahora, confiamos en que el usuarioOrigen cargado tiene los créditos correctos para el día.

                if (!usuarioOrigen.PuedeDarLike())
                {
                    // El mensaje de créditos podría ser más específico si tuviéramos el número por defecto
                    // int creditosPorDefecto = 10; // Obtener de una constante/configuración
                    return (false, $"No tienes créditos de like disponibles hoy. ¡Inténtalo de nuevo mañana!", false);
                }
                
                usuarioOrigen.ConsumirCreditoLike(); // Disminuye el contador en el objeto
                // Actualizar el usuario en la base de datos para persistir la reducción de créditos
                try
                {
                    await _usuarioRepository.UpdateAsync(usuarioOrigen);
                }
                catch(Exception ex)
                {
                    // Loggear el error ex
                    // Considerar si la interacción debe fallar si no se pueden actualizar los créditos.
                    // Por ahora, continuamos con la interacción pero advertimos.
                    Console.WriteLine($"Advertencia: No se pudieron guardar los créditos actualizados para {usuarioOrigen.Nombre}: {ex.Message}");
                    // Podrías decidir retornar un error aquí si la persistencia de créditos es crítica antes de la interacción.
                    // return (false, "Error al actualizar los créditos. Inténtalo de nuevo.", false);
                }
            }

            var nuevaInteraccion = new Interaccion
            {
                UsuarioOrigenID = idUsuarioOrigen,
                UsuarioDestinoID = idUsuarioDestino,
                Tipo = tipoInteraccion,
                FechaInteraccion = DateTime.UtcNow
            };

            await _interaccionRepository.AddAsync(nuevaInteraccion);

            string mensajeResultado = tipoInteraccion == TipoInteraccion.Like ? "Like registrado correctamente." : "Dislike registrado correctamente.";

            if (tipoInteraccion == TipoInteraccion.Like)
            {
                // Verificar si el otro usuario ya había dado Like (match bidireccional)
                var interaccionInversa = await _interaccionRepository.GetLikeEspecificoAsync(idUsuarioDestino, idUsuarioOrigen);
                if (interaccionInversa != null) // ¡Es un Match!
                {
                    // Usar el constructor de Match que ordena los IDs
                    var nuevoMatch = new Match(idUsuarioOrigen, idUsuarioDestino); 

                    // Verificar si el match ya existe (por si acaso, aunque la lógica de interacciones previas debería cubrirlo)
                    if (!await _matchRepository.ExisteMatchAsync(nuevoMatch.Usuario1ID, nuevoMatch.Usuario2ID))
                    {
                        await _matchRepository.AddAsync(nuevoMatch);
                        mensajeResultado = "¡Felicidades! ¡Es un Match! 🎉💖";
                        esNuevoMatch = true;
                    }
                    else
                    {
                        // Esto no debería pasar si la lógica de "ExisteInteraccionAsync" y "ExisteMatchAsync" es robusta
                        // y si un match solo se crea una vez.
                        mensajeResultado = "Like registrado. (Nota: Ya existía un match con este perfil).";
                        esNuevoMatch = true; // Ya era un match
                    }
                }
            }
            return (true, mensajeResultado, esNuevoMatch);
        }

        public async Task<IEnumerable<Usuario>> ObtenerPerfilesSugeridosAsync(Usuario usuarioActual, int cantidad)
        {
            if (usuarioActual == null)
            {
                // Considerar si devolver una lista vacía o lanzar una excepción si el usuario es nulo.
                // Para este caso, una lista vacía es más seguro para el consumidor.
                return Enumerable.Empty<Usuario>();
            }

            // Obtener IDs de usuarios con los que el usuarioActual ya ha interactuado (Like o Dislike)
            var idsInteractuados = (await _interaccionRepository.GetIdsUsuariosInteractuadosPorAsync(usuarioActual.UsuarioID)).ToList();
            
            // Adicionalmente, agregar el ID del propio usuario para asegurarse de que no se sugiera a sí mismo,
            // aunque la estrategia o el repositorio también podrían manejarlo.
            if (!idsInteractuados.Contains(usuarioActual.UsuarioID))
            {
                idsInteractuados.Add(usuarioActual.UsuarioID);
            }
            
            // Usar la estrategia de emparejamiento inyectada
            return await _matchingStrategy.GetSuggestionsAsync(_usuarioRepository, usuarioActual, cantidad, idsInteractuados);
        }

        public async Task<IEnumerable<Usuario>> VerMisMatchesAsync(int idUsuarioActual)
        {
            // Este método devuelve los perfiles completos de los usuarios con los que hay match
            return await _matchRepository.GetMatchesDetalladosParaUsuarioAsync(idUsuarioActual);
        }
    }
}