using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CampusLove.Application.Services;
using CampusLove.Application.Strategies; // Para las estrategias de matching
using CampusLove.Core.Entities;
using CampusLove.Core.Interfaces;
using CampusLove.Infrastructure.Data;
using CampusLove.Infrastructure.Repositories;
using CampusLove.ConsoleApp.Extensions;
using CampusLove.Core.Enums; // Para ToTitleCase
// Nota: El using para ConsoleUI no es necesario si está en el mismo namespace (CampusLove.ConsoleApp)
// Si ConsoleUI estuviera en, por ejemplo, CampusLove.ConsoleApp.UI, necesitarías using CampusLove.ConsoleApp.UI;

namespace CampusLove.ConsoleApp
{
    class Program
    {
        // --- Configuración ---
        // !!! MUY IMPORTANTE: REEMPLAZA ESTO CON TU CADENA DE CONEXIÓN REAL A MYSQL !!!
        private const string MySqlConnectionString = "Server=localhost;Port=3306;Database=CampusLoveDB;Uid=root;Pwd=1234;";
        // Ejemplo: "Server=localhost;Port=3306;Database=CampusLoveDB;Uid=root;Pwd=admin;"

        // --- Estado de la Aplicación ---
        private static Usuario? _usuarioActual = null;
        private static List<int> _perfilesVistosEnSesion = new List<int>(); // Para no repetir perfiles al visualizar

        // --- Servicios (Inyección de Dependencias Manual) ---
        private static IDbConnectionFactory _dbConnectionFactory = null!;
        private static IUsuarioRepository _usuarioRepository = null!;
        private static IInteraccionRepository _interaccionRepository = null!;
        private static IMatchRepository _matchRepository = null!;
        private static IMatchingStrategy _matchingStrategy = null!; // Estrategia por defecto
        private static UsuarioService _usuarioService = null!;
        private static EmparejamientoService _emparejamientoService = null!;
        private static EstadisticasService _estadisticasService = null!;


        static async Task Main(string[] args)
        {
            ConsoleUI.SetCulture("es-ES"); // Configura la cultura y UTF-8 para emojis
            InicializarServicios();

            bool salir = false;
            while (!salir)
            {
                int opcion = ConsoleUI.MostrarMenuPrincipal(_usuarioActual);
                Console.Clear(); // Limpiar después de seleccionar para la siguiente pantalla

                if (_usuarioActual != null) // Menú para usuarios logueados
                {
                    switch (opcion)
                    {
                        case 1:
                            await VerPerfilesYDarLikeDislike();
                            break;
                        case 2:
                            await VerMisMatches();
                            break;
                        case 3:
                            await VerEstadisticas();
                            break;
                        case 4: // Cerrar Sesión
                            CerrarSesion();
                            break;
                        case 0:
                            salir = true;
                            break;
                        default:
                            ConsoleUI.PrintError("Opción no válida.");
                            break;
                    }
                }
                else // Menú para usuarios no logueados
                {
                    switch (opcion)
                    {
                        case 1:
                            await RegistrarNuevoUsuario();
                            break;
                        case 2:
                            await IniciarSesion();
                            break;
                        case 0:
                            salir = true;
                            break;
                        default:
                            ConsoleUI.PrintError("Opción no válida.");
                            break;
                    }
                }

                if (!salir)
                {
                    ConsoleUI.PausaParaContinuar();
                }
            }
            ConsoleUI.PrintMessage("\n❤️ ¡Gracias por usar CampusLove! ¡Hasta la próxima! ❤️\n", ConsoleColor.Cyan);
        }

        private static void InicializarServicios()
        {
            try
            {
                _dbConnectionFactory = new MySqlDbConnectionFactory(MySqlConnectionString);

                // Verificar conexión (opcional pero recomendado al inicio)
                using (var testConnection = _dbConnectionFactory.CreateConnection())
                {
                    testConnection.Open(); // Intenta abrir la conexión
                    ConsoleUI.PrintSuccess("Conexión a la base de datos exitosa.");
                    testConnection.Close();
                }
                Thread.Sleep(1000); // Pequeña pausa para ver el mensaje

                _usuarioRepository = new UsuarioRepository(_dbConnectionFactory);
                _interaccionRepository = new InteraccionRepository(_dbConnectionFactory);
                _matchRepository = new MatchRepository(_dbConnectionFactory);

                // Elige tu estrategia de matching por defecto aquí:
                _matchingStrategy = new RandomMatchingStrategy();
                // _matchingStrategy = new InterestBasedMatchingStrategy(); // O esta otra

                _usuarioService = new UsuarioService(_usuarioRepository);
                _emparejamientoService = new EmparejamientoService(_usuarioRepository, _interaccionRepository, _matchRepository, _matchingStrategy);
                _estadisticasService = new EstadisticasService(_usuarioRepository, _interaccionRepository, _matchRepository);
            }
            catch (Exception ex)
            {
                ConsoleUI.PrintError($"Error crítico al inicializar servicios o conectar a la BD: {ex.Message}");
                ConsoleUI.PrintError("La aplicación no puede continuar. Verifica la cadena de conexión y la disponibilidad de la BD.");
                ConsoleUI.PausaParaContinuar("Presiona cualquier tecla para salir...");
                Environment.Exit(1); // Salir de la aplicación si hay un error crítico
            }
        }

        private static async Task RegistrarNuevoUsuario()
        {
            ConsoleUI.PrintTitle("📝 Registro de Nuevo Usuario");
            string nombre = ConsoleUI.ReadString("Nombre de Usuario:");
            int edad = ConsoleUI.ReadInt("Edad:", 18, 99);
            string genero = ConsoleUI.ReadGenero(); // O un menú más específico
            List<string> intereses = ConsoleUI.ReadIntereses();
            string carrera = ConsoleUI.ReadString("Carrera/Ocupación:", false).ToTitleCase();
            string frasePerfil = ConsoleUI.ReadString("Frase de Perfil (algo que te describa):", false);

            var (nuevoUsuario, mensaje) = await _usuarioService.RegistrarUsuarioAsync(nombre, edad, genero, intereses, carrera, frasePerfil);

            if (nuevoUsuario != null)
            {
                ConsoleUI.PrintSuccess(mensaje);
                ConsoleUI.PrintInfo("¡Ahora puedes iniciar sesión!");
            }
            else
            {
                ConsoleUI.PrintError(mensaje);
            }
        }

        private static async Task IniciarSesion()
        {
            ConsoleUI.PrintTitle("🔑 Iniciar Sesión");
            string nombre = ConsoleUI.ReadString("Nombre de Usuario:");
            _usuarioActual = await _usuarioService.IniciarSesionAsync(nombre);

            if (_usuarioActual != null)
            {
                ConsoleUI.PrintSuccess($"¡Bienvenido de nuevo, {_usuarioActual.Nombre.ToTitleCase()}!");
                _perfilesVistosEnSesion.Clear(); // Limpiar perfiles vistos de sesiones anteriores
            }
            else
            {
                ConsoleUI.PrintError("Nombre de usuario no encontrado o error al iniciar sesión.");
            }
        }

        private static void CerrarSesion()
        {
            ConsoleUI.PrintInfo($"Cerrando sesión de {_usuarioActual?.Nombre.ToTitleCase()}...");
            _usuarioActual = null;
            _perfilesVistosEnSesion.Clear();
            ConsoleUI.PrintSuccess("Sesión cerrada.");
        }

        private static async Task VerPerfilesYDarLikeDislike()
        {
            if (_usuarioActual == null) return;
            ConsoleUI.PrintTitle("👀 Viendo Perfiles");
            
            bool seguirViendo = true;
            while (seguirViendo)
            {
                // Asegurar créditos actualizados antes de mostrar perfiles
                await _usuarioService.AsegurarCreditosDiariosAsync(_usuarioActual);
                if (_usuarioActual.CreditosLikesDiarios <=0)
                {
                    ConsoleUI.PrintWarning("Has agotado tus likes por hoy. ¡Vuelve mañana para más! 😢");
                    ConsoleUI.PausaParaContinuar();
                    break;
                }

                // Obtener un perfil que no sea el actual y no haya sido interactuado ni visto en esta sesión
                var idsExcluir = new List<int>(_perfilesVistosEnSesion);
                // La lógica de GetIdsUsuariosInteractuadosPorAsync ya está en _emparejamientoService.ObtenerPerfilesSugeridosAsync
                // así que _perfilesVistosEnSesion es para evitar repeticiones inmediatas en la misma visualización.

                var perfilesSugeridos = (await _emparejamientoService.ObtenerPerfilesSugeridosAsync(_usuarioActual, 1)).ToList();

                if (!perfilesSugeridos.Any())
                {
                    ConsoleUI.PrintInfo("¡Vaya! Parece que no hay más perfiles disponibles por ahora. 🤔");
                    break;
                }

                Usuario perfilVisto = perfilesSugeridos.First();
                _perfilesVistosEnSesion.Add(perfilVisto.UsuarioID); // Marcar como visto en esta sesión

                Console.Clear();
                ConsoleUI.PrintTitle($"💖 Conoce a {perfilVisto.Nombre.ToTitleCase()} 💖");
                ConsoleUI.MostrarPerfilUsuario(perfilVisto);
                ConsoleUI.PrintInfo($"Tienes {_usuarioActual.CreditosLikesDiarios} likes restantes hoy. 🪙");

                TipoInteraccion? interaccionElegida = ConsoleUI.PedirInteraccionConPerfil(perfilVisto);

                if (interaccionElegida.HasValue) // Si eligió Like o Dislike
                {
                    var (exito, mensaje, esMatch) = await _emparejamientoService.RegistrarInteraccionAsync(
                        _usuarioActual.UsuarioID,
                        perfilVisto.UsuarioID,
                        interaccionElegida.Value);

                    if (exito)
                    {
                        ConsoleUI.PrintSuccess(mensaje);
                        if (esMatch)
                        {
                            ConsoleUI.PrintMessage("¡Felicidades! Revisa tus matches. 😉", ConsoleColor.Magenta);
                        }
                    }
                    else
                    {
                        ConsoleUI.PrintError(mensaje);
                    }
                    ConsoleUI.PausaParaContinuar();
                }
                else // Eligió "ver siguiente" o "volver al menú" desde PedirInteraccionConPerfil
                {
                    // Si PedirInteraccionConPerfil devuelve null y el usuario no eligió "Volver (0)", interpretamos como "siguiente"
                    // La opción 0 (volver) se manejaría preguntando si quiere seguir viendo perfiles
                    ConsoleUI.PrintInfo("Viendo siguiente perfil..."); // O manejar si quiere salir del bucle de ver perfiles
                    // Necesitamos una forma explícita de salir del bucle de "VerPerfiles"
                    Console.WriteLine("\n¿Continuar viendo perfiles? (s/n)");
                    string? continuar = Console.ReadLine()?.ToLower();
                    if (continuar != "s")
                    {
                        seguirViendo = false;
                    }
                }
                 if (!seguirViendo) break; // Salir del bucle while
            }
            if (seguirViendo) { // Si sale del bucle por otra razón (ej. no más perfiles)
                 ConsoleUI.PrintInfo("Has vuelto al menú principal.");
            }
        }

        private static async Task VerMisMatches()
        {
            if (_usuarioActual == null) return;
            ConsoleUI.PrintTitle("❤️ Mis Coincidencias (Matches)");

            var misMatches = (await _emparejamientoService.VerMisMatchesAsync(_usuarioActual.UsuarioID)).ToList();

            if (!misMatches.Any())
            {
                ConsoleUI.PrintInfo("Aún no tienes matches. ¡Sigue deslizando! 😉");
            }
            else
            {
                ConsoleUI.PrintMessage($"¡Felicidades! Tienes {misMatches.Count} match(es):", ConsoleColor.Magenta);
                ConsoleUI.MostrarListaPerfiles(misMatches, "Perfiles de tus Matches");
            }
        }

        private static async Task VerEstadisticas()
        {
            ConsoleUI.PrintTitle("📊 Estadísticas del Sistema");
            string reporte = await _estadisticasService.GenerarReporteEstadisticasTextoAsync();
            Console.WriteLine(reporte);
        }
    }
}