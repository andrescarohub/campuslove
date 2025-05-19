using System;
using System.Collections.Generic;
using CampusLove.Core.Entities; // Para acceder a la clase Usuario

namespace CampusLove.Application.Factories
{
    public static class UsuarioFactory
    {
        private const int CREDITOS_LIKES_POR_DEFECTO_AL_REGISTRAR = 10; // Puedes hacerlo configurable más adelante

        public static Usuario CrearNuevoUsuario(
            string nombre,
            int edad,
            string genero,
            List<string> intereses,
            string carrera,
            string frasePerfil)
        {
            // Aquí podrías añadir validaciones más complejas si fueran necesarias antes de crear el objeto,
            // aunque muchas validaciones de datos de entrada se harán en la UI o al principio del servicio.
            return new Usuario
            {
                Nombre = nombre,
                Edad = edad,
                Genero = genero,
                Intereses = intereses ?? new List<string>(), // Asegurar que la lista no sea null
                Carrera = carrera,
                FrasePerfil = frasePerfil,
                CreditosLikesDiarios = CREDITOS_LIKES_POR_DEFECTO_AL_REGISTRAR,
                UltimoReinicioCreditos = DateTime.Today, // Al crear, se considera reiniciado hoy
                FechaRegistro = DateTime.UtcNow // O DateTime.Now si prefieres hora local para el registro
            };
        }
    }
}