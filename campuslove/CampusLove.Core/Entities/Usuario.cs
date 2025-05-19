using System;
using System.Collections.Generic;
using System.Globalization; // Necesario para ToTitleCase si lo usamos aquí, aunque es mejor en la UI

namespace CampusLove.Core.Entities
{
    public class Usuario
    {
        public int UsuarioID { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Genero { get; set; } = string.Empty;
        public List<string> Intereses { get; set; } = new List<string>();
        public string Carrera { get; set; } = string.Empty;
        public string FrasePerfil { get; set; } = string.Empty;
        public int CreditosLikesDiarios { get; set; }
        public DateTime? UltimoReinicioCreditos { get; set; } // Nullable DateTime
        public DateTime FechaRegistro { get; set; }

        // Constructor por defecto para facilitar la creación (opcional, pero útil)
        public Usuario()
        {
            FechaRegistro = DateTime.UtcNow; // O DateTime.Now si prefieres hora local
            Intereses = new List<string>(); // Asegurar que la lista nunca sea null
        }

        // Métodos de utilidad relacionados con el usuario
        public bool PuedeDarLike() => CreditosLikesDiarios > 0;

        public void ConsumirCreditoLike()
        {
            if (PuedeDarLike())
            {
                CreditosLikesDiarios--;
            }
        }

        public void RestaurarCreditos(int creditosPorDefecto)
        {
            CreditosLikesDiarios = creditosPorDefecto;
            UltimoReinicioCreditos = DateTime.Today; // Usar DateTime.Today para comparar solo la fecha
        }

        public override string ToString()
        {
            // Formateo básico para mostrar el perfil, puedes mejorarlo
            // Para CultureInfo y ToTitleCase, es mejor en la capa de presentación (ConsoleUI)
            // pero si quieres un default aquí:
            // var textInfo = CultureInfo.CurrentCulture.TextInfo;
            // string nombreFormateado = textInfo.ToTitleCase(Nombre.ToLower());

            return $"--- Perfil de {Nombre} ---\n" +
                   $"ID: {UsuarioID}\n" +
                   $"Edad: {Edad} años\n" +
                   $"Género: {Genero}\n" +
                   $"Carrera: {Carrera}\n" +
                   $"Intereses: {(Intereses.Any() ? string.Join(", ", Intereses) : "No especificados")}\n" +
                   $"Frase: \"{FrasePerfil}\"\n" +
                   $"Créditos de Likes restantes hoy: {CreditosLikesDiarios}";
        }
    }
}