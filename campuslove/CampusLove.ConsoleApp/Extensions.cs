using System.Globalization;

namespace CampusLove.ConsoleApp.Extensions // O el namespace que uses para tu app de consola si es diferente
{
    public static class StringFormattingExtensions
    {
        /// <summary>
        /// Convierte una cadena a formato Título (primera letra de cada palabra en mayúscula).
        /// Utiliza la cultura actual por defecto.
        /// </summary>
        /// <param name="input">La cadena de entrada.</param>
        /// <param name="culture">Opcional: La cultura específica a usar para las reglas de capitalización.</param>
        /// <returns>La cadena en formato Título, o string vacío si la entrada es nula o vacía.</returns>
        public static string ToTitleCase(this string? input, CultureInfo? culture = null)
        {
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }

            CultureInfo effectiveCulture = culture ?? CultureInfo.CurrentCulture;
            return effectiveCulture.TextInfo.ToTitleCase(input.ToLower(effectiveCulture));
        }

        // Podrías añadir más extensiones útiles aquí, por ejemplo:
        // public static string Truncate(this string value, int maxLength)
        // {
        //     if (string.IsNullOrEmpty(value)) return value;
        //     return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        // }
    }
}