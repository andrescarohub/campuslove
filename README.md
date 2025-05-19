USE CampusLoveDB; -- Asegúrate de estar en la base de datos correcta

-- --- INSERTS PARA LA TABLA Usuarios ---

INSERT INTO Usuarios (Nombre, Edad, Genero, Intereses, Carrera, FrasePerfil, CreditosLikesDiarios, UltimoReinicioCreditos, FechaRegistro) VALUES
('AnaLopez', 20, 'Femenino', 'Musica,Viajar,Fotografia', 'Diseño Gráfico', 'Buscando inspiración y conexiones genuinas ✨', 10, CURDATE(), NOW()),
('CarlosRuiz', 22, 'Masculino', 'Deportes,Cine,Tecnologia', 'Ingeniería de Software', 'Programando mi vida y buscando mi co-piloto 💻', 8, CURDATE(), NOW()),
('SofiaGomez', 19, 'Femenino', 'Leer,Arte,Yoga,Cocina', 'Psicología', 'Explorando la mente y el corazón 🧘‍♀️', 10, CURDATE(), NOW()),
('DavidMartinez', 21, 'Masculino', 'Videojuegos,Anime,Senderismo', 'Animación Digital', 'Creando mundos y buscando aventuras 🎮', 5, CURDATE(), NOW()),
('LauraPerez', 23, 'Femenino', 'Baile,Idiomas,Netflix', 'Comunicación Audiovisual', 'Amante de las buenas historias y el café ☕', 10, CURDATE(), NOW()),
('JavierTorres', 20, 'Masculino', 'Futbol,Gym,Emprender', 'Administración de Empresas', 'Siempre en movimiento, buscando el próximo gran proyecto 🚀', 7, CURDATE(), NOW()),
('ElenaMorales', 22, 'Femenino', 'Mascotas,Naturaleza,Voluntariado', 'Veterinaria', 'Cuidando a los que no tienen voz 🐾', 10, CURDATE(), NOW()),
('MiguelAngel', 24, 'Masculino', 'Musica,Conciertos,Guitarra,Filosofia', 'Música', 'La vida es una canción, busco con quién compartir la melodía 🎸', 9, CURDATE(), NOW());

-- --- INSERTS PARA LA TABLA Interacciones ---
-- Asumimos que los IDs de usuario son 1 para Ana, 2 para Carlos, etc. (según el orden de inserción anterior)

-- Ana (ID 1) da Like a Carlos (ID 2)
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(1, 2, 'Like', NOW());

-- Carlos (ID 2) da Like a Ana (ID 1) -> Esto debería generar un MATCH
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(2, 1, 'Like', NOW());

-- Sofia (ID 3) da Like a David (ID 4)
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(3, 4, 'Like', NOW());

-- David (ID 4) da Dislike a Sofia (ID 3)
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(4, 3, 'Dislike', NOW());

-- Laura (ID 5) da Like a Javier (ID 6)
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(5, 6, 'Like', NOW());

-- Javier (ID 6) da Like a Laura (ID 5) -> Esto debería generar un MATCH
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(6, 5, 'Like', NOW());

-- Elena (ID 7) da Like a MiguelAngel (ID 8)
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(7, 8, 'Like', NOW());

-- Ana (ID 1) da Like a David (ID 4)
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(1, 4, 'Like', NOW());

-- Carlos (ID 2) da Like a Sofia (ID 3)
INSERT INTO Interacciones (UsuarioOrigenID, UsuarioDestinoID, TipoInteraccion, FechaInteraccion) VALUES
(2, 3, 'Like', NOW());


-- --- INSERTS PARA LA TABLA Matches ---
-- Estos se generarían automáticamente por la lógica de la aplicación si dos usuarios se dan Like.
-- Pero si quieres forzar algunos matches directamente en la BD para probar "Ver Mis Matches":

-- Match entre Ana (ID 1) y Carlos (ID 2)
-- (Asegúrate que Usuario1ID < Usuario2ID)
INSERT INTO Matches (Usuario1ID, Usuario2ID, FechaMatch)
SELECT 1, 2, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Matches WHERE (Usuario1ID = 1 AND Usuario2ID = 2));

-- Match entre Laura (ID 5) y Javier (ID 6)
INSERT INTO Matches (Usuario1ID, Usuario2ID, FechaMatch)
SELECT 5, 6, NOW()
WHERE NOT EXISTS (SELECT 1 FROM Matches WHERE (Usuario1ID = 5 AND Usuario2ID = 6));


-- --- Opcional: Actualizar algunos créditos para simular uso ---
UPDATE Usuarios SET CreditosLikesDiarios = 7, UltimoReinicioCreditos = CURDATE() WHERE UsuarioID = 1; -- Ana usó 3 likes
UPDATE Usuarios SET CreditosLikesDiarios = 6, UltimoReinicioCreditos = CURDATE() WHERE UsuarioID = 2; -- Carlos usó 2 likes (el original era 8)

SELECT 'Datos de ejemplo insertados en CampusLoveDB.' AS Estado;
