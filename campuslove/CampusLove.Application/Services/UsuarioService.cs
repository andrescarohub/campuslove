using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using CampusLove.Core.Entities;
using CampusLove.Core.Interfaces;
using CampusLove.Application.Factories; // Para UsuarioFactory

namespace CampusLove.Application.Services
{
    public class UsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private const int CREDITOS_LIKES_POR_DEFECTO_DIARIOS = 10; // Definido también en UsuarioFactory, mantener consistencia o centralizar

        public UsuarioService(IUsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
        }

        public async Task<(Usuario? usuario, string mensaje)> RegistrarUsuarioAsync(
            string nombre,
            int edad,
            string genero,
            List<string> intereses,
            string carrera,
            string frasePerfil)
        {
            // Validaciones básicas (podrían ser más extensas)
            if (string.IsNullOrWhiteSpace(nombre)) return (null, "El nombre no puede estar vacío.");
            if (edad < 18) return (null, "Debes ser mayor de 18 años para registrarte."); // Ejemplo de validación
            if (await _usuarioRepository.GetByNombreAsync(nombre.Trim()) != null)
            {
                return (null, "El nombre de usuario ya existe. Intenta con otro.");
            }

            var nuevoUsuario = UsuarioFactory.CrearNuevoUsuario(
                nombre.Trim(), // Guardar nombres sin espacios extra
                edad,
                genero, // Podrías normalizar el género (ej. a TitleCase)
                intereses,
                carrera,
                frasePerfil);

            try
            {
                int nuevoId = await _usuarioRepository.AddAsync(nuevoUsuario);
                nuevoUsuario.UsuarioID = nuevoId; // Asignar el ID generado por la BD
                return (nuevoUsuario, "¡Registro exitoso!");
            }
            catch (Exception ex)
            {
                // Aquí podrías loggear el error ex
                return (null, $"Error al registrar el usuario: {ex.Message}");
            }
        }

        public async Task<Usuario?> IniciarSesionAsync(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return null;

            var usuario = await _usuarioRepository.GetByNombreAsync(nombre.Trim());
            if (usuario != null)
            {
                // Asegurar que los créditos de like se reinicien si es un nuevo día
                await AsegurarCreditosDiariosAsync(usuario);
            }
            return usuario;
        }

        public async Task AsegurarCreditosDiariosAsync(Usuario usuario)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            bool necesitaActualizar = false;
            if (usuario.UltimoReinicioCreditos == null || usuario.UltimoReinicioCreditos.Value.Date < DateTime.Today)
            {
                usuario.RestaurarCreditos(CREDITOS_LIKES_POR_DEFECTO_DIARIOS);
                necesitaActualizar = true;
            }
            
            // También podríamos querer limitar los créditos a un máximo incluso si no se han gastado.
            // Math.Min para asegurar que los créditos no excedan el máximo diario permitido.
            // Esto es más útil si los créditos pudieran aumentar por otras vías.
            // Por ahora, la lógica de RestaurarCreditos ya los establece al valor por defecto.
            // usuario.CreditosLikesDiarios = Math.Min(usuario.CreditosLikesDiarios, CREDITOS_LIKES_POR_DEFECTO_DIARIOS);


            if (necesitaActualizar)
            {
                try
                {
                    await _usuarioRepository.UpdateAsync(usuario);
                }
                catch (Exception ex)
                {
                    // Loggear error, pero no impedir el inicio de sesión por esto si el usuario ya fue cargado.
                    Console.WriteLine($"Advertencia: No se pudieron actualizar los créditos para {usuario.Nombre}: {ex.Message}");
                }
            }
        }

        public async Task<Usuario?> GetUsuarioByIdAsync(int id)
        {
            return await _usuarioRepository.GetByIdAsync(id);
        }
    }
}