# 💖 CampusLove - Donde Nace el Amor (Versión Consola) 💖

¡Bienvenido a CampusLove! Una aplicación de consola en C# que simula un sistema de emparejamiento universitario. Conecta con otros estudiantes, descubre perfiles interesantes, ¡y quizás encuentres a tu media naranja académica!

## 🌟 Descripción del Proyecto

CampusLove es una aplicación de consola diseñada para simular un sistema de citas. Permite a los usuarios registrarse, explorar perfiles de otros estudiantes, expresar interés ("Like") o desinterés ("Dislike"), y descubrir coincidencias ("Matches") cuando el interés es mutuo.

El proyecto implementa un flujo completo de interacciones, utilizando C# con .NET 8, una base de datos MySQL para la persistencia de datos, y una interfaz de usuario operada completamente a través de la consola. Se ha puesto énfasis en una arquitectura limpia, principios SOLID y patrones de diseño para una base de código robusta y mantenible.

## ✨ Características Principales

*   **Registro de Usuarios:** Crea tu perfil con nombre, edad, género, intereses, carrera y una frase que te defina.
*   **Visualización de Perfiles:** Explora perfiles de otros usuarios uno por uno.
*   **Interacciones Like/Dislike:** Expresa tu interés o sigue buscando.
*   **Sistema de Matches:** ¡Descubre quién también te dio Like! Se forma un Match cuando ambos usuarios se gustan.
*   **Listado de Matches:** Consulta todos tus Matches en cualquier momento.
*   **Créditos de Interacción:** Un sistema de "créditos de like" diarios para fomentar interacciones más significativas (configurable, por defecto 10 likes/día).
*   **Estadísticas del Sistema:** Visualiza datos interesantes como el usuario con más likes recibidos o más matches.
*   **Interfaz de Consola Amigable:** Menús claros, con colores y emojis para una experiencia más agradable. 😉
*   **Persistencia en Base de Datos:** Todos los datos de usuarios, interacciones y matches se guardan en MySQL.

## 🛠️ Tecnologías Utilizadas

*   **Lenguaje:** C#
*   **Plataforma:** .NET 8.0
*   **Base de Datos:** MySQL
*   **Acceso a Datos:** ADO.NET con la ayuda de Dapper (micro-ORM)
*   **IDE Sugerido (Desarrollo):** Visual Studio Code / Visual Studio
*   **Consola:** Interfaz de usuario principal.

## 📐 Arquitectura y Diseño

El proyecto sigue principios de **Arquitectura Limpia** y **SOLID**:

*   **Core (Dominio):** Contiene las entidades (`Usuario`, `Interaccion`, `Match`), enums y las interfaces de los repositorios. Es el corazón del dominio y no depende de otras capas.
*   **Application (Lógica de Aplicación):** Orquesta los casos de uso. Contiene los servicios (`UsuarioService`, `EmparejamientoService`, `EstadisticasService`), fábricas (`UsuarioFactory`) y estrategias (`IMatchingStrategy`). Depende de `Core`.
*   **Infrastructure (Infraestructura):** Implementa las interfaces definidas en `Core` para el acceso a datos (ej. `UsuarioRepository` para MySQL) y otras preocupaciones externas. Depende de `Core`.
*   **ConsoleApp (Presentación):** Es la aplicación de consola que interactúa con el usuario. Contiene `Program.cs` (punto de entrada y configuración de "DI manual"), `ConsoleUI.cs` (para la interfaz) y extensiones. Depende de `Application` y `Core`.

**Patrones de Diseño Aplicados:**

*   **Repository Pattern:** Para abstraer el acceso a datos.
*   **Factory Pattern:** Para la creación de objetos (`UsuarioFactory`, `DbConnectionFactory`).
*   **Strategy Pattern:** Para definir diferentes algoritmos de emparejamiento/sugerencia de perfiles (`IMatchingStrategy`).
*   **Inyección de Dependencias (Manual):** Las dependencias se inyectan a través de los constructores en `Program.cs`.

## 🚀 Cómo Empezar

### Prerrequisitos

1.  **SDK de .NET 8.0:** Asegúrate de tenerlo instalado ([Descargar .NET](https://dotnet.microsoft.com/download/dotnet/8.0)).
2.  **Servidor MySQL:** Necesitas una instancia de MySQL corriendo (local o remota).
3.  **Gestor de MySQL (Opcional pero recomendado):** MySQL Workbench, DBeaver, phpMyAdmin, etc., para ejecutar el script de creación de la base de datos.

### Pasos para la Configuración

1.  **Clonar el Repositorio (si aplica):**
    ```bash
    git clone https://tu-repositorio-git.com/campuslove.git
    cd campuslove
    ```

2.  **Crear la Base de Datos y Tablas:**
    *   Abre tu gestor de MySQL.
    *   Copia y ejecuta el script SQL proporcionado en `Database/setup.sql` (o el script que te facilitó el desarrollador) para crear la base de datos `CampusLoveDB` y todas sus tablas.
    *   *(Opcional)* Ejecuta el script de `Database/sample-data.sql` (o los inserts que te facilitó el desarrollador) para poblar la base de datos con datos de ejemplo.

3.  **Configurar la Cadena de Conexión:**
    *   Abre el archivo `CampusLove.ConsoleApp/Program.cs`.
    *   Busca la constante `MySqlConnectionString` cerca del inicio de la clase `Program`.
    *   **Modifica la cadena de conexión** con los datos de tu servidor MySQL (servidor, puerto, nombre de la base de datos, usuario y contraseña).
        ```csharp
        private const string MySqlConnectionString = "Server=TU_SERVIDOR;Port=TU_PUERTO;Database=CampusLoveDB;Uid=TU_USUARIO_MYSQL;Pwd=TU_CONTRASEÑA_MYSQL;";
        ```

4.  **Restaurar Dependencias y Compilar:**
    Abre una terminal en la raíz del proyecto (donde está el archivo `campuslove.csproj` o `campuslove.sln`).
    ```bash
    dotnet restore
    dotnet build
    ```

5.  **Ejecutar la Aplicación:**
    ```bash
    dotnet run --project CampusLove.ConsoleApp/CampusLove.ConsoleApp.csproj
    ```
    O si estás en la raíz y tu `campuslove.csproj` es el ejecutable:
    ```bash
    dotnet run
    ```

### 🎮 Uso de la Aplicación

Una vez que la aplicación se esté ejecutando:

1.  Sigue las instrucciones del menú en consola.
2.  **Regístrate** como nuevo usuario o **Inicia Sesión** si ya tienes una cuenta (con los datos de ejemplo si los insertaste).
3.  Explora las opciones: ver perfiles, dar likes/dislikes, revisar tus matches, etc.
4.  ¡Diviértete!

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Si quieres mejorar CampusLove, por favor:

1.  Haz un Fork del proyecto.
2.  Crea tu Feature Branch (`git checkout -b feature/AmazingFeature`).
3.  Commitea tus cambios (`git commit -m 'Add some AmazingFeature'`).
4.  Push al Branch (`git push origin feature/AmazingFeature`).
5.  Abre un Pull Request.

## 📝 Licencia

Este proyecto está bajo la Licencia MIT - mira el archivo `LICENSE.md` para más detalles (si decides añadir uno).

## 🧑‍💻 Desarrollador Principal

*   **[Tu Nombre / Tu Alias]** - [Tu Email o GitHub si quieres]

---

¡Que empiece el flechazo! 🏹❤️
