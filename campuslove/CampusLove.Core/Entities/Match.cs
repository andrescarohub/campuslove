using System;

namespace CampusLove.Core.Entities
{
    public class Match
    {
        public int MatchID { get; set; }
        public int Usuario1ID { get; set; } // Se almacenará el ID menor
        public int Usuario2ID { get; set; } // Se almacenará el ID mayor
        public DateTime FechaMatch { get; set; }

        // Constructor para asegurar el orden de los IDs
        public Match(int usuario1Id, int usuario2Id)
        {
            if (usuario1Id < usuario2Id)
            {
                Usuario1ID = usuario1Id;
                Usuario2ID = usuario2Id;
            }
            else
            {
                Usuario1ID = usuario2Id;
                Usuario2ID = usuario1Id;
            }
            FechaMatch = DateTime.UtcNow;
        }

        // Constructor por defecto para Dapper u ORMs
        public Match()
        {
            FechaMatch = DateTime.UtcNow;
        }
    }
}