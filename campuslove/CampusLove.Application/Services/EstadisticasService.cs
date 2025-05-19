using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text; // Necesario para StringBuilder
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Interfaces;
// Asegúrate de tener tu clase de extensiones en el lugar correcto y referenciarla aquí
// Por ejemplo, si la tienes en CampusLove.ConsoleApp/Extensions.cs:
// using CampusLove.ConsoleApp.Extensions;
// Si la dejaste como StringExtensionsForStats dentro de este mismo namespace, no necesitas un using extra.

namespace CampusLove.Application.Services
{
    public class EstadisticasService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IInteraccionRepository _interaccionRepository;
        private readonly IMatchRepository _matchRepository;

        public EstadisticasService(
            IUsuarioRepository usuarioRepository,
            IInteraccionRepository interaccionRepository,
            IMatchRepository matchRepository)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
            _interaccionRepository = interaccionRepository ?? throw new ArgumentNullException(nameof(interaccionRepository));
            _matchRepository = matchRepository ?? throw new ArgumentNullException(nameof(matchRepository));
        }

        // Clase interna o DTO para resultados de estadísticas si se vuelve complejo
        // Esta no se usa directamente en GenerarReporteEstadisticasTextoAsync, pero es buena tenerla como idea
        public class EstadisticasUsuarioDetalladas
        {
            public string NombreUsuario { get; set; } = string.Empty;
            public int LikesRecibidos { get; set; }
            public int MatchesConseguidos { get; set; }
        }
        
        public async Task<string> GenerarReporteEstadisticasTextoAsync()
        {
            var todosUsuarios = (await _usuarioRepository.GetAllAsync()).ToList();
            if (!todosUsuarios.Any())
            {
                return "No hay usuarios registrados para generar estadísticas.";
            }

            var reporte = new StringBuilder();
            var culture = CultureInfo.CurrentCulture; // Usar la cultura actual para formateo

            reporte.AppendLine("--- Estadísticas del Sistema CampusLove ---");
            reporte.AppendLine($"Total de usuarios registrados: {todosUsuarios.Count.ToString("N0", culture)}");

            // Usuario con más likes recibidos
            // Usaremos una lista de tuplas anónimas para simplificar, ya que solo es para este método
            var likesStats = new List<(string NombreUsuario, int Count)>();
            foreach (var usuario in todosUsuarios)
            {
                var likes = await _interaccionRepository.GetLikesRecibidosPorUsuarioAsync(usuario.UsuarioID);
                // Aquí estaba el error potencial si no se referenciaba bien la extensión ToTitleCase
                // o si la cultura no se pasaba. Lo corregimos usando la extensión que definiremos abajo
                // o una que ya exista.
                likesStats.Add((usuario.Nombre.ToTitleCaseExtension(culture), likes.Count()));
            }

            var topLikes = likesStats.OrderByDescending(s => s.Count).FirstOrDefault();
            if (!string.IsNullOrEmpty(topLikes.NombreUsuario) && topLikes.Count > 0)
            {
                reporte.AppendLine($"Usuario con más likes recibidos: {topLikes.NombreUsuario} ({topLikes.Count.ToString("N0", culture)} likes)");
            }
            else
            {
                reporte.AppendLine("Nadie ha recibido likes aún.");
            }

            // Usuario con más matches
            var matchesStats = new List<(string NombreUsuario, int Count)>();
            foreach (var usuario in todosUsuarios)
            {
                var matchesCount = await _matchRepository.ContarMatchesPorUsuarioAsync(usuario.UsuarioID);
                matchesStats.Add((usuario.Nombre.ToTitleCaseExtension(culture), matchesCount));
            }
            var topMatches = matchesStats.OrderByDescending(s => s.Count).FirstOrDefault();
            if (!string.IsNullOrEmpty(topMatches.NombreUsuario) && topMatches.Count > 0)
            {
                reporte.AppendLine($"Usuario con más matches: {topMatches.NombreUsuario} ({topMatches.Count.ToString("N0", culture)} matches)");
            }
            else
            {
                reporte.AppendLine("No se han formado matches aún.");
            }
            
            // Edad promedio
            if (todosUsuarios.Any())
            {
                double promedioEdad = todosUsuarios.Average(u => u.Edad);
                reporte.AppendLine($"Edad promedio de usuarios: {promedioEdad.ToString("F1", culture)} años");
            }

            // Distribución de género (ejemplo simple)
            var distribucionGenero = todosUsuarios
                .GroupBy(u => u.Genero.ToTitleCaseExtension(culture)) // Usar la extensión
                .Select(g => new { Genero = g.Key, Cantidad = g.Count() })
                .OrderByDescending(g => g.Cantidad);

            if (distribucionGenero.Any()) {
                reporte.AppendLine("Distribución de género:");
                foreach (var item in distribucionGenero)
                {
                    reporte.AppendLine($"- {item.Genero}: {item.Cantidad.ToString("N0", culture)}");
                }
            } else {
                reporte.AppendLine("No hay datos de género para mostrar distribución.");
            }
            
            reporte.AppendLine("-----------------------------------------");
            return reporte.ToString();
        }
    }

    // --- MUEVE ESTA EXTENSIÓN A UN ARCHIVO DEDICADO, EJ: CampusLove.ConsoleApp/Extensions.cs ---
    // --- Y LUEGO AÑADE EL 'USING' CORRESPONDIENTE ARRIBA EN EstadisticasService.cs ---
    public static class StringFormattingExtensionsInternal // Nombre temporal para evitar conflicto si ya tienes una
    {
        public static string ToTitleCaseExtension(this string? input, CultureInfo? culture = null)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty; // Devolver string vacío para nulo/vacío
            
            CultureInfo effectiveCulture = culture ?? CultureInfo.CurrentCulture;
            return effectiveCulture.TextInfo.ToTitleCase(input.ToLower(effectiveCulture));
        }
    }
    // --- FIN DE LA SECCIÓN A MOVER ---
}