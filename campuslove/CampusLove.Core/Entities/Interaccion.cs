using System;
using CampusLove.Core.Enums; // Asegúrate que el namespace del Enum sea correcto

namespace CampusLove.Core.Entities
{
    public class Interaccion
    {
        public int InteraccionID { get; set; }
        public int UsuarioOrigenID { get; set; }
        public int UsuarioDestinoID { get; set; }
        public TipoInteraccion Tipo { get; set; }
        public DateTime FechaInteraccion { get; set; }

        public Interaccion()
        {
            FechaInteraccion = DateTime.UtcNow;
        }
    }
}