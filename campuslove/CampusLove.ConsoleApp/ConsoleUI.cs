 
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CampusLove.Core.Entities;         // Para mostrar Usuario
using CampusLove.ConsoleApp.Extensions;
using CampusLove.Core.Enums;  // Para ToTitleCase

namespace CampusLove.ConsoleApp // O el namespace que uses para tu app de consola
{
    public static class ConsoleUI
    {
        private static CultureInfo _culture = new CultureInfo("es-ES"); // Cultura para formateo (puedes cambiarla)

        // --- Colores y Estilos ---
        public static void SetCulture(string cultureName = "es-ES")
        {
            try
            {
                _culture = new CultureInfo(cultureName);
                CultureInfo.CurrentCulture = _culture;
                CultureInfo.CurrentUICulture = _culture; // Para mensajes del sistema si los hubiera en otros idiomas
                Console.OutputEncoding = System.Text.Encoding.UTF8; // Para emojis y caracteres especiales
            }
            catch (CultureNotFoundException)
            {
                Console.WriteLine($"Advertencia: Cultura '{cultureName}' no encontrada. Usando por defecto.");
                _culture = CultureInfo.InvariantCulture; // O CultureInfo.CurrentCulture como fallback
            }
        }

        public static void PrintMessage(string message, ConsoleColor color = ConsoleColor.White, bool newLine = true)
        {
            Console.ForegroundColor = color;
            if (newLine)
                Console.WriteLine(message);
            else
                Console.Write(message);
            Console.ResetColor();
        }

        public static void PrintError(string message)
        {
            PrintMessage($"❌ Error: {message}", ConsoleColor.Red);
        }

        public static void PrintSuccess(string message)
        {
            PrintMessage($"✅ Éxito: {message}", ConsoleColor.Green);
        }

        public static void PrintWarning(string message)
        {
            PrintMessage($"⚠️ Advertencia: {message}", ConsoleColor.Yellow);
        }
        
        public static void PrintInfo(string message)
        {
            PrintMessage($"ℹ️ Info: {message}", ConsoleColor.Cyan);
        }

        public static void PrintSeparator(char symbol = '-', int length = 50)
        {
            Console.WriteLine(new string(symbol, length));
        }

        public static void PrintTitle(string title)
        {
            PrintSeparator('=', title.Length + 6);
            PrintMessage($"✨ {title.ToUpper()} ✨", ConsoleColor.Magenta);
            PrintSeparator('=', title.Length + 6);
            Console.WriteLine(); // Espacio extra
        }

        // --- Menús ---
        public static int MostrarMenuPrincipal(Usuario? usuarioActual)
        {
            Console.Clear();
            PrintTitle("💖 CampusLove - Donde Nace el Amor 💖");

            if (usuarioActual != null)
            {
                PrintMessage($"¡Hola, {usuarioActual.Nombre.ToTitleCase(_culture)}! 👋 | Créditos Likes: {usuarioActual.CreditosLikesDiarios.ToString("N0",_culture)} 🪙", ConsoleColor.Yellow);
                PrintSeparator();
                Console.WriteLine("1. Ver Perfiles y Dar Like/Dislike 👀");
                Console.WriteLine("2. Ver Mis Coincidencias (Matches) ❤️");
                Console.WriteLine("3. Ver Estadísticas del Sistema 📊");
                Console.WriteLine("4. Cerrar Sesión 🚪");
                Console.WriteLine("0. Salir de CampusLove 💔");
            }
            else
            {
                PrintSeparator();
                Console.WriteLine("1. Registrarse como Nuevo Usuario 📝");
                Console.WriteLine("2. Iniciar Sesión 🔑");
                Console.WriteLine("0. Salir de CampusLove 💔");
            }
            PrintSeparator();
            return ReadInt("Selecciona una opción:", 0, usuarioActual != null ? 4 : 2);
        }

        // --- Lectura de Datos ---
        public static string ReadString(string prompt, bool required = true)
        {
            string? input;
            do
            {
                PrintMessage($"{prompt} ", ConsoleColor.Gray, false); // No nueva línea
                input = Console.ReadLine();
                if (required && string.IsNullOrWhiteSpace(input))
                {
                    PrintError("Este campo es obligatorio. Inténtalo de nuevo.");
                }
                else
                {
                    return input?.Trim() ?? string.Empty;
                }
            } while (true);
        }

        public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
        {
            int value;
            while (true)
            {
                PrintMessage($"{prompt} ", ConsoleColor.Gray, false);
                string? input = Console.ReadLine();
                if (int.TryParse(input, NumberStyles.Integer, _culture, out value) && value >= min && value <= max)
                {
                    return value;
                }
                PrintError($"Entrada inválida. Ingresa un número entero entre {min} y {max}.");
            }
        }
        
        public static List<string> ReadIntereses()
        {
            PrintInfo("Ingresa tus intereses separados por coma (ej: cine,musica,deporte):");
            string interesesStr = ReadString("Intereses:", false); // No es obligatorio
            if (string.IsNullOrWhiteSpace(interesesStr))
            {
                return new List<string>();
            }
            return interesesStr.Split(',')
                               .Select(i => i.Trim().ToTitleCase(_culture)) // Formatear cada interés
                               .Where(i => !string.IsNullOrWhiteSpace(i))
                               .ToList();
        }

        public static string ReadGenero()
        {
            // Podrías ofrecer un menú para género si quieres ser más restrictivo
            return ReadString("Género:").ToTitleCase(_culture);
        }


        // --- Mostrar Información ---
        public static void MostrarPerfilUsuario(Usuario usuario, bool esPropio = false)
        {
            PrintSeparator('*');
            PrintMessage($"👤 PERFIL DE: {usuario.Nombre.ToTitleCase(_culture)}", ConsoleColor.Cyan);
            PrintMessage($"Edad: {usuario.Edad} años", ConsoleColor.White);
            PrintMessage($"Género: {usuario.Genero.ToTitleCase(_culture)}", ConsoleColor.White);
            PrintMessage($"Carrera: {usuario.Carrera.ToTitleCase(_culture)}", ConsoleColor.White);
            
            string intereses = usuario.Intereses.Any() ? 
                string.Join(", ", usuario.Intereses) : 
                "No especificados";
            PrintMessage($"Intereses: {intereses}", ConsoleColor.White);
            
            PrintMessage($"Frase: \"{usuario.FrasePerfil}\"", ConsoleColor.DarkYellow);

            if (esPropio)
            {
                PrintMessage($"Créditos Likes restantes hoy: {usuario.CreditosLikesDiarios.ToString("N0", _culture)} 🪙", ConsoleColor.Yellow);
                PrintMessage($"Registrado el: {usuario.FechaRegistro.ToString("D", _culture)}", ConsoleColor.DarkGray);
            }
            PrintSeparator('*');
        }

        public static void MostrarListaPerfiles(IEnumerable<Usuario> perfiles, string tituloLista)
        {
            PrintTitle(tituloLista);
            if (!perfiles.Any())
            {
                PrintInfo("No hay perfiles para mostrar en esta lista.");
                return;
            }
            foreach (var perfil in perfiles)
            {
                MostrarPerfilUsuario(perfil);
                Console.WriteLine(); // Espacio entre perfiles
            }
        }
        
        public static TipoInteraccion? PedirInteraccionConPerfil(Usuario perfilVisto)
        {
            PrintMessage($"¿Qué deseas hacer con {perfilVisto.Nombre.ToTitleCase(_culture)}?", ConsoleColor.Yellow);
            Console.WriteLine("1. 👍 Dar Like");
            Console.WriteLine("2. 👎 Dar Dislike");
            Console.WriteLine("3. ⏭️ Ver Siguiente Perfil");
            Console.WriteLine("0. 🚪 Volver al Menú Principal");
            PrintSeparator('-');

            int opcion = ReadInt("Tu elección:", 0, 3);
            switch (opcion)
            {
                case 1: return TipoInteraccion.Like;
                case 2: return TipoInteraccion.Dislike;
                case 3: return null; // Indica "ver siguiente"
                case 0: return null; // Indica "volver al menú", pero será manejado por un booleano en el bucle principal
                default: return null; 
            }
        }

        public static void PausaParaContinuar(string mensaje = "Presiona cualquier tecla para continuar...")
        {
            PrintMessage(mensaje, ConsoleColor.DarkGray);
            Console.ReadKey(true);
        }
    }
}